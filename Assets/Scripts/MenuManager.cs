using UnityEngine;
using System.Collections.Generic;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;

    public List<MenuItem> menuItems = new List<MenuItem>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;

            if (menuItems.Count == 0)
                GenerateMenu();
        }
        else
        {
            Destroy(gameObject);
        }
    }



    void GenerateMenu()
    {
        AddItem("Sushi Roll", FoodCategory.Appetizer, 1000, 3000);
        AddItem("Gyoza Dumpling", FoodCategory.Appetizer, 1000, 3000);
        AddItem("Tofu", FoodCategory.Appetizer, 1000, 3000);

        AddItem("Chicken Curry", FoodCategory.Main, 3000, 10000);
        AddItem("Vegetarian Ramen", FoodCategory.Main, 3000, 10000);
        AddItem("Miso Salmon", FoodCategory.Main, 3000, 10000);
        AddItem("Beef Steak Garlic", FoodCategory.Main, 3000, 10000);
        AddItem("Miso Cod", FoodCategory.Main, 3000, 10000);

        AddItem("Cheesecake", FoodCategory.Dessert, 600, 2500);
        AddItem("Dango", FoodCategory.Dessert, 600, 2500);
        AddItem("Pudding", FoodCategory.Dessert, 600, 2500);
    }

    void AddItem(string name, FoodCategory category, int min, int max)
    {
        MenuItem item = ScriptableObject.CreateInstance<MenuItem>();

        item.itemName = name;
        item.category = category;
        item.realPrice = Random.Range(min / 100, max / 100 + 1) * 100;

        menuItems.Add(item);
    }

    public MenuItem GetRandomItem()
    {
        if (menuItems.Count == 0)
        {
            Debug.LogError("Menu is empty!");
            return null;
        }

        return menuItems[Random.Range(0, menuItems.Count)];
    }
}