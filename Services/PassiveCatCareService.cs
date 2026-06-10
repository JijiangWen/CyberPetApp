using CyberPetApp.Models;

namespace CyberPetApp.Services;

/// <summary>家具被动养猫 tick 入口（游戏定时器在活动消耗之后调用）。</summary>
public class PassiveCatCareService
{
    public void Tick(CyberCat cat, IEnumerable<string> unlockedFurnitureIds, int intervalMs, ref int accumulatedMs) =>
        PassiveCatCare.Tick(cat, unlockedFurnitureIds, intervalMs, ref accumulatedMs);

    public IReadOnlyList<string> SummarizeRates(IEnumerable<string> unlockedFurnitureIds) =>
        PassiveCatCare.SummarizeRates(unlockedFurnitureIds);

    public IEnumerable<string> HintLines(IEnumerable<string> unlockedFurnitureIds) =>
        PassiveCatCare.HintLines(unlockedFurnitureIds);
}
