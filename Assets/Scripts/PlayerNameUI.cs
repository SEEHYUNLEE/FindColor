using System.Collections;
using TMPro;
using UnityEngine;

public class PlayerNameUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_Text storyText;

    private string story =
        "{0}님 맞죠?\n\n" +
        "지금 세계는 모든 색을 잃어버렸어요.\n\n" +
        "흑백으로 변해버린 이곳에서\n" +
        "수상한 포탈 하나가 등장했어요.\n\n" +
        "포탈 너머에는 무엇이 있을까요?\n\n" +
        "{0}님이 사라진 색을 되찾기 위한\n" +
        "모험을 시작해주세요.";

    private float textSpeed = 0.05f;

    private Coroutine storyCoroutine;
    private bool isStoryPlaying;

    private void Start()
    {
        panel.SetActive(false);

        if (storyText != null)
            storyText.gameObject.SetActive(false);

        if (DataManager.Instance == null ||
            DataManager.Instance.currentData == null)
        {
            return;
        }

        // 스테이지 1이고 이름이 아직 설정되지 않았을 때만 표시
        if (DataManager.Instance.currentData.stage == 1 &&
            DataManager.Instance.currentData.playerName == "Player")
        {
            panel.SetActive(true);

            nameInput.gameObject.SetActive(true);

            if (storyText != null)
                storyText.gameObject.SetActive(false);

            nameInput.text = "";
            nameInput.Select();
            nameInput.ActivateInputField();
        }
    }

    public bool IsPanelOpen()
    {
        return panel != null && panel.activeSelf;
    }

    public void ConfirmName()
    {
        if (isStoryPlaying)
            return;

        if (nameInput == null)
            return;

        string playerName = nameInput.text.Trim();

        // 이름을 입력하지 않았다면 진행하지 않음
        if (string.IsNullOrWhiteSpace(playerName))
            return;

        // 이름 저장
        DataManager.Instance.SetPlayerName(playerName);

        // 이름 입력창 숨기기
        nameInput.gameObject.SetActive(false);

        // 스토리 시작
        if (storyText != null)
        {
            storyText.gameObject.SetActive(true);

            storyCoroutine = StartCoroutine(TypeStory(playerName));
        }
        else
        {
            panel.SetActive(false);
        }
    }

    private IEnumerator TypeStory(string playerName)
    {
        isStoryPlaying = true;

        // {0} 부분에 플레이어 이름 삽입
        string currentStory = string.Format(story, playerName);

        storyText.text = "";

        foreach (char letter in currentStory)
        {
            storyText.text += letter;

            yield return new WaitForSeconds(textSpeed);
        }

        isStoryPlaying = false;
        storyCoroutine = null;

        yield return new WaitForSeconds(2f);

        // 패널 닫기
        panel.SetActive(false);
    }
}