namespace CyberPetApp.Models;


public class Furniture
{
    public string Id { get; set; }
    public string Name { get; set; }
    public int Price { get; set; }
    public bool IsUnlocked { get; set; } = false;
    public string Description { get; set; }
    public Furniture(string id, string name, int price, string description)
    {
        Id = id;
        Name = name;
        Price = price;
        IsUnlocked = false;
        Description = description;
    }
}

public class PlayerHouse
{
    public int House_Level { get; set; } = 1;
    public Dictionary<string, Room> Rooms { get; set; } = [];

    public PlayerHouse()
    {
        var LivingRoom = new Room("客厅", true);
        LivingRoom.Furniture.Add(new Furniture("Sofa", "赛博懒人沙发", 150, "给小猫咪和主人躺尸专用"));
        LivingRoom.Furniture.Add(new Furniture("TV", "老旧大头电视机", 300, "亮起来挺有氛围"));
        Rooms["客厅"] = LivingRoom;

        var Kitchen = new Room("厨房", false, 200);
        Kitchen.Furniture.Add(new Furniture("Fridge", "老旧冰箱", 200, "可以放食物"));
        Kitchen.Furniture.Add(new Furniture("Stove", "老旧烤箱", 300, "可以烤食物"));
        Rooms["厨房"] = Kitchen;

        var Bedroom = new Room("卧室", false, 300);
        Bedroom.Furniture.Add(new Furniture("Bed", "老旧床", 300, "可以睡觉"));
        Rooms["卧室"] = Bedroom;

        var Bathroom = new Room("卫生间", false, 400);
        Bathroom.Furniture.Add(new Furniture("Toilet", "老旧马桶", 100, "可以上厕所"));
        Bathroom.Furniture.Add(new Furniture("Sink", "老旧洗手池", 200, "可以洗手"));
        Rooms["卫生间"] = Bathroom;

        var Garden = new Room("花园", false, 500);
        Garden.Furniture.Add(new Furniture("Garden", "老旧花园", 300, "可以种花"));
        Rooms["花园"] = Garden;
    }

    public bool UnlockRoom(Player player, Room room)
    {
        if (room.IsUnlocked || player.Money < room.UnlockPrice)
            return false;

        player.Money -= room.UnlockPrice;
        room.IsUnlocked = true;
        return true;
    }

    public bool BuyFurniture(Player player, Furniture furniture)
    {
        if (furniture.IsUnlocked || player.Money < furniture.Price)
            return false;

        player.Money -= furniture.Price;
        furniture.IsUnlocked = true;
        return true;
    }
}
