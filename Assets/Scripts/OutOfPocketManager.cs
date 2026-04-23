using UnityEngine;
using System.Linq;

public class OutOfPocketManager : MonoBehaviour
{
    public static OutOfPocketManager instance;

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void ApplyPenalty()
    {
        var players = GameManager.instance.players;

        int totalBill = 0;

        foreach (var p in players)
            totalBill += p.totalActual;

        var loser = players
            .OrderByDescending(p =>
                Mathf.Abs(
                    p.totalActual -
                    TargetManager.instance.targetPrice))
            .First();

        loser.outOfPocket += totalBill;

        Debug.Log(
            "Who's going to pay for everyone's meals?"
        );
    }

    public void ApplyReward(PlayerData player, int rewardAmount)
    {
        if (player == null) return;

        // Subtract reward
        player.outOfPocket -= rewardAmount;

        // Clamp to 0 (cannot go negative)
        if (player.outOfPocket < 0)
            player.outOfPocket = 0;

        Debug.Log(player.playerName + " received reward: " + rewardAmount +
                  " → New OutOfPocket: " + player.outOfPocket);
    }

    public void ApplyPitariWin(PlayerData player)
    {
        if (player == null) return;

        player.outOfPocket = 0;

        Debug.Log(player.playerName + " hit PITARI! OutOfPocket reset to 0.");
    }
}