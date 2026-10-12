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
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string mainSceneName = "Main";
    [SerializeField] private string normalStageSceneName = "NormalStage";
    [SerializeField] private string bossStageSceneName = "BossCutscene";

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
    }

    // =========================================================
    // 씬 전환 관리
    // =========================================================
    public void LoadMainMenuScene()
    {
        SaveCurrentData();
        SceneManager.LoadScene(mainMenuSceneName);
    }
    public void LoadMainScene()
    {
        SaveCurrentData();
        SceneManager.LoadScene(mainSceneName);
    }
    public void LoadNormalStage()
    {
        SaveCurrentData();
        SceneManager.LoadScene(normalStageSceneName);
    }
    public void LoadBossStage()
    {
        SaveCurrentData();
        SceneManager.LoadScene(bossStageSceneName);
    }

    // 씬 전환 시 저장
    private void SaveCurrentData()
    {
        if (DataManager.Instance != null)
        {
            DataManager.Instance.SaveCurrentSlot();
        }
    }
}