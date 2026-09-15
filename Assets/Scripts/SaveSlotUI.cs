using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Newtonsoft.Json;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    [Header("Slot Buttons")]
    [SerializeField] private Button slot1Button;
    [SerializeField] private Button slot2Button;
    [SerializeField] private Button slot3Button;

    [Header("Delete Buttons")]
    [SerializeField] private Button delete1Button;
    [SerializeField] private Button delete2Button;
    [SerializeField] private Button delete3Button;

    [Header("UI Reference")]
    [SerializeField] private DeleteConfirmUI deleteConfirmUI;

    [Header("Player Model Settings")]
    [SerializeField] private GameObject playerPrefab; // 스폰할 플레이어 프리팹
    [SerializeField] private Transform slot1PlayerSpawnPoint; // 슬롯 1 캐릭터 위치
    [SerializeField] private Transform slot2PlayerSpawnPoint; // 슬롯 2 캐릭터 위치
    [SerializeField] private Transform slot3PlayerSpawnPoint; // 슬롯 3 캐릭터 위치

    private TMP_Text slot1Text;
    private TMP_Text slot2Text;
    private TMP_Text slot3Text;

    // 생성된 플레이어 오브젝트를 관리하기 위한 변수
    private GameObject player1Instance;
    private GameObject player2Instance;
    private GameObject player3Instance;

    private void Awake()
    {
        if (slot1Button != null) slot1Text = slot1Button.GetComponentInChildren<TMP_Text>();
        if (slot2Button != null) slot2Text = slot2Button.GetComponentInChildren<TMP_Text>();
        if (slot3Button != null) slot3Text = slot3Button.GetComponentInChildren<TMP_Text>();

        if (slot1Button != null) slot1Button.onClick.AddListener(() => SelectSlotAndStartGame(1));
        if (slot2Button != null) slot2Button.onClick.AddListener(() => SelectSlotAndStartGame(2));
        if (slot3Button != null) slot3Button.onClick.AddListener(() => SelectSlotAndStartGame(3));

        if (delete1Button != null) delete1Button.onClick.AddListener(() => OpenDeletePopup(1));
        if (delete2Button != null) delete2Button.onClick.AddListener(() => OpenDeletePopup(2));
        if (delete3Button != null) delete3Button.onClick.AddListener(() => OpenDeletePopup(3));
    }

    private void OnEnable()
    {
        UpdateSlotUI();
    }

    public void UpdateSlotUI()
    {
        if (slot1Text != null) slot1Text.text = GetSlotInfoString(1);
        if (slot2Text != null) slot2Text.text = GetSlotInfoString(2);
        if (slot3Text != null) slot3Text.text = GetSlotInfoString(3);

        if (delete1Button != null) delete1Button.gameObject.SetActive(HasSaveFile(1));
        if (delete2Button != null) delete2Button.gameObject.SetActive(HasSaveFile(2));
        if (delete3Button != null) delete3Button.gameObject.SetActive(HasSaveFile(3));

        // 플레이어 캐릭터 표시 업데이트
        UpdatePlayerModels();
    }

    private void UpdatePlayerModels()
    {
        UpdateSinglePlayerModel(1, slot1PlayerSpawnPoint, ref player1Instance);
        UpdateSinglePlayerModel(2, slot2PlayerSpawnPoint, ref player2Instance);
        UpdateSinglePlayerModel(3, slot3PlayerSpawnPoint, ref player3Instance);
    }

    private void UpdateSinglePlayerModel(int slotIndex, Transform spawnPoint, ref GameObject instance)
    {
        bool hasSave = HasSaveFile(slotIndex);

        if (hasSave)
        {
            // 1. 프리팹이 없다면 생성
            if (instance == null && playerPrefab != null && spawnPoint != null)
            {
                instance = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation, spawnPoint);
            }

            // 2. 모델 활성화
            if (instance != null)
            {
                instance.SetActive(true);

                // 3. 해당 슬롯의 JSON 파일을 읽어와서 색상 적용
                string path = Path.Combine(Application.persistentDataPath, $"SaveSlot_{slotIndex}.json");
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    PlayerData saveData = JsonConvert.DeserializeObject<PlayerData>(json);

                    PlayerUi uiColor = instance.GetComponent<PlayerUi>();
                    if (saveData != null)
                    {
                        uiColor.SetColorData(saveData);
                    }
                }
            }
        }
        else
        {
            // 저장 데이터가 없는 슬롯은 비활성화
            if (instance != null)
            {
                instance.SetActive(false);
            }
        }
    }

    private string GetSlotInfoString(int slotIndex)
    {
        string path = Path.Combine(Application.persistentDataPath, $"SaveSlot_{slotIndex}.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            PlayerData saveData = JsonConvert.DeserializeObject<PlayerData>(json);
            return $"[슬롯 {slotIndex}]\n" +
                   $"이름: {saveData?.playerName}\n" +
                   $"스테이지: {saveData?.stage}";
        }
        return $"[슬롯 {slotIndex}]\n\n\n\n\n\n새로운 게임 시작";
    }

    private bool HasSaveFile(int slotIndex)
    {
        string path = Path.Combine(Application.persistentDataPath, $"SaveSlot_{slotIndex}.json");
        return File.Exists(path);
    }

    private void SelectSlotAndStartGame(int slotIndex)
    {
        // 1. 슬롯 데이터 불러오기 (없는 경우 DataManager 내부에서 자동 생성 및 저장)
        if (DataManager.Instance != null)
        {
            DataManager.Instance.LoadSlot(slotIndex);
        }

        // 2. 메인 마을/로비(Main) 씬으로 이동
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMainScene();
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
        }
    }

    private void OpenDeletePopup(int slotIndex)
    {
        if (deleteConfirmUI != null)
        {
            deleteConfirmUI.OpenPopup(slotIndex, this);
        }
    }
}