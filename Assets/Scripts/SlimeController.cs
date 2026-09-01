using System.Collections;
using UnityEngine;

public class SlimeController : MonoBehaviour
{
    [Header("HP")]
    public int maxHP = 20;
    private int currentHP;

    [Header("Animation")]
    public Animator animator;

    [Header("Damage Flash Settings")]
    [SerializeField] private SpriteRenderer spriteRenderer; // 슬라임 스프라이트 렌더러
    [SerializeField] private int flashCount = 3;             // 깜빡이는 횟수
    [SerializeField] private float flashInterval = 0.05f;    // 간격 (초)
    [SerializeField] private Color flashColor1 = Color.black;// 첫 번째 깜빡임 색상 (검정)
    [SerializeField] private Color flashColor2 = Color.gray; // 두 번째 깜빡임 색상 (회색)

    [Tooltip("위치가 이 값보다 이상 변경되면 이동 중으로 판단")]
    public float moveThreshold = 0.001f;

    [Tooltip("죽는 애니메이션이 끝난 후 슬라임 제거")]
    public float dieAnimationLength = 1f;

    private Vector3 lastPosition;

    private bool isDead = false;
    private bool isMoving = false;

    // 색상 데이터 및 코루틴 제어 변수
    private SlimeColorData myColorData;
    private Coroutine flashCoroutine;

    // 외부(아이템 드롭 시스템 등)에서 슬라임 색상을 읽을 수 있는 프로퍼티
    public SlimeColorData MyColorData => myColorData;

    // [추가] 무지개 색상 (빨주노초파남보) 정의
    private readonly Color[] rainbowColors = new Color[]
    {
        new Color(1f, 0.2f, 0.2f),   // 빨강
        new Color(1f, 0.5f, 0.1f),   // 주황
        new Color(1f, 0.9f, 0.2f),   // 노랑
        new Color(0.2f, 0.8f, 0.3f), // 초록
        new Color(0.2f, 0.6f, 1f),   // 파랑
        new Color(0.1f, 0.1f, 0.6f), // 남색
        new Color(0.6f, 0.2f, 0.8f)  // 보라
    };

    public GameObject itemPrefab;

    void Awake()
    {
        currentHP = maxHP;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        // [수정] 생성 시 무지개 색상 중 하나를 랜덤으로 뽑아서 적용 및 originalColor 설정
        if (spriteRenderer != null)
        {
            myColorData = SlimeColorPalette.GetRandomColorData();
            spriteRenderer.color = myColorData.color;
        }
    }


    void Start()
    {
        lastPosition = transform.position;

        PlayIdle();
    }


    void Update()
    {
        // 죽었으면 다른 처리 중지
        if (isDead)
            return;


        // -----------------------------------------
        // 현재 프레임과 이전 프레임의 위치 비교
        // -----------------------------------------
        float movedDistance =
            Vector3.Distance(
                transform.position,
                lastPosition
            );


        if (movedDistance > moveThreshold)
        {
            if (!isMoving)
            {
                isMoving = true;
                PlayWalk();
            }
        }
        else
        {
            if (isMoving)
            {
                isMoving = false;
                PlayIdle();
            }
        }


        lastPosition = transform.position;
    }


    // =========================================================
    // 데미지
    // =========================================================
    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHP -= damage;

        currentHP =
            Mathf.Max(currentHP, 0);


        Debug.Log(
            gameObject.name +
            " HP : " +
            currentHP
        );

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashColorRoutine());

        SlimePathfinding pathfinding = GetComponent<SlimePathfinding>();
        if (pathfinding != null)
        {
            pathfinding.StartEscape();
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    // =========================================================
    // 피격 색상 깜빡임 연출 코루틴
    // =========================================================
    private IEnumerator FlashColorRoutine()
    {
        if (spriteRenderer == null) yield break;

        for (int i = 0; i < flashCount; i++)
        {
            spriteRenderer.color = flashColor1; // 검은색
            yield return new WaitForSeconds(flashInterval);

            spriteRenderer.color = flashColor2; // 회색
            yield return new WaitForSeconds(flashInterval);
        }

        // 원래 색상으로 복구
        spriteRenderer.color = myColorData.color;
        flashCoroutine = null;
    }

    // =========================================================
    // 죽음
    // =========================================================
    public void Die()
    {
        if (isDead)
            return;

        isDead = true;

        // 진행 중인 깜빡임 멈추고 지정 색상으로 복구
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        if (spriteRenderer != null)
        {
            spriteRenderer.color = myColorData.color;
        }

        // -----------------------------------------
        // 슬라임 이동 중지
        // -----------------------------------------
        StopMovement();


        // -----------------------------------------
        // 다른 스크립트 정지
        // -----------------------------------------
        StopOtherScripts();


        // -----------------------------------------
        // Die 애니메이션
        // -----------------------------------------
        PlayDie();


        // -----------------------------------------
        // 애니메이션 종료 후 삭제
        // -----------------------------------------
        StartCoroutine(DestroyAfterDeath());
    }


    // =========================================================
    // 이동 중지
    // =========================================================
    void StopMovement()
    {
        // PathfindingTest가 같은 GameObject에 붙어 있다면
        SlimePathfinding pathfinding =
            GetComponent<SlimePathfinding>();

        if (pathfinding != null)
        {
            pathfinding.enabled = false;
        }


        // Rigidbody2D가 있다면 속도 정지
        Rigidbody2D rb2D =
            GetComponent<Rigidbody2D>();

        if (rb2D != null)
        {
            rb2D.linearVelocity = Vector2.zero;
            rb2D.angularVelocity = 0f;
        }
    }


    // =========================================================
    // 다른 컴포넌트 정지
    // =========================================================
    void StopOtherScripts()
    {
        MonoBehaviour[] scripts =
            GetComponents<MonoBehaviour>();


        foreach (MonoBehaviour script in scripts)
        {
            if (script == this)
                continue;

            if (script is SlimePathfinding)
                continue;

            script.enabled = false;
        }
    }


    // =========================================================
    // 애니메이션
    // =========================================================
    void PlayIdle()
    {
        if (animator == null)
            return;

        animator.Play("SlimeIdle");
    }


    void PlayWalk()
    {
        if (animator == null)
            return;

        animator.Play("SlimeWalk");
    }


    void PlayDie()
    {
        if (animator == null)
            return;

        animator.Play("SlimeDie");
    }


    // =========================================================
    // 죽음 애니메이션 후 삭제
    // =========================================================
    IEnumerator DestroyAfterDeath()
    {
        yield return new WaitForSeconds(
            dieAnimationLength
        );

        if (itemPrefab != null)
        {
            Item item = Instantiate(itemPrefab, transform.position, Quaternion.identity).GetComponent<Item>();

            // 가져온 스크립트에 색상 전달
            if (item != null)
            {
                item.InitializeColor(myColorData.color);
            }
        }

        Destroy(gameObject);
    }


    // =========================================================
    // 외부에서 HP 확인
    // =========================================================
    public int GetCurrentHP()
    {
        return currentHP;
    }


    public bool IsDead()
    {
        return isDead;
    }
}