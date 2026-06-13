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

    public static string ImagePath(string? skinId, string? state)
    {
        var slug = SlugByState.TryGetValue(state ?? "", out var file) ? file : "content";
        
        if (string.IsNullOrWhiteSpace(skinId) || skinId.Equals("default", StringComparison.OrdinalIgnoreCase))
        {
            return $"/assets/cat/{slug}.png?v=5";
        }
        
        return $"/assets/cat_skins/{skinId}/{slug}.png?v=5";
    }
}
