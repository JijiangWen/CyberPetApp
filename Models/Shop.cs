namespace CyberPetApp.Models;

public class ShopItem
{
    public Food Food { get; set; }
    public int Price { get; set; }

    public ShopItem(Food food, int price)
    {
        Food = food;
        Price = price;
    }
}

public class Shop
{
    public List<ShopItem> ShopItems { get; set; } = [];

    public Shop()
    {
        ShopItems.Add(new ShopItem(new Food("普通猫粮", 15, 2, 2), 10));
        ShopItems.Add(new ShopItem(new Food("金枪鱼罐头", 35, 15, 5), 20));
    }
}
