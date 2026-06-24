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
        // Tránh tự hủy với Player
        if (target.CompareTag("Player") || target.name.Contains("Player") || target.GetComponentInChildren<PlayerShooting>() != null || target.transform.root.GetComponentInChildren<PlayerShooting>() != null)
        {
            return; 
        }

        // Bỏ qua các vật thể là phòng, nền đất hoặc camera
        if (target.name.Contains("Main Room") || target.name.Contains("Ground") || target.name.Contains("Room") || target.name.Contains("Grid") || target.name.Contains("Camera") || target.name.Contains("Bounds"))
        {
            return;
        }

        // Nếu bắn trúng Enemy thì gây sát thương và tự hủy
        if (target.TryGetComponent<EnemyHealth>(out EnemyHealth enemyHealth) || target.name.ToLower().Contains("enemy"))
        {
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
            return;
        }

        // Chỉ hủy đạn khi đâm vào tường hoặc vật cản thực sự
        string nameLower = target.name.ToLower();
        if (nameLower.Contains("wall") || nameLower.Contains("collison") || nameLower.Contains("collision") || nameLower.Contains("obstacle"))
        {
            Debug.Log("Đạn va chạm với tường/vật cản và tự hủy: " + target.name);
            Destroy(gameObject);
        }
    }
}
