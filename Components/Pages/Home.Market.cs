using CyberPetApp.Models;
using CyberPetApp.Services;

namespace CyberPetApp.Components.Pages;

public partial class Home
{
    private async Task SellFish(Fish fish)
    {
        await WithDbLock(async () =>
        {
            var newMoney = await _fishingService.SellFishAsync(player!.Id, fish.Id);
            if (newMoney.HasValue)
            {
                lock (catStateLock)
                    cat.ApplyActivityCost(CatActivityType.DirectFishSell, GetHouseBuffs());
                player.FishBackpack.Remove(fish);
                player.Money = newMoney.Value;
                player.TotalFishGoldEarned += MarketService.DirectSellPrice(fish);
                marketMessage = $"直售 [{fish.Name}] 获得 {MarketService.DirectSellPrice(fish)}g（85%）";
                await _achievementService.SyncProgressAsync(player, fishRecords, HasDeepSeaPermanent());
                await _playerService.SaveProgressAsync(player);
            }
        });
    }

    private async Task ListFishOnMarket(Fish fish)
    {
        await WithDbLock(async () =>
        {
            var (ok, msg) = await _marketService.ListFishAsync(player!, cat, fish, GetHouseBuffs());
            marketMessage = msg;
            if (ok)
            {
                await _cyberCatService.SaveAsync(cat);
                await ReloadMarketAsync();
                activeSection = "market";
            }
        });
    }

    private async Task DelistFish(MarketListing listing)
    {
        await WithDbLock(async () =>
        {
            var (ok, msg) = await _marketService.DelistAsync(player!, listing.Id);
            marketMessage = msg;
            if (ok) await ReloadMarketAsync();
        });
    }

    private async Task AcceptOffer(NpcOffer offer)
    {
        await WithDbLock(async () =>
        {
            var (ok, msg, newMoney) = await _marketService.AcceptOfferAsync(player!, offer.Id);
            marketMessage = msg;
            if (ok && newMoney.HasValue)
            {
                player!.Money = newMoney.Value;
                await _achievementService.SyncProgressAsync(player, fishRecords, HasDeepSeaPermanent());
                await ReloadMarketAsync();
            }
        });
    }

    private async Task CounterOffer(NpcOffer offer)
    {
        await WithDbLock(async () =>
        {
            var (ok, msg) = await _marketService.CounterOfferAsync(
                player!, cat, offer.Id, GetHouseBuffs(), milestoneBuffs.CounterBonus);
            marketMessage = msg;
            await _cyberCatService.SaveAsync(cat);
            await ReloadMarketAsync();
        });
    }

    private async Task RejectOffer(NpcOffer offer)
    {
        await WithDbLock(async () =>
        {
            var (ok, msg) = await _marketService.RejectOfferAsync(player!, offer.Id);
            marketMessage = msg;
            if (ok) await ReloadMarketAsync();
        });
    }

    private async Task UnlockRoom(Room room)
    {
        await WithDbLock(async () =>
        {
            if (!await _houseService.SaveRoomUnlockAsync(player!, room))
                feedMessage = "金币不足或房间已解锁";
            else
                InvalidateHouseBuffs();
        });
        await InvokeAsync(StateHasChanged);
    }

    private async Task BuyFurniture(Furniture item)
    {
        await WithDbLock(async () =>
        {
            if (!await _houseService.SaveFurniturePurchaseAsync(player!, item))
                feedMessage = "金币不足或已拥有";
            else
            {
                InvalidateHouseBuffs();
                ApplyFeederCapacity();
            }
        });
        await InvokeAsync(StateHasChanged);
    }

    private async Task UpgradeFurniture(Furniture item)
    {
        await WithDbLock(async () =>
        {
            var (ok, msg) = await _houseService.UpgradeFurnitureAsync(player!, item);
            feedMessage = msg;
            if (ok) InvalidateHouseBuffs();
        });
        await InvokeAsync(StateHasChanged);
    }

    private async Task TreatCat()
    {
        await WithDbLock(async () =>
        {
            var (ok, msg) = await _cyberCatService.TreatAsync(player!, cat);
            feedMessage = msg;
        });
        TryRefreshSidebarVitals();
        await InvokeAsync(StateHasChanged);
    }

    private async Task RefreshBounty()
    {
        await WithDbLock(async () =>
        {
            var (ok, msg) = await _dailyBountyService.RefreshBountyAsync(player!, player!.FishingLevel);
            marketMessage = msg;
        });
    }

    private async Task ReloadMarketAsync()
    {
        await _marketService.ExpireOldOffersAsync(player!.Id);
        marketListings = await _marketService.GetActiveListingsAsync(player.Id);
    }

    private int StallTicketCount() =>
        player!.Backpack.GetValueOrDefault(MarketService.StallTicketItemName);

    private int MarketListingLimit() => MarketService.GetListingLimit(player!);

}
