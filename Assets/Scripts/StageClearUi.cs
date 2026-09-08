using System.Collections;
using TMPro;
using UnityEngine;

public class StageClearUI : MonoBehaviour
{
    [SerializeField] private GameObject clearPanel;      // 카운트다운 패널
    [SerializeField] private TMP_Text countdownText;     // 3, 2, 1 텍스트
    [SerializeField] private GameObject ConvertImage;    // 화면 전환 이미지

    private bool isClearing = false;

    private void Start()
    {
        // 씬 시작 시 카운트다운 패널 끄기
        if (clearPanel != null)
            clearPanel.SetActive(false);

        // 씬 시작 시 ConvertImage도 끄기
        if (ConvertImage != null)
            ConvertImage.SetActive(false);
    }

    // 아이템 획득 시 호출될 함수
    public void StartClearCountdown(int startCount = 3)
    {
        // 이미 클리어 진행 중이면 중복 실행 방지
        if (isClearing)
            return;

        isClearing = true;

        if (clearPanel != null)
            clearPanel.SetActive(true);

        StartCoroutine(Countdown(startCount));
    }

    private IEnumerator Countdown(int count)
    {
        // 3, 2, 1 카운트다운
        while (count > 0)
        {
            if (countdownText != null)
            {
                countdownText.text = count.ToString();
            }

            // 텍스트가 살짝 커졌다 줄어드는 연출
            float timer = 0f;
            Vector3 initialScale = Vector3.one * 1.5f;
            Vector3 targetScale = Vector3.one;

            while (timer < 1.0f)
            {
                timer += Time.deltaTime;

                if (countdownText != null)
                {
                    countdownText.transform.localScale =
                        Vector3.Lerp(initialScale, targetScale, timer);
                }

                yield return null;
            }

            count--;
        }

        // 카운트다운 패널 끄기
        if (clearPanel != null)
            clearPanel.SetActive(false);

        // ConvertImage 활성화
        ConvertImage.SetActive(true);

        // ConvertImage 애니메이션 재생 시간만큼 대기
        yield return new WaitForSeconds(3f);

        // 메인 씬 이동
        GameManager.Instance.LoadMainScene();
        
    }
}
