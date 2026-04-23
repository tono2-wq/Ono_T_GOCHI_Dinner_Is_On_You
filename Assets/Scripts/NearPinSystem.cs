using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class NearPinSystem : MonoBehaviour
{
    public static NearPinSystem instance;

    [Header("Settings")]
    public int nearPinRange = 500;

    [Header("Results")]
    public List<PlayerData> eligiblePlayers = new List<PlayerData>();
    public PlayerData winner;

    void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Call AFTER pitari phase ends
    /// </summary>
    public void CheckNearPin()
    {
        int target = TargetManager.instance.targetPrice;

        eligiblePlayers.Clear();
        winner = null;

        // 🔹 Step 1: Find all eligible players (within 500 yen)
        foreach (var p in GameManager.instance.players)
        {
            int diff = Mathf.Abs(p.totalActual - target);

            if (diff <= nearPinRange)
            {
                eligiblePlayers.Add(p);
                Debug.Log(p.playerName + " is eligible for Near Pin (" + diff + " away)");
            }
        }

        // ❌ No one eligible
        if (eligiblePlayers.Count == 0)
        {
            Debug.Log("No Near Pin winners.");
            return;
        }

        // 🔹 Step 2: Find closest distance
        int bestDiff = eligiblePlayers
            .Min(p => Mathf.Abs(p.totalActual - target));

        // 🔹 Step 3: Get all players tied for closest
        List<PlayerData> closestPlayers = eligiblePlayers
            .Where(p => Mathf.Abs(p.totalActual - target) == bestDiff)
            .ToList();

        // 🔹 Step 4: Handle tie (random chance)
        if (closestPlayers.Count > 1)
        {
            winner = closestPlayers[Random.Range(0, closestPlayers.Count)];

            Debug.Log("Near Pin tie! Random winner: " + winner.playerName);
        }
        else
        {
            winner = closestPlayers[0];
            Debug.Log("Near Pin winner: " + winner.playerName);
        }

        // 🔹 Step 5: Give reward
        GiveReward(winner);
    }

    public void GiveReward(PlayerData player)
    {
        if (player == null) return;

        int reward = Random.value < 0.5f ? 1000 : 50000;

        player.outOfPocket -= reward;

        Debug.Log(player.playerName + " Near Pin Reward: " + reward);
    }

    /// <summary>
    /// Utility check used by Results system
    /// </summary>
    public bool IsNearPinEligible(PlayerData player)
    {
        int target = TargetManager.instance.targetPrice;
        return Mathf.Abs(player.totalActual - target) <= nearPinRange;
    }
}