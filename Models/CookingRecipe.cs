namespace CyberPetApp.Models;

/// <summary>食谱：按鱼稀有度 + 可选多道菜映射成宠物饲料（BackpackItem）。</summary>
public record CookingRecipe(
    string RecipeId,
    string FoodName,
    FishRarity Input,
    int RequiredCookingLevel,
    int Hunger,
    int Energy,
    int Happiness,
    int Xp,
    CatFoodBuffDef? Buff = null,
    CatFoodBuffDef? SecondaryBuff = null,
    CatFoodBuffDef? TertiaryBuff = null)
{
    /// <summary>无持续 Buff 时可装入自动喂食器。</summary>
    public bool AllowAutoFeeder =>
        Buff is null && SecondaryBuff is null && TertiaryBuff is null;

    public IEnumerable<CatFoodBuffDef> AllBuffs()
    {
        if (Buff is not null) yield return Buff;
        if (SecondaryBuff is not null) yield return SecondaryBuff;
        if (TertiaryBuff is not null) yield return TertiaryBuff;
    }

    public string InstantEffectLabel =>
        $"饱腹+{Hunger} 精力+{Energy} 心情+{Happiness}";

    public string BuffEffectLabel
    {
        get
        {
            var parts = AllBuffs().Select(b => b.ShortLabel).ToList();
            return parts.Count == 0 ? "无持续 Buff" : string.Join(" · ", parts);
        }
    }
}

public static class CookBook
{
    public static readonly List<CookingRecipe> Recipes =
    [
        // ── Common ──
        new("common_sashimi", "生鱼片", FishRarity.Common, 1,
            30, 10, 10, 8,
            new CatFoodBuffDef(CatFoodBuffType.HungerRegenOverTime, 2, 30, 10)),
        new("common_grill", "溪烤小鲤", FishRarity.Common, 2,
            35, 15, 12, 10,
            new CatFoodBuffDef(CatFoodBuffType.HungerRegenOverTime, 3, 45, 10)),
        new("common_stew", "清炖溪虾", FishRarity.Common, 2,
            25, 20, 15, 9),

        // ── Rare ──
        new("rare_trout", "炭烤虹鳟", FishRarity.Rare, 3,
            60, 25, 25, 20,
            new CatFoodBuffDef(CatFoodBuffType.HungerRegenOverTime, 5, 60, 10)),
        new("rare_bass", "香煎石鲈", FishRarity.Rare, 4,
            55, 30, 28, 22,
            new CatFoodBuffDef(CatFoodBuffType.EnergyRegenOverTime, 4, 60, 10)),
        new("rare_mist", "迷雾烤鱼", FishRarity.Rare, 5,
            50, 35, 30, 24,
            new CatFoodBuffDef(CatFoodBuffType.HappinessRegenOverTime, 4, 60, 10)),

        // ── Epic ──
        new("epic_golden", "黄金鱼排", FishRarity.Epic, 6,
            120, 50, 60, 50,
            new CatFoodBuffDef(CatFoodBuffType.FishingEnergyDiscount, 0.90, 60, 10)),
        new("epic_feast", "雾海全席", FishRarity.Epic, 7,
            100, 55, 70, 55,
            new CatFoodBuffDef(CatFoodBuffType.HookBonus, 0.03, 30, 10)),
        new("epic_sashimi", "冰纹刺身", FishRarity.Epic, 8,
            110, 60, 55, 52,
            new CatFoodBuffDef(CatFoodBuffType.RareWeightBonus, 0.03, 45, 10)),

        // ── Legendary ──
        new("leg_feast", "传说鱼宴", FishRarity.Legendary, 10,
            250, 100, 150, 120,
            new CatFoodBuffDef(CatFoodBuffType.RareWeightBonus, 0.05, 60, 10)),
        new("leg_soup", "龙涎羹", FishRarity.Legendary, 10,
            200, 120, 130, 110,
            new CatFoodBuffDef(CatFoodBuffType.HookBonus, 0.05, 60, 10)),
        new("leg_aurora", "极光御膳", FishRarity.Legendary, 12,
            180, 90, 100, 130,
            new CatFoodBuffDef(CatFoodBuffType.HungerRegenOverTime, 8, 120, 10),
            new CatFoodBuffDef(CatFoodBuffType.EnergyRegenOverTime, 6, 120, 10),
            new CatFoodBuffDef(CatFoodBuffType.HappinessRegenOverTime, 5, 120, 10)),
    ];

    /// <summary>该稀有度默认食谱（解锁等级最低）。</summary>
    public static CookingRecipe DefaultFor(FishRarity rarity) =>
        Recipes.Where(r => r.Input == rarity).MinBy(r => r.RequiredCookingLevel)!;

    [Obsolete("Use DefaultFor or RecipeById")]
    public static CookingRecipe RecipeFor(FishRarity rarity) => DefaultFor(rarity);

    public static IEnumerable<CookingRecipe> RecipesFor(FishRarity rarity) =>
        Recipes.Where(r => r.Input == rarity).OrderBy(r => r.RequiredCookingLevel);

    public static CookingRecipe? RecipeById(string recipeId) =>
        Recipes.FirstOrDefault(r => r.RecipeId == recipeId);

    public static CookingRecipe? RecipeByName(string foodName) =>
        Recipes.FirstOrDefault(r => r.FoodName == foodName);

    /// <summary>烹饪产物名 → Food（喂猫用）。</summary>
    public static Food? FoodByName(string name)
    {
        var r = RecipeByName(name);
        return r is null ? null : new Food(r.FoodName, r.Hunger, r.Energy, r.Happiness);
    }

    public static bool IsCookedFood(string name) => RecipeByName(name) is not null;

    public static int ProcessingFee(FishRarity rarity) => EconomySinks.CookingProcessingFee(rarity);
}
