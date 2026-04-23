using UnityEngine;
using TMPro;
using System.Linq;

public class FinalResultsUI : MonoBehaviour
{
    public static FinalResultsUI instance;

    public GameObject panel;

    public TextMeshProUGUI winnerText;

    public TextMeshProUGUI player1Row;
    public TextMeshProUGUI player2Row;
    public TextMeshProUGUI player3Row;

    void Awake()
    {
        instance = this;
    }

    public void ShowFinalResults()
    {
        panel.SetActive(true);

        var players = GameManager.instance.players
            .OrderBy(p => p.outOfPocket)
            .ToList();

        winnerText.text = "WINNER: " + players[0].playerName;

        player1Row.text = players[0].playerName + " - " + players[0].outOfPocket + " yen";
        player2Row.text = players[1].playerName + " - " + players[1].outOfPocket + " yen";
        player3Row.text = players[2].playerName + " - " + players[2].outOfPocket + " yen";
    }
}