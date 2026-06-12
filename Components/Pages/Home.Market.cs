using CyberPetApp.Models;

using CyberPetApp.Services;



namespace CyberPetApp.Components.Pages;



public partial class Home

{

    private async Task SellFish(Fish fish)

    {

        if (player is null || !player.FishBackpack.Any(f => f.Id == fish.Id))

            return;



        var payout = MarketService.DirectSellPrice(fish);



        // 乐观更新：立即刷新 UI，不等待 dbLock / 数据库

        lock (catStateLock)

            cat.ApplyActivityCost(CatActivityType.DirectFishSell, GetHouseBuffs());

        player.FishBackpack.RemoveAll(f => f.Id == fish.Id);

        player.Money += payout;

        player.TotalFishGoldEarned += payout;

        marketMessage = $"直售 [{fish.Name}] 获得 {payout}g（85%）";

        tickGeneration++;

        BindGameSession();

        await InvokeAsync(StateHasChanged);



        // 后台持久化，避免与 2s tick 存档争抢 dbLock 时卡住点击

        _ = PersistSellFishAsync(fish);

    }



    private async Task PersistSellFishAsync(Fish fish)

    {

        try

        {

            await WithDbLock(async () =>

            {

                await _fishingService.PersistSellFishAsync(player!, fish);

                await _achievementService.SyncProgressAsync(player!, fishRecords, HasDeepSeaPermanent());

            });

        }

        catch (Exception ex)

        {

            Logger.LogWarning(ex, "PersistSellFish failed for {FishId}", fish.Id);

        }

    }



    private Task ListFishOnMarket(Fish fish)

    {

        if (player is null) return Task.CompletedTask;



        if (!MarketService.TryPrepareList(player, marketListings, fish, out var listing, out var stallFee, out var error))

        {

            marketMessage = error;

            NotifyMarketChrome();

            return Task.CompletedTask;

        }



        MarketService.ApplyListOptimistic(player, marketListings, fish, listing, stallFee);

        activeSection = "market";

        marketMessage = $"已上架 [{fish.Name}]，扣摊位费 {stallFee}g（不退），底价 {listing.ListingFloorPrice}g，等待 NPC 出价…";

        NotifyMarketChrome();

        _ = PersistListFishAsync(fish, listing);

        return Task.CompletedTask;

    }



    private Task FinalizeListFish(MarketListEventArgs args)

    {

        marketMessage = $"已上架 [{args.Fish.Name}]，扣摊位费 {args.StallFee}g（不退），底价 {args.Listing.ListingFloorPrice}g，等待 NPC 出价…";

        NotifyMarketChrome();

        _ = PersistListFishAsync(args.Fish, args.Listing);

        return Task.CompletedTask;

    }



    private Task FinalizeDelistFish(MarketDelistEventArgs args)

    {

        marketMessage = $"已下架 [{args.Listing.FishName}]，鱼已退回背包";

        NotifyMarketChrome();

        _ = PersistDelistFishAsync(args.Listing.Id, args.ReturnedFish);

        return Task.CompletedTask;

    }



    private void NotifyMarketChrome()

    {

        tickGeneration++;

        BindGameSession();

        _ = InvokeAsync(StateHasChanged);

    }



    private async Task PersistListFishAsync(Fish fish, MarketListing listing)

    {

        try

        {

            await WithDbLock(async () =>

            {

                var (ok, msg) = await _marketService.CommitListFishAsync(player!, fish, listing);

                if (!ok)

                {

                    marketMessage = msg;

                    player!.Money += EconomySinks.MarketListingFee(fish.SellPrice);

                    if (!player.FishBackpack.Any(f => f.Id == fish.Id))

                        player.FishBackpack.Add(fish);

                    marketListings.RemoveAll(v => v.Listing.Id == listing.Id);

                    await ReloadMarketAsync();

                }

                else

                {

                    lock (catStateLock)

                        cat.ApplyActivityCost(CatActivityType.MarketList, GetHouseBuffs());

                    await _cyberCatService.SaveAsync(cat);

                }

            });

            NotifyMarketChrome();

        }

        catch (Exception ex)

        {

            Logger.LogWarning(ex, "PersistListFish failed for {FishId}", fish.Id);

        }

    }



    private async Task PersistDelistFishAsync(Guid listingId, Fish fish)

    {

        try

        {

            await WithDbLock(async () =>

            {

                var (ok, msg) = await _marketService.CommitDelistAsync(player!, listingId, fish);

                if (!ok)

                {

                    marketMessage = msg;

                    player!.FishBackpack.RemoveAll(f => f.Id == fish.Id);

                    await ReloadMarketAsync();

                }

            });

            NotifyMarketChrome();

        }

        catch (Exception ex)

        {

            Logger.LogWarning(ex, "PersistDelist failed for listing {ListingId}", listingId);

        }

    }



    private Task AcceptOffer(NpcOffer offer)
    {
        if (player is null) return Task.CompletedTask;

        var view = marketListings.FirstOrDefault(v => v.PendingOffers.Any(o => o.Id == offer.Id));
        if (view is not null)
        {
            player.Money += offer.OfferPrice;
            player.TotalFishGoldEarned += offer.OfferPrice;
            player.MarketSalesCount += 1;
            marketListings.RemoveAll(v => v.Listing.Id == view.Listing.Id);
            marketMessage = $"{MarketService.BuyerLabel(offer.BuyerType)} 以 {offer.OfferPrice}g 买走 [{view.Listing.FishName}]";
            NotifyMarketChrome();
        }

        _ = PersistAcceptOfferAsync(offer);
        return Task.CompletedTask;
    }

    private async Task PersistAcceptOfferAsync(NpcOffer offer)
    {
        try
        {
            await WithDbLock(async () =>
            {
                var (ok, msg, newMoney) = await _marketService.AcceptOfferAsync(player!, offer.Id);
                if (!ok)
                {
                    marketMessage = msg;
                    await ReloadMarketAsync();
                    return;
                }

                if (newMoney.HasValue)
                    player!.Money = newMoney.Value;
                await _achievementService.SyncProgressAsync(player!, fishRecords, HasDeepSeaPermanent());
                await ReloadMarketAsync();
            });
            NotifyMarketChrome();
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "PersistAcceptOffer failed for offer {OfferId}", offer.Id);
            try
            {
                await WithDbLock(ReloadMarketAsync);
                NotifyMarketChrome();
            }
            catch (Exception reloadEx)
            {
                Logger.LogWarning(reloadEx, "ReloadMarket after AcceptOffer failure");
            }
        }
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

        await InvokeAsync(StateHasChanged);

    }



    private async Task RejectOffer(NpcOffer offer)

    {

        await WithDbLock(async () =>

        {

            var (ok, msg) = await _marketService.RejectOfferAsync(player!, offer.Id);

            marketMessage = msg;

            if (ok) await ReloadMarketAsync();

        });

        await InvokeAsync(StateHasChanged);

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

        await InvokeAsync(StateHasChanged);

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

