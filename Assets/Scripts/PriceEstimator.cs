using UnityEngine;

public class PriceEstimator : MonoBehaviour
{
    public static int EstimatePrice(MenuItem item)
    {
        int variance = Random.Range(-500, 500);
        int guess = item.realPrice + variance;
        guess = Mathf.RoundToInt(guess / 100f) * 100;
        return Mathf.Max(100, guess);
    }
}