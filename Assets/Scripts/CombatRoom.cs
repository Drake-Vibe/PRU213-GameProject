using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

/// <summary>
/// Phòng đấu kích hoạt bằng KIM CƯƠNG:
/// - Player BẤM CHUỘT vào viên kim cương -> "lấy" kim cương (ẩn đi), ĐÓNG cổng, bắt đầu sinh quái.
/// - Trên màn hình tối đa "maxAliveEnemies" con cùng lúc; chết con nào sinh bù con đó.
/// - Khi tổng số quái bị tiêu diệt đạt "totalEnemiesToKill" -> MỞ cổng, đánh dấu đã dọn.
/// - Đã dọn thì thôi (không sinh lại).
///
/// LƯU Ý: viên kim cương ("gem") phải có 1 Collider2D để bấm chuột trúng.
/// </summary>
public class CombatRoom : MonoBehaviour
{
    [Header("Kích hoạt bằng kim cương")]
    [Tooltip("Object viên kim cương (collider để bấm chuột). Player bấm vào đây để bắt đầu trận.")]
    public GameObject gem;
    [Tooltip("Tilemap chứa HÌNH viên kim cương (thường là 'Decor'). Khi lấy sẽ xoá ô kim cương khỏi tilemap này. Bỏ trống nếu không cần xoá hình.")]
    public Tilemap gemTilemap;

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

    private bool triggered = false;   // đã bắt đầu trận chưa
    private bool cleared = false;     // đã dọn sạch chưa
    private int spawnedCount = 0;     // đã sinh tổng cộng bao nhiêu
    private int killedCount = 0;      // đã tiêu diệt bao nhiêu
    private float timer = 0f;
    private readonly List<GameObject> aliveEnemies = new List<GameObject>();
    private Camera cam;
    private BoxCollider2D roomArea;
    private Transform playerTf;

    private void Start()
    {
        cam = Camera.main;
        roomArea = GetComponent<BoxCollider2D>();
        SetGatesClosed(false); // cổng mở lúc đầu
    }

    private void Update()
    {
        if (cleared) return;

        // Chưa bắt đầu -> chờ Player bấm chuột vào kim cương
        if (!triggered)
        {
            CheckGemClick();
            return;
        }

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

    // Kiểm tra Player có bấm chuột trái vào viên kim cương không
    private void CheckGemClick()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;

        if (gem == null)
        {
            Debug.LogWarning(name + " (CombatRoom): ô 'Gem' đang TRỐNG — hãy kéo object Gem vào Inspector!");
            return;
        }

        if (cam == null) cam = Camera.main;
        if (cam == null) cam = FindObjectOfType<Camera>();
        if (cam == null) return;

        Vector2 worldPoint = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Collider2D[] hits = Physics2D.OverlapPointAll(worldPoint);
        Debug.Log(name + " (CombatRoom): click tại " + worldPoint + " -> trúng " + hits.Length + " collider");

        foreach (Collider2D h in hits)
        {
            if (h == null) continue;
            if (h.gameObject == gem || h.transform.IsChildOf(gem.transform))
            {
                StartBattle();
                return;
            }
        }
    }

    private void StartBattle()
    {
        triggered = true;
        if (gem != null) gem.SetActive(false); // ẩn object collider của kim cương
        ClearGemTile();                          // xoá HÌNH kim cương khỏi tilemap Decor
        SetGatesClosed(true);                    // đóng cổng nhốt player
        Debug.Log(name + " (CombatRoom): Đã lấy kim cương! Đóng cổng, diệt "
                  + totalEnemiesToKill + " quái để mở.");
    }

    // Xoá các ô tile nằm trong vùng collider của Gem ra khỏi tilemap (làm biến mất hình kim cương)
    private void ClearGemTile()
    {
        if (gem == null) return;
        if (gemTilemap == null)
        {
            Debug.LogWarning(name + " (CombatRoom): ô 'Gem Tilemap' TRỐNG — kéo tilemap chứa hình kim cương (Decor) vào để xoá được hình.");
            return;
        }

        Collider2D col = gem.GetComponent<Collider2D>();
        Bounds b = col != null ? col.bounds : new Bounds(gem.transform.position, Vector3.one * 0.6f);
        // Nới rộng một chút để chắc chắn phủ hết ô chứa viên kim cương
        b.Expand(0.5f);

        Vector3Int min = gemTilemap.WorldToCell(b.min);
        Vector3Int max = gemTilemap.WorldToCell(b.max);
        int count = 0;
        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                Vector3Int cell = new Vector3Int(x, y, 0);
                if (gemTilemap.GetTile(cell) != null)
                {
                    gemTilemap.SetTile(cell, null);
                    count++;
                }
            }
        }
        Debug.Log(name + " (CombatRoom): đã xoá " + count + " ô tile quanh kim cương khỏi '" + gemTilemap.name + "'.");
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
