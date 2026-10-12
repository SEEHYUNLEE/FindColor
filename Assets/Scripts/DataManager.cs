using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public string playerName = "Player";
    public int stage = 1;
    public int gold = 0;

    public int hpLevel = 0;
    public int speedLevel = 0;
    public int damageLevel = 0;

    // HEX 색상 문자열 목록 저장
    public List<string> bodyPartHexColors = new List<string>();

    // 이미 색상이 변경된 부위의 인덱스 목록
    public List<int> coloredParts = new List<int>();

    public int inventoryCapacity = 20;
    public List<InventoryItemData> inventoryItems = new List<InventoryItemData>();
}

public enum StatType { Hp, Speed, Damage }

public class DataManager : MonoBehaviour
{
    private static DataManager instance;
    public static DataManager Instance
    {
        get
        {
            if (instance == null)
                return null;
            return instance;
        }
    }

    // 현재 게임에서 사용 중인 데이터
    public PlayerData currentData = new PlayerData();

    // 현재 선택해서 플레이 중인 슬롯 번호 (기본값 -1 : 선택되지 않음)
    public int currentSlotIndex { get; private set; } = -1;

    // 실제로 슬롯이 선택/로드되었는지 여부
    private bool isSlotLoaded = false;

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

    public string GetSavePath(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, $"SaveSlot_{slotIndex}.json");
    }

    public int GetSlotStage(int slotIndex)
    {
        if (slotIndex < 1 || slotIndex > 3)
            return 0;

        string path = GetSavePath(slotIndex);

        if (!File.Exists(path))
            return 0;

        string json = File.ReadAllText(path);
        PlayerData data = JsonConvert.DeserializeObject<PlayerData>(json);
        if (data == null)
            return 0;

        return data.stage;

    }

    public void SelectSlot(int slotIndex)
    {
        currentSlotIndex = Mathf.Clamp(slotIndex, 1, 3);
    }

    // 현재 선택된 슬롯에 데이터 저장
    public void SaveCurrentSlot()
    {
        // 덮어쓰기 방지
        if (!isSlotLoaded || currentSlotIndex < 1)
        {
            return;
        }

        string path = GetSavePath(currentSlotIndex);
        string json = JsonConvert.SerializeObject(currentData, Formatting.Indented);

        File.WriteAllText(path, json);
    }

    // 지정된 슬롯 데이터 불러오기
    public bool LoadSlot(int slotIndex)
    {
        SelectSlot(slotIndex);
        string path = GetSavePath(slotIndex);
        isSlotLoaded = true; // 슬롯 선택 완료

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            currentData = JsonConvert.DeserializeObject<PlayerData>(json);
            return true;
        }
        else
        {
            currentData = new PlayerData(); // 파일이 없으면 초기화
            SaveCurrentSlot(); // 새 파일 생성
            return false;
        }
    }

    public bool HasSaveFile(int slotIndex)
    {
        return File.Exists(GetSavePath(slotIndex));
    }

    public void QuitGame()
    {
        SaveCurrentSlot();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnApplicationQuit()
    {
        // 슬롯이 로드된 상태일 때만 안전하게 저장
        if (isSlotLoaded)
        {
            SaveCurrentSlot();
        }
    }

    public void DeleteSlot(int slotIndex)
    {
        string path = GetSavePath(slotIndex);

        if (File.Exists(path))
        {
            File.Delete(path);
        }

        if (currentSlotIndex == slotIndex)
        {
            currentData = new PlayerData();
            isSlotLoaded = false; // 현재 슬롯 삭제 시 로드 상태 해제
            currentSlotIndex = -1;
        }
    }

    const int UPGRADE_COST = 100;
    const int MAX_LEVEL = 7;

    public bool UpgradeStat(StatType statType)
    {
        if (currentData.gold < UPGRADE_COST)
        {
            return false;
        }

        switch (statType)
        {
            case StatType.Hp:
                if (currentData.hpLevel >= MAX_LEVEL) return false;
                currentData.hpLevel++;
                AudioManager.Instance.PlaySFX(SFXType.Upgrade);
                break;

            case StatType.Speed:
                if (currentData.speedLevel >= MAX_LEVEL) return false;
                currentData.speedLevel++;
                AudioManager.Instance.PlaySFX(SFXType.Upgrade);
                break;

            case StatType.Damage:
                if (currentData.damageLevel >= MAX_LEVEL) return false;
                currentData.damageLevel++;
                AudioManager.Instance.PlaySFX(SFXType.Upgrade);
                break;
        }

        currentData.gold -= UPGRADE_COST;
        SaveCurrentSlot(); // 강화 즉시 파일 저장
        return true;
    }
    public void AddGold(int amount = 100)
    {
        currentData.gold += amount;
        SaveCurrentSlot();
    }

    public void SetPlayerName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return;

        currentData.playerName = newName.Trim();

        SaveCurrentSlot();
    }
}