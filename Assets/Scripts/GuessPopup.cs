using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class GuessPopup : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public static GuessPopup instance;

    [Header("UI Elements")]
    public TextMeshProUGUI priceText;
    public AudioClip youGuessedPriceSFX;

    [Header("Target UI (IMPORTANT)")]
    public RectTransform popupRect;

    private MenuItem currentItem;
    private int guessValue;
    private Coroutine slideCoroutine;

    [Header("Animation")]
    public float slideDuration = 0.3f;
    public int minGuess = 100;
    public int maxGuess = 10000;

    private float hiddenY = 600f;
    private float visibleY = 0f;

    private bool isProcessing = false;
    private bool firstOrderGuessesAlreadyShown = false;
    public bool firstOrderGuessesShown = false;

    private Coroutine holdRoutine;
    private bool isHoldingAdd = false;
    private bool isHoldingSubtract = false;

    [SerializeField] private float holdDelay = 0.5f; // delay before fast mode
    [SerializeField] private float repeatRate = 30f; // 30 times per second
    void Awake()
    {
        instance = this;

        if (popupRect != null)
            popupRect.anchoredPosition = new Vector2(popupRect.anchoredPosition.x, hiddenY);

        // ✅ DO NOT disable root object
        if (popupRect != null)
            popupRect.gameObject.SetActive(false);
    }

    public void OpenGuess(MenuItem item)
    {
        currentItem = item;
        guessValue = 5000;

        UpdateText();

        if (popupRect != null)
            popupRect.gameObject.SetActive(true);

        if (popupRect != null)
        {
            if (slideCoroutine != null)
                StopCoroutine(slideCoroutine);

            slideCoroutine = StartCoroutine(SlideRect(hiddenY, visibleY));
        }
    }

    public void Add100()
    {
        guessValue = Mathf.Min(guessValue + 100, maxGuess);
        UpdateText();
    }

    public void Subtract100()
    {
        guessValue = Mathf.Max(guessValue - 100, minGuess);
        UpdateText();
    }

    void UpdateText()
    {
        if (priceText != null)
            priceText.text = "¥" + guessValue;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (gameObject.name.Contains("Add")) // or assign manually
        {
            isHoldingAdd = true;
            Add100(); // immediate click
            holdRoutine = StartCoroutine(HoldRoutine(true));
        }
        else if (gameObject.name.Contains("Subtract"))
        {
            isHoldingSubtract = true;
            Subtract100();
            holdRoutine = StartCoroutine(HoldRoutine(false));
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHoldingAdd = false;
        isHoldingSubtract = false;

        if (holdRoutine != null)
            StopCoroutine(holdRoutine);
    }

    IEnumerator HoldRoutine(bool isAdd)
    {
        // Wait before fast mode
        yield return new WaitForSeconds(holdDelay);

        float interval = 1f / repeatRate;

        while ((isAdd && isHoldingAdd) || (!isAdd && isHoldingSubtract))
        {
            if (isAdd)
                Add100();
            else
                Subtract100();

            yield return new WaitForSeconds(interval);
        }
    }
    public void Confirm()
    {
        if (currentItem == null || isProcessing) return;

        isProcessing = true;

        // 🔥 Force store the guess directly into the item
        currentItem.guessedPrice = guessValue;

        GuessSystem.instance.SubmitGuess(currentItem, guessValue);

        UIManager.instance.playerSubmitted = true;

        StartCoroutine(FullFlowRoutine());
    }

    public IEnumerator ShowLastOrderMessage()
    {
        // Mark flags in GuessSystem
        GuessSystem.instance.firstOrderGuessesShown = false;   // first order guesses done
        GuessSystem.instance.isLastOrderGuessPhase = false;   // last order phase starts

        // Show the message only once
        if (!GuessSystem.instance.lastOrderMessageShown)
        {
            GuessSystem.instance.lastOrderMessageShown = true;

            UIManager.instance.ShowMessageForSeconds(
                "Last Order! Choose items until you hit the target!",
                3f
            );

            // Wait for 3 seconds so the player sees the message before next guesses
            yield return new WaitForSeconds(1f);
        }
    }
    private IEnumerator FullFlowRoutine()
    {
        UIManager.instance.isShowingSequence = true;

        // Slide out UI
        if (popupRect != null)
        {
            if (slideCoroutine != null)
                StopCoroutine(slideCoroutine);

            yield return StartCoroutine(SlideRect(popupRect.anchoredPosition.y, hiddenY));

            popupRect.gameObject.SetActive(false);
        }

        // Only show first order guesses sequence if it's not last order
        if (!GuessSystem.instance.isLastOrderGuessPhase)
        {
            int humanCount = GameManager.instance.players.FindAll(p => p.isHuman).Count;

            if (humanCount == 1)
            {
                yield return StartCoroutine(ShowFirstOrderGuesses());
                yield return StartCoroutine(ShowLastOrderMessage());
            }
        }

        UIManager.instance.isShowingSequence = false;

        // Change state if it's first order
        if (!GuessSystem.instance.isLastOrderGuessPhase)
        {
            int humanCount = GameManager.instance.players.FindAll(p => p.isHuman).Count;

            if (humanCount > 1)
            {

            }
            else
            {
                yield return new WaitForSeconds(1f);
                GameManager.instance.ChangeState(GameState.LastOrder);
            }
        }

        isProcessing = false;
    }
    public IEnumerator RunMultiplayerFirstOrderGuessSequence()
    {
        List<PlayerData> players = GameManager.instance.players;

        for (int i = 0; i < players.Count; i++)
        {
            PlayerData p = players[i];
            if (p == null || !p.isHuman)
                continue;

            MenuItem item = (p.orders != null && p.orders.Count > 0) ? p.orders[0] : null;
            if (item == null)
                continue;

            // 🔥 Reset before waiting
            UIManager.instance.playerSubmitted = false;

            // 1️⃣ Show turn message
            UIManager.instance.ShowMessageForSeconds(
                $"{p.playerName}'s turn to guess the price of the first order!",
                2f
            );
            yield return new WaitForSeconds(2f);

            // 2️⃣ Open guess UI
            OpenGuess(item);

            // 3️⃣ Wait for player input
            yield return new WaitUntil(() => UIManager.instance.playerSubmitted);

            // Reset again for safety
            UIManager.instance.playerSubmitted = false;

            // 🔥 Ensure correct guessed value is stored
            if (p.isHuman)
            {
                // Human guess should already be set by SubmitGuess,
                // but enforce it just in case
                // 🔥 Always store the guess into the item FIRST
                currentItem.guessedPrice = guessValue;

                GuessSystem.instance.SubmitGuess(currentItem, guessValue);
            }
            else if (item.guessedPrice <= 0)
            {
                // Bot fallback
                item.guessedPrice = PriceEstimator.EstimatePrice(item);
            }
            // 4️⃣ Play SFX
            if (GameManager.instance.audioSource != null && youGuessedPriceSFX != null)
                GameManager.instance.audioSource.PlayOneShot(youGuessedPriceSFX);

            // 5️⃣ Show result message
            UIManager.instance.SetMessage(
                $"For {item.itemName}, {p.playerName} guessed {item.guessedPrice} yen"
            );
            yield return new WaitForSeconds(2f);
            UIManager.instance.HideMessage();
            yield return new WaitForSeconds(0.2f);
        }

        GuessSystem.instance.firstOrderGuessesShown = true;

        yield return StartCoroutine(ShowLastOrderMessage());

        GameManager.instance.ChangeState(GameState.LastOrder);
    }
    public IEnumerator ShowFirstOrderGuesses()
    {
        if (firstOrderGuessesAlreadyShown)
            yield break;

        GuessSystem.instance.firstOrderGuessesShown = true;
        GuessSystem.instance.isLastOrderGuessPhase = false;
        firstOrderGuessesAlreadyShown = true;

        List<PlayerData> players = GameManager.instance.players;
        int humanCount = players.FindAll(p => p.isHuman).Count;

        if (humanCount > 1)
            yield break;

        for (int i = 0; i < players.Count; i++)
        {
            PlayerData p = players[i];
            MenuItem item = (p.orders.Count > 0) ? p.orders[0] : null;

            if (item == null) continue;

            if (!p.isHuman && item.guessedPrice <= 0)
            {
                item.guessedPrice = PriceEstimator.EstimatePrice(item);
            }

            string message = p.isHuman
                ? $"For {item.itemName}, you guessed {item.guessedPrice} yen"
                : $"For {item.itemName}, {p.playerName} guessed {item.guessedPrice} yen";

            if (GameManager.instance.audioSource != null && youGuessedPriceSFX != null)
                GameManager.instance.audioSource.PlayOneShot(youGuessedPriceSFX);

            UIManager.instance.SetMessage(message);
            yield return new WaitForSeconds(2f);
            UIManager.instance.HideMessage();
            yield return new WaitForSeconds(0.2f);
        }
    }
    public void ResetFirstOrderGuessesFlag()
    {
        firstOrderGuessesAlreadyShown = false;
    }

    public IEnumerator BeginLastOrderPhase(List<MenuItem> lastOrderItems)
    {
        // ✅ Mark flags in GuessSystem at the correct time
        GuessSystem.instance.firstOrderGuessesShown = true;   // First order is now fully done
        GuessSystem.instance.isLastOrderGuessPhase = true;   // Last order phase starts

        // Show "Last Order!" message
        if (!GuessSystem.instance.lastOrderMessageShown)
        {
            GuessSystem.instance.lastOrderMessageShown = true;

            UIManager.instance.ShowMessageForSeconds(
                "Last Order! Choose items until you hit the target!",
                3f
            );

            // Wait so player sees the message
            yield return new WaitForSeconds(0.2f);
        }

        // Now start the normal last order guess sequence
        StartCoroutine(GuessPopup.instance.BeginLastOrderPhase(lastOrderItems));
    }

    public IEnumerator StartLastOrderGuessSequence(List<MenuItem> items)
    {
        // 0) Reset state for last-order guessing
        GuessSystem.instance.ResetGuessSystem();
        GuessSystem.instance.isLastOrderGuessPhase = true;
        UIManager.instance.playerSubmitted = false;

        // 1) Show first-order guess recap first
        yield return StartCoroutine(ShowFirstOrderGuesses());

        // 2) Show last-order guessing message once
        if (!GuessSystem.instance.lastOrderMessageShown)
        {
            GuessSystem.instance.lastOrderMessageShown = true;

            UIManager.instance.ShowMessageForSeconds(
                "Last Order Guessing Time!",
                3f
            );
            yield return new WaitForSeconds(3f);
            UIManager.instance.HideMessage();
        }

        int humanCount = GameManager.instance.players.FindAll(p => p.isHuman).Count;

        // MULTIPLAYER
        if (humanCount > 1)
        {
            List<PlayerData> players = GameManager.instance.players;

            for (int i = 0; i < players.Count; i++)
            {
                PlayerData currentPlayer = players[i];
                if (currentPlayer == null || !currentPlayer.isHuman)
                    continue;

                UIManager.instance.ShowMessageForSeconds(
                    $"It's {currentPlayer.playerName}'s turn to guess each last order food!",
                    2f
                );
                yield return new WaitForSeconds(2f);

                List<MenuItem> playerLastOrderItems = currentPlayer.orders.Skip(1).ToList();

                foreach (MenuItem item in playerLastOrderItems)
                {
                    UIManager.instance.ShowMessageForSeconds($"How much is {item.itemName}?", 1f);
                    yield return new WaitForSeconds(1f);

                    UIManager.instance.playerSubmitted = false;
                    OpenGuess(item);

                    yield return new WaitUntil(() => UIManager.instance.playerSubmitted);
                    UIManager.instance.playerSubmitted = false;

                    if (GameManager.instance.audioSource != null && youGuessedPriceSFX != null)
                        GameManager.instance.audioSource.PlayOneShot(youGuessedPriceSFX);

                    UIManager.instance.SetMessage(
                        $"For {item.itemName}, {currentPlayer.playerName} guessed {item.guessedPrice} yen"
                    );
                    yield return new WaitForSeconds(1f);
                    UIManager.instance.HideMessage();

                    if (popupRect != null)
                        popupRect.gameObject.SetActive(false);
                }
            }

            GuessSystem.instance.InitializeGuessCount();
            GameManager.instance.ChangeState(GameState.ResultsReveal);
            firstOrderGuessesShown = true;
            yield break;
        }

        // SINGLE PLAYER
        List<MenuItem> playerLastOrderItemsSingle = items.Skip(1).ToList();

        foreach (MenuItem item in playerLastOrderItemsSingle)
        {
            UIManager.instance.ShowMessageForSeconds($"How much is {item.itemName}?", 1f);
            yield return new WaitForSeconds(1f);

            UIManager.instance.playerSubmitted = false;
            OpenGuess(item);

            yield return new WaitUntil(() => UIManager.instance.playerSubmitted);
            UIManager.instance.playerSubmitted = false;

            if (GameManager.instance.audioSource != null && youGuessedPriceSFX != null)
                GameManager.instance.audioSource.PlayOneShot(youGuessedPriceSFX);

            UIManager.instance.SetMessage($"For {item.itemName}, you guessed {item.guessedPrice} yen");
            yield return new WaitForSeconds(1f);
            UIManager.instance.HideMessage();

            if (popupRect != null)
                popupRect.gameObject.SetActive(false);
        }

        // BOTS: single-player only
        List<PlayerData> bots = GameManager.instance.players.FindAll(p => !p.isHuman);

        foreach (PlayerData bot in bots)
        {
            if (bot.orders == null || bot.orders.Count <= 1)
                continue;

            int botLastOrderGuessSum = 0;

            foreach (MenuItem item in bot.orders.Skip(1))
            {
                item.guessedPrice = PriceEstimator.EstimatePrice(item);
                botLastOrderGuessSum += item.guessedPrice;
            }

            if (GameManager.instance.audioSource != null && youGuessedPriceSFX != null)
                GameManager.instance.audioSource.PlayOneShot(youGuessedPriceSFX);

            UIManager.instance.SetMessage(
                $"For combined last order foods, {bot.playerName} guessed {botLastOrderGuessSum} yen"
            );
            yield return new WaitForSeconds(1.5f);
            UIManager.instance.HideMessage();
            yield return new WaitForSeconds(0.15f);
        }

        // 5️⃣ Initialize guesses for last order
        GuessSystem.instance.InitializeGuessCount();

        // 6️⃣ Proceed to results
        GameManager.instance.ChangeState(GameState.ResultsReveal);
        firstOrderGuessesShown = true;
    }

    private IEnumerator SlideRect(float startY, float targetY)
    {
        float elapsed = 0f;

        Vector2 startPos = new Vector2(popupRect.anchoredPosition.x, startY);
        Vector2 targetPos = new Vector2(popupRect.anchoredPosition.x, targetY);

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            float ease = 1f - Mathf.Pow(1f - t, 3f);

            popupRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, ease);
            yield return null;
        }

        popupRect.anchoredPosition = targetPos;
    }

    
}