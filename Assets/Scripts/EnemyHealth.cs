using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 50;
    private int currentHealth;

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        // Optionally add a visual effect (flash white, etc) here

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Add death effect or drop XP gem here later
        Destroy(gameObject);
    }
}
