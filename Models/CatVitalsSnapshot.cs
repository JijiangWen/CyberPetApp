namespace CyberPetApp.Models;

/// <summary>侧边栏猫咪状态快照，用于 ShouldRender 精细化比较。</summary>
public readonly record struct CatVitalsSnapshot(
    string Name,
    int CatLevel,
    int CatXp,
    int XpToNext,
    int Hunger,
    int Thirst,
    int Energy,
    int Happiness,
    string CatState,
    string CatMoodText,
    bool CanTreat,
    string? Message);
