using CyberPetApp.Data;
using CyberPetApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberPetApp.Services;

public class MarketListingView
{
    public MarketListing Listing { get; init; } = null!;
    public List<NpcOffer> PendingOffers { get; init; } = [];
}

public sealed class MarketListEventArgs
{
    public required Fish Fish { get; init; }
    public required MarketListing Listing { get; init; }
    public required int StallFee { get; init; }
}

public sealed class MarketDelistEventArgs
{
    public required MarketListing Listing { get; init; }
    public required Fish ReturnedFish { get; init; }
}

public class MarketService
{
    public const string StallTicketItemName = "摊位券";
    public const int BaseListingLimit = 3;
    public const int MaxTicketBonus = 2;
    public const double DirectSellRate = 0.85;
    public const double MarketFloorRate = 0.90;
    public const int OfferLifetimeMinutes = 30;
    public const int NpcOfferIntervalTicks = 150; // 5 分钟 @ 2s/tick
    public const double CounterPercent = 0.10; // 还价幅度 +10%
    public const int BuyerBanHours = 24;

    private readonly AppDbContext _context;
    private readonly Random _random = new();

    public MarketService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>挂单上限 = 3 + min(摊位券数量, 2)，最高 5。</summary>
    public static int GetListingLimit(Player player)
    {
        int tickets = player.Backpack.GetValueOrDefault(StallTicketItemName);
        return BaseListingLimit + Math.Min(tickets, MaxTicketBonus);
    }

    /// <summary>鱼市搬运工种：NPC 报价间隔缩短为 120 tick。</summary>
    public static int NpcOfferIntervalForJob(WorkJobType job) =>
        job == WorkJobType.FishMarketPorter ? 120 : NpcOfferIntervalTicks;

    public static int DirectSellPrice(Fish fish) =>
        Math.Max(1, (int)(fish.SellPrice * DirectSellRate));

    public static int MarketFloorPrice(Fish fish) =>
        Math.Max(1, (int)(fish.SellPrice * MarketFloorRate));

    /// <summary>上架前校验（UI 与持久化共用）。</summary>
    public static bool TryPrepareList(
        Player player,
        IReadOnlyList<MarketListingView> listings,
        Fish fish,
        out MarketListing listing,
        out int stallFee,
        out string error)
    {
        listing = null!;
        stallFee = 0;
        error = "";

        if (!player.FishBackpack.Any(f => f.Id == fish.Id))
        {
            error = "背包里没有这条鱼";
            return false;
        }

        int limit = GetListingLimit(player);
        if (listings.Count >= limit)
        {
            error = $"挂单已满（{listings.Count}/{limit}），打工获取摊位券可提升上限";
            return false;
        }

        stallFee = EconomySinks.MarketListingFee(fish.SellPrice);
        if (player.Money < stallFee)
        {
            error = $"金币不足，上架摊位费需 {stallFee}g（拒绝/下架不退）";
            return false;
        }

        listing = CreateListingFromFish(fish, player.Id);
        return true;
    }

    public static void ApplyListOptimistic(
        Player player,
        List<MarketListingView> listings,
        Fish fish,
        MarketListing listing,
        int stallFee)
    {
        player.Money -= stallFee;
        player.FishBackpack.RemoveAll(f => f.Id == fish.Id);
        listings.Insert(0, new MarketListingView { Listing = listing, PendingOffers = [] });
    }

    public static void ApplyDelistOptimistic(
        Player player,
        List<MarketListingView> listings,
        MarketListing listing,
        out Fish returnedFish)
    {
        returnedFish = ListingToFish(listing, player.Id);
        listings.RemoveAll(v => v.Listing.Id == listing.Id);
        player.FishBackpack.Add(returnedFish);
    }

    /// <summary>
    /// 还价成功率 = clamp(50% + Happiness/1000×20% + CHM/1000×15% - 还价幅度×30% + milestoneBonus, 10%, 90%)
    /// </summary>
    public static double ComputeCounterSuccessRate(int catHappiness, int catChm, double counterPercent = CounterPercent, double milestoneBonus = 0)
    {
        double rate = 0.50 + (catHappiness / 1000.0) * 0.20 + (catChm / 1000.0) * 0.15 - counterPercent * 0.30 + milestoneBonus;
        return Math.Clamp(rate, 0.10, 0.90);
    }

