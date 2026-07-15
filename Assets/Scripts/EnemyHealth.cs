using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 50;
    [Tooltip("Thời gian chờ animation Death chạy xong trước khi xóa (giây).")]
    [SerializeField] private float deathAnimationDuration = 1f;

    private int currentHealth;
    private Animator animator;
    private bool isDead = false;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else if (animator != null)
        {
            // Chỉ chạy animation Hurt khi còn sống (đòn chí mạng thì bỏ qua để nhảy sang Death)
            animator.SetTrigger("Hurt");
        }
    }

    private void Die()
    {
        isDead = true;

        if (animator != null)
        {
            animator.SetTrigger("Die");
        }

        // Ngừng đuổi + tắt va chạm để xác quái không còn tương tác trong lúc chờ animation
        EnemyChase chase = GetComponent<EnemyChase>();
        if (chase != null) chase.enabled = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // Xóa sau khi animation Death chạy xong (nếu không có Animator thì xóa ngay)
        float delay = animator != null ? deathAnimationDuration : 0f;
        Destroy(gameObject, delay);
    }
}
