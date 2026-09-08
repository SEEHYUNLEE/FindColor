using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class EscUI : MonoBehaviour
{
    [Header("UI Panel")]
    [SerializeField] private GameObject escMenu;

    [Header("Buttons")]
    [SerializeField] private Button saveAndMainMenuButton;
    [SerializeField] private Button saveAndQuitButton;

    private void Awake()
    {
        // 버튼 이벤트 연결
        if (saveAndMainMenuButton != null)
            saveAndMainMenuButton.onClick.AddListener(OnSaveAndMainMenuClicked);

        if (saveAndQuitButton != null)
            saveAndQuitButton.onClick.AddListener(OnSaveAndQuitClicked);
    }

    private void Start()
    {
        // 시작 시 UI 비활성화
        if (escMenu != null)
            escMenu.SetActive(false);
    }

    private void Update()
    {
        // ESC 키 입력 시 메뉴 토글
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleEscMenu();
        }
    }

    // 메뉴 열기/닫기
    public void ToggleEscMenu()
    {
        if (escMenu == null) return;

        bool isActive = !escMenu.activeSelf;
        escMenu.SetActive(isActive);

        // 일시정지 / 시간 재생
        Time.timeScale = isActive ? 0f : 1f;
    }

    // 저장 후 메인 메뉴(슬롯 선택 화면)로 이동
    private void OnSaveAndMainMenuClicked()
    {
        // 일시정지 해제
        Time.timeScale = 1f;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.LoadMainMenuScene();
        }
        else if (DataManager.Instance != null)
        {
            DataManager.Instance.SaveCurrentSlot();
        }
    }

    // 저장 후 게임 종료
    private void OnSaveAndQuitClicked()
    {
        // 일시정지 해제
        Time.timeScale = 1f;

        if (DataManager.Instance != null)
        {
            DataManager.Instance.QuitGame();
        }
        else
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}