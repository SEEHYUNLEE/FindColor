using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    private static InventoryManager instance;
    public static InventoryManager Instance => instance;

    [Header("Inventory Settings")]
    [SerializeField] private int defaultCapacity = 20;

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

            return DataManager.Instance.currentData.inventoryCapacity;
        }
    }

    public List<InventoryItemData> Items
    {
        get
        {
            if (DataManager.Instance == null ||
                DataManager.Instance.currentData == null)
            {
                return null;
            }

            if (DataManager.Instance.currentData.inventoryItems == null)
            {
                DataManager.Instance.currentData.inventoryItems =
                    new List<InventoryItemData>();
            }

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
            ToggleInventory();
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindInventoryUI();
    }

    private void FindInventoryUI()
    {
        inventoryUI = FindFirstObjectByType<InventoryUI>(FindObjectsInactive.Include);

        if (inventoryUI != null)
        {
            inventoryUI.Close();
        }
    }

    public void Initialize()
    {
        if (DataManager.Instance == null)
            return;

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

    public bool AddItem(SlimeColorType colorType, int amount = 1)
    {
        if (amount <= 0)
            return false;

        Initialize();

        InventoryItemData existingItem =
            Items.Find(item => item.colorType == colorType);

        if (existingItem != null)
        {
            existingItem.amount += amount;
        }
        else
        {
            if (Items.Count >= Capacity)
            {
                Debug.LogWarning("인벤토리가 가득 찼습니다.");
                return false;
            }

            Items.Add(new InventoryItemData(colorType, amount));
        }

        DataManager.Instance.SaveCurrentSlot();

        if (inventoryUI != null)
        {
            inventoryUI.Refresh();
        }

        return true;
    }

    public bool RemoveItem(SlimeColorType colorType, int amount = 1)
    {
        if (amount <= 0)
            return false;

        Initialize();

        InventoryItemData item =
            Items.Find(value => value.colorType == colorType);

        if (item == null || item.amount < amount)
            return false;

        item.amount -= amount;

        if (item.amount <= 0)
        {
            Items.Remove(item);
        }

        DataManager.Instance.SaveCurrentSlot();

        if (inventoryUI != null)
        {
            inventoryUI.Refresh();
        }

        return true;
    }

    public bool HasItem(SlimeColorType colorType, int amount = 1)
    {
        if (Items == null)
            return false;

        InventoryItemData item =
            Items.Find(value => value.colorType == colorType);

        return item != null && item.amount >= amount;
    }

    public int GetItemAmount(SlimeColorType colorType)
    {
        if (Items == null)
            return 0;

        InventoryItemData item =
            Items.Find(value => value.colorType == colorType);

        return item != null ? item.amount : 0;
    }

    public bool ExpandInventory(int amount)
    {
        if (amount <= 0)
            return false;

        Initialize();

        DataManager.Instance.currentData.inventoryCapacity += amount;

        DataManager.Instance.SaveCurrentSlot();

        if (inventoryUI != null)
        {
            inventoryUI.Refresh();
        }

        return true;
    }
}