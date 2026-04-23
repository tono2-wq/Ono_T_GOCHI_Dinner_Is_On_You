using UnityEngine;
using TMPro;

public class ResultsUIController : MonoBehaviour
{
    public static ResultsUIController instance;

    public GameObject resultsPanel;

    public TextMeshProUGUI targetText;

    public TextMeshProUGUI player1Text;
    public TextMeshProUGUI player2Text;
    public TextMeshProUGUI player3Text;

    public TextMeshProUGUI winnerText;
    public TextMeshProUGUI droneText;

    void Awake()
    {
        instance = this;
    }

    public void ShowResults()
    {
        resultsPanel.SetActive(true);

        int target = TargetManager.instance.targetPrice;

        targetText.text = "Target: " + target + " yen";

        var players = GameManager.instance.players;

        player1Text.text = players[0].playerName + ": " + players[0].totalActual;
        player2Text.text = players[1].playerName + ": " + players[1].totalActual;
        player3Text.text = players[2].playerName + ": " + players[2].totalActual;

        PlayerData winner = ResultManager.instance.GetWinner();

        winnerText.text = "Winner: " + winner.playerName;

        PlayerData droneLoser = DroneSystem.instance.ChooseDroneLoser();

        droneText.text = "Drone chose: " + droneLoser.playerName;
    }
}