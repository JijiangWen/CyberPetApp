namespace CyberPetApp.Models;

public class CyberCat
{
    // name
    public string Name { get; set; } = "猫";

    // properties
    public int Health { get; set; } = 100;
    public int Happiness { get; set; } = 100;
    public int Energy { get; set; } = 100;
    public int Hunger { get; set; } = 100;
    public int Thirst { get; set; } = 100;

    //dynamic return the current mood of the cat
    public string GetMood()
    {
        return $"The cat is {Happiness} happy and {Energy} energy.";
    }

    // actions
    public void FeedFood(Food food)
    {
        Console.WriteLine($"{Name} 吃了 {food.Name}.");
        Hunger += food.HungerRestore;
        Energy += food.EnergyRestore;
        Happiness += food.HappinessRestore;

        if (Hunger > 100) Hunger = 100;
        if (Energy > 100) Energy = 100;
        if (Happiness > 100) Happiness = 100;
    }

    public void DrinkWater(int amount)
    {
        Thirst += amount;
        if (Thirst > 100) Thirst = 100;
    }

    // stroke
    public void Stroke()
    {
        Happiness += 5;
        Energy += 5;

        if (Happiness > 100) Happiness = 100;
        if (Energy > 100) Energy = 100;
    }

    //定时睡觉减少饥饿值
    public void Sleep()
    {
        Hunger -= 10;
        Energy += 10;

        if (Hunger < 0) Hunger = 0;
        if (Energy > 100) Energy = 100;
    }

    public void Tick()
    {
        Hunger --;
        Energy --;
        Happiness --;
        Thirst --;

        if (Hunger < 0) Hunger = 0;
        if (Energy < 0) Energy = 0;
        if (Happiness < 0) Happiness = 0;
        if (Thirst < 0) Thirst = 0;
    }



}
