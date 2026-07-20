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

        BossCrystalKnight boss = GetComponent<BossCrystalKnight>();
        if (boss != null)
        {
            boss.TakeDamage(damage);
            currentHealth = boss.CurrentHealth;
            if (currentHealth <= 0)
            {
                Die();
            }
            return;
        }

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
        if (rb != null) rb.linearDamping = 5f;

        // Xóa sau khi animation Death chạy xong (nếu không có Animator thì xóa ngay)
        float delay = animator != null ? deathAnimationDuration : 0f;
        Destroy(gameObject, delay);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Bullet"))
        {
            Bullet bullet = collision.GetComponent<Bullet>();
            if (bullet != null && bullet.GetOwnerTag() == "Player")
            {
                TakeDamage((int)bullet.GetDamage());
            }
        }
    }
}
