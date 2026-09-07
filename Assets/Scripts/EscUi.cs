using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EscUI : MonoBehaviour
{
    [Header("UI Panel")]
    [SerializeField] private GameObject escMenu;

    [Header("Buttons")]
    [SerializeField] private Button saveAndMainMenuButton;
    [SerializeField] private Button saveAndQuitButton;

    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // 메인메뉴 씬 이름

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

        // 시간 정지/재생 (선택 사항)
        Time.timeScale = isActive ? 0f : 1f;
    }

    // 저장 후 메인메뉴 이동
    private void OnSaveAndMainMenuClicked()
    {
        // 시간을 원래대로 복구
        Time.timeScale = 1f;

        // 데이터 저장
        if (DataManager.Instance != null)
        {
            DataManager.Instance.SaveCurrentSlot();
        }

        // 메인메뉴 씬 로드
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // 저장 후 게임 종료
    private void OnSaveAndQuitClicked()
    {
        // 시간을 원래대로 복구
        Time.timeScale = 1f;

        // DataManager의 QuitGame에서 저장을 이미 수행함
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