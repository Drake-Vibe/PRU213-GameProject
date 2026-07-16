using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    [Header("Cấu hình Di chuyển")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private bool chaseForever = false;

    [Header("Cấu hình Chiến đấu")]
    [SerializeField] private int damageAmount = 10;
    [SerializeField] private float damageInterval = 1f;

    private Transform playerTransform;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private float lastDamageTime;
    private bool isChasing = false;

    // Knockback
    private bool isKnockedBack = false;
    private float knockbackEndTime = 0f;

    // Chống kẹt: khi đâm vào cột/góc tường mà không nhích được thì tự lách sang bên
    private Vector2 lastPosition;
    private float stuckTime = 0f;
    private float slideDir = 1f;        // +1 hoặc -1: lách sang phải/trái
    private float slideFlipTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        lastPosition = transform.position;
    }

    private void Start()
    {
        // Cách 1: Tìm qua script Player
        Player player = FindAnyObjectByType<Player>();
        if (player != null)
        {
            playerTransform = player.transform;
            return;
        }


        // Cách 3: Tìm qua Tag "Player" (dự phòng)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("EnemyChase: Không tìm thấy Player! Kiểm tra lại Tag hoặc Script trên Player.");
        }
    }

    private void FixedUpdate()
    {
        if (playerTransform == null) return;

        // Đang bị knockback: dừng đuổi, chờ knockback xong
        if (isKnockedBack)
        {
            if (Time.time >= knockbackEndTime)
            {
                isKnockedBack = false;
                rb.linearVelocity = Vector2.zero; // Dừng lại hẳn sau khi knockback xong
            }
            if (animator != null) animator.SetBool("IsMoving", false);
            return; // Bỏ qua di chuyển khi đang bị đẩy lùi
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRadius)
        {
            isChasing = true;
        }
        else if (!chaseForever)
        {
            isChasing = false;
        }

        // Phát hiện kẹt: nếu đang đuổi mà gần như không nhích được -> tăng stuckTime
        if (isChasing)
        {
            float moved = Vector2.Distance(rb.position, lastPosition);
            if (moved < speed * Time.fixedDeltaTime * 0.4f)
                stuckTime += Time.fixedDeltaTime;
            else
                stuckTime = 0f;

            MoveTowardsPlayer();
        }
        else
        {
            stuckTime = 0f;
        }
        lastPosition = rb.position;

        // Bật animation Run khi đang đuổi, ngược lại về Idle
        if (animator != null) animator.SetBool("IsMoving", isChasing);
    }

    // Gọi từ PlayerMelee để kích hoạt knockback
    public void ApplyKnockback(Vector2 force, float duration = 0.35f)
    {
        isKnockedBack = true;
        knockbackEndTime = Time.time + duration;
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(force, ForceMode2D.Impulse);
    }

    private void MoveTowardsPlayer()
    {
        // Hướng thẳng tới Player
        Vector2 toPlayer = ((Vector2)playerTransform.position - rb.position).normalized;
        Vector2 moveDir = toPlayer;

        // Nếu đang bị kẹt (đâm cột/góc tường) -> trộn thêm hướng VUÔNG GÓC để lách qua
        if (stuckTime > 0.2f)
        {
            Vector2 perpendicular = new Vector2(-toPlayer.y, toPlayer.x) * slideDir;
            moveDir = (toPlayer * 0.5f + perpendicular).normalized;

            // Định kỳ đổi bên lách để thử cả 2 phía (tránh lách mãi 1 bên không qua)
            slideFlipTimer += Time.fixedDeltaTime;
            if (slideFlipTimer > 0.6f)
            {
                slideDir = -slideDir;
                slideFlipTimer = 0f;
            }
        }

        // Di chuyển bằng Rigidbody2D để va chạm vật lý đúng
        rb.MovePosition(rb.position + moveDir * speed * Time.fixedDeltaTime);

        // Lật sprite theo hướng ngang
        if (spriteRenderer != null)
        {
            if (moveDir.x > 0.01f) spriteRenderer.flipX = false;   // quay phải
            else if (moveDir.x < -0.01f) spriteRenderer.flipX = true; // quay trái
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        AttemptDamage(collision.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        AttemptDamage(collision.gameObject);
    }

    // Hỗ trợ cả Is Trigger = true
    private void OnTriggerEnter2D(Collider2D other)
    {
        AttemptDamage(other.gameObject);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        AttemptDamage(other.gameObject);
    }

    private void AttemptDamage(GameObject target)
    {
        // Tìm script Player trên chính object hoặc object cha
        Player playerScript = target.GetComponent<Player>();
        if (playerScript == null)
            playerScript = target.GetComponentInParent<Player>();

        if (playerScript != null)
        {
            if (Time.time - lastDamageTime >= damageInterval)
            {
                playerScript.TakeDamage(damageAmount, transform.position);
                lastDamageTime = Time.time;
                Debug.Log("Quái gây " + damageAmount + " sát thương cho Player!");
            }
        }
    }

    // Vẽ bán kính phát hiện trong Editor để dễ gỡ lỗi/tinh chỉnh
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
