namespace CyberPetApp.Models;

public class ShopItem
{
    public Food Food { get; set; }
    public int Price { get; set; }
    public string Category { get; set; } = "food";
    public string Hint { get; set; } = "";

    public ShopItem(Food food, int price, string category = "food", string hint = "")
    {
        Food = food;
        Price = price;
        Category = category;
        Hint = hint;
    }
}

public class Shop
{
    public List<ShopItem> ShopItems { get; set; } = [];

    public Shop()
    {
        ShopItems.Add(new ShopItem(new Food("纯净水", 0, 0, 0), 5, "water", "装入饮水器或手动饮用"));
        ShopItems.Add(new ShopItem(new Food("普通猫粮", 15, 2, 2), 10, "food", "基础饱腹"));
        ShopItems.Add(new ShopItem(new Food("高级猫粮", 25, 5, 3), 15, "food", "喂食器/手动喂"));
        ShopItems.Add(new ShopItem(new Food("金枪鱼罐头", 35, 15, 5), 20, "food", "高饱腹高精力"));
        ShopItems.Add(new ShopItem(new Food("猫薄荷包", 0, 0, 50), 30, "treat", "手动快乐+50 或装入喂食器"));
        ShopItems.Add(new ShopItem(new Food("能量饮料", 0, 40, 0), 25, "drink", "手动精力+40"));

        // 炼金锻造素材（craft.gear）
        ShopItems.Add(new ShopItem(new Food(AlchemyMaterials.NylonFilament, 0, 0, 0), 15, "craft", "炼制鱼线 · 尼龙原丝"));
        ShopItems.Add(new ShopItem(new Food(AlchemyMaterials.Resin, 0, 0, 0), 80, "craft", "炼制竿/轮 · 环氧树脂"));
        ShopItems.Add(new ShopItem(new Food(AlchemyMaterials.Bearing, 0, 0, 0), 200, "craft", "炼制渔轮 · 精密轴承"));
        ShopItems.Add(new ShopItem(new Food(AlchemyMaterials.AlloyFrame, 0, 0, 0), 350, "craft", "炼制渔轮 · 钛合金框"));
    }
}
