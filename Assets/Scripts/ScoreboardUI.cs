using UnityEngine;

public class ScoreboardUI : MonoBehaviour
{
    public void UpdateBoard()
    {
        foreach (var p in GameManager.instance.players)
        {
            Debug.Log(p.playerName + " OOP: " + p.outOfPocket);
        }
    }
}