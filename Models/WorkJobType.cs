namespace CyberPetApp.Models;

/// <summary>打工工种：影响金币产出、摊位券速率与附加效果。</summary>
public enum WorkJobType
{
    /// <summary>工地：默认 +1g/tick，摊位券 150 tick/张。</summary>
    Construction,
    /// <summary>猫咖兼职：+0g，每 tick 给猫 +幸福，摊位券 180 tick/张。</summary>
    CatCafe,
    /// <summary>鱼市搬运：钓鱼 Lv3 解锁，+0g，摊位券 120 tick/张，市场报价更频繁。</summary>
    FishMarketPorter
}
