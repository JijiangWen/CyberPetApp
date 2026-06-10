namespace CyberPetApp.Models;

public class FishingSpot
{
    public string Name { get; set; }
    public int RequiredLevel { get; set; }
    public int FishingTime { get; set; }
    /// <summary>钓点售价系数（高级钓点略高于静溪，配合更短周期拉高分钟收益）。</summary>
    public double PriceMultiplier { get; set; } = 1.0;

    /// <summary>主水层：拟饵深度匹配钓点或鱼种时抓口加成。</summary>
    public WaterDepth PrimaryDepth { get; set; } = WaterDepth.Middle;

    /// <summary>钓点背景描述（UI / 图鉴）。</summary>
    public string Description { get; set; } = "";

    public List<FishTemplate> FishTable { get; set; } = [];

    public Dictionary<FishRarity, int> FishRarityTable { get; set; } = new()
    {
        {FishRarity.Common, 70},
        {FishRarity.Rare, 20},
        {FishRarity.Epic, 8},
        {FishRarity.Legendary, 2}
    };

    private readonly Random _random = new();

    public FishingSpot(string name) => Name = name;

    public Fish FishRoll() => RollCatch().Fish;

    /// <summary>
    /// 抽一条鱼并返回其模板（状态机需要模板上的精明度/爆发力做判定）。
    /// rarityBonus：拟饵品质加成（0~1+），提高稀有档位权重与巨物概率。
    /// </summary>
    public (Fish Fish, FishTemplate Template) RollCatch(double rarityBonus = 0) =>
        RollCatch(new FishingRollContext(rarityBonus));

    /// <summary>装备匹配特殊饵时，本轮直接命中指定鱼的概率（QA 体感调整：15%→22%）。</summary>
    public const double TargetFishDirectRollChance = 0.22;

    public (Fish Fish, FishTemplate Template) RollCatch(FishingRollContext ctx)
    {
        if (FishTable.Count == 0) throw new Exception("鱼塘里没有鱼！");

        bool targetLureActive = ctx.ActiveTargetLureRecipeId is not null
            && AlchemyRecipes.TargetLureSpot(ctx.ActiveTargetLureRecipeId) == Name;

        FishTemplate baseFish;
        if (!targetLureActive && ctx.LureGearTier >= 8 && ctx.LureMythicBonus > 0)
        {
            var mythPool = FishTable.Where(f => f.TargetLureRecipeId is not null).ToList();
            if (mythPool.Count > 0 && _random.NextDouble() < ctx.LureMythicBonus * 2.5)
                baseFish = mythPool[_random.Next(mythPool.Count)];
            else
                baseFish = PickFishByRarity(ctx.RarityBonus, null, false);
        }
        else if (targetLureActive)
        {
            var targetTpl = FishTable.FirstOrDefault(f =>
                f.TargetLureRecipeId == ctx.ActiveTargetLureRecipeId);
            if (targetTpl is not null && _random.NextDouble() < TargetFishDirectRollChance)
                baseFish = targetTpl;
            else
                baseFish = PickFishByRarity(ctx.RarityBonus, ctx.ActiveTargetLureRecipeId, true);
        }
        else
            baseFish = PickFishByRarity(ctx.RarityBonus, null, false);

        // 2. 体型：大部分是小鱼，偶尔爆出巨物甚至超规格鱼
        double sizeBonus = ctx.RarityBonus + (targetLureActive && baseFish.TargetLureRecipeId == ctx.ActiveTargetLureRecipeId ? 0.5 : 0);
        double sizePercentage = RollSizePercentage(sizeBonus);
        bool isOversized = sizePercentage > 1.0; // 超出图鉴记录的怪物级
        double actualWeight = Math.Round(
            baseFish.MinWeight + sizePercentage * (baseFish.MaxWeight - baseFish.MinWeight), 1);

        // 3. 体型稀有度最多把鱼种档位上抬一档，避免 max(档位,体型) 直接钉死传说
        FishRarity sizeRarity;
        if (isOversized || sizePercentage >= 0.98) sizeRarity = FishRarity.Legendary;
        else if (sizePercentage >= 0.90)           sizeRarity = FishRarity.Epic;
        else if (sizePercentage >= 0.70)           sizeRarity = FishRarity.Rare;
        else                                       sizeRarity = FishRarity.Common;
        FishRarity finalRarity = baseFish.Rarity;
        if ((int)sizeRarity > (int)baseFish.Rarity)
            finalRarity = (FishRarity)Math.Min((int)baseFish.Rarity + 1, (int)FishRarity.Legendary);

        // 4. 价格：重量 × 稀有度倍率 × 体型加成（大体型超线性溢价）
        double rarityMultiplier = finalRarity switch
        {
            FishRarity.Legendary => 5.0,
            FishRarity.Epic => 2.5,
            FishRarity.Rare => 1.5,
            _ => 1.0
        };
        if (baseFish.TargetLureRecipeId is not null)
            rarityMultiplier *= 1.4; // 神话鱼售价比同点传说高约 40%
        double sizePriceBonus = 1.0 + sizePercentage * sizePercentage; // 体型越满，溢价越夸张
        int sellPrice = Math.Max(1, (int)(actualWeight * 10 * rarityMultiplier * sizePriceBonus * PriceMultiplier));

        string displayName = baseFish.TargetLureRecipeId is not null
            ? baseFish.Name
            : isOversized ? $"超规格·{baseFish.Name}" : baseFish.Name;
        var rolledFish = new Fish(
            displayName,
            baseFish.HungerRestore,
            baseFish.EnergyRestore,
            baseFish.HappinessRestore,
            sellPrice,
            finalRarity,
            actualWeight)
        {
            SizePercentage = Math.Round(sizePercentage * 100, 1)
        };
        return (rolledFish, baseFish);
    }

