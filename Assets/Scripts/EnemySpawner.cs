using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject enemyPrefab;
    public float spawnInterval = 2f;
    public float spawnRadius = 15f; // Khoảng cách sinh quái so với Player

    private Transform player;
    private float timer = 0f;

    void Start()
    {
        // Tự động tìm Player trong Scene
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
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
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }
}
