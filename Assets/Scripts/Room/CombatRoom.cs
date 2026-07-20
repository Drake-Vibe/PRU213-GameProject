using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Phòng đấu tự động kích hoạt khi Player bước vào:
/// - Khi Player bước vào vùng Trigger của phòng -> ĐÓNG cổng, bắt đầu sinh quái / kích hoạt quái đặt sẵn.
/// - Trên màn hình tối đa "maxAliveEnemies" con cùng lúc; chết con nào sinh bù con đó.
/// - Khi tổng số quái bị tiêu diệt đạt "totalEnemiesToKill" -> MỞ cổng, kích hoạt Exit Portal (nếu có).
/// </summary>
public class CombatRoom : MonoBehaviour
{
    [Header("Kích hoạt & Quái đặt sẵn")]
    [Tooltip("Tự động kích hoạt trận đấu khi Player bước vào vùng Trigger của phòng này.")]
    public bool triggerOnPlayerEnter = true;
    [Tooltip("Kéo các quái vật đặt sẵn trong phòng này vào đây nếu muốn dùng quái sinh sẵn thay vì sinh bằng Prefab.")]
    public List<GameObject> prePlacedEnemies = new List<GameObject>();

    [Header("Spawn Settings")]
    [Tooltip("Prefab quái (kéo prefab Zombie vào).")]
    public GameObject enemyPrefab;
    [Tooltip("Tổng số quái phải tiêu diệt để mở cổng.")]
    public int totalEnemiesToKill = 10;
    [Tooltip("Số quái tối đa cùng lúc trên màn hình.")]
    public int maxAliveEnemies = 5;
    [Tooltip("Giãn cách giữa 2 lần sinh quái (giây).")]
    public float spawnInterval = 1.5f;
    [Tooltip("Các điểm sinh quái (đặt bên trong phòng). Bỏ trống = sinh ngẫu nhiên quanh tâm phòng.")]
    public Transform[] spawnPoints;
    [Tooltip("Bán kính sinh quanh tâm phòng (khi không dùng Spawn Points).")]
    public float spawnRadius = 4f;

    [Header("Gates (thanh chặn cổng)")]
    [Tooltip("Các barrier sẽ BẬT (chặn) khi đóng cổng, TẮT khi mở. Để trống nếu chưa làm cổng.")]
    public GameObject[] gates;

    [Header("Nhốt player trong phòng khi đang đánh")]
    [Tooltip("Bật: khi đang đánh, player KHÔNG ra khỏi vùng Box Collider 2D của object này được (khỏi cần làm Gate). " +
             "Nhớ thêm Box Collider 2D (tick Is Trigger) bao trọn căn phòng.")]
    public bool keepPlayerInside = true;
    [Tooltip("Khoảng đệm để player không dính sát mép tường (đơn vị Unity).")]
    public float insideMargin = 0.3f;

    [Header("Cổng dịch chuyển qua màn (Exit Portal)")]
    [Tooltip("Cổng này sẽ xuất hiện (SetActive(true)) khi dọn sạch phòng. Để trống nếu không dùng.")]
    public GameObject exitPortal;

    private bool triggered = false;   // đã bắt đầu trận chưa
    private bool cleared = false;     // đã dọn sạch chưa
    private int spawnedCount = 0;     // đã sinh tổng cộng bao nhiêu
    private int killedCount = 0;      // đã tiêu diệt bao nhiêu
    private float timer = 0f;
    private readonly List<GameObject> aliveEnemies = new List<GameObject>();
    private BoxCollider2D roomArea;
    private Transform playerTf;

    private void Start()
    {
        roomArea = GetComponent<BoxCollider2D>();
        SetGatesClosed(false); // cổng mở lúc đầu
        
        // Cổng qua màn ẩn lúc đầu (chỉ hiện khi dọn sạch phòng)
        if (exitPortal != null)
        {
            exitPortal.SetActive(false);
            NextLevelPortal portal = exitPortal.GetComponent<NextLevelPortal>();
            if (portal != null) portal.SetLocked(true);
        }
    }

