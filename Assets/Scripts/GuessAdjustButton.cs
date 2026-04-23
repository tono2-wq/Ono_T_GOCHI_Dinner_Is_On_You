using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class GuessAdjustButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public GuessPopup guessPopup;
    public bool isAddButton = true;

    [SerializeField] private float holdDelay = 0.25f;
    [SerializeField] private float repeatRate = 30f;

    private Coroutine holdCoroutine;
    private bool isHolding = false;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (guessPopup == null)
            return;

        isHolding = true;

        // Immediate change on press
        if (isAddButton)
            guessPopup.Add100();
        else
            guessPopup.Subtract100();

        // Start hold-to-repeat
        if (holdCoroutine != null)
            StopCoroutine(holdCoroutine);

        holdCoroutine = StartCoroutine(HoldRoutine());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopHolding();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopHolding();
    }

    private void StopHolding()
    {
        isHolding = false;

        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }
    }

    private IEnumerator HoldRoutine()
    {
        yield return new WaitForSeconds(holdDelay);

        float interval = 1f / repeatRate;

        while (isHolding)
        {
            if (isAddButton)
                guessPopup.Add100();
            else
                guessPopup.Subtract100();

            yield return new WaitForSeconds(interval);
        }
    }
}