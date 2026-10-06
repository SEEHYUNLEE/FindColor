using UnityEngine;
using UnityEngine.UI;

public class BodyPartSelectionPopup : MonoBehaviour
{
    [Header("Body Part Buttons")]
    [SerializeField] private Button[] bodyPartButtons;

    private InventoryItemData selectedItem;
    private PlayerColorManager playerColorManager;

    [SerializeField] private RectTransform panel;

    private void Awake()
    {
        for (int i = 0; i < bodyPartButtons.Length; i++)
        {
            if (bodyPartButtons[i] == null)
                continue;

            int index = i;

            bodyPartButtons[i].onClick.AddListener(
                () => SelectBodyPart(index)
            );
        }

        Hide();
    }

    public void Show(InventoryItemData item)
    {
        if (item == null)
            return;

        selectedItem = item;

        if (playerColorManager == null)
        {
            playerColorManager =
                FindFirstObjectByType<PlayerColorManager>();
        }

        if (playerColorManager == null)
        {
            return;
        }

        gameObject.SetActive(true);

        panel.anchoredPosition = Vector2.zero;

        transform.SetAsLastSibling();
    }

    private void SelectBodyPart(int partIndex)
    {
        if (selectedItem == null)
            return;

        if (playerColorManager == null)
        {
            playerColorManager =
                FindFirstObjectByType<PlayerColorManager>();
        }

        if (playerColorManager == null)
            return;

        Color itemColor =
            GetColor(selectedItem.colorType);

        bool success =
            playerColorManager.ApplyColorToPart(
                partIndex,
                itemColor
            );

        if (!success)
            return;

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.RemoveItem(
                selectedItem.colorType,
                1
            );
        }

        selectedItem = null;

        Hide();
    }

    private Color GetColor(SlimeColorType colorType)
    {
        foreach (SlimeColorData colorData in
                 SlimeColorPalette.Colors)
        {
            if (colorData.colorType == colorType)
            {
                return colorData.color;
            }
        }

        return Color.white;
    }

    public void Hide()
    {
        selectedItem = null;
        gameObject.SetActive(false);
    }
}