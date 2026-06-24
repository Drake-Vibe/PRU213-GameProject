using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 3f;
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private bool chaseForever = false;

    [Header("Combat Settings")]
    [SerializeField] private int damageAmount = 10;
    [SerializeField] private float damageInterval = 1f;

    private Transform playerTransform;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    
    private float lastDamageTime;
    private bool isChasing = false;

    // Knockback
    private bool isKnockedBack = false;
    private float knockbackEndTime = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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

        // Cách 2: Tìm qua script PlayerShooting (dự phòng)
        PlayerShooting playerShooting = FindAnyObjectByType<PlayerShooting>();
        if (playerShooting != null)
        {
            playerTransform = playerShooting.transform;
            return;
        }

        // Cách 3: Tìm qua Tag "Player" (dự phòng)
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            return;
        }

        Debug.LogWarning("EnemyChase: Không tìm thấy Player! Kiểm tra lại Tag hoặc Script trên Player.");
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

        if (isChasing)
        {
            MoveTowardsPlayer();
        }
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
        // Calculate direction to player
        Vector2 direction = ((Vector2)playerTransform.position - rb.position).normalized;

        // Move the enemy via Rigidbody2D for proper physical collision behavior
        rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);

        // Flip the sprite orientation based on horizontal movement direction
        if (spriteRenderer != null)
        {
            if (direction.x > 0.01f)
            {
                spriteRenderer.flipX = false; // Facing right
            }
            else if (direction.x < -0.01f)
            {
                spriteRenderer.flipX = true; // Facing left
            }
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
                playerScript.TakeDamage(damageAmount);
                lastDamageTime = Time.time;
                Debug.Log("Quái gây " + damageAmount + " sát thương cho Player!");
            }
        }
    }

    // Draw detection radius in editor for easy debugging/tuning
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
