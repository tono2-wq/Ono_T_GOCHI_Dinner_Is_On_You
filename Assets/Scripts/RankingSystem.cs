using UnityEngine;
using System.Linq;

public class RankingSystem : MonoBehaviour
{
    public static RankingSystem instance;

    void Awake()
    {
        instance = this;
    }

    public PlayerData GetLastPlace()
    {
        return GameManager.instance.players
            .OrderByDescending(p =>
                Mathf.Abs(p.totalActual - TargetManager.instance.targetPrice))
            .First();
    }
}