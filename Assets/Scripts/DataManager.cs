using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public string playerName = "Player";
    public int stage = 1;

    // HEX 색상 문자열 목록 저장
    public List<string> bodyPartHexColors = new List<string>();

    // 이미 색상이 변경된 부위의 인덱스 목록
    public List<int> coloredParts = new List<int>();
}

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

    public void SelectSlot(int slotIndex)
    {
        currentSlotIndex = Mathf.Clamp(slotIndex, 1, 3);
    }

    // 현재 선택된 슬롯에 데이터 저장
    public void SaveCurrentSlot()
    {
        // 슬롯이 정식으로 선택되지 않은 상태라면 저장하지 않음 (덮어쓰기 방지)
        if (!isSlotLoaded || currentSlotIndex < 1)
        {
            Debug.LogWarning("[Save] 선택된 슬롯이 없어 저장을 건너뜁니다.");
            return;
        }

        string path = GetSavePath(currentSlotIndex);
        string json = JsonConvert.SerializeObject(currentData, Formatting.Indented);

        File.WriteAllText(path, json);
        Debug.Log($"[Save] {currentSlotIndex}번 슬롯에 저장되었습니다: {path}");
    }

    // 지정된 슬롯 데이터 불러오기
    public bool LoadSlot(int slotIndex)
    {
        SelectSlot(slotIndex);
        string path = GetSavePath(slotIndex);
        isSlotLoaded = true; // 슬롯 선택/로드 완료 플래그 활성화

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            currentData = JsonConvert.DeserializeObject<PlayerData>(json);
            Debug.Log($"[Load] {slotIndex}번 슬롯의 데이터를 불러왔습니다.");
            return true;
        }
        else
        {
            Debug.Log($"[Load] {slotIndex}번 슬롯에 저장 파일이 없습니다. 새 데이터를 생성합니다.");
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
        Debug.Log("게임을 저장하고 종료합니다.");

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
            Debug.Log($"[Delete] {slotIndex}번 슬롯 파일이 삭제되었습니다.");
        }

        if (currentSlotIndex == slotIndex)
        {
            currentData = new PlayerData();
            isSlotLoaded = false; // 현재 슬롯 삭제 시 로드 상태 해제
            currentSlotIndex = -1;
        }
    }
}