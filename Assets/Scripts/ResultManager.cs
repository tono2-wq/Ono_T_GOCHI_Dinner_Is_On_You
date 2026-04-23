using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.SceneManagement;

public class ResultManager : MonoBehaviour
{
    public static ResultManager instance;

    [Header("Ending")]
    public AudioSource musicSource;
    public AudioClip GOCHIEndingMusic;
    [Header("Final Results UI")]
    public GameObject finalResultsBox;
    public TMPro.TextMeshProUGUI finalOutOfPocketText;

    [TextArea] public string[] creditsLines = new string[5];

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void StartReveal()
    {
        StartCoroutine(RevealRoutine());
    }
    string GetRankSuffix(int rank)
    {
        switch (rank)
        {
            case 1: return "st";
            case 2: return "nd";
            case 3: return "rd";
            default: return "th";
        }
    }

    int GetPlayerIndex(PlayerData player)
    {
        if (player == null || GameManager.instance == null || GameManager.instance.players == null)
            return -1;

        return GameManager.instance.players.IndexOf(player);
    }

    Animator GetAnimatorForPlayer(PlayerData player)
    {
        if (player == null || RoundManager.instance == null) return null;

        switch (GetPlayerIndex(player))
        {
            case 0: return RoundManager.instance.animatorYou;
            case 1: return RoundManager.instance.animatorYabe;
            case 2: return RoundManager.instance.animatorOkamura;
            default: return null;
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

    IEnumerator PlayFinalWinnerThenRaiseHands(Animator animator, string winAnim, string raiseAnim)
    {
        if (animator == null || string.IsNullOrEmpty(winAnim) || string.IsNullOrEmpty(raiseAnim))
            yield break;

        animator.speed = 1f;
        animator.Play(winAnim, 0, 0f);
        yield return null;

        AnimationClip winClip = GetAnimationClip(animator, winAnim);
        if (winClip == null)
        {
            Debug.LogWarning("Final winner clip not found: " + winAnim);
            yield break;
        }

        yield return new WaitForSeconds(winClip.length);

        animator.Play(raiseAnim, 0, 0f);
        yield return null;

        AnimationClip raiseClip = GetAnimationClip(animator, raiseAnim);
        if (raiseClip == null)
        {
            Debug.LogWarning("Raise hands clip not found: " + raiseAnim);
            yield break;
        }

        yield return new WaitForSeconds(raiseClip.length);

        // Hold on the last frame
        animator.Play(raiseAnim, 0, 1f);
        animator.Update(0f);
    }

    string GetFinalWinnerAnimation(PlayerData player)
    {
        switch (GetPlayerIndex(player))
        {
            case 0: return "PitariWonYou";
            case 1: return "PitariWonYabe";
            case 2: return "PitariWonOkamura";
            default: return null;
        }
    }
    string GetFinalWinnerRaiseHandsAnimation(PlayerData player)
    {
        switch (GetPlayerIndex(player))
        {
            case 0: return "RaiseBothHandsYou";
            case 1: return "RaiseBothHandsYabe";
            case 2: return "RaiseBothHandsOkamura";
            default: return null;
        }
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

    void AnimateFinalWinner(PlayerData player)
    {
        Animator animator = GetAnimatorForPlayer(player);
        string winAnim = GetFinalWinnerAnimation(player);
        string raiseAnim = GetFinalWinnerRaiseHandsAnimation(player);

        if (animator != null &&
            !string.IsNullOrEmpty(winAnim) &&
            !string.IsNullOrEmpty(raiseAnim))
        {
            StartCoroutine(PlayFinalWinnerThenRaiseHands(animator, winAnim, raiseAnim));
        }
    }
    IEnumerator RevealRoutine()
    {
        // 🔹 Show intro message
        UIManager.instance.ShowMessage("Results Reveal");

        // 🔹 Calculate totals for all players
        OrderManager.instance.CalculateTotals();

        yield return new WaitForSeconds(2f);

        // 🔹 Apply penalties (if any)
        OutOfPocketManager.instance.ApplyPenalty();

        yield return new WaitForSeconds(2f);



    }

    public IEnumerator ShowPlayerTotals()
    {
        OrderManager.instance.CalculateTotals();
        int target = TargetManager.instance.targetPrice;

        var ranked = GameManager.instance.players
            .OrderBy(p => Mathf.Abs(p.totalActual - target))
            .ToList();

        foreach (var player in ranked)
        {
            UIManager.instance.ShowMessage($"{player.playerName} total: {player.totalActual}");
            yield return new WaitForSeconds(1f);
        }
        // 🔹 Show out-of-pocket ranking table
        yield return StartCoroutine(ShowOutOfPocketTable());
    }

    // --- New Out of Pocket Table ---
    public IEnumerator ShowOutOfPocketTable()
    {
        // 🔹 Sort players by total payment across game (lowest is winner)
        var rankedByPayment = GameManager.instance.players
            .OrderBy(p => p.outOfPocket)
            .ToList();

        UIManager.instance.ShowMessage("=== Out of Pocket Rankings ===");
        yield return new WaitForSeconds(1f);

        foreach (var player in rankedByPayment)
        {
            UIManager.instance.ShowMessage($"{player.playerName}: {player.outOfPocket} yen total paid");
            yield return new WaitForSeconds(1f);
        }


    }

    public void StartFinalResults()
    {
        StartCoroutine(FinalResultsRoutine());
    }

    IEnumerator FinalResultsRoutine()
    {
        var players = GameManager.instance.players;

        // 🔹 Find lowest out-of-pocket
        int minValue = players.Min(p => p.outOfPocket);

        // 🔹 Get all tied players
        var tiedPlayers = players
            .Where(p => p.outOfPocket == minValue)
            .ToList();

        // 🎲 Random winner among ties
        PlayerData winner;

        if (tiedPlayers.Count == 1)
        {
            winner = tiedPlayers[0];
        }
        else
        {
            int index = Random.Range(0, tiedPlayers.Count);
            winner = tiedPlayers[index];
        }

        // 🎵 Play ending music
        if (musicSource != null && GOCHIEndingMusic != null)
        {
            musicSource.clip = GOCHIEndingMusic;
            musicSource.loop = false;
            musicSource.Play();
        }

        // 🏆 Show winner (7 seconds)
        UIManager.instance.ShowMessage("FINAL WINNER: " + winner.playerName);
        AnimateFinalWinner(winner);
        AnimateNonFinalWinnersIdle(winner);
        yield return new WaitForSeconds(2f);

        // 📊 Show final out-of-pocket rankings
        yield return StartCoroutine(ShowFinalOutOfPocketResults(winner));

        // 🎬 Credits (5 lines, 5 sec each)
        foreach (string line in creditsLines)
        {
            UIManager.instance.ShowMessage(line);
            yield return new WaitForSeconds(5f);
        }

        // ⏳ Wait 2 seconds
        yield return new WaitForSeconds(2f);

        // 🔄 Load Title Screen scene
        SceneManager.LoadScene("TitleScreen");
    }
    string GetIdleAnimation(PlayerData player)
    {
        switch (GetPlayerIndex(player))
        {
            case 0: return "IdleYou";
            case 1: return "IdleYabe";
            case 2: return "IdleOkamura";
            default: return null;
        }
    }
    void AnimateIdle(PlayerData player)
    {
        Animator animator = GetAnimatorForPlayer(player);
        string idleAnim = GetIdleAnimation(player);

        if (animator != null && !string.IsNullOrEmpty(idleAnim))
        {
            animator.speed = 1f;
            animator.Play(idleAnim, 0, 0f);
            animator.Update(0f);
        }
    }
    void AnimateNonFinalWinnersIdle(PlayerData finalWinner)
    {
        foreach (var player in GameManager.instance.players)
        {
            if (player == finalWinner)
                continue;

            AnimateIdle(player);
        }
    }
    IEnumerator ShowFinalOutOfPocketResults(PlayerData finalWinner)
    {
        var ranked = GameManager.instance.players
            .OrderBy(p => p == finalWinner ? 0 : 1)
            .ThenBy(p => p.outOfPocket)
            .ToList();

        string resultText = "Final Results (Out-of-Pocket Totals)\n";

        for (int i = 0; i < ranked.Count; i++)
        {
            int rank = i + 1;
            string suffix = GetRankSuffix(rank);

            resultText += $"{rank}{suffix}: {ranked[i].playerName} {ranked[i].outOfPocket} yen\n";
        }

        finalResultsBox.SetActive(true);
        finalOutOfPocketText.text = resultText;

        yield return new WaitForSeconds(5f);

        finalResultsBox.SetActive(false);
    }
    public PlayerData GetWinner()
    {
        int target = TargetManager.instance.targetPrice;

        return GameManager.instance.players
            .OrderBy(p => Mathf.Abs(p.totalActual - target))
            .First();
    }
}