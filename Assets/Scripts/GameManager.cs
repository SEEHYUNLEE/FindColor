using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null) return null;
            return instance;
        }
    }

    [Header("Scene Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu"; // 슬롯 선택/타이틀 화면
    [SerializeField] private string mainSceneName = "Main";         // 마을 / 메인 로비
    [SerializeField] private string normalStageSceneName = "NormalStage"; // 노말 스테이지

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

    // =========================================================
    // 스테이지 및 재화 관리
    // =========================================================

    // 현재 스테이지 반환
    public int GetCurrentStage()
    {
        return DataManager.Instance != null ? DataManager.Instance.currentData.stage : 1;
    }

    // 다음 스테이지로 해금 및 데이터 저장
    public void ClearCurrentStage()
    {
        if (DataManager.Instance == null) return;

        DataManager.Instance.currentData.stage++;
        DataManager.Instance.SaveCurrentSlot();
        Debug.Log($"[GameManager] 스테이지 클리어! 다음 스테이지: {DataManager.Instance.currentData.stage}");
    }

    // =========================================================
    // 씬 전환 관리
    // =========================================================

    // 1. 타이틀 / 슬롯 선택 화면으로 이동
    public void LoadMainMenuScene()
    {
        SaveCurrentData();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // 2. 메인 마을 / 로비 씬으로 이동
    public void LoadMainScene()
    {
        SaveCurrentData();
        SceneManager.LoadScene(mainSceneName);
    }

    // 3. 노말 스테이지로 이동
    public void LoadNormalStage()
    {
        SaveCurrentData();
        SceneManager.LoadScene(normalStageSceneName);
    }

    // 현재 진행 중인 스테이지 재시작
    public void RestartStage()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 씬 전환 시 헬퍼 메서드 (자동 저장)
    private void SaveCurrentData()
    {
        if (DataManager.Instance != null)
        {
            DataManager.Instance.SaveCurrentSlot();
        }
    }
}