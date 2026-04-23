using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public TextMeshProUGUI messageText;
    public AudioSource audioSource;
    public AudioClip messageClip;
    public GameObject messageBox;

    public bool playerSubmitted = false;
    public bool isShowingSequence = false;

    private Coroutine messageCoroutine;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 🔥 NEW: DIRECT SAFE SET (no coroutine conflicts)
    public void SetMessage(string message)
    {
        if (messageBox != null)
            messageBox.SetActive(true);

        if (messageText != null)
            messageText.text = message;

        if (audioSource != null && messageClip != null)
            audioSource.PlayOneShot(messageClip);
    }

    public void ShowMessageForSeconds(string message, float duration = 2f)
    {
        if (isShowingSequence) return;

        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);

        messageCoroutine = StartCoroutine(ShowMessageRoutine(message, duration));
    }

    private IEnumerator ShowMessageRoutine(string message, float duration)
    {
        SetMessage(message);
        yield return new WaitForSeconds(duration);
        HideMessage();
    }

    public void HideMessage()
    {
        if (messageText != null)
            messageText.text = "";

        if (messageBox != null)
            messageBox.SetActive(false);
    }

    public void ShowMessage(string message)
    {
        ShowMessageForSeconds(message, 2f);
    }

    public void ShowTarget(int targetPrice)
    {
        int currentRound = GameManager.instance.currentRound;
        int maxRounds = GameManager.instance.maxRounds;

        string message;

        if (currentRound == maxRounds)
        {
            // Final round
            message = $"The final round! Target Price: ¥{targetPrice}";
        }
        else
        {
            // Normal round
            message = $"Round {currentRound}! Target Price: ¥{targetPrice}";
        }

        ShowMessageForSeconds(message, 8f);
    }

    public void ShowFirstOrderUI()
    {
        playerSubmitted = false;
        ShowMessage("Choose your first item");
    }

    public void ShowLastOrderUI()
    {
        playerSubmitted = false;
        ShowMessage("Last Order! Choose items until you hit the target!");
    }

    public IEnumerator ShowLastOrderMessageThenStopButton(GameObject stopOrderButton, float duration = 3f)
    {
        // Reset submit state
        playerSubmitted = false;

        // Show message
        ShowMessageForSeconds("Last Order! Choose items until you hit the target!", duration);

        // Wait for message to finish
        yield return new WaitForSeconds(duration);

        // Show STOP button AFTER message
        if (stopOrderButton != null)
        {
            stopOrderButton.SetActive(true);
        }
        else
        {
            Debug.LogError("StopOrderButton reference is NULL!");
        }
    }

    public void ShowGuessUI()
    {
        playerSubmitted = false;
        ShowMessage("Enter your price guess");
    }

    public void Submit()
    {
        playerSubmitted = true;
    }

    public bool IsAudioPlaying()
    {
        return audioSource != null && audioSource.isPlaying;
    }
}