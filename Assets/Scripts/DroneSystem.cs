using UnityEngine;

public class DroneSystem : MonoBehaviour
{
    public static DroneSystem instance;

    void Awake()
    {
        instance = this;
    }

    public PlayerData ChooseDroneLoser()
    {
        int index = Random.Range(0, GameManager.instance.players.Count);

        PlayerData loser = GameManager.instance.players[index];

        Debug.Log("Drone landed on: " + loser.playerName);

        return loser;
    }
}