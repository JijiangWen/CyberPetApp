namespace CyberPetApp.Models;

public class BackpackItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlayerId { get; set; }
    public string ItemName { get; set; } = "";
    public int Quantity { get; set; }
}
