using UnityEngine;
using TMPro;
using System.Collections;

public class TargetPriceDialogue : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public GameObject dialoguePanel;
    public GochiGameManager gameManager;

    private AudioSource audioSource; // ✅ ADD THIS

    private int targetPrice;
    private bool showingSecondLine = false;

    void Start()
    {
        dialoguePanel.SetActive(true);
        dialogueText.text = "Alright, the target amount of the price is...";

        // ✅ GET AudioSource
        audioSource = GetComponent<AudioSource>();

        // ✅ PLAY SOUND
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!showingSecondLine)
            {
                GenerateTargetPrice();
                showingSecondLine = true;
            }
            else
            {
                dialoguePanel.SetActive(false);

                if (gameManager != null)
                {
                    gameManager.StartFirstOrderSequence();
                }
            }
        }
    }
    void GenerateTargetPrice()
    {
        int[] excluded = { 11000, 19000, 21000, 27000, 29000 };

        while (true)
        {
            int randomValue = Random.Range(10, 31) * 1000;

            bool isExcluded = false;
            foreach (int ex in excluded)
            {
                if (randomValue == ex)
                {
                    isExcluded = true;
                    break;
                }
            }

            if (!isExcluded)
            {
                targetPrice = randomValue;
                break;
            }
        }

        int dollarAmount = targetPrice / 150;
        dialogueText.text = targetPrice + " yen, which is $" + dollarAmount + "!";
    }
}