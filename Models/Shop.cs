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
        ShopItems.Add(new ShopItem(new Food("纯净水", 0, 0, 0), 2, "water", "装入饮水器或手动饮用"));
        ShopItems.Add(new ShopItem(new Food("干瘪的猫粮", 10, 1, 0), 5, "food", "廉价的基础饱腹"));
        ShopItems.Add(new ShopItem(new Food("混合肉干猫粮", 20, 5, 5), 15, "food", "性价比口粮"));
        ShopItems.Add(new ShopItem(new Food("鲜肉营养罐头", 35, 10, 10), 30, "food", "高饱腹、恢复精力"));
        ShopItems.Add(new ShopItem(new Food("浓缩猫薄荷", 0, 0, 60), 50, "treat", "大幅提升快乐（+60）"));
        ShopItems.Add(new ShopItem(new Food("赛博能量液", 0, 50, -5), 45, "drink", "快速恢复精力（+50）"));

        // 炼金锻造素材（craft.gear）
        ShopItems.Add(new ShopItem(new Food(AlchemyMaterials.NylonFilament, 0, 0, 0), 15, "craft", "炼制鱼线 · 尼龙原丝"));
        ShopItems.Add(new ShopItem(new Food(AlchemyMaterials.Resin, 0, 0, 0), 80, "craft", "炼制竿/轮 · 环氧树脂"));
        ShopItems.Add(new ShopItem(new Food(AlchemyMaterials.Bearing, 0, 0, 0), 200, "craft", "炼制渔轮 · 精密轴承"));
        ShopItems.Add(new ShopItem(new Food(AlchemyMaterials.AlloyFrame, 0, 0, 0), 350, "craft", "炼制渔轮 · 钛合金框"));
    }
}
