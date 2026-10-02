using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryDragHandler :
    MonoBehaviour,
    IBeginDragHandler,
    IDragHandler
{
    [SerializeField] private RectTransform inventoryUI;

    private Canvas canvas;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (inventoryUI == null)
            return;

        float scaleFactor =
            canvas != null ? canvas.scaleFactor : 1f;

        inventoryUI.anchoredPosition +=
            eventData.delta / scaleFactor;
    }
}