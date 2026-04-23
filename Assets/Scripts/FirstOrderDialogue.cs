using UnityEngine;
using TMPro;
using System.Collections;

public class FirstOrderDialogue : MonoBehaviour
{
    public static FirstOrderDialogue instance;

    [Header("UI")]
    public GameObject panel;
    public TextMeshProUGUI textUI;

    [Header("Dialogues")]
    [TextArea] public string[] serveDialogues = new string[4];
    [TextArea] public string[] eatingDialogues = new string[4];
    [TextArea] public string[] tasteDialogues = new string[4];

    private Coroutine runningRoutine;

    void Awake()
    {
        instance = this;

        // ✅ Start OFF
        if (panel != null)
            panel.SetActive(false);
    }

    // 🎬 Called during First Order
    public void StartFirstOrderDialogue(AudioSource musicSource)
    {
        if (runningRoutine != null)
            StopCoroutine(runningRoutine);

        runningRoutine = StartCoroutine(FirstOrderRoutine(musicSource));
    }



    IEnumerator FirstOrderRoutine(AudioSource musicSource)
    {
        if (panel == null || textUI == null)
            yield break;

        panel.SetActive(true);

        // 1️⃣ Serve (5s)
        textUI.text = GetRandom(serveDialogues);
        yield return new WaitForSeconds(5f);

        // 2️⃣ Eating (6s)
        textUI.text = GetRandom(eatingDialogues);
        yield return new WaitForSeconds(6f);

        // 3️⃣ Taste (WAIT until Guess Phase music ends)
        textUI.text = GetRandom(tasteDialogues);

        // ⚠️ DO NOT turn off here
        // We wait for Guess Phase to finish
    }

    // 🎯 Called from Guess Phase
    public void EndDialogueWhenMusicEnds(AudioSource musicSource)
    {
        StartCoroutine(EndRoutine(musicSource));
    }

    public void ForceHide()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    IEnumerator EndRoutine(AudioSource musicSource)
    {
        if (musicSource != null)
            yield return new WaitWhile(() => musicSource.isPlaying);

        if (panel != null)
            panel.SetActive(false);
    }

    string GetRandom(string[] list)
    {
        if (list == null || list.Length == 0)
            return "";

        return list[Random.Range(0, list.Length)];
    }
}