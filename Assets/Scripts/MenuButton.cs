using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class MenuButton : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    public MenuItem item;
    private RectTransform rect;

    public AudioSource audioSource;
    public AudioClip confirmSound;

    private Canvas parentCanvas;
    private Vector2 dragOffset;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
    }

    public void Select()
    {
        if (audioSource != null && confirmSound != null)
        {
            audioSource.PlayOneShot(confirmSound);
        }

        if (item == null)
        {
            Debug.LogError("MenuButton item is NULL!");
            return;
        }

        if (PlayerOrderController.instance == null)
        {
            Debug.LogError("PlayerOrderController instance is NULL!");
            return;
        }

        PlayerOrderController.instance.SelectFood(item);
    }
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
    public void MoveButton(Vector3 newPos)
    {
        if (rect != null)
        {
            rect.localPosition = newPos;
        }
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