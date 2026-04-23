using UnityEngine;
using UnityEngine.EventSystems;

public class StopOrderButton : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    public GameObject buttonContainer;

    private RectTransform rect;
    private Vector2 dragOffset;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    void Start()
    {
        // Initially hide the button
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Call this when the player presses STOP
    /// </summary>
    public void StopOrdering()
    {
        // Hide the menu button container
        if (buttonContainer != null)
            buttonContainer.SetActive(false);

        UIManager.instance.Submit();
    }

    /// <summary>
    /// Hide button (for First Order)
    /// </summary>
    public void HideButton()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Show button (for Last Order / menu panel open)
    /// </summary>
    public void ShowButton()
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
    }

    /// <summary>
    /// Reset button state at the start of a new round
    /// </summary>
    public void ResetButton()
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();

        if (buttonContainer != null)
            buttonContainer.SetActive(true);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (rect == null)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rect.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        dragOffset = rect.anchoredPosition - localPoint;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (rect == null)
            return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rect.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        rect.anchoredPosition = localPoint + dragOffset;
    }
}