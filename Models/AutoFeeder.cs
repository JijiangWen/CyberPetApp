using System.Collections.Generic;

namespace CyberPetApp.Models;

public class AutoFeeder
{
    public List<Food> Foods { get; set; } = [];
    public int FoodCount { get; set; } = 0;
    public int MaxFoodCount { get; set; } = 10;

    public bool AddFoodFromBackpack(Player player, Food food)
    {
        // 1. 检查喂食器满没满
        if (Foods.Count >= MaxFoodCount) return false;

        if (player.Backpack.TryGetValue(food.Name, out int value) && value > 0)
        {
            Foods.Add(food);
            FoodCount++;
            player.Backpack[food.Name] = --value;
            if (player.Backpack[food.Name] == 0)
            {
                Console.WriteLine($"成功从背包中移除了 {food.Name}！");
                player.Backpack.Remove(food.Name);
            }
            return true;
        }
        return false;
    }

    public Food? RemoveLastFood()
    {
        if (FoodCount <= 0) return null;

        // 1. 找到最后一个食物的索引
        int lastIndex = FoodCount - 1;
        Food lastFood = Foods[lastIndex];

        // 2. 精准删除最后一个
        Foods.RemoveAt(lastIndex);
        FoodCount--;

        return lastFood; // 把取出的食物返回，方便以后在背包里恢复
    }

    public void CheckAndFeed(CyberCat cat)
    {
        if (FoodCount > 0 && cat.Hunger < 50)
        {
            Food foodToEat = Foods[0];
            cat.FeedFood(foodToEat);

            Foods.RemoveAt(0);
            FoodCount--;
        }
    }
}
