using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlotUI :
    MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [Header("UI")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private Image highlightImage;

    [Header("Item Icons")]
    [SerializeField] private ItemIconData[] itemIcons;

    private InventoryItemData currentItem;

    private InventoryUI inventoryUI;
    private RectTransform slotRect;
    private RectTransform contentArea;
    private Canvas canvas;

    private int slotIndex;
    private bool isDragging;

    private void Awake()
    {
        slotRect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        if (itemIcon != null)
        {
            itemIcon.raycastTarget = false;
        }

        if (amountText != null)
        {
            amountText.raycastTarget = false;
        }

        if (highlightImage != null)
        {
            highlightImage.raycastTarget = false;
            highlightImage.gameObject.SetActive(false);
        }
    }

    public void Initialize(
        InventoryUI inventoryUI,
        RectTransform contentArea,
        int slotIndex)
    {
        this.inventoryUI = inventoryUI;
        this.contentArea = contentArea;
        this.slotIndex = slotIndex;
    }

    public void SetItem(InventoryItemData item)
    {
        currentItem = item;

        if (item == null)
        {
            ClearSlot();
            return;
        }

        Sprite icon =
            GetIcon(item.colorType);

        if (itemIcon != null)
        {
            itemIcon.sprite = icon;
            itemIcon.enabled = icon != null;
            itemIcon.color = Color.white;
        }

        if (amountText != null)
        {
            amountText.text =
                item.amount.ToString();

            amountText.gameObject.SetActive(
                item.amount > 1
            );
        }
    }

    public void ClearSlot()
    {
        currentItem = null;

        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
        }

        if (amountText != null)
        {
            amountText.text = "";
            amountText.gameObject.SetActive(false);
        }

        HideHighlight();
    }

    private Sprite GetIcon(
        SlimeColorType colorType)
    {
        if (itemIcons == null)
            return null;

        foreach (ItemIconData iconData in itemIcons)
        {
            if (iconData.colorType == colorType)
                return iconData.icon;
        }

        return null;
    }

    public InventoryItemData GetItem()
    {
        return currentItem;
    }

    public int Index => slotIndex;

    public void OnPointerClick(PointerEventData eventData)
    {
        // 우클릭인지 확인
        if (eventData.button != PointerEventData.InputButton.Right)
            return;

        // 빈 슬롯이면 사용하지 않음
        if (currentItem == null)
            return;

        // 드래그 중이면 사용하지 않음
        if (isDragging)
            return;

        if (inventoryUI == null)
            return;

        // BodyPartSelectionPopup 열기
        inventoryUI.OpenBodyPartPopup(currentItem);
    }

    public void OnPointerEnter(
        PointerEventData eventData)
    {
        if (isDragging)
            return;

        if (inventoryUI != null &&
            inventoryUI.IsDragging)
            return;

        ShowHighlight();
    }

    public void OnPointerExit(
        PointerEventData eventData)
    {
        HideHighlight();
    }

    public void OnBeginDrag(
        PointerEventData eventData)
    {
        if (currentItem == null)
            return;

        if (inventoryUI == null ||
            contentArea == null)
            return;

        isDragging = true;

        inventoryUI.SetDragging(true);

        inventoryUI.HideAllHighlights();

        transform.SetAsLastSibling();
    }

    public void OnDrag(
        PointerEventData eventData)
    {
        if (!isDragging)
            return;

        Camera eventCamera = null;

        if (canvas != null &&
            canvas.renderMode !=
            RenderMode.ScreenSpaceOverlay)
        {
            eventCamera = canvas.worldCamera;
        }

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            contentArea,
            eventData.position,
            eventCamera,
            out Vector2 localPosition))
        {
            slotRect.anchoredPosition =
                localPosition;
        }
    }

    public void OnEndDrag(
        PointerEventData eventData)
    {
        if (!isDragging)
            return;

        isDragging = false;

        ItemSlotUI endDragSlot =
            RaycastAndGetFirstComponent(
                eventData
            );

        if (endDragSlot != null &&
            endDragSlot != this)
        {
            InventoryManager.Instance.Swap(
                Index,
                endDragSlot.Index
            );
        }

        if (inventoryUI != null)
        {
            inventoryUI.SetDragging(false);
            inventoryUI.HideAllHighlights();
            inventoryUI.Refresh();
        }

        if (endDragSlot != null &&
            endDragSlot != this)
        {
            endDragSlot.ShowHighlight();
        }
    }

    private ItemSlotUI RaycastAndGetFirstComponent(
        PointerEventData eventData)
    {
        if (EventSystem.current == null)
            return null;

        List<RaycastResult> results =
            new List<RaycastResult>();

        EventSystem.current.RaycastAll(
            eventData,
            results
        );

        foreach (RaycastResult result in results)
        {
            ItemSlotUI slot =
                result.gameObject
                    .GetComponentInParent<ItemSlotUI>();

            if (slot == null)
                continue;

            if (slot == this)
                continue;

            return slot;
        }

        return null;
    }

    public void ShowHighlight()
    {
        if (inventoryUI != null &&
            inventoryUI.IsDragging)
            return;

        if (highlightImage != null)
        {
            highlightImage.gameObject.SetActive(true);
        }
    }

    public void HideHighlight()
    {
        if (highlightImage != null)
        {
            highlightImage.gameObject.SetActive(false);
        }
    }
}