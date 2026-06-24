using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject enemyPrefab;
    public float spawnInterval = 2f;
    public float spawnRadius = 4f; // Khoảng cách sinh quái so với Player

    private Transform player;
    private float timer = 0f;

    void Start()
    {
        // Tự động tìm Player trong Scene
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            // Dự phòng nếu quên set Tag: Tìm thông qua script PlayerShooting
            PlayerShooting playerShooting = FindObjectOfType<PlayerShooting>();
            if (playerShooting != null)
            {
                playerObj = playerShooting.gameObject;
            }
        }

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("EnemySpawner: Không tìm thấy Player! Hãy chắc chắn Player được gắn Tag 'Player'.");
        }
    }

    void Update()
    {
        if (player == null) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemySpawner: Chưa gán Enemy Prefab!");
            return;
        }

        // Random góc từ 0 đến 360 độ (tính bằng radian cho hàm lượng giác)
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        
        // Tính toán hướng sinh ra dựa trên góc
        Vector2 spawnDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)).normalized;
        
        // Vị trí sinh ra = Vị trí player + hướng * khoảng cách
        Vector2 spawnPos = (Vector2)player.position + spawnDirection * spawnRadius;

        // Sinh ra Enemy
        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        // Ép Enemy dùng đúng Sorting Layer (tránh tàng hình)
        SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Player";
            sr.sortingOrder = 2;
        }

        // Tự động thêm Rigidbody2D nếu chưa có (cần thiết để EnemyChase di chuyển)
        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = enemy.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        // Tự động thêm EnemyChase nếu chưa có (để quái đuổi theo Player)
        EnemyChase chase = enemy.GetComponent<EnemyChase>();
        if (chase == null)
        {
            enemy.AddComponent<EnemyChase>();
        }

        // Tự động thêm EnemyHealth nếu chưa có
        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        if (health == null)
        {
            enemy.AddComponent<EnemyHealth>();
        }

        Debug.Log("Đã sinh ra Enemy tại: " + spawnPos);
    }
}
