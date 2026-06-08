namespace CyberPetApp.Models;

public enum ItemType
{
    Food,
    Tool,
    Other
}

public class Item
{
    public ItemType Type { get; set; }
    public string Name { get; set; }
    public int Price { get; set; }
    public string Description { get; set; }
    public Item(ItemType type, string name, int price, string description)
    {
        Type = type;
        Name = name;
        Price = price;
        Description = description;
    }
}
