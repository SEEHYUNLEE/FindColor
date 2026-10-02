// ItemSlotUI.cs
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text amountText;

    [Header("Item Icons")]
    [SerializeField] private ItemIconData[] itemIcons;

    private InventoryItemData currentItem;

    public void SetItem(InventoryItemData item)
    {
        currentItem = item;

        if (item == null)
        {
            ClearSlot();
            return;
        }

        Sprite icon = GetIcon(item.colorType);

        if (itemIcon != null)
        {
            itemIcon.sprite = icon;
            itemIcon.enabled = icon != null;
            itemIcon.color = Color.white;
        }

        if (amountText != null)
        {
            amountText.text = item.amount.ToString();
            amountText.gameObject.SetActive(item.amount > 1);
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
    }

    private Sprite GetIcon(SlimeColorType colorType)
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
}