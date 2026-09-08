using UnityEngine;
using System.Collections;

public class Portal : MonoBehaviour
{
    [SerializeField] private GameObject ConvertImage;
    private float transitionTime = 3f;

    private bool isLoading = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Player가 아니면 무시
        if (!collision.CompareTag("Player"))
            return;

        // 이미 씬 전환을 시작했다면 중복 실행 방지
        if (isLoading)
            return;

        isLoading = true;

        // 화면 전환 시작
        StartCoroutine(ConvertScene());
    }

    private IEnumerator ConvertScene()
    {
        // ConvertImage 활성화
        if (ConvertImage != null)
        {
            ConvertImage.SetActive(true);
        }

        // 애니메이션 재생 시간만큼 대기
        yield return new WaitForSeconds(transitionTime);

        // 일반 스테이지 이동
        GameManager.Instance.LoadNormalStage();
    }
}
