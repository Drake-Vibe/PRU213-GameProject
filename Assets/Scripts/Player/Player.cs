using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Main Player script handling health, damage, weapon management, and sprite flipping.
/// Integrates with Soul Knight-style combat while keeping PRU213's HealthBar system.
/// </summary>
public class Player : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 10;
    public int currentHealth;
    public int maxArmor = 5;
    public int currentArmor;
    public int maxEnergy = 100;
    public int currentEnergy;

    [Header("Weapon")]
    public GameObject currentWeapon;

    [Header("Damage Cooldown")]
    [SerializeField] private float damageCooldown = 0.5f; // seconds of invincibility after hit
    private float lastDamageTime = -999f;

    [Header("References")]
    private SpriteRenderer spriteRenderer;

    private float manaRegenTimer = 0f;
    private float shieldRegenTimer = 0f;

    [Header("Potions")]
    public int healthPotions = 3;
    public int manaPotions = 3;

    private static Player _instance;
    public static Player Instance => _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        currentHealth = maxHealth;
        currentArmor = maxArmor;
        currentEnergy = maxEnergy;

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        FollowMouse();
        RegenerateStats();
        CheckPotionInputs();

        // Cap potion counts at 3 max
        healthPotions = Mathf.Clamp(healthPotions, 0, 3);
        manaPotions = Mathf.Clamp(manaPotions, 0, 3);
    }

    private void CheckPotionInputs()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UseHealthPotion();
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            UseManaPotion();
        }
    }

    private void RegenerateStats()
    {
        // Regenerate 1 Mana per 1 second
        if (currentEnergy < maxEnergy)
        {
            manaRegenTimer += Time.deltaTime;
            if (manaRegenTimer >= 1f)
            {
                currentEnergy = Mathf.Min(currentEnergy + 1, maxEnergy);
                manaRegenTimer = 0f;
            }
        }
        else
        {
            manaRegenTimer = 0f;
        }

        // Regenerate 1 Shield per 1 second
        if (currentArmor < maxArmor)
        {
            shieldRegenTimer += Time.deltaTime;
            if (shieldRegenTimer >= 1f)
            {
                currentArmor = Mathf.Min(currentArmor + 1, maxArmor);
                shieldRegenTimer = 0f;
            }
        }
        else
        {
            shieldRegenTimer = 0f;
        }
    }

    /// <summary>
    /// Flip player sprite to face the mouse cursor direction.
    /// Similar to Soul Knight's Player.followMouse().
    /// </summary>
    private void FollowMouse()
    {
        if (spriteRenderer == null || Camera.main == null) return;

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        float directionX = mousePosition.x - transform.position.x;

        spriteRenderer.flipX = directionX < 0;
    }

    /// <summary>
    /// Take damage from any source. Respects invincibility frames.
    /// </summary>
    public void TakeDamage(int damage)
    {
        TakeDamage(damage, transform.position);
    }

    /// <summary>
    /// Overload for compatibility with EnemyChase and other scripts.
    /// </summary>
    public void TakeDamage(int damage, Vector2 sourcePosition)
    {
        TakeDamage(damage, (Vector3)sourcePosition);
    }

    /// <summary>
    /// Overload for compatibility with EnemyChase and other scripts.
    /// </summary>
    public void TakeDamage(int damage, Vector3 sourcePosition)
    {
        // Check invincibility cooldown
        if (Time.time - lastDamageTime < damageCooldown) return;

        lastDamageTime = Time.time;
        
        // Interrupt shield regeneration on taking damage
        shieldRegenTimer = 0f;

        // Armor absorbs damage first
        if (currentArmor > 0)
        {
            if (damage <= currentArmor)
            {
                currentArmor -= damage;
                damage = 0;
            }
            else
            {
                damage -= currentArmor;
                currentArmor = 0;
            }
        }

        currentHealth -= damage;

        // Play hit animation
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }

        Debug.Log($"Player took damage! HP: {currentHealth}/{maxHealth}, Armor: {currentArmor}/{maxArmor}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player died!");
        SceneManager.LoadScene("GameOver");
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

    public void UseHealthPotion()
    {
        if (healthPotions > 0 && currentHealth < maxHealth)
        {
            healthPotions--;
            currentHealth = Mathf.Min(currentHealth + 3, maxHealth); // Restores 3 HP
            Debug.Log($"Used HP Potion! HP: {currentHealth}/{maxHealth}, Potions left: {healthPotions}");
        }
    }

    public void UseManaPotion()
    {
        if (manaPotions > 0 && currentEnergy < maxEnergy)
        {
            manaPotions--;
            currentEnergy = Mathf.Min(currentEnergy + 30, maxEnergy); // Restores 30 Mana
            Debug.Log($"Used MP Potion! Energy: {currentEnergy}/{maxEnergy}, Potions left: {manaPotions}");
        }
    }
}
