using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Animator animator;

    // [추가] 공격 이펙트 관련 변수
    [Header("Attack Settings")]
    [SerializeField] private GameObject attackEffectPrefab; // 날아갈 이펙트 프리팹
    [SerializeField] private float projectileSpeed = 3f;   // 이펙트가 날아가는 속도
    [SerializeField] private Transform firePoint;           // (선택) 발사 위치. 비워두면 플레이어 몸 중앙(transform.position)에서 발사

    private float walkSpeed = 2f;
    private float runSpeed = 4f;

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool isFacingRight = true;
    private bool isAttacking = false;

    // [추가] 마우스 클릭 방향을 저장할 변수
    private Vector2 attackDirection;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        // 공격 중인 상태라면 이동 및 추가 공격 입력을 완전히 차단
        if (isAttacking)
        {
            if (animator != null)
            {
                animator.SetBool("IsWalking", false);
                animator.SetBool("IsRunning", false);
            }
            return;
        }

        float moveX = 0f;
        float moveY = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX -= 1f;

            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveY += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveY -= 1f;
        }

        movement = new Vector2(moveX, moveY).normalized;

        bool isShiftPressed = Keyboard.current != null &&
                              (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);

        bool isMoving = movement.sqrMagnitude > 0.01f;
        float currentSpeed = isShiftPressed ? runSpeed : walkSpeed;

        if (moveX > 0f && !isFacingRight)
        {
            Flip();
        }
        else if (moveX < 0f && isFacingRight)
        {
            Flip();
        }

        if (animator != null)
        {
            bool isWalking = isMoving && !isShiftPressed;
            bool isRunning = isMoving && isShiftPressed;

            animator.SetBool("IsWalking", isWalking);
            animator.SetBool("IsRunning", isRunning);
        }

        // [수정] 마우스 왼쪽 클릭 시 공격 시작 및 방향 계산
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (animator != null && !isAttacking)
            {
                isAttacking = true;
                movement = Vector2.zero;
                animator.SetTrigger("Attack");

                // 1. 마우스의 화면 좌표를 월드 좌표로 변환
                Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
                Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
                mouseWorldPosition.z = 0f; // 2D이므로 z축은 0으로 맞춤

                // 2. 발사 기준 위치 설정 (firePoint가 없으면 transform.position 사용)
                Vector3 originPosition = firePoint != null ? firePoint.position : transform.position;

                // 3. 공격 방향 계산 (마우스 위치 - firePoint 위치)
                attackDirection = (mouseWorldPosition - originPosition).normalized;
            }
        }

        if (floorTilemap == null) return;

        Vector2 nextPosition = rb.position + movement * currentSpeed * Time.fixedDeltaTime;
        Vector3Int cellPos = floorTilemap.WorldToCell(nextPosition);

        if (floorTilemap.HasTile(cellPos))
        {
            rb.MovePosition(nextPosition);
        }
    }

    // [추가] 애니메이션 이벤트에서 호출할 프리팹 발사 함수
    public void SpawnAttackEffect()
    {
        if (attackEffectPrefab == null)
        {
            Debug.LogWarning("공격 이펙트 프리팹이 할당되지 않았습니다!");
            return;
        }

        // 1. 발사 위치 설정 (firePoint가 지정되어 있으면 그 위치, 없으면 플레이어 중심)
        Vector3 spawnPosition = firePoint != null ? firePoint.position : transform.position;

        // 2. 이펙트 생성
        GameObject effect = Instantiate(attackEffectPrefab, spawnPosition, Quaternion.identity);

        // 3. 이펙트가 날아가는 방향을 바라보도록 회전 (이미지가 돌아가야 자연스러움)
        float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg;
        effect.transform.rotation = Quaternion.Euler(0, 0, angle);

        // 4. Rigidbody2D를 이용해 이펙트 날려보내기
        Rigidbody2D effectRb = effect.GetComponent<Rigidbody2D>();
        if (effectRb != null)
        {
            effectRb.linearVelocity = attackDirection * projectileSpeed;
        }
    }

    public void OnAttackEnd()
    {
        isAttacking = false;
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 currentScale = transform.localScale;
        currentScale.x *= -1;
        transform.localScale = currentScale;
    }
}