using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage = 10;
    public float lifeTime = 3f;

    private void Start()
    {
        // Destroy the bullet after some time to prevent memory leaks if it misses
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void HandleCollision(GameObject target)
    {
        if (target.CompareTag("Player"))
        {
            // Optional: prevent bullet from destroying itself if it touches the player who shot it
            return; 
        }

        // If it hits an enemy, deal damage
        if (target.TryGetComponent<EnemyHealth>(out EnemyHealth enemyHealth))
        {
            enemyHealth.TakeDamage(damage);
        }

        // Destroy the bullet on any collision (except Player)
        Destroy(gameObject);
    }
}
