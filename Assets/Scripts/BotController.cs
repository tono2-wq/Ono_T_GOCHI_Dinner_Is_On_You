using UnityEngine;

public class BotController : MonoBehaviour
{
    public static BotController instance;

    void Awake()
    {
        if (instance == null)
            instance = this;
    }


}