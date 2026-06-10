namespace CyberPetApp.Models;

public class Room
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlayerHouseId { get; set; }
    public string Name { get; set; }
    public bool IsUnlocked { get; set; }
    public int UnlockPrice { get; set; }

    public List<Furniture> Furniture { get; set; } = [];

    public Room() => Name = "";

    public Room(string name, bool isUnlocked, int unlockPrice = 0)
    {
        Name = name;
        IsUnlocked = isUnlocked;
        UnlockPrice = unlockPrice;
    }
}
