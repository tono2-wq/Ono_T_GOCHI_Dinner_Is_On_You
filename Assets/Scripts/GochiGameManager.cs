using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GochiGameManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject firstOrderText;
    public GameObject menuPanel;
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;

    [Header("Guess Popup")]
    public GameObject guessPopup;
    public TMP_InputField guessInputField;

    [Header("Food Items")]
    public FoodItem[] allFoodItems;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip youGuessedPriceClip;

    [Header("Last Order Phase")]
    public GameObject stopButton;  // Button to end selection
    private bool isLastOrderPhase = false;
    private List<FoodItem> lastOrderItems = new List<FoodItem>();

    private FoodItem selectedFood;

    private List<FoodItem> playerOrders = new List<FoodItem>();
    private List<FoodItem> yabeOrders = new List<FoodItem>();
    private List<FoodItem> okamuraOrders = new List<FoodItem>();

    private bool canSelectFood = false;

    void Start()
    {
        resultPanel.SetActive(false);
        guessPopup.SetActive(false);
        menuPanel.SetActive(false);
        firstOrderText.SetActive(false);
    }

    public void StartFirstOrderSequence()
    {
        Debug.Log("StartFirstOrderSequence CALLED on: " + gameObject.name);
        StartCoroutine(GameStartSequence());
    }

    IEnumerator GameStartSequence()
    {
        Debug.Log("GameStartSequence START");

        canSelectFood = false;

        if (firstOrderText != null)
        {
            firstOrderText.SetActive(true);
            Debug.Log("FirstOrderText should now be visible!");
            firstOrderText.transform.SetAsLastSibling();
        }

        yield return new WaitForSeconds(2.5f);

        firstOrderText.SetActive(false);
        Debug.Log("FirstOrderText turned off, menu should show next");

        if (menuPanel != null)
        {
            menuPanel.SetActive(true);
            Debug.Log("MenuPanel SetActive(true) called!");
        }

        canSelectFood = true;
    }

    void GenerateAllPrices()
    {
        foreach (FoodItem food in allFoodItems)
        {
            food.GeneratePrice();
            Debug.Log(food.foodName + " real price: " + food.realPrice);
        }
    }

    public bool CanSelectFood()
    {
        return canSelectFood;
    }

    public void OpenGuessPopup(FoodItem food)
    {
        if (!canSelectFood)
            return;

        selectedFood = food;

        if (isLastOrderPhase)
        {
            // In last order, store item but do not stop game immediately
            lastOrderItems.Add(food);
        }

        guessPopup.SetActive(true);
    }

    public void ConfirmGuess()
    {
        if (selectedFood == null) return;

        int guessValue;
        if (!int.TryParse(guessInputField.text, out guessValue))
        {
            Debug.Log("Invalid input");
            return;
        }

        selectedFood.SetPlayerGuess(guessValue);
        PlayYouGuessedSound();

        guessPopup.SetActive(false);
        guessInputField.text = "";

        if (!isLastOrderPhase)
        {
            playerOrders.Add(selectedFood);
            AISelectOrders();
            ShowResults();
        }
        else
        {
            // In last order phase, we wait until player presses Stop
            Debug.Log(selectedFood.foodName + " added to last order phase.");
        }
    }
    public void StopLastOrderPhase()
    {
        canSelectFood = false;
        stopButton.SetActive(false);
        Debug.Log("Player stopped selecting items. Now guessing prices for last order.");

        StartCoroutine(GuessLastOrderPrices());
    }
    IEnumerator GuessLastOrderPrices()
    {
        foreach (FoodItem food in lastOrderItems)
        {
            selectedFood = food;
            guessPopup.SetActive(true);

            bool guessed = false;
            guessInputField.onEndEdit.RemoveAllListeners();
            guessInputField.onEndEdit.AddListener((value) => { guessed = true; });

            while (!guessed)
                yield return null;

            food.SetPlayerGuess(int.Parse(guessInputField.text));
            guessInputField.text = "";
            guessPopup.SetActive(false);
        }

        playerOrders.AddRange(lastOrderItems);
        AISelectOrders();
        ShowResults();
    }

    void PlayYouGuessedSound()
    {
        if (audioSource != null && youGuessedPriceClip != null)
        {
            audioSource.PlayOneShot(youGuessedPriceClip);
        }
        else
        {
            Debug.LogWarning("AudioSource or YouGuessedPriceClip not assigned.");
        }
    }

    void AISelectOrders()
    {
        yabeOrders.Add(allFoodItems[Random.Range(0, allFoodItems.Length)]);
        okamuraOrders.Add(allFoodItems[Random.Range(0, allFoodItems.Length)]);
    }

    void ShowResults()
    {
        menuPanel.SetActive(false);
        resultPanel.SetActive(true);

        resultText.text = "";

        resultText.text += "PLAYER ORDER:\n";
        foreach (FoodItem food in playerOrders)
        {
            resultText.text += food.foodName +
                "\nGuess: " + food.playerGuess +
                "\nReal: " + food.realPrice +
                "\n\n";
        }

        resultText.text += "\nYABE ORDER:\n";
        foreach (FoodItem food in yabeOrders)
        {
            resultText.text += food.foodName + "\n";
        }

        resultText.text += "\nOKAMURA ORDER:\n";
        foreach (FoodItem food in okamuraOrders)
        {
            resultText.text += food.foodName + "\n";
        }
    }
}