using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;


public class DialogueManager : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public GameObject yesButton;
    public GameObject noButton;

    [Header("Audio")]
    public AudioSource audioSource;      // For voice/SFX
    public AudioSource musicSource;      // For background music

    public AudioClip GourmetChickenRaceIntro;
    public AudioClip Applause1;
    public AudioClip GochiShoutTogether;
    public AudioClip ConfirmSound;

    public AudioClip GOCHI_Intro_Music;

    [Header("Animators")]
    public Animator okamuraAnimator;
    public Animator yabeAnimator;
    public Animator youAnimator;

    private int currentLine = 0;
    private bool waitingForChoice = false;
    private bool inTutorial = false;
    private bool atFinalLine = false;
    private bool introFinished = false;

    public TMP_InputField nameInputField;

    private bool choosingPlayerMode = false;
    private bool enteringMultiplayerNames = false;

    private List<string> multiplayerNames = new List<string>();
    private int currentNameIndex = 0;
    private int totalMultiplayerPlayers = 3;

    public CanvasGroup panelCanvasGroup;
    public static bool startInMultiplayer = false;
    public static List<string> pendingMultiplayerNames = new List<string>();

    private string[] introLines =
    {
        "Gourmet Chicken Race - Gochi ni narimasu!",
        "Do you know the basics of how to play?"
    };

    private string finalLine = "Alright, hop on in and have a seat!";

    private string[] tutorialLines =
    {
        "Alright, this character on the left side is Masuda as you.",
        "This character in the center is Yabe.",
        "And this character on the right side is Okamura.",
        "Here's the basics, you pick foods and try to guess the price.",
        "There are 4 rounds in this game.",
        "You try to hit the target amount of the price by ordering specific amount of food.",
        "The price of every food will be random, depending on realistic foods.",
        "Appetizers are ranges of 1000-3000 yen, Mains are ranges of 3000-10000 yen, and Desserts are ranges of 600-2400 yen.",
        "Remember, the $1 amount is equal to around 150 yen.",
        "There are two types of orders: First Order & Last Order.",
        "For first order, each player picks only one food in the menu.",
        "For the last order, each player picks as many as they want until they hit the target price. When finishing order, press the finish order button!",
        "When you guess, you can press and hold the value button to change the values quickly. If you want to submit guess, press the confirm button!",
        "Be careful, you'll be last place if you're the farthest from the target price.",
        "If you get the closest to the target price, you win!",
        "You'll win 1000 or 50000 coins if you get the near pin!",
        "Near pin eligibility: Needs to get within 500-yen difference from target price, but only one person will be awarded per round.",
        "If you hit the exact amount as the target price, you win 1 million coins! Only one person is awarded per round!",
        "In the event of the tie, there will be an even random chance to win or get better ranking!",
        "Last place player in the round pays for everyone's meal amounts!",
        "At the end of the game, the player with least amount of payment total amount declares the final winner!!",
        "Sounds good?"
    };

    private int tutorialIndex = 0;

    void Start()
    {

        dialoguePanel.SetActive(true);
        if (nameInputField != null)
            nameInputField.gameObject.SetActive(false);
        // 🔥 Start invisible
        if (panelCanvasGroup != null)
            panelCanvasGroup.alpha = 0f;

        yesButton.SetActive(false);
        noButton.SetActive(false);
        dialogueText.text = "";

        if (audioSource != null) audioSource.loop = false;
        if (musicSource != null) musicSource.loop = false;

        StartCoroutine(ShowPanelAfterDelay(8.5f)); // 👈 ADD THIS
        StartCoroutine(PlayIntroSequence());
    }

    IEnumerator ShowPanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (panelCanvasGroup != null)
            panelCanvasGroup.alpha = 1f; // 100% visible
    }

    IEnumerator PlayIntroSequence()
    {
        introFinished = false;

        // Idle start
        if (okamuraAnimator != null)
            okamuraAnimator.Play("IdleOkamura");

        if (yabeAnimator != null)
            yabeAnimator.Play("IdleYabe");

        if (youAnimator != null)
            youAnimator.Play("IdleYou");

        // 🎵 Background music
        if (musicSource != null && GOCHI_Intro_Music != null)
        {
            musicSource.PlayOneShot(GOCHI_Intro_Music, 0.2f);
        }

        // 🎤 Intro voice
        if (audioSource != null && GourmetChickenRaceIntro != null)
        {
            audioSource.clip = GourmetChickenRaceIntro;
            audioSource.Play();
        }

        // 🗣 Okamura speaks
        if (okamuraAnimator != null)
            okamuraAnimator.Play("SpeakOkamura");

        yield return new WaitForSeconds(2f);


        // 👏 Applause (NOW WORKS)
        if (audioSource != null && Applause1 != null)
        {
            audioSource.PlayOneShot(Applause1);
        }
        // 😐 Back to idle
        if (okamuraAnimator != null)
            okamuraAnimator.Play("IdleOkamura");

        yield return new WaitForSeconds(5.5f);



        // 🗣 Group shout
        if (audioSource != null && GochiShoutTogether != null)
        {
            audioSource.PlayOneShot(GochiShoutTogether);
        }

        if (okamuraAnimator != null)
            okamuraAnimator.Play("SpeakOkamura");

        if (yabeAnimator != null)
            yabeAnimator.Play("SpeakYabe");

        if (youAnimator != null)
            youAnimator.Play("SpeakYou");

        yield return new WaitForSeconds(1f);

        // 😐 Back to idle
        if (okamuraAnimator != null)
            okamuraAnimator.Play("IdleOkamura");

        if (yabeAnimator != null)
            yabeAnimator.Play("IdleYabe");

        if (youAnimator != null)
            youAnimator.Play("IdleYou");

        // Wait for shout to finish safely
        if (GochiShoutTogether != null)
            yield return new WaitForSeconds(GochiShoutTogether.length - 1f);

        ShowLine(introLines[currentLine]);
        introFinished = true;
    }

    void Update()
    {
        if (!introFinished) return;

        if (Input.GetMouseButtonDown(0) && !waitingForChoice)
        {
            HandleClick();
        }
    }



    void ShowNextNamePrompt()
    {
        ShowLine("Enter Player " + (currentNameIndex + 1) + " Name:");
    }

    void StartMultiplayerNameEntry()
    {
        enteringMultiplayerNames = true;
        multiplayerNames.Clear();
        currentNameIndex = 0;

        ShowNextNamePrompt();
        if (nameInputField != null)
        {
            nameInputField.gameObject.SetActive(true);
            nameInputField.text = "";
            nameInputField.ActivateInputField();
            nameInputField.Select();
        }
    }

    void HandleClick()
    {
        if (atFinalLine)
        {
            ShowLine("Would you like to play with your friends? (3 players if Yes) (Single player if No)");
            choosingPlayerMode = true;
            waitingForChoice = true;
            atFinalLine = false;

            yesButton.SetActive(true);
            noButton.SetActive(true);
            return;
        }

        if (!inTutorial)
        {
            currentLine++;
            if (currentLine < introLines.Length)
            {
                ShowLine(introLines[currentLine]);

                if (currentLine == 1)
                    ShowChoice();
            }
        }
        else
        {
            tutorialIndex++;

            if (tutorialIndex < tutorialLines.Length)
            {
                ShowLine(tutorialLines[tutorialIndex]);
            }
            else
            {
                inTutorial = false;
                ShowLine(finalLine);
                atFinalLine = true;
            }
        }
    }

    void ShowChoice()
    {
        waitingForChoice = true;
        yesButton.SetActive(true);
        noButton.SetActive(true);
    }

    public void OnYes()
    {
        yesButton.SetActive(false);
        noButton.SetActive(false);
        waitingForChoice = false;

        if (choosingPlayerMode)
        {
            choosingPlayerMode = false;
            StartMultiplayerNameEntry();
            return;
        }

        ShowLine(finalLine);
        atFinalLine = true;
    }

    public void OnNo()
    {
        yesButton.SetActive(false);
        noButton.SetActive(false);
        waitingForChoice = false;

        if (choosingPlayerMode)
        {
            choosingPlayerMode = false;
            DialogueManager.startInMultiplayer = false;
            DialogueManager.pendingMultiplayerNames.Clear();


            if (GameManager.instance != null)
            {
                GameManager.instance.isMultiplayer = false;
                SetSinglePlayer();
            }

            SceneManager.LoadScene("GameScene2");
            return;
        }

        inTutorial = true;
        tutorialIndex = 0;
        ShowLine(tutorialLines[tutorialIndex]);
    }


    public void OnNameSubmitted(string _)
    {
        if (!enteringMultiplayerNames)
            return;

        string cleanName = "";

        if (nameInputField != null)
            cleanName = nameInputField.text.Trim();

        if (string.IsNullOrEmpty(cleanName))
            cleanName = "Player " + (currentNameIndex + 1);

        multiplayerNames.Add(cleanName);
        Debug.Log("Added multiplayer name: " + cleanName);

        currentNameIndex++;

        if (currentNameIndex >= totalMultiplayerPlayers)
        {
            enteringMultiplayerNames = false;

            if (nameInputField != null)
                nameInputField.gameObject.SetActive(false);

            DialogueManager.startInMultiplayer = true;
            DialogueManager.pendingMultiplayerNames = new List<string>(multiplayerNames);

            Debug.Log("Saved multiplayer names: " + string.Join(", ", DialogueManager.pendingMultiplayerNames));

            SceneManager.LoadScene("GameScene2");
        }
        else
        {
            ShowNextNamePrompt();

            if (nameInputField != null)
            {
                nameInputField.text = "";
                nameInputField.ActivateInputField();
                nameInputField.Select();
            }
        }
    }
    void SetSinglePlayer()
    {
        if (GameManager.instance == null)
            return;

        GameManager.instance.players.Clear();

        GameManager.instance.players.Add(new PlayerData
        {
            playerName = "You",
            isHuman = true
        });

        GameManager.instance.players.Add(new PlayerData
        {
            playerName = "Yabe",
            isHuman = false
        });

        GameManager.instance.players.Add(new PlayerData
        {
            playerName = "Okamura",
            isHuman = false
        });
    }

    void SetMultiplayerPlayers()
    {
        if (GameManager.instance == null)
            return;

        GameManager.instance.players.Clear();

        for (int i = 0; i < 3; i++)
        {
            string playerName = multiplayerNames[i].Trim();

            if (string.IsNullOrEmpty(playerName))
                playerName = "Player " + (i + 1);

            GameManager.instance.players.Add(new PlayerData
            {
                playerName = playerName,
                isHuman = true
            });
        }
    }
    void SetPlayerCount(int count)
    {
        if (GameManager.instance == null) return;

        GameManager.instance.players.Clear();

        // Always add YOU
        GameManager.instance.players.Add(new PlayerData
        {
            playerName = "You",
            isHuman = true
        });

        if (count == 3)
        {
            GameManager.instance.players.Add(new PlayerData { playerName = "Yabe" });
            GameManager.instance.players.Add(new PlayerData { playerName = "Okamura" });
        }
    }
    void ShowLine(string line)
    {
        dialogueText.text = line;

        if (ConfirmSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(ConfirmSound);
        }
    }
}