using UnityEngine;

[CreateAssetMenu(fileName = "MenuItem", menuName = "Menu/Menu Item")]
public class MenuItem : ScriptableObject
{
    public string itemName;

    public FoodCategory category;

    public int realPrice;

    [HideInInspector]
    public int guessedPrice;

    public string itemID;

    private void OnValidate()
    {
        if (!string.IsNullOrEmpty(itemName))
        {
            itemID = itemName.Replace(" ", "_").ToLower();
        }
    }
    public enum FoodType
    {
        ChickenCurry,
        Cheesecake,
        Dango,
        GyozaDumpling,
        MisoSalnmon,
        MisoCod,
        Pudding,
        SteakGarlic,
        SushiRoll,
        Tofu,
        VegetarianRamen
    }
}


public enum FoodCategory
{
    Appetizer,
    Main,
    Dessert
}