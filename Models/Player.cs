namespace CyberPetApp.Models;

public class Player
{
    public int Money { get; set; } = 100;

    // palyer backpack (stackable shop items)
    public Dictionary<string, int> Backpack { get; set; } = [];

    // caught fish — each entry keeps weight / rarity / price
    public List<Fish> FishBackpack { get; set; } = [];

    public List<ShopItem> Items { get; set; } = [];

    public bool IsWorking { get; set; } = false;

    public void BuyItem(ShopItem item)
    {
        if (Money < item.Price)
        {
            Console.WriteLine("金币不足！");
            return;
        }

        Money -= item.Price;

        // 3. 核心：把商品里包含的具体 Item 丢进背包
        string name = item.Food.Name;
        if (Backpack.TryGetValue(name, out int value))
            Backpack[name] = ++value; // 数量加 1
        else
            Backpack[name] = 1; // 第一次获得
    }
}
