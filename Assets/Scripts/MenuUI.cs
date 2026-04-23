using UnityEngine;

public class MenuUI : MonoBehaviour
{
    public GameObject panel;
    public GameObject stopOrderButtonPrefab; // assign prefab in Inspector
    private StopOrderButton stopOrderButtonInstance;

    public void OpenMenu()
    {
        if (panel != null)
        {
            panel.SetActive(true);
            Debug.Log("Menu panel opened");
        }
    }

    public void CloseMenu()
    {
        if (panel != null)
            panel.SetActive(false);
    }


    void OnEnable()
    {

    }

    void OnDisable()
    {

    }

    void Awake()
    {

    }

    void OnDestroy()
    {

    }


}