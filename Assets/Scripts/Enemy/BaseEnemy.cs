using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Base class for all enemies. Handles health, damage reception, health bar UI, and death.
/// Enemies can be damaged by player bullets (OnTriggerEnter2D) and melee weapons.
/// </summary>
public class BaseEnemy : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 10;
    public int scoreValue = 10; // points awarded when killed

    [Header("UI")]
    public Slider healthbar;

    protected float currentHealth;
    protected Player targetPlayer;

    public float Health => currentHealth;

    protected virtual void Start()
    {
        currentHealth = maxHealth;

        if (healthbar != null)
        {
            healthbar.maxValue = maxHealth;
            healthbar.value = maxHealth;
        }

        // Find player reference
        targetPlayer = FindAnyObjectByType<Player>();
    }

    protected virtual void Update()
    {
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthbar == null) return;

        healthbar.value = currentHealth;
        // Only show healthbar when damaged
        healthbar.gameObject.SetActive(currentHealth < maxHealth);
    }

    /// <summary>
    /// Public method to apply damage to this enemy.
    /// Called by Bullet.OnTriggerEnter2D and MeleeWeapon.OnTriggerEnter2D.
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return; // already dead

        currentHealth -= damage;
        OnDamaged(damage);

        Debug.Log($"{gameObject.name} took {damage} damage! HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            OnDeath();
        }
    }

    /// <summary>
    /// Handle bullet triggers.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Hit by player bullet
        if (collision.CompareTag(Tags.BULLET))
        {
            Bullet bullet = collision.GetComponent<Bullet>();
            if (bullet != null && bullet.GetOwnerTag() == Tags.PLAYER)
            {
                TakeDamage(bullet.GetDamage());
            }
        }
    }

    /// <summary>
    /// Override for custom behavior when damaged (knockback, flash, etc.)
    /// </summary>
    protected virtual void OnDamaged(float damage) { }

    /// <summary>
    /// Override for custom death behavior. Base implementation adds score and destroys.
    /// </summary>
    protected virtual void OnDeath()
    {
        // Add score via GameManager if available
        GameManager manager = FindAnyObjectByType<GameManager>();
        if (manager != null)
        {
            manager.AddScore(scoreValue);
        }

        Debug.Log($"{gameObject.name} died! +{scoreValue} score");
        Destroy(gameObject);
    }
}
