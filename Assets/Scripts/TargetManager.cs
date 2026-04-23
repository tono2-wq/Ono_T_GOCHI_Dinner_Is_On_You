using UnityEngine;
using System.Collections;

public class TargetManager : MonoBehaviour
{
    public static TargetManager instance;

    public int targetPrice;
    public GameObject targetRevealPanel;  // Assign this in the Inspector
    public AudioSource audioSource;       // Assign AudioSource in the Inspector
    public AudioClip targetPriceRevealedAudio; // Assign Target Price Revealed Audio Clip in the Inspector

    [Header("Character Animations")]
    public Animator animatorOkamura;      // Assign Okamura Animator in Inspector
    public Animator animatorYabe;         // Assign Yabe Animator in Inspector
    public Animator animatorYou;          // Assign You Animator in Inspector

    void Awake()
    {
        instance = this;
        GenerateTarget(); // Generate when game starts
    }

    public void GenerateTarget()
    {
        int[] excluded = { 11000, 19000, 21000, 27000, 29000 };

        do
        {
            targetPrice = Random.Range(10, 31) * 1000;
        }
        while (System.Array.Exists(excluded, x => x == targetPrice));

    }

    // Call this method to show the TargetRevealPanel, play the audio, and trigger animations
    public void ShowTargetRevealPanel()
    {
        // Show the TargetRevealPanel
        if (targetRevealPanel != null)
        {
            targetRevealPanel.SetActive(true);
            TriggerCharacterAnimations();  // Trigger animations as the panel is shown
            PlayTargetPriceAudio();  // Play the audio
        }
        else
        {
            Debug.LogError("TargetRevealPanel not assigned!");
        }
    }

    // Trigger the animations for the characters
    void TriggerCharacterAnimations()
    {
        if (animatorOkamura != null)
            animatorOkamura.Play("SitDownOkamura", 0, 0.4f);

        if (animatorYabe != null)
            animatorYabe.Play("SitDownYabe", 0, 0.4f);

        if (animatorYou != null)
            animatorYou.Play("SitDownYou", 0, 0.4f);
    }

    // Start the Target Price audio
    void PlayTargetPriceAudio()
    {
        if (audioSource != null && targetPriceRevealedAudio != null)
        {
            audioSource.clip = targetPriceRevealedAudio;
            audioSource.Play();
        }
        else
        {
            Debug.LogError("AudioSource or Target Price Revealed Audio not assigned!");
        }
    }

    // Coroutine to stop animation after a delay (0.3 seconds)

}