    public static double ComputeCounterSuccessRate(int catHappiness, double counterPercent = CounterPercent, double milestoneBonus = 0)
    {
        return ComputeCounterSuccessRate(catHappiness, 10, counterPercent, milestoneBonus);
    }

    public async Task<List<MarketListingView>> GetActiveListingsAsync(Guid playerId)
    {
        var listings = await _context.MarketListings
            .AsNoTracking()
            .Where(l => l.PlayerId == playerId && l.IsActive)
            .OrderByDescending(l => l.ListedAt)
            .ToListAsync();

        if (listings.Count == 0) return [];

        var listingIds = listings.Select(l => l.Id).ToList();
        var now = DateTime.UtcNow;
        var offers = await _context.NpcOffers
            .AsNoTracking()
            .Where(o => listingIds.Contains(o.ListingId) && !o.IsAccepted && o.ExpiresAt > now)
            .ToListAsync();

        return listings.Select(l => new MarketListingView
        {
            Listing = l,
            PendingOffers = offers.Where(o => o.ListingId == l.Id).OrderByDescending(o => o.OfferPrice).ToList()
        }).ToList();
    }

    public async Task<(bool Ok, string Message)> ListFishAsync(
        Player player, CyberCat cat, Fish fish, HouseBuffs houseBuffs = default)
    {
        int limit = GetListingLimit(player);
        int activeCount = await _context.MarketListings
            .CountAsync(l => l.PlayerId == player.Id && l.IsActive);
        if (activeCount >= limit)
            return (false, $"挂单已满（{activeCount}/{limit}），打工获取摊位券可提升上限");

        var dbFish = await _context.Fishes
            .FirstOrDefaultAsync(f => f.Id == fish.Id && f.PlayerId == player.Id);
        if (dbFish is null)
            return (false, "背包里没有这条鱼");

        int stallFee = EconomySinks.MarketListingFee(dbFish.SellPrice);
        var dbPlayer = await _context.Players.FindAsync(player.Id);
        if (dbPlayer is null)
            return (false, "玩家不存在");
        if (dbPlayer.Money < stallFee)
            return (false, $"金币不足，上架摊位费需 {stallFee}g（拒绝/下架不退）");

        dbPlayer.Money -= stallFee;
        player.Money = dbPlayer.Money;

        var listing = CreateListingFromFish(dbFish, player.Id);

        _context.Fishes.Remove(dbFish);
        _context.MarketListings.Add(listing);
        await _context.SaveChangesAsync();

        cat.ApplyActivityCost(CatActivityType.MarketList, houseBuffs);
        player.FishBackpack.Remove(fish);
        return (true, $"已上架 [{dbFish.Name}]，扣摊位费 {stallFee}g（不退），底价 {listing.ListingFloorPrice}g，等待 NPC 出价…");
    }

    /// <summary>乐观 UI 已扣费/移鱼后，仅同步数据库。</summary>
    public async Task<(bool Ok, string Message)> CommitListFishAsync(Player player, Fish fish, MarketListing listing)
    {
        if (listing.PlayerId != player.Id)
            return (false, "无效挂单");

        int limit = GetListingLimit(player);
        int activeCount = await _context.MarketListings
            .CountAsync(l => l.PlayerId == player.Id && l.IsActive);
        if (activeCount >= limit)
            return (false, $"挂单已满（{activeCount}/{limit}）");

        var dbFish = await _context.Fishes
            .FirstOrDefaultAsync(f => f.Id == fish.Id && f.PlayerId == player.Id);
        if (dbFish is null)
            return (false, "背包里没有这条鱼");

        var dbPlayer = await _context.Players.FindAsync(player.Id);
        if (dbPlayer is null)
            return (false, "玩家不存在");

        dbPlayer.Money = player.Money;
        _context.Fishes.Remove(dbFish);

        if (!await _context.MarketListings.AnyAsync(l => l.Id == listing.Id))
            _context.MarketListings.Add(listing);

        await _context.SaveChangesAsync();
        return (true, "已同步");
    }

    public async Task<(bool Ok, string Message)> DelistAsync(Player player, Guid listingId)
    {
        var (ok, msg, fish) = await DelistCoreAsync(player.Id, listingId);
        if (ok && fish is not null)
            player.FishBackpack.Add(fish);
        return (ok, msg);
    }

