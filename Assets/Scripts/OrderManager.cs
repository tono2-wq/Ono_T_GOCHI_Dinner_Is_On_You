using UnityEngine;

public class OrderManager : MonoBehaviour
{
    public static OrderManager instance;

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public void CalculateTotals()
    {
        if (GameManager.instance == null || GameManager.instance.players == null)
        {
            Debug.LogError("GameManager or players list is missing!");
            return;
        }

        foreach (var player in GameManager.instance.players)
        {
            int total = 0;

            if (player.orders != null)
            {
                foreach (var item in player.orders)
                {
                    if (item != null)
                        total += item.realPrice;
                }
            }

            player.totalActual = total;
        }
    }
}