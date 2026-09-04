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
    [SerializeField] private DeleteConfirmUI deleteConfirmUI; // 삭제 확인 팝업 UI

    // 자식 텍스트들을 저장할 내부 변수
    private TMP_Text slot1Text;
    private TMP_Text slot2Text;
    private TMP_Text slot3Text;

    private void Awake()
    {
        // 1. 슬롯 버튼 자식의 TextMeshPro 컴포넌트 가져오기
        if (slot1Button != null) slot1Text = slot1Button.GetComponentInChildren<TMP_Text>();
        if (slot2Button != null) slot2Text = slot2Button.GetComponentInChildren<TMP_Text>();
        if (slot3Button != null) slot3Text = slot3Button.GetComponentInChildren<TMP_Text>();

        // 2. 슬롯 클릭 이벤트 연결
        if (slot1Button != null) slot1Button.onClick.AddListener(() => SelectSlotAndStartGame(1));
        if (slot2Button != null) slot2Button.onClick.AddListener(() => SelectSlotAndStartGame(2));
        if (slot3Button != null) slot3Button.onClick.AddListener(() => SelectSlotAndStartGame(3));

        // 3. 삭제(X) 버튼 클릭 이벤트 연결
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

        // 데이터가 없는 슬롯은 삭제(X) 버튼 숨기기
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
                   $"스테이지: {saveData?.stage} | 코인: {saveData?.coin}";
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
        DataManager.Instance.LoadSlot(slotIndex);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }

    // X 버튼 클릭 시 삭제 팝업 호출
    private void OpenDeletePopup(int slotIndex)
    {
        if (deleteConfirmUI != null)
        {
            deleteConfirmUI.OpenPopup(slotIndex, this);
        }
    }
}