    private void Update()
    {
        if (cleared || !triggered) return;

        // ===== Đang trong trận =====
        // Nhốt player trong phòng
        if (keepPlayerInside) ClampPlayerInside();

        // Đếm quái đã chết (bị Destroy -> null)
        for (int i = aliveEnemies.Count - 1; i >= 0; i--)
        {
            if (aliveEnemies[i] == null)
            {
                aliveEnemies.RemoveAt(i);
                killedCount++;
                Debug.Log(name + " (CombatRoom): Đã diệt " + killedCount + "/" + totalEnemiesToKill);
            }
        }

        // Diệt đủ -> mở cổng
        if (killedCount >= totalEnemiesToKill)
        {
            cleared = true;
            SetGatesClosed(false);
            if (exitPortal != null)
            {
                exitPortal.SetActive(true);
                NextLevelPortal portal = exitPortal.GetComponent<NextLevelPortal>();
                if (portal != null) portal.SetLocked(false); // Mở khóa cổng khi diệt sạch quái
            }
            Debug.Log(name + " (CombatRoom): DỌN SẠCH PHÒNG! Mở cổng.");
            return;
        }

        // Sinh thêm: giữ tối đa maxAliveEnemies, không sinh quá tổng cần diệt
        timer += Time.deltaTime;
        if (timer >= spawnInterval
            && aliveEnemies.Count < maxAliveEnemies
            && spawnedCount < totalEnemiesToKill)
        {
            SpawnOne();
            timer = 0f;
        }
    }

    public void StartBattle()
    {
        if (triggered || cleared) return;
        triggered = true;

        // Add pre-placed enemies to track list
        if (prePlacedEnemies != null && prePlacedEnemies.Count > 0)
        {
            foreach (var enemy in prePlacedEnemies)
            {
                if (enemy != null)
                {
                    if (!aliveEnemies.Contains(enemy))
                    {
                        aliveEnemies.Add(enemy);
                    }
                    ConfigureEnemy(enemy);

                    BossCrystalKnight boss = enemy.GetComponent<BossCrystalKnight>();
                    if (boss != null)
                    {
                        boss.ActivateBoss();
                    }
                }
            }
            // If enemyPrefab is not defined, we only need to kill the preplaced ones
            if (enemyPrefab == null)
            {
                totalEnemiesToKill = aliveEnemies.Count;
                spawnedCount = aliveEnemies.Count;
            }
        }

        SetGatesClosed(true); // đóng cổng nhốt player
        Debug.Log(name + " (CombatRoom): Đấu trường bắt đầu! Đóng cổng, diệt "
                  + totalEnemiesToKill + " quái để mở.");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (cleared || triggered) return;

        if (triggerOnPlayerEnter && collision.CompareTag("Player"))
        {
            StartBattle();
        }
    }

    private void SpawnOne()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning(name + " (CombatRoom): chưa gán Enemy Prefab!");
            return;
        }

        Vector2 pos = GetSpawnPosition(spawnedCount);
        GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
        ConfigureEnemy(enemy);
        aliveEnemies.Add(enemy);
        spawnedCount++;
    }

    // Giữ player nằm trong vùng Box Collider của phòng (khi đang đánh)
    private void ClampPlayerInside()
    {
        if (roomArea == null) return;

        if (playerTf == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTf = p.transform;
        }
        if (playerTf == null) return;

        Bounds b = roomArea.bounds;
        Vector3 pos = playerTf.position;
        pos.x = Mathf.Clamp(pos.x, b.min.x + insideMargin, b.max.x - insideMargin);
        pos.y = Mathf.Clamp(pos.y, b.min.y + insideMargin, b.max.y - insideMargin);
        playerTf.position = pos;
    }

    private void SetGatesClosed(bool closed)
    {
        if (gates == null) return;
        foreach (var g in gates)
            if (g != null) g.SetActive(closed);
    }

    private Vector2 GetSpawnPosition(int index)
    {
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform p = spawnPoints[index % spawnPoints.Length];
            if (p != null) return p.position;
        }

        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        return (Vector2)transform.position + dir * Random.Range(spawnRadius * 0.4f, spawnRadius);
    }

    /// <summary>Đảm bảo quái có đủ component (giống EnemySpawner).</summary>
    private void ConfigureEnemy(GameObject enemy)
    {
        SpriteRenderer sr = enemy.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Player";
            sr.sortingOrder = 2;
        }

        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        if (rb == null) rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (enemy.GetComponent<EnemyChase>() == null) enemy.AddComponent<EnemyChase>();
        if (enemy.GetComponent<EnemyHealth>() == null) enemy.AddComponent<EnemyHealth>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0.1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
