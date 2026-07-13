using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main Player script handling health, damage, weapon management, and sprite flipping.
/// Integrates with Soul Knight-style combat while keeping PRU213's HealthBar system.
/// </summary>
public class Player : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth;
    public HealthBar healthBar;

    [Header("Weapon")]
    public GameObject currentWeapon;

    [Header("Damage Cooldown")]
    [SerializeField] private float damageCooldown = 0.5f; // seconds of invincibility after hit
    private float lastDamageTime = -999f;

    [Header("References")]
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        currentHealth = maxHealth;
        if (healthBar != null)
            healthBar.SetMaxHealth(currentHealth);

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        FollowMouse();
    }

    /// <summary>
    /// Flip player sprite to face the mouse cursor direction.
    /// Similar to Soul Knight's Player.followMouse().
    /// </summary>
    private void FollowMouse()
    {
        if (spriteRenderer == null) return;

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float directionX = mousePosition.x - transform.position.x;

        spriteRenderer.flipX = directionX < 0;
    }

    /// <summary>
    /// Take damage from any source. Respects invincibility frames.
    /// </summary>
    public void TakeDamage(int damage)
    {
        // Check invincibility cooldown
        if (Time.time - lastDamageTime < damageCooldown) return;

        lastDamageTime = Time.time;
        currentHealth -= damage;

        if (healthBar != null)
            healthBar.SetHealth(currentHealth);

        Debug.Log($"Player took {damage} damage! HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player died!");
        SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// Handle collision damage from enemies and enemy bullets.
    /// Mirrors Soul Knight's OnCollisionEnter2D damage system.
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        switch (collision.collider.tag)
        {
            case Tags.ENEMY:
                TakeDamage(20);
                break;
        }
    }

    /// <summary>
    /// Handle continuous collision damage (staying in contact with enemy).
    /// </summary>
    private void OnCollisionStay2D(Collision2D collision)
    {
        switch (collision.collider.tag)
        {
            case Tags.ENEMY:
                TakeDamage(10);
                break;
        }
    }

    /// <summary>
    /// Handle trigger-based damage (enemy bullets use triggers).
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tags.ENEMY_BULLET))
        {
            Bullet bullet = collision.GetComponent<Bullet>();
            if (bullet != null)
            {
                TakeDamage((int)bullet.GetDamage());
            }
        }
    }
}
