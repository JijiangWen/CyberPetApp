namespace CyberPetApp.Models;

public class WorkingPlace
{
    public string Name { get; set; } = "工地";

    public void ToggleWork(Player player)
    {
        player.IsWorking = !player.IsWorking;
    }
}
