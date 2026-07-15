using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Cấu hình Spawner")]
    public GameObject enemyPrefab;
    public float spawnInterval = 2f;
    public float spawnRadius = 4f; // Khoảng cách sinh quái so với Player (khi KHÔNG dùng Spawn Points)

    [Header("Spawn Points (điểm sinh cố định trong map)")]
    [Tooltip("Kéo các object điểm sinh (đặt bên trong map) vào đây. " +
             "Nếu có ít nhất 1 điểm -> quái sinh tại các điểm này (KHÔNG sinh quanh Player nữa) => quái luôn ở trong map.")]
    public Transform[] spawnPoints;

    [Header("Trigger Settings")]
    [Tooltip("Nếu bật: chỉ sinh quái sau khi Player đi vào vùng trigger của object này.")]
    public bool activateOnTrigger = true;
    [Tooltip("Nếu bật: khi Player rời khỏi vùng thì ngừng sinh quái.")]
    public bool stopWhenPlayerLeaves = false;
    [Tooltip("Chỉ kích hoạt 1 lần duy nhất (lần đầu Player bước vào).")]
    public bool triggerOnce = false;

    private Transform player;
    private float timer = 0f;
    private bool isActive = false; // Spawner có đang được phép sinh quái hay không
    private bool hasTriggered = false;

    void Start()
    {
        // Nếu KHÔNG dùng trigger thì hoạt động ngay từ đầu (giữ hành vi cũ)
        if (!activateOnTrigger)
        {
            isActive = true;
        }

        // Tự động tìm Player trong Scene
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            // Dự phòng nếu quên set Tag: Tìm thông qua script Player
            Player playerScript = FindAnyObjectByType<Player>();
            if (playerScript != null)
            {
                playerObj = playerScript.gameObject;
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
        if (!isActive) return; // Chưa vào vùng trigger thì không sinh quái

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    // Player bước vào vùng -> bật spawner
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!activateOnTrigger) return;
        if (!other.CompareTag("Player")) return;
        if (triggerOnce && hasTriggered) return;

        isActive = true;
        hasTriggered = true;
        timer = spawnInterval; // Sinh quái ngay lập tức khi vừa bước vào
    }

    // Player rời khỏi vùng -> tắt spawner (nếu bật tùy chọn)
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!activateOnTrigger || !stopWhenPlayerLeaves) return;
        if (!other.CompareTag("Player")) return;

        isActive = false;
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("EnemySpawner: Chưa gán Enemy Prefab!");
            return;
        }

        Vector2 spawnPos;
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            // Sinh tại 1 điểm cố định ngẫu nhiên (các điểm này bạn đặt BÊN TRONG map)
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
            spawnPos = point != null ? (Vector2)point.position : (Vector2)player.position;
        }
        else
        {
            // Không có điểm cố định -> sinh quanh Player (có thể lọt ra ngoài nếu Player sát tường)
            float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector2 spawnDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)).normalized;
            spawnPos = (Vector2)player.position + spawnDirection * spawnRadius;
        }

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