    /// <summary>乐观 UI 已将鱼退回背包后，仅同步数据库。</summary>
    public async Task<(bool Ok, string Message)> CommitDelistAsync(Player player, Guid listingId, Fish returnedFish)
    {
        var listing = await _context.MarketListings
            .FirstOrDefaultAsync(l => l.Id == listingId && l.PlayerId == player.Id && l.IsActive);
        if (listing is null)
            return (false, "挂单不存在");

        returnedFish.PlayerId = player.Id;
        if (!await _context.Fishes.AnyAsync(f => f.Id == returnedFish.Id))
            _context.Fishes.Add(returnedFish);

        listing.IsActive = false;
        await RemoveListingOffersAndBansAsync(listingId);
        await _context.SaveChangesAsync();
        return (true, "已同步");
    }

    private async Task<(bool Ok, string Message, Fish? Fish)> DelistCoreAsync(Guid playerId, Guid listingId)
    {
        var listing = await _context.MarketListings
            .FirstOrDefaultAsync(l => l.Id == listingId && l.PlayerId == playerId && l.IsActive);
        if (listing is null)
            return (false, "挂单不存在", null);

        var fish = ListingToFish(listing, playerId);
        _context.Fishes.Add(fish);
        listing.IsActive = false;
        await RemoveListingOffersAndBansAsync(listingId);
        await _context.SaveChangesAsync();
        return (true, $"已下架 [{listing.FishName}]，鱼已退回背包", fish);
    }

    private async Task RemoveListingOffersAndBansAsync(Guid listingId)
    {
        var pendingOffers = await _context.NpcOffers
            .Where(o => o.ListingId == listingId && !o.IsAccepted)
            .ToListAsync();
        _context.NpcOffers.RemoveRange(pendingOffers);

        var bans = await _context.NpcListingBans.Where(b => b.ListingId == listingId).ToListAsync();
        _context.NpcListingBans.RemoveRange(bans);
    }

    public async Task<(bool Ok, string Message, int? NewMoney)> AcceptOfferAsync(Player player, Guid offerId)
    {
        var offer = await _context.NpcOffers
            .FirstOrDefaultAsync(o => o.Id == offerId && o.PlayerId == player.Id && !o.IsAccepted);
        if (offer is null || offer.ExpiresAt <= DateTime.UtcNow)
            return (false, "报价不存在或已过期", null);

        var listing = await _context.MarketListings
            .FirstOrDefaultAsync(l => l.Id == offer.ListingId && l.IsActive);
        if (listing is null)
            return (false, "挂单已失效", null);

        var dbPlayer = await _context.Players.FindAsync(player.Id);
        if (dbPlayer is null)
            return (false, "玩家不存在", null);

        dbPlayer.Money += offer.OfferPrice;
        dbPlayer.TotalFishGoldEarned += offer.OfferPrice;
        dbPlayer.MarketSalesCount += 1;
        offer.IsAccepted = true;
        listing.IsActive = false;

        var otherOffers = await _context.NpcOffers
            .Where(o => o.ListingId == listing.Id && o.Id != offer.Id && !o.IsAccepted)
            .ToListAsync();
        _context.NpcOffers.RemoveRange(otherOffers);

        await _context.SaveChangesAsync();
        player.Money = dbPlayer.Money;
        player.TotalFishGoldEarned = dbPlayer.TotalFishGoldEarned;
        player.MarketSalesCount = dbPlayer.MarketSalesCount;
        return (true, $"{BuyerLabel(offer.BuyerType)} 以 {offer.OfferPrice}g 买走 [{listing.FishName}]", player.Money);
    }

    /// <summary>拒绝报价：移除当前出价，该 NPC 可稍后再次出价。</summary>
    public async Task<(bool Ok, string Message)> RejectOfferAsync(Player player, Guid offerId)
    {
        var offer = await _context.NpcOffers
            .FirstOrDefaultAsync(o => o.Id == offerId && o.PlayerId == player.Id && !o.IsAccepted);
        if (offer is null)
            return (false, "报价不存在");

        _context.NpcOffers.Remove(offer);
        await _context.SaveChangesAsync();
        return (true, $"已拒绝 {BuyerLabel(offer.BuyerType)} 的报价");
    }

