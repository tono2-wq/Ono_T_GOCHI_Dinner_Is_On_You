using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.ParticleSystem;


public class RoundManager : MonoBehaviour
{
    public static RoundManager instance;

    [Header("UI Panels")]
    public GameObject menuUI;
    public GameObject targetRevealPanel;
    public GameObject menuButtonPrefab;

    [Header("ORDER STOP")]
    public GameObject orderStopObject; // Assign ORDER_STOP! in Inspector

    [Header("Audio")]
    public AudioSource musicSource;
    public AudioClip firstOrderMusic;
    public AudioClip firstOrderMusicFinalRoundIntro;
    public AudioClip firstOrderMusicFinalRound;
    public AudioSource introMusicSource; // NEW (use for intro)

    [System.Serializable]
    public class StopHandColorSet
    {
        public string colorName;
        public Sprite sprite2;
        public Sprite sprite1;
        public Sprite sprite0;
    }

    [Header("Plates")]
    public Transform plate1;
    public Transform plate2;
    public Transform plate3;

    [Header("Order Eating Music")]
    public AudioSource orderEatingSceneMusic;
    public GameObject Plate_1;
    public GameObject Plate_2;
    public GameObject Plate_3;
    [Header("Plate Motion Controller")]
    public PlateMotionController plateMotionController;

    [Header("First Order Dialogue")]
    public GameObject firstOrderDialoguePanel;
    public TextMeshProUGUI firstOrderDialogueText;

    [TextArea] public string[] serveDialogues = new string[4];
    [TextArea] public string[] eatingDialogues = new string[4];
    [TextArea] public string[] tasteDialogues = new string[4];

    [Header("Character Animations")]
    public Animator animatorOkamura;
    public Animator animatorYabe;
    public Animator animatorYou;

    [Header("Last Order")]
    public AudioClip lastOrderMusic;
    public AudioClip lastOrderStopSFX;
    public AudioClip lastOrderStopHorn;
    [Header("Last Order UI")]
    public GameObject stopOrderButtonPrefab; // assign prefab in Inspector

    private StopOrderButton stopOrderButtonInstance;
    [Header("Stop Order Button Settings")]
    public GameObject stopOrderPrefab; // Assign in inspector
    public Transform stopOrderParent;  // The UI panel or container to hold the button

    [Header("Last Order Stop Hands")]
    public GameObject Stop_HandPlayer1;
    public GameObject Stop_HandPlayer2;
    public GameObject Stop_HandPlayer3;

    [Header("Stop Hand Sprite Sets")]
    public StopHandColorSet[] stopHandSpriteSets;

    private readonly string[] stopHandColors =
    {
        "Red", "Orange", "Yellow", "Green", "Blue", "Purple", "Pink"
    };

    [Header("Results Intro")]
    public AudioClip resultsIntroSFX;

    [Header("Pitari Award")]
    public AudioClip pitariChanceMusic;
    public AudioClip noPitariSFX;
    public AudioClip pitariEligibleSFX;

    [Header("SFX - Results")]
    public AudioClip resultsPlayerReveal;
    public AudioClip nearPinEligible;
    public AudioClip nearPinWinner;
    public AudioClip firstPlace;
    public AudioClip goodOrder;
    public AudioClip lastPlaceFate;
    public AudioClip lastPlaceTimpani;
    public AudioClip lastPlaceCanon;
    public AudioClip lastPlaceReveal;
    public AudioClip lastPlaceMusic;
    public AudioClip ResultsAmountShowed;
    public AudioClip SecondLastPlaceResultsAmountShow;
    public AudioClip LastPlaceResultsAmountShow;
    public AudioClip mealFeeShown;
    public AudioClip gochiShout;

    [Header("Results Sprite")]
    public GameObject ResultsSprite;

    [Header("Results Text Sprites")]
    public Sprite ResultsText_3;
    public Sprite ResultsText_4;
    public Sprite ResultsText_5;
    public Sprite ResultsText_6;
    public Sprite ResultsText_7;
    public Sprite ResultsText_8;
    public Sprite ResultsText_9;
    public Sprite ResultsText_11;

    [Header("SFX - Near Pin Rewards")]
    public AudioClip nearPin1000;
    public AudioClip nearPin50000;

    [Header("Near Pin Award")]
    public GameObject NearPinAward;

    [Header("SFX - Applause")]
    public AudioClip[] applauseClips;

    [Header("SFX - Pitari")]
    public AudioClip lastPlaceTimpanipitari;
    public AudioClip lastPlaceCanonpitari;
    public AudioClip lastPlaceRevealed;
    public AudioClip pitariWin;

    [Header("Drone Animation")]
    public Animator droneAnimator;

    const string DRONE_IDLE = "DroneIdle";
    const string DRONE_MOVING = "DroneMoving";

    [Header("SFX Sources")]
    public AudioSource timpani;

    [Header("Drone")]
    public GameObject lastPlaceDrone; // Assign LastPlaceDrone_0 in Inspector

    [TextArea] public string[] pitariIntroLines = new string[3];
    [TextArea] public string[] pitariHypeLines = new string[3];


