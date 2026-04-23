using UnityEngine;

public class FoodItem : MonoBehaviour
{
    [Header("Food Info")]
    public string foodName;
    public string category;
    // Appetizer / Main / Dessert

    [HideInInspector]
    public int realPrice;

    [HideInInspector]
    public int playerGuess;

    private GochiGameManager gameManager;

    void Start()
    {
        gameManager = FindObjectOfType<GochiGameManager>();
    }

    // Called at game start
    public void GeneratePrice()
    {
        switch (category)
        {
            case "Appetizer":
                realPrice = Random.Range(8, 36) * 100;
                break;

            case "Main":
                realPrice = Random.Range(25, 101) * 100;
                break;

            case "Dessert":
                realPrice = Random.Range(6, 26) * 100;
                break;

            default:
                Debug.LogWarning(foodName + " has invalid category!");
                realPrice = 1000;
                break;
        }

        Debug.Log(foodName + " real price: " + realPrice);
    }
    // Called when player clicks food
    public void OnFoodSelected()
    {
        // Prevent clicking if game not ready
        if (gameManager == null)
            return;

        if (!gameManager.CanSelectFood())
            return;

        gameManager.OpenGuessPopup(this);
    }

    // Called after player enters guess
    public void SetPlayerGuess(int guessInput)
    {
        // Force increments of 100
        playerGuess = Mathf.RoundToInt(guessInput / 100f) * 100;

        Debug.Log(foodName + " guessed: " + playerGuess);
    }
}