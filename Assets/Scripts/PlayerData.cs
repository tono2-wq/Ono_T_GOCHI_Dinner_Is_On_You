using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public string playerName;
    public bool isHuman;

    public List<MenuItem> orders = new List<MenuItem>();
    public List<MenuItem> lastOrders = new List<MenuItem>();

    public int totalActual;
    public int outOfPocket;

    [HideInInspector] public Transform uiTransform; // assign in code or inspector

    public void ResetRound()
    {
        foreach (var item in orders)
            if (item != null) item.guessedPrice = 0;

        foreach (var item in lastOrders)
            if (item != null) item.guessedPrice = 0;

        orders.Clear();
        lastOrders.Clear();

        totalActual = 0;
        outOfPocket = 0;
    }
}