    private bool isLastOrderActive = false;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // ✅ Correct check
        if (stopOrderButtonPrefab == null)
        {
            Debug.LogError("StopOrderButton PREFAB is NOT assigned in Inspector!");
        }
    }
    int GetPlayerIndex(PlayerData player)
    {
        if (player == null || GameManager.instance == null || GameManager.instance.players == null)
            return -1;

        return GameManager.instance.players.IndexOf(player);
    }
    void ShowNearPinAward()
    {
        if (NearPinAward != null)
            NearPinAward.SetActive(true);
    }

    void SetDroneIdle()
    {
        if (droneAnimator == null) return;

        droneAnimator.speed = 1f;
        droneAnimator.Play(DRONE_IDLE, 0, 0f);
    }

    void SetDroneMoving()
    {
        if (droneAnimator == null) return;

        droneAnimator.speed = 1f;
        droneAnimator.Play(DRONE_MOVING, 0, 0f);
    }
    void HideNearPinAward()
    {
        if (NearPinAward != null)
            NearPinAward.SetActive(false);
    }

    /// <summary>
    /// Returns the players ranked by closeness to target price:
    /// 1st element = closest (first place), last element = farthest (last place)
    /// </summary>
    public List<PlayerData> GetPlayersRankedByTarget()
    {
        if (GameManager.instance == null || GameManager.instance.players == null || GameManager.instance.players.Count == 0)
            return new List<PlayerData>();

        int target = TargetManager.instance.targetPrice;

        // Order by absolute distance from target
        var ranked = GameManager.instance.players
            .OrderBy(p => Mathf.Abs(p.totalActual - target))
            .ToList();

        return ranked;
    }

    /// <summary>
    /// Returns the first place player (closest to target price)
    /// </summary>
    public PlayerData GetFirstPlace()
    {
        var ranked = GetPlayersRankedByTarget();
        return ranked.Count > 0 ? ranked[0] : null;
    }

    /// <summary>
    /// Returns the last place player (furthest from target price)
    /// </summary>
    public PlayerData GetWinner()
    {
        int target = TargetManager.instance.targetPrice;

        // 1️⃣ Order players by distance to target
        var sorted = GameManager.instance.players
            .OrderBy(p => Mathf.Abs(p.totalActual - target))
            .ToList();

        // 2️⃣ Find the minimum distance
        float minDistance = Mathf.Abs(sorted[0].totalActual - target);

        // 3️⃣ Get all players tied with that distance
        var tiedPlayers = sorted
            .Where(p => Mathf.Abs(p.totalActual - target) == minDistance)
            .ToList();

        // 4️⃣ If more than one, pick randomly among them
        if (tiedPlayers.Count > 1)
        {
            int index = Random.Range(0, tiedPlayers.Count);
            return tiedPlayers[index];
        }

        return sorted[0];
    }

    public PlayerData GetLastPlace()
    {
        int target = TargetManager.instance.targetPrice;

        // 1️⃣ Order by farthest from target
        var sorted = GameManager.instance.players
            .OrderByDescending(p => Mathf.Abs(p.totalActual - target))
            .ToList();

        float maxDistance = Mathf.Abs(sorted[0].totalActual - target);

        // 2️⃣ Get all tied for last place
        var tiedPlayers = sorted
            .Where(p => Mathf.Abs(p.totalActual - target) == maxDistance)
            .ToList();

        // 3️⃣ Pick randomly if tie
        if (tiedPlayers.Count > 1)
        {
            int index = Random.Range(0, tiedPlayers.Count);
            return tiedPlayers[index];
        }

        return sorted[0];
    }
    void Start()
    {
        StartCharacterAnimations();

        if (orderStopObject != null)
            orderStopObject.SetActive(false);

        HideAllStopHands();
        HideResultsSprite();
        SetDroneIdle();

        if (ResultsSprite != null)
        {
            RectTransform rect = ResultsSprite.GetComponent<RectTransform>();
            if (rect != null)
                rect.anchoredPosition = new Vector2(0f, -4.5f);
            else
                ResultsSprite.transform.localPosition = new Vector3(0f, -4.5f, ResultsSprite.transform.localPosition.z);
        }
        HideNearPinAward(); // ✅ add this
    }

    void HideAllStopHands()
    {
        if (Stop_HandPlayer1 != null) Stop_HandPlayer1.SetActive(false);
        if (Stop_HandPlayer2 != null) Stop_HandPlayer2.SetActive(false);
        if (Stop_HandPlayer3 != null) Stop_HandPlayer3.SetActive(false);
    }

    void HideResultsSprite()
    {
        if (ResultsSprite != null)
            ResultsSprite.SetActive(false);
    }

    void ShowResultsSprite(Sprite sprite, float x, float y)
    {
        if (ResultsSprite == null)
        {
            Debug.LogError("ResultsSprite is not assigned.");
            return;
        }

        if (sprite == null)
        {
            Debug.LogError("Results sprite is missing.");
            return;
        }

        Image image = ResultsSprite.GetComponent<Image>();
        if (image == null)
            image = ResultsSprite.GetComponentInChildren<Image>(true);

        SpriteRenderer spriteRenderer = null;
        if (image == null)
        {
            spriteRenderer = ResultsSprite.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = ResultsSprite.GetComponentInChildren<SpriteRenderer>(true);
        }

        if (image == null && spriteRenderer == null)
        {
            Debug.LogError("ResultsSprite is missing both Image and SpriteRenderer.");
            return;
        }

        RectTransform rect = ResultsSprite.GetComponent<RectTransform>();
        if (rect == null)
            rect = ResultsSprite.GetComponentInChildren<RectTransform>(true);

        if (rect != null)
        {
            Vector2 pos = rect.anchoredPosition;
            rect.anchoredPosition = new Vector2(x, y);
        }
        else
        {
            Vector3 pos = ResultsSprite.transform.localPosition;
            ResultsSprite.transform.localPosition = new Vector3(x, y, pos.z);
        }

        if (image != null)
            image.sprite = sprite;
        else
            spriteRenderer.sprite = sprite;

        ResultsSprite.SetActive(true);
    }
    StopHandColorSet GetRandomStopHandSet()
    {
        if (stopHandSpriteSets == null || stopHandSpriteSets.Length == 0)
        {
            Debug.LogError("Stop hand sprite sets are not assigned.");
            return null;
        }

        int index = Random.Range(0, stopHandSpriteSets.Length);
        return stopHandSpriteSets[index];
    }

    IEnumerator ShowStopHandSequence(GameObject handObject, float startY, float endY)
    {
        if (handObject == null)
            yield break;

        Image image = handObject.GetComponent<Image>();
        if (image == null)
            image = handObject.GetComponentInChildren<Image>(true);

        SpriteRenderer spriteRenderer = null;
        if (image == null)
        {
            spriteRenderer = handObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = handObject.GetComponentInChildren<SpriteRenderer>(true);
        }

        if (image == null && spriteRenderer == null)
        {
            Debug.LogError($"{handObject.name} is missing both Image and SpriteRenderer components.");
            yield break;
        }

        StopHandColorSet chosenSet = GetRandomStopHandSet();
        if (chosenSet == null)
            yield break;

        if (chosenSet.sprite2 == null || chosenSet.sprite1 == null || chosenSet.sprite0 == null)
        {
            Debug.LogError($"Stop hand sprite set is incomplete for color: {chosenSet.colorName}");
            yield break;
        }

        handObject.SetActive(true);

        RectTransform rect = handObject.GetComponent<RectTransform>();
        if (rect == null)
            rect = handObject.GetComponentInChildren<RectTransform>(true);

        Transform moveTarget = handObject.transform;

        bool isUI = rect != null;

        Vector2 startAnchoredPos = Vector2.zero;
        Vector3 startLocalPos = Vector3.zero;

        if (isUI)
        {
            startAnchoredPos = rect.anchoredPosition;
            rect.anchoredPosition = new Vector2(startAnchoredPos.x, startY);
        }
        else
        {
            startLocalPos = moveTarget.localPosition;
            moveTarget.localPosition = new Vector3(startLocalPos.x, startY, startLocalPos.z);
        }

        if (image != null)
            image.sprite = chosenSet.sprite2;
        else
            spriteRenderer.sprite = chosenSet.sprite2;

        float duration = 0.25f;
        float elapsed = 0f;
        bool switchedTo1 = false;
        bool switchedTo0 = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            float y = Mathf.Lerp(startY, endY, t);

            if (isUI)
            {
                rect.anchoredPosition = new Vector2(startAnchoredPos.x, y);
            }
            else
            {
                moveTarget.localPosition = new Vector3(startLocalPos.x, y, startLocalPos.z);
            }

            if (!switchedTo1 && elapsed >= 0.1f)
            {
                if (image != null)
                    image.sprite = chosenSet.sprite1;
                else
                    spriteRenderer.sprite = chosenSet.sprite1;

                switchedTo1 = true;
            }

            if (!switchedTo0 && elapsed >= 0.2f)
            {
                if (image != null)
                    image.sprite = chosenSet.sprite0;
                else
                    spriteRenderer.sprite = chosenSet.sprite0;

                switchedTo0 = true;
            }

            yield return null;
        }

        if (isUI)
            rect.anchoredPosition = new Vector2(startAnchoredPos.x, endY);
        else
            moveTarget.localPosition = new Vector3(startLocalPos.x, endY, startLocalPos.z);

        if (image != null)
            image.sprite = chosenSet.sprite0;
        else
            spriteRenderer.sprite = chosenSet.sprite0;
    }

    #region First Order
    public void StartFirstOrder()
    {
        StartCoroutine(FirstOrderRoutine());
    }

    void OnDestroy()
    {

    }

    IEnumerator FirstOrderRoutine()
    {
        int humanCount = GameManager.instance.players.FindAll(p => p.isHuman).Count;
        if (targetRevealPanel != null)
            targetRevealPanel.SetActive(false);

        if (musicSource != null)
        {
            bool isFinalRound = GameManager.instance != null &&
                                GameManager.instance.currentRound == GameManager.instance.maxRounds;

            if (isFinalRound && firstOrderMusicFinalRoundIntro != null && firstOrderMusicFinalRound != null)
            {
                StartCoroutine(PlayFinalRoundMusic());
            }
            else if (firstOrderMusic != null)
            {
                musicSource.clip = firstOrderMusic;
                musicSource.loop = true;
                musicSource.volume = 1f;
                musicSource.Play();
            }
        }

        yield return StartCoroutine(ShowFirstOrderIntro());


        if (menuUI == null)
            menuUI = FindObjectOfType<MenuUI>()?.gameObject;

        if (menuUI != null)
        {
            menuUI.SetActive(true);
            SetButtonContainerY(-500f);
            PopulateMenuButtons();
            SetAllMenuButtonsVisible(true);  // show
        }


        yield return new WaitUntil(() => UIManager.instance.playerSubmitted);

        if (menuUI != null)
            menuUI.SetActive(false);
        SetButtonContainerY(5000f);
        SetAllMenuButtonsVisible(false); // hide

    }

    IEnumerator PlayFinalRoundMusic()
    {
        if (introMusicSource == null || musicSource == null)
            yield break;

        // 🔊 Play intro (DO NOT loop)
        if (firstOrderMusicFinalRoundIntro != null)
        {
            introMusicSource.clip = firstOrderMusicFinalRoundIntro;
            introMusicSource.loop = false;
            introMusicSource.volume = 1f;
            introMusicSource.Play();
        }

        // ⏳ Wait before loop starts (no cutting!)
        yield return new WaitForSeconds(13.73f);

        // 🔁 Start loop WITHOUT stopping intro
        if (firstOrderMusicFinalRound != null)
        {
            musicSource.clip = firstOrderMusicFinalRound;
            musicSource.loop = true;
            musicSource.volume = 1f;
            musicSource.Play();
        }
    }
    IEnumerator ShowFirstOrderIntro()
    {
        bool isFinalRound = GameManager.instance != null &&
                            GameManager.instance.currentRound == GameManager.instance.maxRounds;

        // Final round special intro
        if (isFinalRound)
        {
            UIManager.instance.ShowMessageForSeconds(
                "Final round! Who's going to be the final winner of the entire game?",
                4f
            );
            yield return new WaitForSeconds(4f);

            UIManager.instance.ShowMessageForSeconds(
                "Person who get the least amount in out-of-pocket data wins the entire game!",
                4f
            );
            yield return new WaitForSeconds(4f);

            UIManager.instance.ShowMessageForSeconds(
                "Let's do this!! Don't be a sore loser!!",
                4f
            );
            yield return new WaitForSeconds(4f);
        }

        // Normal first-order intro
        UIManager.instance.ShowMessageForSeconds(
            "First Order! Choose one item in the menu!",
            2f
        );

        yield return new WaitForSeconds(2f);

        int humanCount = GameManager.instance.players.FindAll(p => p.isHuman).Count;

        if (humanCount > 1)
        {
            GameManager.instance.ResetTurnOrder();

            var player = GameManager.instance.GetCurrentPlayer();

            if (player != null)
            {
                UIManager.instance.ShowMessageForSeconds(
                    $"It's {player.playerName}'s turn to order!",
                    2f
                );
            }

            yield return new WaitForSeconds(2f);
        }

        UIManager.instance.playerSubmitted = false;
    }


    public PlateMotionController plateController;
    IEnumerator ShowFirstOrderDialogueSequence()
    {
        if (firstOrderDialoguePanel == null || firstOrderDialogueText == null)
            yield break;

        // ✅ Start plate animation immediately
      
        yield return new WaitForSeconds(5f);

        firstOrderDialoguePanel.SetActive(true);

        firstOrderDialogueText.text = GetRandomDialogue(serveDialogues);
        yield return new WaitForSeconds(5f);

        firstOrderDialogueText.text = GetRandomDialogue(eatingDialogues);
        yield return new WaitForSeconds(6f);

        firstOrderDialogueText.text = GetRandomDialogue(tasteDialogues);

        if (orderEatingSceneMusic != null)
            yield return new WaitWhile(() => orderEatingSceneMusic.isPlaying);

        firstOrderDialoguePanel.SetActive(false);
        plateController.ResetPlates(this);
    }


    IEnumerator AnimatePlates()
    {
        // Horizontal glide (4s)
        float durationX = 4f;

        Vector3 plate1Start = plate1.position;
        Vector3 plate2Start = plate2.position;
        Vector3 plate3Start = plate3.position;

        Vector3 plate1TargetX = new Vector3(-7f, plate1Start.y, plate1Start.z);
        Vector3 plate2TargetX = new Vector3(0f, plate2Start.y, plate2Start.z);
        Vector3 plate3TargetX = new Vector3(7f, plate3Start.y, plate3Start.z);

        float t = 0f;
        while (t < durationX)
        {
            t += Time.deltaTime;
            float lerp = t / durationX;

            plate1.position = Vector3.Lerp(plate1Start, plate1TargetX, lerp);
            plate2.position = Vector3.Lerp(plate2Start, plate2TargetX, lerp);
            plate3.position = Vector3.Lerp(plate3Start, plate3TargetX, lerp);

            yield return null;
        }

        // Vertical glide (1s)
        float durationY = 1f;

        Vector3 plate1TargetY = new Vector3(plate1.position.x, -0.5f, plate1.position.z);
        Vector3 plate2TargetY = new Vector3(plate2.position.x, -0.25f, plate2.position.z);
        Vector3 plate3TargetY = new Vector3(plate3.position.x, -0.1f, plate3.position.z);

        t = 0f;
        Vector3 plate1Current = plate1.position;
        Vector3 plate2Current = plate2.position;
        Vector3 plate3Current = plate3.position;

        while (t < durationY)
        {
            t += Time.deltaTime;
            float lerp = t / durationY;

            plate1.position = Vector3.Lerp(plate1Current, plate1TargetY, lerp);
            plate2.position = Vector3.Lerp(plate2Current, plate2TargetY, lerp);
            plate3.position = Vector3.Lerp(plate3Current, plate3TargetY, lerp);

            yield return null;
        }
    }

    void ResetPlatePositions()
    {
        plate1.position = new Vector3(-34f, 2.05f, plate1.position.z);
        plate2.position = new Vector3(-27f, 2.2f, plate2.position.z);
        plate3.position = new Vector3(-20f, 2.35f, plate3.position.z);
    }

    // Make this public so PlayerOrderController can use it
    public string GetRandomDialogue(string[] list)
    {
        if (list == null || list.Length == 0)
            return "";

        return list[Random.Range(0, list.Length)];
    }
    #endregion
    void SetButtonContainerY(float y)
    {
        if (menuUI == null) return;

        Transform container = menuUI.transform.Find("ButtonContainer");
        if (container == null) return;

        RectTransform rect = container.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, y);
        }
    }
    void HideMenuUIForGuessPhase()
    {
        if (menuUI != null)
            menuUI.SetActive(false);
        SetButtonContainerY(5000f);
        SetAllMenuButtonsVisible(false); // hide

        StopOrderButton stopBtn = stopOrderButtonInstance;
        if (stopOrderButtonInstance != null)
        {
            stopOrderButtonInstance.gameObject.SetActive(false);
        }

    }
    #region Last Order
    public void StartLastOrder()
    {

        if (isLastOrderActive)
        {
            StartCoroutine(LastOrderRoutine());
            return;
        }

        isLastOrderActive = true;
        StartCoroutine(LastOrderRoutine());

    }

    void HideLastOrderUIElements()
    {
        if (menuUI == null) return;

        Transform container = menuUI.transform.Find("ButtonContainer");
        if (container != null)
            container.gameObject.SetActive(false);

        StopOrderButton stopBtn = stopOrderButtonInstance;

        if (stopBtn != null)
        {
            stopBtn.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("STOP BUTTON NOT ASSIGNED IN INSPECTOR");
        }
    }

    void ShowLastOrderUIElements()
    {
        if (menuUI == null) return;

        // 🔹 Get ButtonContainer
        Transform container = menuUI.transform.Find("ButtonContainer");

        if (container != null)
        {
            container.gameObject.SetActive(true);

            RectTransform rect = container.GetComponent<RectTransform>();
            if (rect != null)
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, 0f);
        }
        else
        {
            Debug.LogError("ButtonContainer not found!");
        }

        // 🔹 Show STOP button
        StopOrderButton stopBtn = stopOrderButtonInstance;

        if (stopBtn != null)
        {
            stopBtn.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("StopOrderButton not found!");
        }
    }
    IEnumerator LastOrderRoutine()
    {
        UIManager.instance.playerSubmitted = false;

        if (menuUI == null)
            menuUI = FindObjectOfType<MenuUI>()?.gameObject;

        Transform container = null;

        if (menuUI != null)
        {
            menuUI.SetActive(true);
            SetButtonContainerY(2000f);
            PopulateMenuButtons();
            SetAllMenuButtonsVisible(true); // show

            SpawnStopOrderButton();

            container = menuUI.transform.Find("ButtonContainer");

            if (container != null)
                container.gameObject.SetActive(true); // ✅ KEEP IT ON

            if (stopOrderButtonInstance != null)
            {
                stopOrderButtonInstance.gameObject.SetActive(true);
                stopOrderButtonInstance.transform.SetAsLastSibling();
            }
        }

            StopOrderButton stopBtn = stopOrderButtonInstance;

        if (stopBtn != null)
        {
            stopBtn.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("STOP BUTTON NOT ASSIGNED IN INSPECTOR");
        }

        UIManager.instance.ShowMessageForSeconds(
            "Last Order! Choose items until you hit the target!",
            3f
        );

        yield return new WaitForSeconds(3f);
        // 🔥 SHOW STOP BUTTON HERE
        ShowStopOrderButton();
        if (musicSource != null && lastOrderMusic != null)
        {
            musicSource.clip = lastOrderMusic;
            musicSource.loop = true;
            musicSource.volume = 1f;
            musicSource.Play();
        }

        int humanCount = GameManager.instance.players.FindAll(p => p.isHuman).Count;

        if (humanCount > 1)
        {
            GameManager.instance.ResetTurnOrder();

            yield return StartCoroutine(RunMultiplayerLastOrderTurns(container, stopBtn));
        }
        else
        {
            yield return StartCoroutine(RunSinglePlayerLastOrderTurn(container, stopBtn));

            yield return StartCoroutine(HandleBotLastOrders());
        }

        if (musicSource != null)
            musicSource.Stop();

        if (musicSource != null && lastOrderStopHorn != null)
            musicSource.PlayOneShot(lastOrderStopHorn);

        if (orderStopObject != null)
            orderStopObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        if (orderStopObject != null)
            orderStopObject.SetActive(false);

        HideAllStopHands();

        var firstPlayer = GameManager.instance.players[0];

        yield return StartCoroutine(
            GuessPopup.instance.StartLastOrderGuessSequence(firstPlayer.orders)
        );

        OrderManager.instance.CalculateTotals();

        yield return StartCoroutine(ResultsIntroAndPitariFlow());
    }
    #endregion

    private IEnumerator RunSinglePlayerLastOrderTurn(Transform container, StopOrderButton stopBtn)
    {
        var currentPlayer = GameManager.instance.GetCurrentPlayer();

        UIManager.instance.playerSubmitted = false;

        if (container != null)
        {
            container.gameObject.SetActive(true);

            RectTransform rect = container.GetComponent<RectTransform>();
            if (rect != null)
                rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, 0f);
        }

        if (stopBtn != null)
        {
            stopBtn.gameObject.SetActive(true);
            stopBtn.transform.SetAsLastSibling();
        }
        else
        {
            Debug.LogError("STOP BUTTON NOT FOUND");
        }

        yield return new WaitUntil(() => UIManager.instance.playerSubmitted);

        PlayStopSFX(1);

        if (menuUI != null)
            menuUI.SetActive(false);
        SetButtonContainerY(5000f);
        SetAllMenuButtonsVisible(false); // hide

        if (stopBtn != null)
            stopBtn.gameObject.SetActive(false);

        string result = $"{currentPlayer.playerName}'s orders:\n";
        foreach (var item in currentPlayer.orders)
            result += "- " + item.itemName + "\n";

        UIManager.instance.ShowMessageForSeconds(result, 4f);
        yield return new WaitForSeconds(4f);
    }

    private IEnumerator RunMultiplayerLastOrderTurns(Transform container, StopOrderButton stopBtn)
    {
        List<PlayerData> players = GameManager.instance.players;

        for (int i = 0; i < players.Count; i++)
        {
            PlayerData currentPlayer = players[i];
            if (currentPlayer == null || !currentPlayer.isHuman)
                continue;

            UIManager.instance.playerSubmitted = false;

            UIManager.instance.ShowMessageForSeconds(
                $"It's {currentPlayer.playerName}'s turn to order until you hit the target price!",
                2f
            );

            yield return new WaitForSeconds(2f);
            // 🔥 SHOW STOP BUTTON HERE
            ShowStopOrderButton();
            // 🔥 FULL RESET StopOrderButton(Clone) + all children
            if (stopOrderButtonInstance == null)
            {
                SpawnStopOrderButton();
            }

            stopBtn = stopOrderButtonInstance;

            if (stopBtn != null)
            {
                stopBtn.gameObject.SetActive(true);

                // ✅ Enable ALL children recursively
                foreach (Transform child in stopBtn.transform)
                {
                    child.gameObject.SetActive(true);

                    // If deeper hierarchy exists
                    foreach (Transform subChild in child)
                    {
                        subChild.gameObject.SetActive(true);
                    }
                }

                stopBtn.transform.SetAsLastSibling();
            }
            else
            {
                Debug.LogError("STOP BUTTON NOT FOUND");
            }

            if (menuUI != null)
                menuUI.SetActive(true);
            SetButtonContainerY(-500f);
            SetAllMenuButtonsVisible(true); // show

            if (container != null)
            {
                container.gameObject.SetActive(true);

                RectTransform rect = container.GetComponent<RectTransform>();
                if (rect != null)
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, 0f);
            }



            stopBtn = stopOrderButtonInstance;

            if (stopBtn != null)
            {
                stopBtn.gameObject.SetActive(true);
                stopBtn.transform.SetAsLastSibling();
            }
            else
            {
                Debug.LogError("STOP BUTTON NOT FOUND");
            }
            // Show StopOrderButton(Clone) only after the 2-second message
            if (stopOrderButtonInstance == null)
            {
                SpawnStopOrderButton();
            }
            yield return new WaitUntil(() => UIManager.instance.playerSubmitted);

            PlayStopSFX(i + 1);

            if (menuUI != null)
                menuUI.SetActive(false);
            SetButtonContainerY(5000f);
            SetAllMenuButtonsVisible(false); // hide


            if (stopBtn != null)
                stopBtn.gameObject.SetActive(false);

            string result = $"{currentPlayer.playerName}'s orders:\n";
            foreach (var item in currentPlayer.orders)
                result += "- " + item.itemName + "\n";

            UIManager.instance.ShowMessageForSeconds(result, 4f);
            yield return new WaitForSeconds(4f);
            UIManager.instance.HideMessage();

            if (i < players.Count - 1)
            {
                GameManager.instance.MoveToNextPlayer();
            }
        }

        isLastOrderActive = false;
    }
    IEnumerator HandleBotLastOrders()
    {
        var target = TargetManager.instance.targetPrice;

        // 🤖 Yabe
        var bot1 = GameManager.instance.players[1];
        yield return StartCoroutine(SimulateBotOrdering(bot1, "Yabe", target));

        // 🔊 STOP sound (2nd time)
        PlayStopSFX(2);

        yield return new WaitForSeconds(2f);

        // 🤖 Okamura
        var bot2 = GameManager.instance.players[2];
        yield return StartCoroutine(SimulateBotOrdering(bot2, "Okamura", target));

        // 🔊 STOP sound (3rd time)
        PlayStopSFX(3);
        yield return new WaitForSeconds(1.5f);
    }

    void StopOrderEatingSceneMusic()
    {
        if (GameManager.instance != null &&
            GameManager.instance.audioSource != null &&
            GameManager.instance.audioSource.isPlaying)
        {
            GameManager.instance.audioSource.Stop();
        }
    }

    void SetButtonContainerPosition(float yPos)
    {
        if (menuUI == null) return;

        Transform container = menuUI.transform.Find("ButtonContainer");
        if (container == null)
        {
            Debug.LogError("ButtonContainer not found!");
            return;
        }

        RectTransform rect = container.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, yPos);
        }
    }
    IEnumerator SimulateBotOrdering(PlayerData bot, string botName, int target)
    {
        int runningTotal = 0;

        List<MenuItem> menu = MenuManager.instance.menuItems;

        if (menu == null || menu.Count == 0)
            yield break;

        // 🎯 Weakness settings
        float badChoiceChance = 0.05f;     // chance to pick random bad item
        float earlyStopChance = 0.05f;     // chance to stop too early
        float overshootChance = 0.25f;     // chance to go over target

        int safety = 0;

        while (safety < 20)
        {
            safety++;

            int remaining = target - runningTotal;

            // 🛑 Sometimes bots just stop early (even if not close)
            if (Random.value < earlyStopChance && runningTotal > 0)
                break;

            // 🛑 Exact hit
            if (remaining == 0)
                break;

            List<MenuItem> candidates = menu
                .OrderBy(item => Mathf.Abs(remaining - item.realPrice))
                .ToList();

            if (candidates.Count == 0)
                break;

            MenuItem chosenItem;

            // 🎲 BAD CHOICE: pick random item (not optimal)
            if (Random.value < badChoiceChance)
                chosenItem = menu[Random.Range(0, menu.Count)];
            else
            {
                int pickRange = Mathf.Min(5, candidates.Count);
                chosenItem = candidates[Random.Range(0, pickRange)];
            }

            // 🛑 Prevent infinite loop
            if (runningTotal + chosenItem.realPrice == runningTotal)
                break;

            int newTotal = runningTotal + chosenItem.realPrice;
            int newRemaining = target - newTotal;

            // 🛑 Sometimes avoid going over
            if (newTotal > target && Random.value > overshootChance)
                break;

            bot.orders.Add(chosenItem);
            bot.lastOrders.Add(chosenItem);
            runningTotal = newTotal;

            // 🛑 Stop if getting worse (but not always)
            if (Mathf.Abs(newRemaining) > Mathf.Abs(remaining))
            {
                if (Random.value < 0.8f) // usually stop, sometimes keep going badly
                    break;
            }

            yield return new WaitForSeconds(0.25f);
        }

        // 📢 Show result
        string result = botName + " ordered:\n";
        foreach (var item in bot.orders)
            result += "- " + item.itemName + "\n";

        UIManager.instance.ShowMessageForSeconds(result, 3f);
        yield return new WaitForSeconds(1.5f);
    }

    void PlayStopSFX(int handIndex = 0)
    {
        if (musicSource != null && lastOrderStopSFX != null)
            musicSource.PlayOneShot(lastOrderStopSFX);

        switch (handIndex)
        {
            case 1:
                StartCoroutine(ShowStopHandSequence(Stop_HandPlayer1, 0.5f, 0f));
                break;
            case 2:
                StartCoroutine(ShowStopHandSequence(Stop_HandPlayer2, 0.65f, 0.15f));
                break;
            case 3:
                StartCoroutine(ShowStopHandSequence(Stop_HandPlayer3, 0.85f, 0.35f));
                break;
        }
    }

    void ShowStopOrderButton()
    {
        if (stopOrderButtonInstance == null)
        {
            Debug.LogError("StopOrderButton is NULL");
            return;
        }

        stopOrderButtonInstance.gameObject.SetActive(true);
        stopOrderButtonInstance.transform.SetAsLastSibling();
    }

    #region Guess Phase
    public void StartGuessPhase()
    {
        StartCoroutine(GuessRoutine());
    }

    IEnumerator GuessRoutine()
    {
        // 🔴 HARD STOP any leftover eating music
        StopOrderEatingSceneMusic();
        // 🔴 HIDE menu immediately when entering Guess Phase
        HideMenuUIForGuessPhase();

        UIManager.instance.ShowMessage("Guess Phase");

        if (musicSource != null && GameManager.instance.orderEatingSceneMusic != null)
        {
            musicSource.clip = GameManager.instance.orderEatingSceneMusic;
            musicSource.loop = false;
            musicSource.volume = 1f;
            musicSource.Play();

            yield return new WaitWhile(() => musicSource.isPlaying);
        }

        // 🔥 Initialize guess count BEFORE guessing starts
        GuessSystem.instance.InitializeGuessCount();

        if (GuessPopup.instance != null)
        {
            var firstOrderItem = GameManager.instance.players[0].orders[0];
            GuessPopup.instance.OpenGuess(firstOrderItem);
        }

        // ❌ REMOVE direct state change — GuessSystem handles it now
        yield break;
    }
    #endregion

    #region Music Fade
    public IEnumerator FadeOutMusic(AudioSource source, float duration)
    {
        if (source == null) yield break;

        float startVolume = source.volume;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        source.Stop();
        source.volume = startVolume;
    }
    #endregion

    // Add inside RoundManager class

    #region Character Animations
    void StartCharacterAnimations()
    {
        StartCoroutine(StartCharacterAnimationsWithDelay());
    }

    IEnumerator StartCharacterAnimationsWithDelay()
    {
        yield return null;
        TriggerCharacterAnimations();
    }

    void TriggerCharacterAnimations()
    {
        if (animatorOkamura != null)
            StartCoroutine(PlayAndFreeze(animatorOkamura, "SitDownOkamura", 0.7f));

        if (animatorYabe != null)
            StartCoroutine(PlayAndFreeze(animatorYabe, "SitDownYabe", 0.7f));

        if (animatorYou != null)
            StartCoroutine(PlayAndFreeze(animatorYou, "SitDownYou", 0.7f));
    }

    IEnumerator PlayAndFreeze(Animator animator, string stateName, float normalizedTime)
    {
        animator.speed = 1f;
        animator.Play(stateName, 0, 0f);
        yield return null;
        animator.Play(stateName, 0, normalizedTime);
        animator.speed = 0f;
    }

    IEnumerator PlayAnimationOnce(Animator animator, string stateName)
    {
        if (animator == null || string.IsNullOrEmpty(stateName))
            yield break;

        animator.speed = 1f;
        animator.Play(stateName, 0, 0f);
        yield return null;

        float waitTime = 0f;
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);

        if (info.IsName(stateName) && info.length > 0f)
            waitTime = info.length;
        else
            waitTime = 1f;

        yield return new WaitForSeconds(waitTime);

        animator.speed = 0f;
        animator.Play(stateName, 0, 1f);
    }

    void PlayLoopingAnimation(Animator animator, string stateName)
    {
        if (animator == null || string.IsNullOrEmpty(stateName))
            return;

        animator.speed = 1f;
        animator.Play(stateName, 0, 0f);
    }

    Animator GetAnimatorForPlayer(PlayerData player)
    {
        int index = GetPlayerIndex(player);

        switch (index)
        {
            case 0: return animatorYou;
            case 1: return animatorYabe;
            case 2: return animatorOkamura;
            default: return null;
        }
    }

    string GetStandUpAnimation(PlayerData player)
    {
        int index = GetPlayerIndex(player);

        switch (index)
        {
            case 0: return "StandingYou";
            case 1: return "StandUpYabe";
            case 2: return "StandUpOkamura";
            default: return null;
        }
    }


    string GetRaiseHandsAnimation(PlayerData player)
    {
        int index = GetPlayerIndex(player);

        switch (index)
        {
            case 0: return "RaiseBothHandsYou";
            case 1: return "RaiseBothHandsYabe";
            case 2: return "RaiseBothHandsOkamura";
            default: return null;
        }
    }

    string GetLastPlaceAnimation(PlayerData player)
    {
        int index = GetPlayerIndex(player);

        switch (index)
        {
            case 0: return "LastPlaceYou";
            case 1: return "LastPlaceYabe";
            case 2: return "LastPlaceOkamura";
            default: return null;
        }
    }

    string GetPitariWinAnimation(PlayerData player)
    {
        int index = GetPlayerIndex(player);

        switch (index)
        {
            case 0: return "PitariWonYou";
            case 1: return "PitariWonYabe";
            case 2: return "PitariWonOkamura";
            default: return null;
        }
    }

    string GetIdleAnimation(PlayerData player)
    {
        int index = GetPlayerIndex(player);

        switch (index)
        {
            case 0: return "IdleYou";
            case 1: return "IdleYabe";
            case 2: return "IdleOkamura";
            default: return null;
        }
    }

    string GetSpeakAnimation(PlayerData player)
    {
        int index = GetPlayerIndex(player);

        switch (index)
        {
            case 0: return "SpeakYou";
            case 1: return "SpeakYabe";
            case 2: return "SpeakOkamura";
            default: return null;
        }
    }

    IEnumerator PlayAnimationThenIdle(Animator animator, string stateName, string idleState)
    {
        if (animator == null || string.IsNullOrEmpty(stateName) || string.IsNullOrEmpty(idleState))
            yield break;

        animator.speed = 1f;
        animator.Play(stateName, 0, 0f);

        yield return null;

        AnimationClip clip = GetAnimationClip(animator, stateName);
        if (clip == null)
        {
            Debug.LogWarning("Animation clip not found: " + stateName);
            yield break;
        }

        yield return new WaitForSeconds(clip.length);

        // Transition to Idle
        animator.Play(idleState, 0, 0f);
    }

    IEnumerator PlayPitariThenRaiseHands(Animator animator, string pitariAnim, string raiseAnim)
    {
        if (animator == null || string.IsNullOrEmpty(pitariAnim) || string.IsNullOrEmpty(raiseAnim))
            yield break;

        animator.speed = 1f;
        animator.Play(pitariAnim, 0, 0f);

        yield return null;

        AnimationClip pitariClip = GetAnimationClip(animator, pitariAnim);
        if (pitariClip == null)
        {
            Debug.LogWarning("Pitari clip not found: " + pitariAnim);
            yield break;
        }

        // Wait until Pitari animation finishes
        yield return new WaitForSeconds(pitariClip.length);

        // 👉 Transition to RaiseBothHands
        animator.Play(raiseAnim, 0, 0f);

        yield return null;

        AnimationClip raiseClip = GetAnimationClip(animator, raiseAnim);
        if (raiseClip == null)
        {
            Debug.LogWarning("RaiseHands clip not found: " + raiseAnim);
            yield break;
        }

        // Let it play fully
        yield return new WaitForSeconds(raiseClip.length);

        // 👉 HOLD last frame (no looping)
        animator.Play(raiseAnim, 0, 1f);
        animator.Update(0f);
    }
    void AnimateStandUp(PlayerData player)
    {
        Animator animator = GetAnimatorForPlayer(player);
        string standAnim = GetStandUpAnimation(player);
        string idleAnim = GetIdleAnimation(player);

        if (animator != null && !string.IsNullOrEmpty(standAnim))
        {
            StartCoroutine(PlayAnimationThenIdle(animator, standAnim, idleAnim));
        }
    }
    void AnimateLastPlaceSad(PlayerData player)
    {
        Animator animator = GetAnimatorForPlayer(player);
        string animName = GetLastPlaceAnimation(player);

        if (animator != null && !string.IsNullOrEmpty(animName))
            StartCoroutine(PlayAnimationOnce(animator, animName));
    }

    void StopFreezeForPlayer(PlayerData player)
    {
        Animator animator = GetAnimatorForPlayer(player);
        if (animator == null)
            return;

        animator.speed = 1f;
    }

    void AnimatePitariWinner(PlayerData player)
    {
        Animator animator = GetAnimatorForPlayer(player);
        string pitariAnim = GetPitariWinAnimation(player);
        string raiseAnim = GetRaiseHandsAnimation(player);

        if (animator != null &&
            !string.IsNullOrEmpty(pitariAnim) &&
            !string.IsNullOrEmpty(raiseAnim))
        {
            StartCoroutine(PlayPitariThenRaiseHands(animator, pitariAnim, raiseAnim));
        }
    }

    void AnimateIdle(PlayerData player)
    {
        Animator animator = GetAnimatorForPlayer(player);
        string animName = GetIdleAnimation(player);

        if (animator != null && !string.IsNullOrEmpty(animName))
            StartCoroutine(PlayAnimationOnce(animator, animName));
    }

    void StartSpeakingForNonLastPlace(PlayerData lastPlace)
    {
        foreach (var p in GameManager.instance.players)
        {
            if (p == lastPlace) continue;

            Animator animator = GetAnimatorForPlayer(p);
            string animName = GetSpeakAnimation(p);
            PlayLoopingAnimation(animator, animName);
        }
    }

    void StopSpeakingAndGoIdleForNonLastPlace(PlayerData lastPlace)
    {
        foreach (var p in GameManager.instance.players)
        {
            if (p == lastPlace) continue;

            AnimateIdle(p);
        }
    }

    AnimationClip GetAnimationClip(Animator animator, string clipName)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
            return null;

        foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip != null && clip.name == clipName)
                return clip;
        }

        return null;
    }

    IEnumerator PlayAnimationOnceAndFreeze(Animator animator, string stateName)
    {
        if (animator == null || string.IsNullOrEmpty(stateName))
            yield break;

        animator.speed = 1f;
        animator.Play(stateName, 0, 0f);

        yield return null;

        AnimationClip clip = GetAnimationClip(animator, stateName);

        if (clip == null)
        {
            Debug.LogWarning("Animation clip not found: " + stateName);
            yield break;
        }

        float frameTime = (clip.frameRate > 0f) ? (1f / clip.frameRate) : 0.0167f;
        float waitTime = Mathf.Max(0f, clip.length - frameTime);

        yield return new WaitForSeconds(waitTime);

        // Freeze exactly on the last frame
        animator.Play(stateName, 0, 1f);
        animator.Update(0f);
        animator.speed = 0f;
    }
    IEnumerator PlayAnimationOnceAndHoldLastFrame(Animator animator, string stateName)
    {
        if (animator == null || string.IsNullOrEmpty(stateName))
            yield break;

        animator.speed = 1f;
        animator.Play(stateName, 0, 0f);

        yield return null;

        AnimationClip clip = GetAnimationClip(animator, stateName);
        if (clip == null)
        {
            Debug.LogWarning("Animation clip not found: " + stateName);
            yield break;
        }

        yield return new WaitForSeconds(clip.length);

        // Snap to last frame, but do NOT freeze animator speed
        animator.Play(stateName, 0, 1f);
        animator.Update(0f);
    }
    #endregion

    IEnumerator ResultsIntroAndPitariFlow()
    {
        // 🔊 1. PLAY "RESULTS!" AUDIO
        if (musicSource != null && resultsIntroSFX != null)
        {
            ShowResultsSprite(ResultsText_3, 0f, -4.5f);
            musicSource.PlayOneShot(resultsIntroSFX);
            yield return new WaitForSeconds(resultsIntroSFX.length);
            HideResultsSprite();
        }

        // 📝 2. MESSAGE (3 seconds)
        UIManager.instance.ShowMessageForSeconds(
            "Results are in! But first, it's pitari award!",
            3f
        );

        yield return new WaitForSeconds(3f);

        // 🔊 3. PLAY PITARI CHANCE MUSIC
        if (musicSource != null && pitariChanceMusic != null)
        {
            musicSource.clip = pitariChanceMusic;
            musicSource.loop = false;
            musicSource.Play();
        }

        int target = TargetManager.instance.targetPrice;

        yield return new WaitForSeconds(2f);
        ShowResultsSprite(ResultsText_4, 0f, -4.5f);

        // 🎲 4. FIRST DIALOGUE (7 sec)
        string line1 = GetRandomDialogue(pitariIntroLines)
            .Replace("(target price amount)", target.ToString());

        UIManager.instance.SetMessage(line1);
        yield return new WaitForSeconds(7f);

        // 🎲 5. SECOND DIALOGUE (until music ends)
        string line2 = GetRandomDialogue(pitariHypeLines);
        UIManager.instance.SetMessage(line2);

        if (musicSource != null)
            yield return new WaitWhile(() => musicSource.isPlaying);

        UIManager.instance.HideMessage();

        // 🧠 6. CHECK PITARI
        bool hasPerfect = false;

        foreach (var p in GameManager.instance.players)
        {
            if (p.totalActual == target)
            {
                hasPerfect = true;
                break;
            }
        }

        // ✅ PITARI ELIGIBLE
        if (hasPerfect)
        {
            ShowResultsSprite(ResultsText_5, 0f, -4.5f);

            if (musicSource != null && pitariEligibleSFX != null)
            {
                musicSource.PlayOneShot(pitariEligibleSFX);
            }
            PlayRandomApplause();
            UIManager.instance.ShowMessageForSeconds(
                "We have a PITARI AWARD contender!!!",
                3f
            );

            yield return new WaitForSeconds(3f);
        }
        else
        {
            ShowResultsSprite(ResultsText_6, 0f, -4.5f);

            if (musicSource != null && noPitariSFX != null)
            {
                musicSource.PlayOneShot(noPitariSFX);
            }

            UIManager.instance.ShowMessageForSeconds(
                "No one got the Pitari Award...",
                3f
            );

            yield return new WaitForSeconds(3f);
        }

        yield return StartCoroutine(FullResultsSequence());
        HideResultsSprite();
    }

    IEnumerator FullResultsSequence()
    {

        int target = TargetManager.instance.targetPrice;
        var playerDifferences = GameManager.instance.players
    .Select(p => new { Player = p, Diff = p.totalActual - TargetManager.instance.targetPrice })
    .ToList();
        var rankedPlayers = playerDifferences
            .OrderBy(pd => Mathf.Abs(pd.Diff))
            .ThenBy(pd => pd.Diff < 0 ? 1 : 0)
            .Select(pd => pd.Player)
            .ToList();

        // Later
        var rankedByTarget = GameManager.instance.players
            .OrderBy(p => Mathf.Abs(p.totalActual - target))
            .ToList();

        // 🟡 PITARI CHECK
        bool hasPerfect = GameManager.instance.players
            .Any(p => p.totalActual == target);
        OrderManager.instance.CalculateTotals();
        // 🎵 WAIT until pitari music fully ends
        yield return new WaitWhile(() => musicSource.isPlaying);

        // 🟡 SHOW NEAR PIN INTRO
        HideResultsSprite();
        UIManager.instance.ShowMessageForSeconds(
            "The near pin award goes to anyone within 500 yen of the target! Only one near pin player is awarded!",
            3f
        );
        yield return new WaitForSeconds(3f);

        var ranked = GameManager.instance.players
            .OrderBy(p => Mathf.Abs(p.totalActual - target))
            .ToList();

        if (!hasPerfect)
            yield return StartCoroutine(HandleNoPitari(ranked, target));
        else
            yield return StartCoroutine(HandleWithPitari(ranked, target));

        HandleRoundTransition();
    }

    IEnumerator HandleNoPitari(List<PlayerData> ranked, int target)
    {
        ShowResultsSprite(ResultsText_7, -6.5f, -5.5f);

        PlayerData first = ranked[0];

        yield return ShowAmount(first);

        bool nearPin = Mathf.Abs(first.totalActual - target) <= 510;
        if (nearPin)
        {
            ShowNearPinAward();
        }
        if (nearPin)
            yield return PlaySFXAndWait("NearPinEligible");

        yield return new WaitForSeconds(2f);

        yield return PlaySFXAndWait("ResultsPlayerGoingToReveal");

        UIManager.instance.SetMessage(first.playerName + " came in FIRST PLACE!!");
        StopFreezeForPlayer(first);
        AnimateStandUp(first);
        PlayRandomApplause();

        yield return new WaitForSeconds(1f);

        if (nearPin)
        {
            yield return PlaySFXAndWait("NearPinWinnerResults");
        }
        else
        {
            yield return PlaySFXAndWait("FirstPlaceResults");
        }

        UIManager.instance.HideMessage();

        if (nearPin)
        {
            int reward = Random.value < 0.5f ? 1000 : 50000;
            UIManager.instance.SetMessage(first.playerName + " won " + reward + " yen!!");
            if (reward == 1000)
                yield return PlaySFXAndWait("NearPin1000Yen");
            else
                yield return PlaySFXAndWait("NearPin50000Yen");

            OutOfPocketManager.instance.ApplyReward(first, reward);
        }
        HideResultsSprite();
        HideNearPinAward(); // ✅ add this
        yield return StartCoroutine(HandleLastPlaceFlow(ranked));

    }
    IEnumerator HandleWithPitari(List<PlayerData> ranked, int target)
    {
        ShowResultsSprite(ResultsText_8, -6.5f, -5.5f);

        PlayerData second = ranked[1];

        yield return ShowAmount(second);

        bool nearPin = Mathf.Abs(second.totalActual - target) <= 510;
        if (nearPin)
        {
            ShowNearPinAward();
        }
        if (nearPin)
            yield return PlaySFXAndWait("NearPinEligible");

        yield return new WaitForSeconds(2f);

        yield return PlaySFXAndWait(resultsPlayerReveal);

        UIManager.instance.SetMessage(second.playerName + " came in SECOND PLACE!!");
        StopFreezeForPlayer(second);
        AnimateStandUp(second);
        PlayRandomApplause();

        yield return new WaitForSeconds(1f);

        if (nearPin)
            yield return PlaySFXAndWait("NearPinWinnerResults");
        else
            yield return PlaySFXAndWait("GoodOrderResults");

        UIManager.instance.HideMessage();

        if (nearPin)
        {
            int reward = Random.value < 0.5f ? 1000 : 50000;
            UIManager.instance.SetMessage(second.playerName + " won " + reward + " yen!!");
            if (reward == 1000)
                yield return PlaySFXAndWait("NearPin1000Yen");
            else
                yield return PlaySFXAndWait("NearPin50000Yen");

            OutOfPocketManager.instance.ApplyReward(second, reward);
        }

        PlayerData loser = ranked
            .OrderByDescending(p => Mathf.Abs(p.totalActual - target))
            .First();
        HideResultsSprite();
        HideNearPinAward(); // ✅ add this
        yield return StartCoroutine(HandlePitariDroneFlow(ranked, loser));

    }

    public IEnumerator HandleLastPlaceFlow(List<PlayerData> ranked)
    {
        int target = TargetManager.instance.targetPrice;

        PlaySFX("LastPlaceResultsFate");
        UIManager.instance.SetMessage("Who's going to be the last place player? If the drone lands on you, you'll be in last place!");
        yield return new WaitForSeconds(12.5f);
        UIManager.instance.HideMessage();

        PlaySFX("LastPlaceTimpaniRoll");
        SetDroneMoving();

        PlayerData droneA = null;
        PlayerData droneB = null;
        PlayerData loser = null;

        PlayerData rankedSecond = ranked[1];
        PlayerData rankedLast = ranked[^1];

        float firstDist = Mathf.Abs(ranked[0].totalActual - target);
        float secondDist = Mathf.Abs(ranked[1].totalActual - target);
        float lastDist = Mathf.Abs(ranked[^1].totalActual - target);

        bool secondAndLastTied = Mathf.Approximately(secondDist, lastDist);

        if (secondAndLastTied)
        {
            // Covers:
            // - normal 2-way tie for last
            // - 3-way tie where ranked[1] and ranked[2] are the two suspense players
            droneA = rankedSecond;
            droneB = rankedLast;

            // Randomize first 0.5 lap target
            if (Random.value < 0.5f)
            {
                PlayerData temp = droneA;
                droneA = droneB;
                droneB = temp;
            }

            // Random final loser between the two suspense players only
            loser = (Random.value < 0.5f) ? rankedSecond : rankedLast;
        }
        else
        {
            // Normal case: unique last place
            PlayerData first = ranked[0];

            var tiedSecond = ranked
                .Where(p => Mathf.Approximately(Mathf.Abs(p.totalActual - target), secondDist))
                .ToList();

            tiedSecond.Remove(first);

            PlayerData secondPlayer = tiedSecond[Random.Range(0, tiedSecond.Count)];
            PlayerData lastPlayer = rankedLast;

            // First 0.5 lap randomly goes to last or non-last
            if (Random.value < 0.5f)
            {
                droneA = lastPlayer;
                droneB = secondPlayer;
            }
            else
            {
                droneA = secondPlayer;
                droneB = lastPlayer;
            }

            loser = lastPlayer;
        }

        yield return StartCoroutine(
            GlideDroneToPlayers(droneA, droneB, loser, false)
        );

        if (timpani != null && timpani.isPlaying)
            timpani.Stop();

        SetDroneIdle();
        PlaySFX("LastPlaceCanon");
        AnimateLastPlaceSad(loser);
        yield return new WaitForSeconds(1f);

        PlaySFX("LastPlaceRevealed");
        UIManager.instance.SetMessage("Last place: " + loser.playerName + "!");
        yield return new WaitForSeconds(1f);

        yield return PlaySFXAndWait("LastPlaceMusic");
        yield return ShowFinalBreakdown(ranked, loser);
    }
    IEnumerator ShowFinalBreakdown(List<PlayerData> ranked, PlayerData loser)
    {
        yield return new WaitForSeconds(2f);
        OrderManager.instance.CalculateTotals();
        int target = TargetManager.instance.targetPrice;

        ranked = ranked
            .OrderBy(p => Mathf.Abs(p.totalActual - target))
            .ThenBy(p => p.totalActual > target)
            .ToList();

        PlayerData firstPlace = ranked[0];
        PlayerData secondPlace = ranked.Count > 1 ? ranked[1] : null;
        PlayerData lastPlace = loser;

        bool firstPlaceHitPitari = firstPlace.totalActual == TargetManager.instance.targetPrice;

        if (!firstPlaceHitPitari && secondPlace != null)
        {
            ShowResultsSprite(ResultsText_8, -6.5f, -5.5f);
            yield return ShowAmount(secondPlace, SecondLastPlaceResultsAmountShow);
        }

        if (lastPlace != null)
        {
            ShowResultsSprite(ResultsText_9, -6.5f, -5.5f);
            yield return ShowAmount(lastPlace, LastPlaceResultsAmountShow);
        }

        HideResultsSprite();

        int mealTotal = ranked.Sum(p => p.totalActual);
        UIManager.instance.SetMessage("Total Meal Fee: " + mealTotal + " yen");
        PlaySFX("MealFeeShown");
        yield return new WaitForSeconds(3f);
        UIManager.instance.SetMessage("This time, GOCHI is on " + loser.playerName + "!!");
        switch (GetPlayerIndex(lastPlace))
        {
            case 0:
                yield return PlaySFXAndWait("YouLastPlace");
                break;
            case 1:
                yield return PlaySFXAndWait("YabeLastPlace");
                break;
            case 2:
                yield return PlaySFXAndWait("OkamuraLastPlace");
                break;
        }
        StartSpeakingForNonLastPlace(lastPlace);
        yield return PlaySFXAndWait("GochiShoutTogether");
        StopSpeakingAndGoIdleForNonLastPlace(lastPlace);
        Vector4 startPos = new Vector4(-17.5f, 3.1f, lastPlaceDrone.transform.position.z);

        yield return StartCoroutine(ResultManager.instance.ShowPlayerTotals());
    }

    void ResetAnimatorToIdle(Animator animator, string idleState)
    {
        if (animator == null || string.IsNullOrEmpty(idleState))
            return;

        StopCoroutineSafe(animator);
        animator.speed = 1f;
        animator.Play(idleState, 0, 0f);
        animator.Update(0f);
    }

    void StopCoroutineSafe(Animator animator)
    {
        // placeholder in case you later track animation coroutines per animator
        // right now this just ensures speed is restored
        if (animator != null)
            animator.speed = 1f;
    }

    void ResetAllCharacterAnimations()
    {
        ResetAnimatorToIdle(animatorYou, "IdleYou");
        ResetAnimatorToIdle(animatorYabe, "IdleYabe");
        ResetAnimatorToIdle(animatorOkamura, "IdleOkamura");
    }
    void HandleRoundTransition()
    {
        if (GameManager.instance.currentRound < GameManager.instance.maxRounds)
        {
            GameManager.instance.currentRound++;
            GuessPopup.instance.ResetFirstOrderGuessesFlag();
            GuessSystem.instance.firstOrderGuessesShown = true;
            GuessSystem.instance.isLastOrderGuessPhase = false;
            GuessSystem.instance.guessesSubmitted = 0;

            Vector3 startPos = new Vector3(-17.5f, 3.1f, lastPlaceDrone.transform.position.z);
            lastPlaceDrone.transform.position = startPos;

            foreach (var p in GameManager.instance.players)
            {
                p.orders.Clear();
                p.lastOrders.Clear();
                p.totalActual = 0;

                foreach (var item in MenuManager.instance.menuItems)
                {
                    item.guessedPrice = 0;
                }
            }

            SpawnStopOrderButton();
            stopOrderButtonInstance.gameObject.SetActive(false);

            if (stopOrderButtonInstance == null)
            {
                GameObject stopOrder = Instantiate(stopOrderPrefab, stopOrderParent);
                stopOrderButtonInstance = stopOrder.GetComponent<StopOrderButton>();
            }

            stopOrderButtonInstance.ResetButton();
            stopOrderButtonInstance.gameObject.SetActive(false);

            // ✅ reset all character animations before target reveal
            ResetAllCharacterAnimations();


            GameManager.instance.ChangeState(GameState.TargetReveal);

            // ✅ restart sit-down intro for new round
            StartCoroutine(StartCharacterAnimationsWithDelay());
        }
        else
        {
            Vector3 startPos = new Vector3(-17.5f, 3.1f, lastPlaceDrone.transform.position.z);
            lastPlaceDrone.transform.position = startPos;
            GameManager.instance.ChangeState(GameState.FinalResults);
            PlayRandomApplause();
        }
    }

    IEnumerator PlaySFXAndWait(AudioClip clip)
    {
        if (clip != null && musicSource != null)
        {
            musicSource.PlayOneShot(clip);
            yield return new WaitForSeconds(clip.length);
        }
    }

    IEnumerator PlaySFXAndWait(string sfxName)
    {
        AudioClip clip = SFXLibrary.instance.GetClip(sfxName);

        if (clip != null && musicSource != null)
        {
            musicSource.PlayOneShot(clip);
            yield return new WaitForSeconds(clip.length);
        }
    }

    void PlaySFX(AudioClip clip)
    {
        if (clip != null && musicSource != null)
        {
            musicSource.PlayOneShot(clip);
        }
    }

    void PlaySFX(string sfxName)
    {
        AudioClip clip = SFXLibrary.instance.GetClip(sfxName);
        if (clip != null && musicSource != null)
            musicSource.PlayOneShot(clip);
    }

    IEnumerator GlideDroneToPlayers(PlayerData playerA, PlayerData playerB, PlayerData finalLandingPlayer, bool isPitariFlow = false)
    {
        if (lastPlaceDrone == null)
        {
            Debug.LogError("LastPlaceDrone not assigned!");
            yield break;
        }

        if (playerA == null || playerB == null)
        {
            Debug.LogError("Drone players are null!");
            yield break;
        }

        if (playerA == playerB)
        {
            Debug.LogWarning("Drone received SAME player twice! Skipping back-and-forth.");
        }

        float topY = 3.1f;
        float dipY = -0.65f;

        Vector3 startPos = new Vector3(-17.5f, topY, lastPlaceDrone.transform.position.z);
        lastPlaceDrone.transform.position = startPos;

        float GetX(PlayerData p)
        {
            switch (GetPlayerIndex(p))
            {
                case 0: return -6.8f; // You slot
                case 1: return 0f;    // Yabe slot
                case 2: return 7f;    // Okamura slot
                default: return 0f;
            }
        }

        Vector3 posA = new Vector3(GetX(playerA), topY, startPos.z);
        Vector3 posB = new Vector3(GetX(playerB), topY, startPos.z);

        if (Mathf.Approximately(posA.x, posB.x) && playerA != playerB)
        {
            Debug.LogWarning("Drone positions are identical! Adding small variation.");
            posB.x += 0.5f;
        }

        int laps = Random.Range(2, 4);

        IEnumerator MoveHalfLapWithPossibleDip(Vector3 targetPos, bool allowDip = true)
        {
            yield return MoveDroneSmooth(lastPlaceDrone, targetPos, Random.Range(2f, 3f));

            if (allowDip && Random.value < 0.4f)
            {
                Vector3 downPos = new Vector3(
                    lastPlaceDrone.transform.position.x,
                    dipY,
                    lastPlaceDrone.transform.position.z
                );

                Vector3 upPos = new Vector3(
                    lastPlaceDrone.transform.position.x,
                    topY,
                    lastPlaceDrone.transform.position.z
                );

                yield return MoveDroneSmooth(lastPlaceDrone, downPos, 1.5f);
                yield return MoveDroneSmooth(lastPlaceDrone, upPos, 0.5f);
            }
        }

        Vector3 lastPos = (finalLandingPlayer == playerA) ? posA : posB;
        Vector3 otherPos = (finalLandingPlayer == playerA) ? posB : posA;

        // Randomize first 0.5 lap
        bool goToLastFirst = Random.value < 0.5f;

        // Do all full back-and-forth laps EXCEPT the final suspense half-lap
        for (int i = 0; i < laps; i++)
        {
            bool isFirstCycle = (i == 0);
            bool isLastCycle = (i == laps - 1);

            if (isFirstCycle)
            {
                if (goToLastFirst)
                {
                    yield return StartCoroutine(MoveHalfLapWithPossibleDip(lastPos, true));

                    // On the very last cycle, don't do the second move here
                    if (!isLastCycle)
                        yield return StartCoroutine(MoveHalfLapWithPossibleDip(otherPos, true));
                }
                else
                {
                    yield return StartCoroutine(MoveHalfLapWithPossibleDip(otherPos, true));

                    // On the very last cycle, don't do the second move here
                    if (!isLastCycle)
                        yield return StartCoroutine(MoveHalfLapWithPossibleDip(lastPos, true));
                }
            }
            else
            {
                yield return StartCoroutine(MoveHalfLapWithPossibleDip(posA, true));

                if (!isLastCycle)
                    yield return StartCoroutine(MoveHalfLapWithPossibleDip(posB, true));
            }
        }

        // FINAL suspense half-lap: always end above the final landing player, no dip
        yield return StartCoroutine(MoveHalfLapWithPossibleDip(lastPos, false));

        // Final vertical landing
        Vector3 finalPos = new Vector3(GetX(finalLandingPlayer), -0.5f, startPos.z);
        yield return MoveDroneSmooth(lastPlaceDrone, finalPos, 1.5f);

        if (timpani != null && timpani.isPlaying)
            timpani.Stop();
    }

    IEnumerator MoveDroneSmooth(GameObject droneObj, Vector3 targetPos, float duration)
    {
        Vector3 startPos = droneObj.transform.position;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            droneObj.transform.position = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0, 1, t));
            yield return null;
        }
        droneObj.transform.position = targetPos;
    }
    IEnumerator ShowAmount(PlayerData player, AudioClip sfx = null, bool showName = true)
    {
        string message = showName ? player.totalActual + " yen"
                                  : player.totalActual + " yen";

        UIManager.instance.SetMessage(message);

        if (sfx != null && musicSource != null)
            musicSource.PlayOneShot(sfx);
        else if (ResultsAmountShowed != null && musicSource != null)
            musicSource.PlayOneShot(ResultsAmountShowed);

        yield return new WaitForSeconds(2f);
    }
    IEnumerator HandlePitariDroneFlow(List<PlayerData> ranked, PlayerData loser)
    {
        int target = TargetManager.instance.targetPrice;

        PlaySFX("LastPlaceResultsFate");
        UIManager.instance.SetMessage("Who's going to win the pitari award?!?");
        yield return new WaitForSeconds(12.5f);
        UIManager.instance.HideMessage();

        PlayerData winner = ranked.First(p => p.totalActual == target);

        PlaySFX("LastPlaceTimpaniRoll");
        SetDroneMoving();
        yield return StartCoroutine(
            GlideDroneToPlayers(winner, loser, loser, true)
        );
        SetDroneIdle();
        PlaySFX("LastPlaceCanon");
        AnimateLastPlaceSad(loser);
        yield return new WaitForSeconds(1f);

        PlaySFX("LastPlaceRevealed");
        UIManager.instance.SetMessage("Last place: " + loser.playerName + "!");
        yield return new WaitForSeconds(5f);

        OutOfPocketManager.instance.ApplyPitariWin(winner);

        ShowResultsSprite(ResultsText_11, 0f, -4.5f);
        UIManager.instance.SetMessage("PITARI WINNER: " + winner.playerName + "!!!");
        AnimatePitariWinner(winner);
        PlayRandomApplause();
        yield return new WaitForSeconds(0.5f);

        yield return PlaySFXAndWait(pitariWin);
        AnimateIdle(winner);
        yield return StartCoroutine(ShowFinalBreakdown(ranked, loser));
    }
    public void StartNextRound()
    {
        if (GameManager.instance.currentState != GameState.ResultsReveal)
        {
            Debug.LogWarning("Cannot start next round until results are fully revealed!");
            return;
        }

        GameManager.instance.currentRound++;
        if (GameManager.instance.currentRound > GameManager.instance.maxRounds)
        {
            Debug.Log("Game finished!");
            return;
        }

        GameManager.instance.ChangeState(GameState.FirstOrder);
    }
    void PlayRandomApplause()
    {
        string[] applause = { "Applause1", "Applause2", "Applause3" };
        PlaySFX(applause[Random.Range(0, applause.Length)]);
    }



    #region Menu Buttons
    void SpawnStopOrderButton()
    {
        if (menuUI == null)
        {
            Debug.LogError("menuUI is null!");
            return;
        }

        Transform container = menuUI.transform.Find("ButtonContainer");
        if (container == null)
        {
            Debug.LogError("ButtonContainer not found!");
            return;
        }

        // Already exists? Just re-parent and refresh order.
        if (stopOrderButtonInstance != null)
        {
            stopOrderButtonInstance.transform.SetParent(container, false);
            stopOrderButtonInstance.transform.SetAsLastSibling();
            return;
        }

        GameObject stopObj = Instantiate(stopOrderButtonPrefab, container, false);
        stopObj.name = "StopOrder";

        stopOrderButtonInstance = stopObj.GetComponent<StopOrderButton>();

        if (stopOrderButtonInstance == null)
        {
            Debug.LogError("Spawned StopOrder is missing StopOrderButton component!");
            return;
        }

        // Match menu button container behavior
        RectTransform rect = stopObj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.localScale = Vector3.one;
            rect.anchoredPosition3D = Vector3.zero;
            rect.offsetMin = rect.offsetMin;
            rect.offsetMax = rect.offsetMax;
        }

        stopOrderButtonInstance.buttonContainer = container.gameObject;
        stopOrderButtonInstance.transform.SetAsLastSibling();
        stopOrderButtonInstance.gameObject.SetActive(false);
    }
    void PopulateMenuButtons()
    {
        Transform container = menuUI.transform.Find("ButtonContainer");
        if (container == null)
        {
            Debug.LogError("ButtonContainer not found!");
            return;
        }

        // Keep StopOrderButton, destroy normal menu buttons
        foreach (Transform child in container)
        {
            if (child.GetComponent<StopOrderButton>() == null)
                Destroy(child.gameObject);
        }

        // Show only 1 menu button
        if (MenuManager.instance.menuItems.Count > 0)
        {
            MenuItem item = MenuManager.instance.menuItems[0];

            GameObject btn = Instantiate(menuButtonPrefab, container);

            MenuButton menuButton = btn.GetComponent<MenuButton>();
            if (menuButton != null)
            {
                menuButton.item = item;
                menuButton.MoveButton(new Vector3(60, -200, 8));

                var button = btn.GetComponent<UnityEngine.UI.Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => menuButton.Select());
                }
            }
        }

        // Keep StopOrder shown only during Last Order phase
        bool isLastOrder = GameManager.instance != null &&
                           GameManager.instance.currentState == GameState.LastOrder;

        if (isLastOrder)
        {
            Transform stopOrderTransform = null;

            foreach (Transform child in container)
            {
                if (child.name == "StopOrder")
                {
                    stopOrderTransform = child;
                    break;
                }
            }

            if (stopOrderTransform == null)
            {
                SpawnStopOrderButton();

                foreach (Transform child in container)
                {
                    if (child.name == "StopOrder")
                    {
                        stopOrderTransform = child;
                        break;
                    }
                }
            }

            if (stopOrderTransform != null)
            {
                stopOrderTransform.gameObject.SetActive(true);
                stopOrderTransform.SetAsLastSibling();
            }
            else
            {
                Debug.LogWarning("StopOrder not found under ButtonContainer during Last Order!");
            }
        }
    }
    #endregion
    void SetAllMenuButtonsVisible(bool visible)
    {
        Transform container = menuUI.transform.Find("ButtonContainer");
        if (container == null) return;

        foreach (Transform child in container)
        {
            MenuButton btn = child.GetComponent<MenuButton>();
            if (btn != null)
            {
                btn.SetVisible(visible);
            }
        }
    }
    IEnumerator MoveDroneToPlayer(PlayerData player, float duration)
    {
        // Example: just wait for duration for now
        Debug.Log("Moving drone to " + player.playerName);
        yield return new WaitForSeconds(duration);
    }
    public void StartNewRound()
    {
        // 🔄 Reset Guess System
        GuessPopup.instance.ResetFirstOrderGuessesFlag();
        GuessSystem.instance.ResetGuessSystem();

        // 🔄 Reset ALL player food data (KEEP out-of-pocket!)
        foreach (var p in GameManager.instance.players)
        {
            p.orders.Clear();
            p.lastOrders.Clear();
            p.totalActual = 0;

            // 🔥 IMPORTANT: reset guesses inside each item if needed
            foreach (var item in MenuManager.instance.menuItems)
            {
                item.guessedPrice = 0;
            }
        }

        Debug.Log("New Round Started: All food orders reset.");
    }
}