    /// <summary>还价 +10%：成功则抬价，失败则该 NPC 24h 内不再对此挂单出价。</summary>
    public async Task<(bool Ok, string Message)> CounterOfferAsync(
        Player player, CyberCat cat, Guid offerId, HouseBuffs houseBuffs = default, double milestoneCounterBonus = 0)
    {
        var offer = await _context.NpcOffers
            .FirstOrDefaultAsync(o => o.Id == offerId && o.PlayerId == player.Id && !o.IsAccepted);
        if (offer is null || offer.ExpiresAt <= DateTime.UtcNow)
            return (false, "报价不存在或已过期");

        double successRate = ComputeCounterSuccessRate(cat.Happiness, cat.Chm, CounterPercent, milestoneCounterBonus);
        bool success = _random.NextDouble() < successRate;
        cat.ApplyActivityCost(CatActivityType.MarketCounter, houseBuffs);

        if (success)
        {
            int newPrice = Math.Max(offer.OfferPrice + 1, (int)Math.Ceiling(offer.OfferPrice * (1 + CounterPercent)));
            offer.OfferPrice = newPrice;
            await _context.SaveChangesAsync();
            return (true, $"{BuyerLabel(offer.BuyerType)} 同意还价 → {newPrice}g（成功率 {(int)(successRate * 100)}%）");
        }

        // 还价失败：24h 禁令
        var ban = await _context.NpcListingBans
            .FirstOrDefaultAsync(b => b.ListingId == offer.ListingId && b.BuyerType == offer.BuyerType);
        var bannedUntil = DateTime.UtcNow.AddHours(BuyerBanHours);
        if (ban is null)
        {
            _context.NpcListingBans.Add(new NpcListingBan
            {
                ListingId = offer.ListingId,
                BuyerType = offer.BuyerType,
                BannedUntil = bannedUntil
            });
        }
        else
            ban.BannedUntil = bannedUntil;

        _context.NpcOffers.Remove(offer);
        await _context.SaveChangesAsync();
        return (false, $"{BuyerLabel(offer.BuyerType)} 拒绝还价，24h 内不再对此鱼出价（成功率 {(int)(successRate * 100)}%）");
    }

    public async Task TryGenerateNpcOffersAsync(Guid playerId, int catHappiness = 500, double npcOfferChanceMultiplier = 1.0, int catChm = 10)
    {
        var now = DateTime.UtcNow;
        var listings = await _context.MarketListings
            .Where(l => l.PlayerId == playerId && l.IsActive)
            .ToListAsync();

        double offerChance = Math.Min(0.85, 0.5 * npcOfferChanceMultiplier);

        foreach (var listing in listings)
        {
            bool hasPending = await _context.NpcOffers
                .AnyAsync(o => o.ListingId == listing.Id && !o.IsAccepted && o.ExpiresAt > now);
            if (hasPending) continue;

            if (_random.NextDouble() > offerChance) continue;

            var buyer = PickBuyer(listing);
            if (await IsBuyerBannedAsync(listing.Id, buyer, now)) continue;

            double pref = BuyerPreference(buyer, listing, catHappiness, catChm);
            if (pref < 0.3) continue;

            double jitter = 0.9 + _random.NextDouble() * 0.2;
            int price = Math.Max(listing.ListingFloorPrice,
                (int)(listing.BaseSellPrice * pref * jitter));

            _context.NpcOffers.Add(new NpcOffer
            {
                ListingId = listing.Id,
                PlayerId = playerId,
                BuyerType = buyer,
                OfferPrice = price,
                CreatedAt = now,
                ExpiresAt = now.AddMinutes(OfferLifetimeMinutes)
            });
        }

        await _context.SaveChangesAsync();
    }

    public async Task ExpireOldOffersAsync(Guid playerId)
    {
        var now = DateTime.UtcNow;
        var expired = await _context.NpcOffers
            .Where(o => o.PlayerId == playerId && !o.IsAccepted && o.ExpiresAt <= now)
            .ToListAsync();
        if (expired.Count > 0)
        {
            _context.NpcOffers.RemoveRange(expired);
            await _context.SaveChangesAsync();
        }

        var staleBans = await _context.NpcListingBans.Where(b => b.BannedUntil <= now).ToListAsync();
        if (staleBans.Count > 0)
        {
            _context.NpcListingBans.RemoveRange(staleBans);
            await _context.SaveChangesAsync();
        }
    }

    private async Task<bool> IsBuyerBannedAsync(Guid listingId, NpcBuyerType buyer, DateTime now)
    {
        return await _context.NpcListingBans
            .AnyAsync(b => b.ListingId == listingId && b.BuyerType == buyer && b.BannedUntil > now);
    }

