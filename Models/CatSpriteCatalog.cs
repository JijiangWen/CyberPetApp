namespace CyberPetApp.Models;

/// <summary>猫咪状态 → wwwroot/assets/cat 透明 PNG 路径。</summary>
public static class CatSpriteCatalog
{
    private static readonly Dictionary<string, string> SlugByState =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["happy"] = "happy",
            ["sleep"] = "sleep",
            ["hungry"] = "hungry",
            ["fishing"] = "fishing",
            ["content"] = "content",
            ["idle"] = "content",
            ["wink"] = "wink",
        };

    public static string ImagePath(string? state)
    {
        var slug = SlugByState.TryGetValue(state ?? "", out var file) ? file : "content";
        return $"/assets/cat/{slug}.png?v=5";
    }
}
