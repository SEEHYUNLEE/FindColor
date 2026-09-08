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

    private TMP_Text slot1Text;
    private TMP_Text slot2Text;
    private TMP_Text slot3Text;

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
        return $"[슬롯 {slotIndex}]\n새로운 게임 시작";
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