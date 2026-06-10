namespace CyberPetApp.Models;

public class Furniture
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RoomId { get; set; }
    public string FurnitureId { get; set; }
    public string Name { get; set; }
    public int Price { get; set; }
    public bool IsUnlocked { get; set; }
    /// <summary>家具升级等级（0=未升级，最高 1 级，加成 +5%）。</summary>
    public int UpgradeLevel { get; set; }
    public string Description { get; set; }

    public Furniture() => FurnitureId = Name = Description = "";

    public Furniture(string furnitureId, string name, int price, string description)
    {
        FurnitureId = furnitureId;
        Name = name;
        Price = price;
        IsUnlocked = false;
        Description = description;
    }
}

public class PlayerHouse
{
    /// <summary>房间字典键与俯瞰图布局常量（须与 DB Room.Name 一致）。</summary>
    public static class RoomKeys
    {
        public const string LivingRoom = "客厅";
        public const string Kitchen = "厨房";
        public const string Bedroom = "卧室";
        public const string Bathroom = "卫生间";
        public const string Garden = "花园";

        public static readonly string[][] FloorRows =
        [
            [LivingRoom, Kitchen],
            [Bedroom, Bathroom]
        ];
    }

    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlayerId { get; set; }
    public int House_Level { get; set; } = 1;
    public Dictionary<string, Room> Rooms { get; set; } = [];

    public PlayerHouse()
    {
        var LivingRoom = new Room(RoomKeys.LivingRoom, true);
        LivingRoom.Furniture.Add(new Furniture("Sofa", "赛博懒人沙发", 150, "给小猫咪和主人躺尸专用"));
        LivingRoom.Furniture.Add(new Furniture("TV", "老旧大头电视机", 300, "亮起来挺有氛围"));
        LivingRoom.Furniture.Add(new Furniture("CatToy", "电动逗猫棒", 250, "自动旋转激光点，快乐+20 精力+5/3s"));
        LivingRoom.Furniture.Add(new Furniture("JoyPad", "赛博猫爬架", 320, "多层跳板，猫咪攀爬玩耍"));
        LivingRoom.Furniture.Add(new Furniture("WaterDispenser", "宠物饮水泉", 200, "解锁侧边栏自动饮水，口渴<600 补水"));
        LivingRoom.Furniture.Add(new Furniture("FishTank", "观赏鱼缸", 280, "鱼儿游动让猫开心，钓鱼阈值-30"));
        LivingRoom.Furniture.Add(new Furniture("SunLamp", "日照灯", 220, "模拟日光，精力与快乐双恢复"));
        LivingRoom.Furniture.Add(new Furniture("AromaDiffuser", "香薰扩散器", 300, "舒缓香气，持续提升快乐"));
        LivingRoom.Furniture.Add(new Furniture("LuxuryTower", "豪华猫爬架", 800, "顶级娱乐设施，支撑 24h 挂机"));
        Rooms[RoomKeys.LivingRoom] = LivingRoom;

        var Kitchen = new Room(RoomKeys.Kitchen, false, 200);
        Kitchen.Furniture.Add(new Furniture("Fridge", "老旧冰箱", 200, "可以放食物"));
        Kitchen.Furniture.Add(new Furniture("Stove", "老旧烤箱", 300, "可以烤食物"));
        Kitchen.Furniture.Add(new Furniture("AutoFeederUnit", "智能喂食站", 400, "喂食器+3 槽，饥饿<600 自动取食"));
        Rooms[RoomKeys.Kitchen] = Kitchen;

        var Bedroom = new Room(RoomKeys.Bedroom, false, 300);
        Bedroom.Furniture.Add(new Furniture("Bed", "老旧床", 300, "可以睡觉"));
        Bedroom.Furniture.Add(new Furniture("CozyBed", "恒温猫窝", 350, "猫咪专属暖窝，精力<800 时恢复"));
        Rooms[RoomKeys.Bedroom] = Bedroom;

        var Bathroom = new Room(RoomKeys.Bathroom, false, 400);
        Bathroom.Furniture.Add(new Furniture("Toilet", "老旧马桶", 100, "可以上厕所"));
        Bathroom.Furniture.Add(new Furniture("Sink", "老旧洗手池", 200, "可以洗手"));
        Rooms[RoomKeys.Bathroom] = Bathroom;

        var Garden = new Room(RoomKeys.Garden, false, 500);
        Garden.Furniture.Add(new Furniture("Garden", "老旧花园", 300, "可以种花"));
        Rooms[RoomKeys.Garden] = Garden;
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
