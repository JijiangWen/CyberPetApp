using System.Collections.Generic;

namespace CyberPetApp.Models;

public class AutoFeeder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PlayerId { get; set; }
    public List<Food> Foods { get; set; } = [];
    public int FoodCount { get; set; } = 0;
    public int MaxFoodCount { get; set; } = 10;

    public bool AddFoodFromBackpack(Player player, Food food)
    {
        if (Foods.Count >= MaxFoodCount) return false;

        if (player.Backpack.TryGetValue(food.Name, out int value) && value > 0)
        {
            Foods.Add(food);
            FoodCount++;
            player.Backpack[food.Name] = --value;
            if (player.Backpack[food.Name] == 0)
                player.Backpack.Remove(food.Name);
            return true;
        }
        return false;
    }

    public Food? RemoveLastFood()
    {
        if (FoodCount <= 0) return null;

        int lastIndex = FoodCount - 1;
        Food lastFood = Foods[lastIndex];
        Foods.RemoveAt(lastIndex);
        FoodCount--;
        return lastFood;
    }

    public bool AddFood(Food food)
    {
        if (Foods.Count >= MaxFoodCount) return false;
        Foods.Add(food);
        FoodCount++;
        return true;
    }

    /// <summary>饥饿&lt;600 或精力&lt;400 时自动喂食；分别优先取 HungerRestore / EnergyRestore 最高的食物。</summary>
    public void CheckAndFeed(CyberCat cat)
    {
        if (FoodCount <= 0) return;

        bool needHunger = cat.Hunger < 600;
        bool needEnergy = cat.Energy < 400;
        if (!needHunger && !needEnergy) return;

        int bestIdx = 0;
        for (int i = 1; i < Foods.Count; i++)
        {
            if (needHunger && !needEnergy)
            {
                if (Foods[i].HungerRestore > Foods[bestIdx].HungerRestore)
                    bestIdx = i;
            }
            else if (needEnergy && !needHunger)
            {
                if (Foods[i].EnergyRestore > Foods[bestIdx].EnergyRestore)
                    bestIdx = i;
            }
            else
            {
                // 饥饿与精力均低：取 HungerRestore + EnergyRestore 综合最高
                int scoreI = Foods[i].HungerRestore + Foods[i].EnergyRestore;
                int scoreBest = Foods[bestIdx].HungerRestore + Foods[bestIdx].EnergyRestore;
                if (scoreI > scoreBest)
                    bestIdx = i;
            }
        }

        Food foodToEat = Foods[bestIdx];
        cat.FeedFood(foodToEat);
        Foods.RemoveAt(bestIdx);
        FoodCount--;
    }
}
