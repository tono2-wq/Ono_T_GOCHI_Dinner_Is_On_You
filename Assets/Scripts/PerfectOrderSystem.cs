using UnityEngine;

public class PerfectOrderSystem : MonoBehaviour
{
    public static PerfectOrderSystem instance;

    void Awake()
    {
        instance = this;
    }

    public void CheckPerfectOrder()
    {
        int target = TargetManager.instance.targetPrice;

        foreach (var p in GameManager.instance.players)
        {
            if (p.totalActual == target)
            {
                Debug.Log(p.playerName + " PERFECT ORDER!");

                p.outOfPocket = 0;
            }
        }
    }
}