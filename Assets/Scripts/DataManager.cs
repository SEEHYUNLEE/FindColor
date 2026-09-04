using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public string playerName = "Player";
    public int stage = 1;
    public int coin = 0;

    // HEX 색상 문자열 목록 저장
    public List<string> bodyPartHexColors = new List<string>();

    // 이미 색상이 변경된 부위의 인덱스 목록
    public List<int> coloredParts = new List<int>();
}

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    // 현재 게임에서 사용 중인 데이터
    public PlayerData currentData = new PlayerData();

    // 현재 선택해서 플레이 중인 슬롯 번호 (기본값 1)
    public int currentSlotIndex { get; private set; } = 1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    // 영구 저장 경로(persistentDataPath) 반환
    public string GetSavePath(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, $"SaveSlot_{slotIndex}.json");
    }

    // 슬롯 지정
    public void SelectSlot(int slotIndex)
    {
        currentSlotIndex = Mathf.Clamp(slotIndex, 1, 3);
    }

    // 현재 선택된 슬롯에 데이터 저장
    public void SaveCurrentSlot()
    {
        string path = GetSavePath(currentSlotIndex);
        string json = JsonConvert.SerializeObject(currentData, Formatting.Indented);

        File.WriteAllText(path, json);
        Debug.Log($"[Save] {currentSlotIndex}번 슬롯에 저장되었습니다: {path}");
    }

    // 지정된 슬롯 데이터 불러오기 (슬롯 선택 + 불러오기 동시 수행)
    public bool LoadSlot(int slotIndex)
    {
        SelectSlot(slotIndex);
        string path = GetSavePath(slotIndex);

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

    // 특정 슬롯에 세이브 파일이 존재하는지 확인
    public bool HasSaveFile(int slotIndex)
    {
        return File.Exists(GetSavePath(slotIndex));
    }

    // 게임 종료 및 저장 처리
    public void QuitGame()
    {
        SaveCurrentSlot(); // 저장 후 종료
        Debug.Log("게임을 저장하고 종료합니다.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터 모드 종료
#else
        Application.Quit(); // 빌드된 게임 종료
#endif
    }

    // 앱이 강제 종료되거나 모바일에서 백그라운드로 전환될 때 자동 저장
    private void OnApplicationQuit()
    {
        SaveCurrentSlot();
    }
}