    /// <summary>
    /// 先按 FishRarityTable 抽稀有度档位，再按 SpawnWeight 在同档鱼种里加权抽取。
    /// 拟饵品质加成：非 Common 档位权重 × (1 + rarityBonus)，即整体向高稀有度倾斜。
    /// </summary>
    private IEnumerable<FishTemplate> EligibleFish(string? targetLureId, bool targetLureActive)
    {
        foreach (var fish in FishTable)
        {
            if (fish.TargetLureRecipeId is null)
            {
                yield return fish;
                continue;
            }
            if (targetLureActive && fish.TargetLureRecipeId == targetLureId)
                yield return fish;
        }
    }

    private FishTemplate PickFishByRarity(double rarityBonus, string? targetLureId, bool targetLureActive)
    {
        var pool = EligibleFish(targetLureId, targetLureActive).ToList();
        if (pool.Count == 0) pool = FishTable.Where(f => f.TargetLureRecipeId is null).ToList();

        var weights = FishRarityTable.ToDictionary(
            kv => kv.Key,
            kv => kv.Key == FishRarity.Common ? (double)kv.Value : kv.Value * (1 + rarityBonus));

        double totalWeight = weights.Values.Sum();
        double roll = _random.NextDouble() * totalWeight;
        FishRarity rolledRarity = FishRarity.Common;
        foreach (var (rarity, weight) in weights)
        {
            if (roll < weight) { rolledRarity = rarity; break; }
            roll -= weight;
        }

        // 从抽中的档位往低档找，保证一定能挑出鱼
        for (int r = (int)rolledRarity; r >= 0; r--)
        {
            var candidates = pool.Where(f => f.Rarity == (FishRarity)r).ToList();
            if (candidates.Count > 0)
                return PickWeighted(candidates, targetLureId, targetLureActive);
        }
        return PickWeighted(pool, targetLureId, targetLureActive);
    }

    private FishTemplate PickWeighted(IReadOnlyList<FishTemplate> candidates, string? targetLureId, bool targetLureActive)
    {
        int Weight(FishTemplate f)
        {
            int w = Math.Max(1, f.SpawnWeight);
            if (targetLureActive && f.TargetLureRecipeId == targetLureId)
                w *= 20;
            return w;
        }

        int total = candidates.Sum(Weight);
        int roll = _random.Next(total);
        foreach (var fish in candidates)
        {
            roll -= Weight(fish);
            if (roll < 0) return fish;
        }
        return candidates[^1];
    }

    /// <summary>
    /// 体型分布（0~1，极小概率 >1）：
    /// - ~94%：偏小体型（幂函数压低，满体型自然稀有）
    /// - ~5%：咬钩手感异常沉 → 必出 85% 以上的大鱼
    /// - ~1%：超规格巨物，体重突破图鉴上限（100%~130%），必为传说
    /// 拟饵品质加成：巨物/超规格触发概率 × (1 + rarityBonus)。
    /// </summary>
    private double RollSizePercentage(double rarityBonus = 0)
    {
        double luck = _random.NextDouble();
        double oversizedChance = 0.01 * (1 + rarityBonus);
        double giantChance = 0.06 * (1 + rarityBonus);
        if (luck < oversizedChance) return 1.0 + _random.NextDouble() * 0.3;  // 超规格
        if (luck < giantChance) return 0.85 + _random.NextDouble() * 0.15;    // 巨物
        return Math.Pow(_random.NextDouble(), 2.2);                           // 日常：小鱼为主
    }
}
