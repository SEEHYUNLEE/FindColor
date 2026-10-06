using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InventoryManager : MonoBehaviour
{
    private static InventoryManager instance;
    public static InventoryManager Instance => instance;

    [Header("Inventory Settings")]
    [SerializeField] private int defaultCapacity = 10;

    private InventoryUI inventoryUI;

    public int Capacity
    {
        get
        {
            if (DataManager.Instance == null ||
                DataManager.Instance.currentData == null)
            {
                return defaultCapacity;
            }

            if (DataManager.Instance.currentData.inventoryCapacity <= 0)
            {
                DataManager.Instance.currentData.inventoryCapacity =
                    defaultCapacity;
            }

            return DataManager.Instance.currentData.inventoryCapacity;
        }
    }

    public List<InventoryItemData> Items
    {
        get
        {
            Initialize();
            return DataManager.Instance.currentData.inventoryItems;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        Initialize();
        FindInventoryUI();
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.iKey.wasPressedThisFrame)
        {
            if (inventoryUI == null)
            {
                FindInventoryUI();
            }

            if (inventoryUI == null)
                return;

            ToggleInventory();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindInventoryUI();
    }

    private void FindInventoryUI()
    {
        inventoryUI =
            FindFirstObjectByType<InventoryUI>(
                FindObjectsInactive.Include
            );

        if (inventoryUI != null)
        {
            inventoryUI.Close();
        }
    }

    public void Initialize()
    {
        if (DataManager.Instance == null ||
            DataManager.Instance.currentData == null)
        {
            return;
        }

        if (DataManager.Instance.currentData.inventoryItems == null)
        {
            DataManager.Instance.currentData.inventoryItems =
                new List<InventoryItemData>();
        }

        if (DataManager.Instance.currentData.inventoryCapacity <= 0)
        {
            DataManager.Instance.currentData.inventoryCapacity =
                defaultCapacity;
        }

        // 저장된 아이템 리스트를 실제 슬롯 수만큼 맞춤
        while (DataManager.Instance.currentData.inventoryItems.Count <
               DataManager.Instance.currentData.inventoryCapacity)
        {
            DataManager.Instance.currentData.inventoryItems.Add(null);
        }

        // 슬롯 수보다 리스트가 큰 경우
        while (DataManager.Instance.currentData.inventoryItems.Count >
               DataManager.Instance.currentData.inventoryCapacity)
        {
            int lastIndex =
                DataManager.Instance.currentData.inventoryItems.Count - 1;

            if (DataManager.Instance.currentData.inventoryItems[lastIndex] != null)
            {
                break;
            }

            DataManager.Instance.currentData.inventoryItems.RemoveAt(lastIndex);
        }
    }

    public bool AddItem(
        SlimeColorType colorType,
        int amount = 1)
    {
        if (amount <= 0)
            return false;

        Initialize();

        List<InventoryItemData> items = Items;

        // 같은 아이템이 이미 존재하면 수량 증가
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] != null &&
                items[i].colorType == colorType)
            {
                items[i].amount += amount;

                DataManager.Instance.SaveCurrentSlot();

                if (inventoryUI != null)
                    inventoryUI.Refresh();

                return true;
            }
        }

        // 빈 슬롯 찾기
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null)
            {
                items[i] =
                    new InventoryItemData(colorType, amount);

                DataManager.Instance.SaveCurrentSlot();

                if (inventoryUI != null)
                    inventoryUI.Refresh();

                return true;
            }
        }

        Debug.LogWarning("인벤토리가 가득 찼습니다.");
        return false;
    }

    public bool RemoveItem(
        SlimeColorType colorType,
        int amount = 1)
    {
        if (amount <= 0)
            return false;

        Initialize();

        List<InventoryItemData> items = Items;

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i] == null)
                continue;

            if (items[i].colorType != colorType)
                continue;

            if (items[i].amount < amount)
                return false;

            items[i].amount -= amount;

            if (items[i].amount <= 0)
            {
                items[i] = null;
            }

            DataManager.Instance.SaveCurrentSlot();

            if (inventoryUI != null)
                inventoryUI.Refresh();

            return true;
        }

        return false;
    }

    public bool Swap(
        int indexA,
        int indexB)
    {
        Initialize();

        List<InventoryItemData> items = Items;

        if (!IsValidIndex(indexA) ||
            !IsValidIndex(indexB))
        {
            return false;
        }

        if (indexA == indexB)
            return false;

        InventoryItemData itemA = items[indexA];
        InventoryItemData itemB = items[indexB];

        // 비어있는 슬롯으로 이동
        if (itemA != null && itemB == null)
        {
            items[indexB] = itemA;
            items[indexA] = null;
        }
        // 비어있는 슬롯에서 시작하는 경우
        else if (itemA == null && itemB != null)
        {
            items[indexA] = itemB;
            items[indexB] = null;
        }
        // 두 슬롯 모두 아이템 존재
        else if (itemA != null && itemB != null)
        {
            // 같은 아이템이면 합치기
            if (itemA.colorType == itemB.colorType)
            {
                itemB.amount += itemA.amount;
                items[indexA] = null;
            }
            // 서로 다른 아이템이면 위치 교환
            else
            {
                items[indexA] = itemB;
                items[indexB] = itemA;
            }
        }

        DataManager.Instance.SaveCurrentSlot();

        if (inventoryUI != null)
            inventoryUI.Refresh();

        return true;
    }

    private bool IsValidIndex(int index)
    {
        return index >= 0 &&
               index < Items.Count;
    }

    public bool HasItem(
        SlimeColorType colorType,
        int amount = 1)
    {
        if (Items == null)
            return false;

        foreach (InventoryItemData item in Items)
        {
            if (item != null &&
                item.colorType == colorType &&
                item.amount >= amount)
            {
                return true;
            }
        }

        return false;
    }

    public int GetItemAmount(
        SlimeColorType colorType)
    {
        if (Items == null)
            return 0;

        foreach (InventoryItemData item in Items)
        {
            if (item != null &&
                item.colorType == colorType)
            {
                return item.amount;
            }
        }

        return 0;
    }

    public bool ExpandInventory(int amount)
    {
        if (amount <= 0)
            return false;

        Initialize();

        DataManager.Instance.currentData.inventoryCapacity += amount;

        Initialize();

        DataManager.Instance.SaveCurrentSlot();

        if (inventoryUI != null)
            inventoryUI.Refresh();

        return true;
    }

    private void ToggleInventory()
    {
        if (inventoryUI == null)
            return;

        if (inventoryUI.IsOpen)
            inventoryUI.Close();
        else
            inventoryUI.Open();
    }
}