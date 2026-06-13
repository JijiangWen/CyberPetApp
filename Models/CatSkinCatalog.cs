namespace CyberPetApp.Models;

public static class CatSkinCatalog
{
    public static readonly List<CatSkin> Skins =
    [
        new CatSkin
        {
            Id = "default",
            Name = "橘猫",
            Description = "最经典的橘猫外观。十个橘猫九个胖。",
            Price = 0
        },
        new CatSkin
        {
            Id = "pili",
            Name = "雷霆霹雳猫",
            Description = "浑身环绕着闪电的赛博风猫咪。",
            Price = 5000
        },
        new CatSkin
        {
            Id = "void",
            Name = "虚空暗影猫",
            Description = "来自虚空的暗影使者，散发着幽暗的紫光。",
            Price = 8000
        },
        new CatSkin
        {
            Id = "sakura",
            Name = "樱花猫",
            Description = "粉粉嫩嫩的樱花限定外观。",
            Price = 3000
        },
        new CatSkin
        {
            Id = "gold",
            Name = "招财金猫",
            Description = "通体纯金打造，据说能带来好运。",
            Price = 15000
        }
    ];

    public static CatSkin? GetSkin(string id) =>
        Skins.FirstOrDefault(s => s.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
}
