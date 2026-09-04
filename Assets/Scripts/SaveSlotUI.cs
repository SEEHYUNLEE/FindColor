using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Newtonsoft.Json;
using TMPro;

public class SaveSlotUI : MonoBehaviour
{
    // [수정] 텍스트 대신 버튼 3개만 인스펙터에 연결
    [Header("Slot Buttons")]
    [SerializeField] private Button slot1Button;
    [SerializeField] private Button slot2Button;
    [SerializeField] private Button slot3Button;

    // 자식 텍스트들을 저장할 내부 변수
    private TMP_Text slot1Text;
    private TMP_Text slot2Text;
    private TMP_Text slot3Text;

    private void Awake()
    {
        // 1. 버튼 자식에 있는 TextMeshPro 컴포넌트 자동 가져오기
        if (slot1Button != null) slot1Text = slot1Button.GetComponentInChildren<TMP_Text>();
        if (slot2Button != null) slot2Text = slot2Button.GetComponentInChildren<TMP_Text>();
        if (slot3Button != null) slot3Text = slot3Button.GetComponentInChildren<TMP_Text>();

        // 2. 버튼 클릭 이벤트(함수) 코드로 자동 연결
        if (slot1Button != null) slot1Button.onClick.AddListener(() => SelectSlotAndStartGame(1));
        if (slot2Button != null) slot2Button.onClick.AddListener(() => SelectSlotAndStartGame(2));
        if (slot3Button != null) slot3Button.onClick.AddListener(() => SelectSlotAndStartGame(3));
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
    }
    private string GetSlotInfoString(int slotIndex)
    {
        // DataManager를 거치지 않고 Application.persistentDataPath 경로 직접 조립
        string path = Path.Combine(Application.persistentDataPath, $"SaveSlot_{slotIndex}.json");
        bool exists = File.Exists(path);

        Debug.Log($"[{slotIndex}번 슬롯] 직접 검사 경로: {path} | 존재 여부: {exists}");

        if (exists)
        {
            string json = File.ReadAllText(path);
            PlayerData saveData = JsonConvert.DeserializeObject<PlayerData>(json);

            return $"[슬롯 {slotIndex}]\n" +
                   $"이름: {saveData?.playerName}\n" +
                   $"스테이지: {saveData?.stage} | 코인: {saveData?.coin}";
        }

        return $"[슬롯 {slotIndex}]\n새로운 게임 시작";
    }

    private void SelectSlotAndStartGame(int slotIndex)
    {
        DataManager.Instance.LoadSlot(slotIndex);
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }
}