    private NpcBuyerType PickBuyer(MarketListing listing)
    {
        if (listing.Rarity == FishRarity.Legendary || listing.SizePercentage > 100)
            return _random.NextDouble() < 0.6 ? NpcBuyerType.Collector : NpcBuyerType.MoodNpc;
        if (listing.Rarity >= FishRarity.Epic)
            return _random.NextDouble() < 0.4 ? NpcBuyerType.Gourmet : NpcBuyerType.ChefCat;
        if (listing.Rarity == FishRarity.Rare)
            return _random.NextDouble() < 0.5 ? NpcBuyerType.ChefCat : NpcBuyerType.MoodNpc;
        if (listing.Rarity == FishRarity.Common)
            return NpcBuyerType.StrayCat;

        var roll = _random.NextDouble();
        if (roll < 0.2) return NpcBuyerType.Gourmet;
        if (roll < 0.4) return NpcBuyerType.Collector;
        if (roll < 0.6) return NpcBuyerType.ChefCat;
        if (roll < 0.8) return NpcBuyerType.MoodNpc;
        return NpcBuyerType.StrayCat;
    }

    /// <summary>NPC 偏好系数（相对 SellPrice），含人格溢价与魅力修正。</summary>
    private static double BuyerPreference(NpcBuyerType buyer, MarketListing listing, int catHappiness, int catChm)
    {
        bool oversized = listing.SizePercentage > 100;
        double basePref = buyer switch
        {
            NpcBuyerType.Gourmet => listing.Rarity switch
            {
                FishRarity.Legendary => 1.45,
                FishRarity.Epic => 1.30,
                FishRarity.Rare => 0.90,
                _ => 0.50
            },
            NpcBuyerType.StrayCat => listing.Rarity switch
            {
                FishRarity.Common => 0.75,
                FishRarity.Rare => 0.50,
                _ => 0.25
            },
            NpcBuyerType.Collector => oversized ? 2.20 : listing.Rarity switch
            {
                FishRarity.Legendary => 1.85,
                FishRarity.Epic => 1.15,
                FishRarity.Rare => 0.70,
                _ => 0.40
            },
            NpcBuyerType.ChefCat => listing.Rarity switch
            {
                FishRarity.Epic => 1.25,
                FishRarity.Rare => 1.10,
                FishRarity.Common => 0.65,
                _ => 0.45
            },
            NpcBuyerType.MoodNpc => 0.70 + (catHappiness / 1000.0) * 0.50,
            _ => 0.5
        };

        // 魅力修正：CHM/1000 × 15% 溢价
        basePref += (catChm / 1000.0) * 0.15;

        return basePref;
    }

    public static string BuyerLabel(NpcBuyerType type) => type switch
    {
        NpcBuyerType.Gourmet => "饕客",
        NpcBuyerType.StrayCat => "流浪猫",
        NpcBuyerType.Collector => "收藏家",
        NpcBuyerType.ChefCat => "厨师猫",
        NpcBuyerType.MoodNpc => "心情NPC",
        _ => "NPC"
    };

    public static string BuyerDesc(NpcBuyerType type) => type switch
    {
        NpcBuyerType.Gourmet => "偏好 Epic/Legendary 高价收",
        NpcBuyerType.StrayCat => "偏好 Common 低价批量",
        NpcBuyerType.Collector => "偏好 Legendary/超规格",
        NpcBuyerType.ChefCat => "偏好 Rare/Epic 食材",
        NpcBuyerType.MoodNpc => "出价随猫心情浮动",
        _ => ""
    };

    public static MarketListing CreateListingFromFish(Fish fish, Guid playerId) => new()
    {
        PlayerId = playerId,
        FishName = fish.Name,
        Rarity = fish.Rarity,
        ActualWeight = fish.ActualWeight,
        SizePercentage = fish.SizePercentage,
        HungerRestore = fish.HungerRestore,
        EnergyRestore = fish.EnergyRestore,
        HappinessRestore = fish.HappinessRestore,
        BaseSellPrice = fish.SellPrice,
        ListingFloorPrice = MarketFloorPrice(fish),
        ListedAt = DateTime.UtcNow,
        IsActive = true
    };

    public static Fish ListingToFish(MarketListing listing, Guid playerId) => new(
        listing.FishName,
        listing.HungerRestore,
        listing.EnergyRestore,
        listing.HappinessRestore,
        listing.BaseSellPrice,
        listing.Rarity,
        listing.ActualWeight)
    {
        PlayerId = playerId,
        SizePercentage = listing.SizePercentage
    };
}
