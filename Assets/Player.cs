using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    public HealthBar healthBar;

    [Header("Hit Feedback Settings")]
    public float knockbackForce = 8f;
    public float knockbackDuration = 0.2f;
    public float invincibilityDuration = 0.6f;
    public float blinkInterval = 0.08f;

    private SpriteRenderer spriteRenderer;
    private bool isInvincible = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        if (healthBar != null)
        {
            healthBar.SetMaxHealth(currentHealth);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(20);
        }
    }

    public void TakeDamage(int damage)
    {
        TakeDamage(damage, (Vector2)transform.position - Vector2.up);
    }

    public void TakeDamage(int damage, Vector2 sourcePosition)
    {
        if (isInvincible) return;

        currentHealth -= damage;
        Debug.Log("Player bị trúng đòn! Mất " + damage + " máu. Máu hiện tại: " + currentHealth);

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
        }

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        // Kích hoạt nhấp nháy và bất tử tạm thời
        StartCoroutine(InvincibilityRoutine());

        // Đẩy lùi Player
        Vector2 knockbackDirection = ((Vector2)transform.position - sourcePosition).normalized;
        if (knockbackDirection == Vector2.zero)
        {
            knockbackDirection = Vector2.up;
        }

        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null)
        {
            movement.ApplyKnockback(knockbackDirection, knockbackForce, knockbackDuration);
        }
    }

    private System.Collections.IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        float elapsed = 0f;
        while (elapsed < invincibilityDuration)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
            }
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
        isInvincible = false;
    }

    void Die()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
