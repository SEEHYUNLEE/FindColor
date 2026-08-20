using System.Collections;
using UnityEngine;

public class SlimeController : MonoBehaviour
{
    [Header("HP")]
    public int maxHP = 100;
    private int currentHP;

    [Header("Animation")]
    public Animator animator;

    [Tooltip("위치가 이 값보다 이상 변경되면 이동 중으로 판단")]
    public float moveThreshold = 0.001f;

    [Tooltip("죽는 애니메이션이 끝난 후 슬라임 제거")]
    public float dieAnimationLength = 1f;

    private Vector3 lastPosition;

    private bool isDead = false;
    private bool isMoving = false;


    void Awake()
    {
        currentHP = maxHP;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
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


        if (currentHP <= 0)
        {
            Die();
        }
    }


    // =========================================================
    // 죽음
    // =========================================================
    public void Die()
    {
        if (isDead)
            return;

        isDead = true;


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
        PathfindingTest pathfinding =
            GetComponent<PathfindingTest>();

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


        // Rigidbody가 있다면 속도 정지
        Rigidbody rb =
            GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
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

            if (script is PathfindingTest)
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