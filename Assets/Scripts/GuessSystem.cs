using UnityEngine;
using System.Collections;
using System.Linq;

public class GuessSystem : MonoBehaviour
{
    public static GuessSystem instance;

    public int totalGuessesRequired;
    public int guessesSubmitted = 0;
    public bool isLastOrderGuessPhase = false;
    public bool firstOrderGuessesShown = false;
    public bool lastOrderMessageShown = false;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }


    public void InitializeGuessCount()
    {
        guessesSubmitted = 0;

        totalGuessesRequired = GameManager.instance.players
            .Where(p => p.isHuman)
            .Sum(p => p.orders.Count);

        Debug.Log("Total guesses required: " + totalGuessesRequired);
    }

    public void StartGuessPhase(bool lastOrder)
    {
        InitializeGuessCount();

        if (!lastOrder)
        {
            // Only first order message is shown here
            UIManager.instance.ShowMessageForSeconds(
                "First Order! Guess the prices of the items.",
                3f
            );
        }

        // ❌ Remove any last-order message logic from here
    }


    public void SubmitGuess(MenuItem item, int guess)
    {
        if (item.guessedPrice != 0) return; // already guessed

        item.guessedPrice = guess;
        guessesSubmitted++;


        // Only trigger first order guesses once
        if (!isLastOrderGuessPhase && !firstOrderGuessesShown && guessesSubmitted >= totalGuessesRequired)
        {
            firstOrderGuessesShown = true;
            GuessPopup.instance.StartCoroutine(GuessPopup.instance.ShowFirstOrderGuesses());
        }

        if (guessesSubmitted >= totalGuessesRequired)
        {
            StartCoroutine(AllGuessesComplete());
        }
    }

    public void ResetGuessSystem()
    {
        guessesSubmitted = 0;
        totalGuessesRequired = 0;
        firstOrderGuessesShown = false;
        isLastOrderGuessPhase = false;
        lastOrderMessageShown = false;
    }

    /// <summary>
    /// Resets all GuessSystem flags to false (unchecked in Inspector)
    /// </summary>
    public void ResetAllFlags()
    {
        isLastOrderGuessPhase = true;
        firstOrderGuessesShown = false;
        lastOrderMessageShown = false;

        Debug.Log("GuessSystem flags reset: LastOrderGuessPhase = true, FirstOrderGuessesShown = false, LastOrderMessageShown = false");
    }

    IEnumerator AllGuessesComplete()
    {
        yield return new WaitForSeconds(0.5f);
        yield break;
    }
}