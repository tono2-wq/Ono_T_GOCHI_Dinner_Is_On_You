using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOrderController : MonoBehaviour
{
    public static PlayerOrderController instance;

    public GameObject buttonContainer;
    public AudioSource audioSource;
    public AudioClip confirmSound;
    public PlateMotionController plateController;

    private Coroutine hideRoutine;
    private List<FoodMotion> selectedFoods = new List<FoodMotion>();

    [Header("Eating Animators")]
    public Animator animatorYou;
    public Animator animatorYabe;
    public Animator animatorOkamura;

    [Header("Eating Clips")]
    public AnimationClip eatingYouClip;
    public AnimationClip eatingYabeClip;
    public AnimationClip eatingOkamuraClip;
    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private string NormalizeName(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";

        value = value.ToLower();
        value = value.Replace(" ", "");
        value = value.Replace("_", "");
        value = value.Replace("-", "");
        value = value.Replace("(clone)", "");
        value = value.Replace("(", "");
        value = value.Replace(")", "");

        return value;
    }

    public void SelectFood(MenuItem item)
    {
        if (item == null)
        {
            Debug.LogError("Selected MenuItem is null!");
            return;
        }

        if (plateController == null)
        {
            Debug.LogError("PlateMotionController is not assigned!");
            return;
        }

        var player = GameManager.instance.GetCurrentPlayer();
        if (player == null)
        {
            Debug.LogError("Current player is null!");
            return;
        }

        if (player.orders == null)
            player.orders = new List<MenuItem>();

        if (GameManager.instance.currentState == GameState.LastOrder)
        {
            player.orders.Add(item);

            if (UIManager.instance != null)
                UIManager.instance.ShowMessageForSeconds($"{player.playerName} ordered {item.itemName}", 1.5f);

            if (audioSource != null && confirmSound != null)
                audioSource.PlayOneShot(confirmSound);

            if (hideRoutine != null)
                StopCoroutine(hideRoutine);

            hideRoutine = StartCoroutine(HideTemporarily(1.5f));
            return;
        }

        player.orders.Add(item);

        if (UIManager.instance != null)
        {
            
            UIManager.instance.ShowMessageForSeconds($"{player.playerName} ordered {item.itemName}", 2f);

            if (audioSource != null && confirmSound != null)
                audioSource.PlayOneShot(confirmSound);
        }

        MoveAndCloseButtonContainer();
        int humanCount = GameManager.instance.players.FindAll(p => p.isHuman).Count;

        // SINGLE PLAYER
        if (humanCount == 1)
        {
            StartCoroutine(ShowBotOrders(item));

            if (RoundManager.instance != null)
            StartCoroutine(ShowFirstOrderDialogueWithDelay(6f, item, 0));
            return;
        }

        // MULTIPLAYER
        bool hasNextPlayer = GameManager.instance.MoveToNextPlayer();

        if (hasNextPlayer)
        {
            var nextPlayer = GameManager.instance.GetCurrentPlayer();

            StartCoroutine(HandleNextPlayerTurn(player, item, nextPlayer));
            return;
        }



        // All 3 players finished first order
        GameManager.instance.ResetTurnOrder();

        // Spawn all players' foods onto plates
        SpawnMultiplayerFirstOrderFoods();

        int finalDelay = 2; // 2 sec last-player-ordered message + 2 sec extra wait
        StartCoroutine(ShowFirstOrderDialogueWithDelay(finalDelay, item, 0));
        return;
    }
    IEnumerator ShowNextTurnMessage(PlayerData nextPlayer)
    {
        // Wait for previous message to finish (2 seconds)
        yield return new WaitForSeconds(2f);

        UIManager.instance.ShowMessageForSeconds(
            $"{nextPlayer.playerName}'s turn to order!",
            2f
        );
    }

    IEnumerator HandleNextPlayerTurn(PlayerData currentPlayer, MenuItem item, PlayerData nextPlayer)
    {
        // 1️⃣ CLOSE MENU immediately
        if (buttonContainer != null)
            buttonContainer.SetActive(false);

        // 2️⃣ Show "ordered" message
        UIManager.instance.ShowMessageForSeconds(
            $"{currentPlayer.playerName} ordered {item.itemName}",
            2f
        );

        yield return new WaitForSeconds(2f);

        // 3️⃣ Show next player message
        UIManager.instance.ShowMessageForSeconds(
            $"{nextPlayer.playerName}'s turn to order!",
            2f
        );

        yield return new WaitForSeconds(2f);

        // 4️⃣ REOPEN MENU
        if (buttonContainer != null)
        {
            buttonContainer.SetActive(true);

            RectTransform rect = buttonContainer.GetComponent<RectTransform>();
            if (rect != null)
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -300f);
        }
    }
    IEnumerator HideTemporarily(float duration)
    {
        if (buttonContainer == null) yield break;

        RectTransform rect = buttonContainer.GetComponent<RectTransform>();
        if (rect == null) yield break;

        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, 2000f);

        yield return new WaitForSeconds(duration);

        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, -300f);
    }

    private FoodMotion SpawnFoodToPlate(MenuItem item, Transform plate)
    {
        if (item == null || plate == null || plateController == null) return null;

        string sourceID = !string.IsNullOrEmpty(item.itemID) ? item.itemID : item.itemName;
        string targetID = NormalizeName(sourceID);

        Debug.Log($"Spawn request: itemName='{item.itemName}', itemID='{item.itemID}', normalized='{targetID}'");

        FoodMotion spawnedFood = plateController.SpawnFoodForPlate(targetID, plate);

        if (spawnedFood != null)
            Debug.Log($"Spawned {targetID} on {plate.name}");
        else
            Debug.LogWarning($"Failed to spawn food for targetID: {targetID}");

        return spawnedFood;
    }

    private IEnumerator ShowBotOrders(MenuItem playerItem)
    {
        selectedFoods.Clear();

        FoodMotion playerFood = SpawnFoodToPlate(playerItem, plateController.plate1);
        if (playerFood != null) selectedFoods.Add(playerFood);

        yield return new WaitForSeconds(2f);

        var bot1 = GameManager.instance.players[1];
        var bot1Item = MenuManager.instance.GetRandomItem();
        bot1.orders.Add(bot1Item);
        UIManager.instance.ShowMessageForSeconds($"Yabe ordered {bot1Item.itemName}", 2f);

        FoodMotion bot1Food = SpawnFoodToPlate(bot1Item, plateController.plate2);
        if (bot1Food != null) selectedFoods.Add(bot1Food);

        yield return new WaitForSeconds(2f);

        var bot2 = GameManager.instance.players[2];
        var bot2Item = MenuManager.instance.GetRandomItem();
        bot2.orders.Add(bot2Item);
        UIManager.instance.ShowMessageForSeconds($"Okamura ordered {bot2Item.itemName}", 2f);

        FoodMotion bot2Food = SpawnFoodToPlate(bot2Item, plateController.plate3);
        if (bot2Food != null) selectedFoods.Add(bot2Food);

        yield return new WaitForSeconds(1f);

        plateController.StartPlateAnimation(this, selectedFoods);

        if (GameManager.instance.currentState == GameState.FirstOrder &&
            GameManager.instance.audioSource != null &&
            GameManager.instance.orderEatingSceneMusic != null)
        {
            GameManager.instance.audioSource.clip = GameManager.instance.orderEatingSceneMusic;
            GameManager.instance.audioSource.loop = false;
            GameManager.instance.audioSource.volume = 1f;
            GameManager.instance.audioSource.Play();

            yield return new WaitWhile(() =>
                GameManager.instance.audioSource.isPlaying &&
                GameManager.instance.currentState == GameState.FirstOrder
            );

            if (RoundManager.instance != null &&
                RoundManager.instance.firstOrderDialoguePanel != null)
            {
                RoundManager.instance.firstOrderDialoguePanel.SetActive(false);
            }
        }

        if (GuessPopup.instance != null)
            GuessPopup.instance.OpenGuess(playerItem);
    }

    private IEnumerator ShowFirstOrderDialogueWithDelay(float delay, MenuItem firstOrderItem, int playerIndex)
    {
        yield return new WaitForSeconds(delay);

        if (GameManager.instance.currentState != GameState.FirstOrder)
            yield break;

        if (RoundManager.instance == null ||
            RoundManager.instance.firstOrderDialoguePanel == null ||
            RoundManager.instance.firstOrderDialogueText == null)
            yield break;

        RoundManager.instance.firstOrderDialoguePanel.SetActive(true);
        StartCoroutine(RoundManager.instance.FadeOutMusic(RoundManager.instance.musicSource, 0.5f));

        RoundManager.instance.firstOrderDialogueText.text =
            RoundManager.instance.GetRandomDialogue(RoundManager.instance.serveDialogues);

        int humanCount = GameManager.instance.players.FindAll(p => p.isHuman).Count;

        // Multiplayer: start eating scene music here
        if (humanCount > 1 &&
            GameManager.instance.audioSource != null &&
            GameManager.instance.orderEatingSceneMusic != null)
        {
            GameManager.instance.audioSource.clip = GameManager.instance.orderEatingSceneMusic;
            GameManager.instance.audioSource.loop = false;
            GameManager.instance.audioSource.volume = 1f;
            GameManager.instance.audioSource.Play();
        }

        yield return new WaitForSeconds(5f);

        PlayEatingAnimations();

        yield return new WaitForSeconds(1f);
        plateController.ResetFoodsOnly();

        RoundManager.instance.firstOrderDialogueText.text =
            RoundManager.instance.GetRandomDialogue(RoundManager.instance.eatingDialogues);
        yield return new WaitForSeconds(7f);

        RoundManager.instance.firstOrderDialogueText.text =
            RoundManager.instance.GetRandomDialogue(RoundManager.instance.tasteDialogues);
        yield return new WaitForSeconds(6f);

        plateController.ResetPlates(true);
        // Multiplayer: start full turn-based guess sequence after taste dialogue
        if (humanCount > 1 && GuessPopup.instance != null)
        {
            // 🔥 Hide dialogue panel before guessing starts
            if (RoundManager.instance != null &&
                RoundManager.instance.firstOrderDialoguePanel != null)
            {
                RoundManager.instance.firstOrderDialoguePanel.SetActive(false);
            }

            yield return StartCoroutine(GuessPopup.instance.RunMultiplayerFirstOrderGuessSequence());
        }
    }

    private void MoveAndCloseButtonContainer()
    {
        if (buttonContainer != null)
        {
            buttonContainer.SetActive(false);
            RectTransform rectTransform = buttonContainer.GetComponent<RectTransform>();
            if (rectTransform != null)
                rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, 2000f);
        }
        else
        {
            Debug.LogError("ButtonContainer not assigned!");
        }
    }

    public void StartRound(List<MenuItem> playerAndBotItems)
    {
        selectedFoods.Clear();

        for (int i = 0; i < playerAndBotItems.Count; i++)
        {
            Transform targetPlate = (i == 0) ? plateController.plate1 :
                                   (i == 1) ? plateController.plate2 :
                                              plateController.plate3;

            FoodMotion food = SpawnFoodToPlate(playerAndBotItems[i], targetPlate);
            if (food != null) selectedFoods.Add(food);
        }

        plateController.StartPlateAnimation(this, selectedFoods);
    }

    private void PlayEatingAnimations()
    {
        if (animatorYou != null)
        {
            animatorYou.speed = 1f;
            animatorYou.Play("EatingYou", 0, 0f);
        }

        if (animatorYabe != null)
        {
            animatorYabe.speed = 1f;
            animatorYabe.Play("EatingYabe", 0, 0f);
        }

        if (animatorOkamura != null)
        {
            animatorOkamura.speed = 1f;
            animatorOkamura.Play("EatingOkamura", 0, 0f);
        }

        StartCoroutine(FreezeAnimatorAtFrame(animatorYou, "EatingYou", eatingYouClip, 36));
        StartCoroutine(FreezeAnimatorAtFrame(animatorYabe, "EatingYabe", eatingYabeClip, 36));
        StartCoroutine(FreezeAnimatorAtFrame(animatorOkamura, "EatingOkamura", eatingOkamuraClip, 36));
    }
    private void SpawnMultiplayerFirstOrderFoods()
    {
        selectedFoods.Clear();

        var players = GameManager.instance.players;

        if (players.Count < 3) return;

        // Player 1
        if (players[0].orders.Count > 0)
        {
            var food = SpawnFoodToPlate(players[0].orders[0], plateController.plate1);
            if (food != null) selectedFoods.Add(food);
        }

        // Player 2
        if (players[1].orders.Count > 0)
        {
            var food = SpawnFoodToPlate(players[1].orders[0], plateController.plate2);
            if (food != null) selectedFoods.Add(food);
        }

        // Player 3
        if (players[2].orders.Count > 0)
        {
            var food = SpawnFoodToPlate(players[2].orders[0], plateController.plate3);
            if (food != null) selectedFoods.Add(food);
        }

        plateController.StartPlateAnimation(this, selectedFoods);
    }
    private IEnumerator FreezeAnimatorAtFrame(Animator animator, string stateName, AnimationClip clip, int frame)
    {
        if (animator == null || clip == null)
            yield break;

        float frameRate = clip.frameRate;
        float stopTime = frame / frameRate;

        yield return new WaitForSeconds(stopTime);

        float normalizedTime = stopTime / clip.length;
        normalizedTime = Mathf.Clamp01(normalizedTime);

        animator.speed = 1f;
        animator.Play(stateName, 0, normalizedTime);
        animator.Update(0f);
        animator.speed = 0f;
    }
}