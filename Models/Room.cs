namespace CyberPetApp.Models;

public class Room
{
    public string Name { get; set; }
    public bool IsUnlocked { get; set; } = false;
    public int UnlockPrice { get; set; }

    public List<Furniture> Furniture { get; set; } = [];

    public Room(string name,bool isUnlocked, int unlockPrice = 0)
    {
        Name = name;
        IsUnlocked = isUnlocked;
        UnlockPrice = unlockPrice;
    }
}
