using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Tilemap floorTilemap;
    [SerializeField] private Animator animator;
    private float walkSpeed = 2f;  // 걷기 속도
    private float runSpeed = 4f;   // [추가] 달리기 속도

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool isFacingRight = true;
    private bool isAttacking = false;

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
                animator.SetBool("IsRunning", false); // [추가] 공격 중일 때 달리기 애니메이션도 끄기
            }
            return;
        }

        float moveX = 0f;
        float moveY = 0f;

        if (Keyboard.current != null)
        {
            // A/D 또는 좌/우 화살표
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) moveX += 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) moveX -= 1f;

            // W/S 또는 상/하 화살표
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) moveY += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) moveY -= 1f;
        }

        movement = new Vector2(moveX, moveY).normalized;

        // [추가] 왼쪽 또는 오른쪽 Shift 키가 눌려있는지 확인
        bool isShiftPressed = Keyboard.current != null &&
                              (Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed);

        // [추가] 이동 중이면서 Shift를 누르고 있으면 달리기 속도, 아니면 걷기 속도 적용
        bool isMoving = movement.sqrMagnitude > 0.01f;
        float currentSpeed = isShiftPressed ? runSpeed : walkSpeed;

        // 스케일을 이용한 좌우 반전 처리
        if (moveX > 0f && !isFacingRight)
        {
            Flip();
        }
        else if (moveX < 0f && isFacingRight)
        {
            Flip();
        }

        // [수정] 걷기 및 달리기 애니메이션 처리
        if (animator != null)
        {
            bool isWalking = isMoving && !isShiftPressed;
            bool isRunning = isMoving && isShiftPressed;

            animator.SetBool("IsWalking", isWalking);
            animator.SetBool("IsRunning", isRunning);
        }

        // 마우스 왼쪽 클릭 시 공격 시작
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (animator != null && !isAttacking)
            {
                isAttacking = true;
                movement = Vector2.zero;
                animator.SetTrigger("Attack");
            }
        }

        if (floorTilemap == null) return;

        // [수정] 현재 속도(currentSpeed)를 반영하여 다음 위치 계산
        Vector2 nextPosition = rb.position + movement * currentSpeed * Time.fixedDeltaTime;

        // 바닥 타일맵 위인지 검사
        Vector3Int cellPos = floorTilemap.WorldToCell(nextPosition);

        if (floorTilemap.HasTile(cellPos))
        {
            rb.MovePosition(nextPosition);
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