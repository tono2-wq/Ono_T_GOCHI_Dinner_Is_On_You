using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameState currentState;
    public int currentRound = 1;
    public int maxRounds = 4;

    public List<PlayerData> players = new List<PlayerData>();

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip targetPriceRevealedSFX;
    public AudioClip orderEatingSceneMusic;
    [Header("UI References")]
    public GameObject targetPricePanel; // The panel GameObject
    public TMPro.TextMeshProUGUI targetPriceText; // The text displaying the target price
    [Header("Results Text")]
    public GameObject resultsTextObject;
    public SpriteRenderer resultsTextSpriteRenderer;
    public Sprite resultsText0Sprite;
    public Sprite resultsText1Sprite;
    public Sprite resultsText3Sprite;
    public int currentPlayerIndex = 0;
    public bool isMultiplayer = false;
    public TextMeshProUGUI[] playerNameTexts;

    public PlayerData GetCurrentPlayer()
    {
        if (players == null || players.Count == 0)
            return null;

        if (currentPlayerIndex < 0 || currentPlayerIndex >= players.Count)
            currentPlayerIndex = 0;

        return players[currentPlayerIndex];
    }

    void UpdatePlayerNameUI()
    {
        if (players == null || playerNameTexts == null)
            return;

        for (int i = 0; i < players.Count; i++)
        {
            if (i < playerNameTexts.Length && playerNameTexts[i] != null)
                playerNameTexts[i].text = players[i].playerName;
        }
    }
    public void ResetTurnOrder()
    {
        currentPlayerIndex = 0;
    }

    public bool MoveToNextPlayer()
    {
        if (players == null || players.Count == 0)
            return false;

        currentPlayerIndex++;

        return currentPlayerIndex < players.Count;
    }
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (players == null)
            players = new List<PlayerData>();
    }

    void Start()
    {
        UpdatePlayerNameUI();
        InitializeGame();
    }

    void InitializeGame()
    {
        CreatePlayers();
        ChangeState(GameState.TargetReveal);
    }

    void CreatePlayers()
    {
        Debug.Log("Pending names count = " +
    (DialogueManager.pendingMultiplayerNames == null ? -1 : DialogueManager.pendingMultiplayerNames.Count));
        if (players != null && players.Count > 0)
            return;

        players.Clear();

        if (DialogueManager.startInMultiplayer)
        {
            if (DialogueManager.pendingMultiplayerNames == null || DialogueManager.pendingMultiplayerNames.Count < 3)
            {
                Debug.LogError("Multiplayer names were not passed correctly from DialogueManager.");
                return;
            }

            for (int i = 0; i < 3; i++)
            {
                string playerName = DialogueManager.pendingMultiplayerNames[i].Trim();

                players.Add(new PlayerData
                {
                    playerName = playerName,
                    isHuman = true
                });

                Debug.Log("Created multiplayer player: " + playerName);
            }
        }
        else
        {
            players.Add(new PlayerData { playerName = "You", isHuman = true });
            players.Add(new PlayerData { playerName = "Yabe", isHuman = false });
            players.Add(new PlayerData { playerName = "Okamura", isHuman = false });
        }

        UpdatePlayerNameUI();
    }

    public void ChangeState(GameState newState)
    {
        currentState = newState;

        switch (newState)
        {
            case GameState.TargetReveal:
                HideResultsText();
                StartTargetReveal();
                break;

            case GameState.FirstOrder:
                ShowResultsText(resultsText0Sprite);
                if (RoundManager.instance != null)
                    RoundManager.instance.StartFirstOrder();
                break;

            case GameState.LastOrder:
                ShowResultsText(resultsText1Sprite);
                RoundManager.instance.StartLastOrder();

                StopOrderButton stopButton = FindObjectOfType<StopOrderButton>();
                if (stopButton != null)
                {
                    stopButton.ResetButton();
                }
                break;

            case GameState.GuessPhase:
                if (RoundManager.instance != null)
                    RoundManager.instance.StartGuessPhase();
                break;

            case GameState.ResultsReveal:
                ShowResultsText(resultsText3Sprite);

                if (ResultManager.instance != null)
                    ResultManager.instance.StartReveal();

                if (ResultsUIController.instance != null)
                    ResultsUIController.instance.ShowResults();
                break;

            case GameState.RoundEnd:
                EndRound();
                break;

            case GameState.FinalResults:
                if (ResultManager.instance != null)
                    ResultManager.instance.StartFinalResults();
                HideResultsText();

                if (targetPricePanel != null)
                    targetPricePanel.SetActive(false);

                break;
        }
    }

    void StartTargetReveal()
    {
        if (UIManager.instance == null || TargetManager.instance == null)
        {
            Debug.LogError("UIManager or TargetManager instance is NULL");
            return;
        }

        TargetManager.instance.GenerateTarget();
        // Update UI
        if (targetPricePanel != null && targetPriceText != null)
        {
            targetPriceText.text = "Target Price: " + TargetManager.instance.targetPrice.ToString("¥0");
            // "C0" formats as currency with no decimals (e.g., $100)
            targetPricePanel.SetActive(true);
        }

        UIManager.instance.ShowTarget(TargetManager.instance.targetPrice);

        StartCoroutine(TargetRevealRoutine());
    }

    IEnumerator TargetRevealRoutine()
    {
        if (audioSource != null && targetPriceRevealedSFX != null)
        {
            audioSource.clip = targetPriceRevealedSFX;
            audioSource.Play();
            yield return new WaitWhile(() => audioSource.isPlaying);
        }
        else
        {
            yield return new WaitForSeconds(2f);
        }

        ChangeState(GameState.FirstOrder);
    }

    void EndRound()
    {
        currentRound++;

        if (currentRound > maxRounds)
        {
            if (FinalResultsUI.instance != null)
                FinalResultsUI.instance.ShowFinalResults();
        }
        else
        {
            StartNextRound();
        }
    }

    void StartNextRound()
    {
        foreach (var p in players)
            p.ResetRound();

        ChangeState(GameState.TargetReveal);
    }

    private void HideResultsText()
    {
        if (resultsTextObject != null)
            resultsTextObject.SetActive(false);
    }

    private void ShowResultsText(Sprite sprite)
    {
        if (resultsTextObject != null)
            resultsTextObject.SetActive(true);

        if (resultsTextSpriteRenderer != null && sprite != null)
            resultsTextSpriteRenderer.sprite = sprite;
    }
}