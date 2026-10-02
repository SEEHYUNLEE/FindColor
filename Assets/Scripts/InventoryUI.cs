using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("Slot Options")]
    [SerializeField] private int horizontalSlotCount = 5;

    [SerializeField] private float slotMargin = 0f;

    [SerializeField] private float contentAreaPadding = 10f;

    [SerializeField] private float slotSize = 100f;

    [Header("Connected Objects")]
    [SerializeField] private RectTransform contentArea;
    [SerializeField] private ItemSlotUI itemSlotPrefab;

    private readonly List<ItemSlotUI> slotList =
        new List<ItemSlotUI>();

    public bool IsOpen => gameObject.activeSelf;

    private void Start()
    {
        // Content Area의 기준점을 왼쪽 위로 설정
        if (contentArea != null)
        {
            contentArea.pivot = new Vector2(0f, 1f);
        }

        Refresh();
    }

    public void Open()
    {
        transform.localPosition = Vector3.zero;

        gameObject.SetActive(true);

        Refresh();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void Toggle()
    {
        if (IsOpen)
            Close();
        else
            Open();
    }

    public void Refresh()
    {
        if (InventoryManager.Instance == null)
            return;

        if (contentArea == null)
        {
            Debug.LogWarning("[InventoryUI] Content Area가 연결되지 않았습니다.");
            return;
        }

        if (itemSlotPrefab == null)
        {
            Debug.LogWarning("[InventoryUI] Item Slot Prefab이 연결되지 않았습니다.");
            return;
        }

        InitSlots();

        List<InventoryItemData> items =
            InventoryManager.Instance.Items;

        if (items == null)
            return;

        for (int i = 0; i < slotList.Count; i++)
        {
            if (i < items.Count)
            {
                slotList[i].SetItem(items[i]);
            }
            else
            {
                slotList[i].ClearSlot();
            }
        }
    }

    private void InitSlots()
    {
        int slotCount = InventoryManager.Instance.Capacity;

        // 이미 만들어진 슬롯보다 필요한 슬롯이 많으면 생성
        while (slotList.Count < slotCount)
        {
            CreateSlot(slotList.Count);
        }

        // 인벤토리 최대 칸 수가 줄었다면 제거
        while (slotList.Count > slotCount)
        {
            int lastIndex = slotList.Count - 1;

            ItemSlotUI slot = slotList[lastIndex];

            slotList.RemoveAt(lastIndex);

            if (slot != null)
            {
                Destroy(slot.gameObject);
            }
        }

        PositionSlots(slotCount);
    }

    private void CreateSlot(int slotIndex)
    {
        ItemSlotUI slotUI =
            Instantiate(itemSlotPrefab, contentArea);

        RectTransform slotRT =
            slotUI.GetComponent<RectTransform>();

        slotRT.sizeDelta =
            new Vector2(slotSize, slotSize);

        slotRT.anchorMin =
            new Vector2(0f, 1f);

        slotRT.anchorMax =
            new Vector2(0f, 1f);

        slotRT.pivot =
            new Vector2(0f, 1f);

        slotUI.gameObject.SetActive(true);

        slotUI.gameObject.name =
            $"Item Slot [{slotIndex}]";

        slotList.Add(slotUI);
    }

    private void PositionSlots(int slotCount)
    {
        float contentWidth =
            contentArea.rect.width;

        int actualHorizontalCount =
            Mathf.FloorToInt(
                (contentWidth -
                 contentAreaPadding * 2f +
                 slotMargin) /
                (slotSize + slotMargin)
            );

        actualHorizontalCount =
            Mathf.Max(1, actualHorizontalCount);

        actualHorizontalCount =
            Mathf.Min(
                actualHorizontalCount,
                horizontalSlotCount
            );

        int rowCount =
            Mathf.CeilToInt(
                (float)slotCount /
                actualHorizontalCount
            );

        for (int i = 0; i < slotList.Count; i++)
        {
            int column =
                i % actualHorizontalCount;

            int row =
                i / actualHorizontalCount;

            float x =
                contentAreaPadding +
                column * (slotSize + slotMargin);

            float y =
                -contentAreaPadding -
                row * (slotSize + slotMargin);

            RectTransform slotRT =
                slotList[i].GetComponent<RectTransform>();

            slotRT.anchoredPosition =
                new Vector2(x, y);
        }

        float contentHeight =
            contentAreaPadding * 2f +
            rowCount * slotSize +
            Mathf.Max(0, rowCount - 1) * slotMargin;

        contentArea.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            contentHeight
        );
    }
}