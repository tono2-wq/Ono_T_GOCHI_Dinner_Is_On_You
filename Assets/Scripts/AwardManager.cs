using UnityEngine;

public class AwardManager : MonoBehaviour
{
    public static AwardManager instance;

    void Awake()
    {
        instance = this;
    }

    public void CheckAwards()
    {
        PerfectOrderSystem.instance.CheckPerfectOrder();
        NearPinSystem.instance.CheckNearPin();
    }
}