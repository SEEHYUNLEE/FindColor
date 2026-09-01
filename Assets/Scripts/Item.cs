using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Item : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Light2D itemLight;

    [Header("Jump Settings")]
    [SerializeField] private float jumpDuration = 0.5f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float dropDistance = 1.0f;

    [Header("Magnet / Magnetize Settings")]
    [SerializeField] private float magnetDistance = 1f; // 흡수 시작 거리
    [SerializeField] private float magnetSpeed = 3.0f;    // 빨려 들어가는 속도
    [SerializeField] private float pickupDistance = 0.2f; // 최종 획득 처리 거리

    private Transform playerTransform;
    private bool isLand = false;        // 착지 완료 여부
    private bool isBeingAbsorbed = false; // 흡수 진행 중 여부

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (itemLight == null)
            itemLight = GetComponentInChildren<Light2D>();
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }

        StartCoroutine(AnimatePopUp());
    }

    private void Update()
    {
        // 땅에 착지하지 않았거나 이미 흡수 중인 경우 감지 안 함
        if (!isLand || isBeingAbsorbed || playerTransform == null)
            return;

        // 플레이어와의 거리 체크
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= magnetDistance)
        {
            StartCoroutine(AbsorbToPlayer());
        }
    }

    public void InitializeColor(Color targetColor)
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            spriteRenderer.color = targetColor;

        if (itemLight != null)
            itemLight.color = targetColor;
    }

    // 1. 드롭 및 점프 연출
    private IEnumerator AnimatePopUp()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos;

        if (playerTransform != null)
        {
            Vector3 directionToPlayer = (playerTransform.position - startPos).normalized;
            Vector3 offset = new Vector3(
                directionToPlayer.x * dropDistance,
                directionToPlayer.y * dropDistance * 0.5f,
                0f
            );
            targetPos = startPos + offset;
        }
        else
        {
            Vector2 randomOffset = Random.insideUnitCircle * dropDistance;
            targetPos = startPos + new Vector3(randomOffset.x, randomOffset.y * 0.5f, 0f);
        }

        float timer = 0f;

        while (timer < jumpDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / jumpDuration;

            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, progress);
            float height = Mathf.Sin(progress * Mathf.PI) * jumpHeight;
            currentPos.y += height;

            transform.position = currentPos;
            yield return null;
        }

        transform.position = targetPos;
        isLand = true; // 착지 완료 flag
    }

    // 2. 플레이어 흡수 이동 및 획득
    private IEnumerator AbsorbToPlayer()
    {
        isBeingAbsorbed = true;
        float currentSpeed = magnetSpeed;

        while (playerTransform != null)
        {
            // 발 위치(position)에서 Y축 높이를 살짝 올린 몸통 위치를 목표로 설정
            Vector3 targetPosition = playerTransform.position + new Vector3(0f, 0.5f, 0f);

            // 흡수 속도 가속
            currentSpeed += Time.deltaTime * 10f;

            // 몸통 위치를 향해 이동
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                currentSpeed * Time.deltaTime
            );

            // 완전히 닿았을 때 획득 처리
            if (Vector3.Distance(transform.position, targetPosition) <= pickupDistance)
            {
                OnCollect();
                yield break;
            }

            yield return null;
        }
    }

    // 3. 아이템 획득 시 로직
    private void OnCollect()
    {
        if (playerTransform != null)
        {
            // 플레이어에게서 색상 관리 스크립트 검색 후 전달
            if (playerTransform.TryGetComponent<PlayerColorManager>(out var colorManager))
            {
                // spriteRenderer는 Item의 SpriteRenderer
                colorManager.ApplyColorToRandomPart(spriteRenderer.color);
            }
        }

        Destroy(gameObject);
    }
}