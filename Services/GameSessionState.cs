using CyberPetApp.Models;

namespace CyberPetApp.Services;

/// <summary>
/// 每 Circuit 共享的游戏内存态引用（由 Home 填充，经 CascadingValue 下发给 Tab 子组件）。
/// P2-2：Tick 快照由 <see cref="GameTickOrchestrator"/> 写入，Tab 可读最新渲染态而无需父级整页 diff。
/// </summary>
public class GameSessionState
{
    public Player? Player { get; set; }
    public CyberCat Cat { get; set; } = new();
    public AutoFeeder Feeder { get; set; } = new();
    public AutoWaterer Waterer { get; set; } = new();
    public PlayerHouse PlayerHouse { get; set; } = new();
    public FishingManager FishingManager { get; set; } = new();
    public MilestoneBuffs MilestoneBuffs { get; set; } = MilestoneBuffs.Empty;
    public FishingLoadout Loadout { get; set; } = new();
    public List<FishCatchRecord> FishRecords { get; set; } = [];
    public List<SpotLicense> SpotLicenses { get; set; } = [];

    /// <summary>最新 tick 渲染快照（P1-2 / P2-2）。</summary>
    public TickRenderSnapshot TickRenderSnapshot { get; set; }

    /// <summary>父级条件刷新世代号，与 Home tickGeneration 同步。</summary>
    public int TickGeneration { get; set; }

    /// <summary>侧边栏猫咪体征快照（独立于 tickGeneration 通道）。</summary>
    public CatVitalsSnapshot CatVitalsSnapshot { get; set; }
}
