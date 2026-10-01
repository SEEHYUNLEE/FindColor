using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject nonColor;
    [SerializeField] private GameObject color;

    [SerializeField] private GameObject saveUI;

    [SerializeField] private Button startButton;
    [SerializeField] private Button startButtonColor;

    private void Start()
    {
        CheckColorMode();

        startButton.onClick.AddListener(OpenSaveUI);
        startButtonColor.onClick.AddListener(OpenSaveUI);
    }

    private void CheckColorMode()
    {
        bool isColorUnlocked = false;

        // 1~3번 슬롯 확인
        for (int i = 1; i <= 3; i++)
        {
            int stage = DataManager.Instance.GetSlotStage(i);

            if (stage >= 9)
            {
                isColorUnlocked = true;
                break;
            }
        }

        // 하나라도 Stage 9 이상이면 Color 메뉴 활성화
        nonColor.SetActive(!isColorUnlocked);
        color.SetActive(isColorUnlocked);
    }

    private void OpenSaveUI()
    {
        saveUI.SetActive(true);
    }
}