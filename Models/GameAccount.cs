namespace CyberPetApp.Models;

public class GameAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public Guid PlayerId { get; set; }
}
