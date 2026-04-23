using UnityEngine;

public class DebugTools : MonoBehaviour
{
    [ContextMenu("Skip To Results")]
    void SkipToResults()
    {
        GameManager.instance.ChangeState(GameState.ResultsReveal);
    }

    [ContextMenu("Force Target 20000")]
    void ForceTarget()
    {
        TargetManager.instance.targetPrice = 20000;

        Debug.Log("Forced Target: 20000");
    }

    [ContextMenu("Test Results")]
    void TestResults()
    {
        ResultsUIController.instance.ShowResults();
    }
}