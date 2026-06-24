using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMelee : MonoBehaviour
{
    [Header("Melee Settings")]
    public float attackRange = 1.5f;        // Tầm đánh tay
    public int meleeDamage = 30;            // Sát thương mỗi đòn
    public float attackCooldown = 0.5f;     // Thời gian chờ giữa 2 lần đánh
    public float knockbackForce = 8f;       // Lực đẩy lùi quái khi bị đánh

    private float lastAttackTime = -999f;
    private Camera mainCamera;
    private SpriteRenderer playerSr;

    private void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null) mainCamera = FindObjectOfType<Camera>();
        playerSr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Nhấn chuột PHẢI để tấn công melee
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            TryMeleeAttack();
        }
    }

    private void TryMeleeAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;

        // Tính hướng từ player đến chuột
        Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 attackDir = (mouseWorldPos - (Vector2)transform.position).normalized;

        // ===== Tạo hiệu ứng vòng tròn flash =====
        ShowSlashEffect(attackDir);

        // ===== Gây sát thương cho enemy trong tầm =====
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, attackRange);
        foreach (Collider2D hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            // Kiểm tra góc tấn công (120 độ phía trước)
            Vector2 dirToTarget = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
            float dot = Vector2.Dot(attackDir, dirToTarget);
            if (dot > -0.5f)
            {
                EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(meleeDamage);
                    Debug.Log("Melee trúng: " + hit.name + " (-" + meleeDamage + " máu)");

                    // ===== ĐẨY LÙI QUÁI =====
                    Vector2 knockDir = ((Vector2)hit.transform.position - (Vector2)transform.position).normalized;
                    EnemyChase chase = hit.GetComponent<EnemyChase>();
                    if (chase != null)
                    {
                        // Dùng method ApplyKnockback của EnemyChase (tạm dừng AI đuổi trong lúc bị đẩy)
                        chase.ApplyKnockback(knockDir * knockbackForce);
                    }
                    else
                    {
                        // Fallback: Đẩy trực tiếp qua Rigidbody2D
                        Rigidbody2D enemyRb = hit.GetComponent<Rigidbody2D>();
                        if (enemyRb != null)
                        {
                            enemyRb.linearVelocity = Vector2.zero;
                            enemyRb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
                        }
                    }
                }
            }
        }
    }

    private void ShowSlashEffect(Vector2 direction)
    {
        // Tạo GameObject hiệu ứng tạm thời
        GameObject slashObj = new GameObject("SlashEffect");
        slashObj.transform.position = (Vector2)transform.position + direction * (attackRange * 0.5f);

        // Thêm SpriteRenderer với hình tròn trắng/vàng
        SpriteRenderer sr = slashObj.AddComponent<SpriteRenderer>();
        sr.sprite = GetComponent<SpriteRenderer>()?.sprite; // dùng sprite bất kỳ làm hình
        sr.color = new Color(1f, 0.9f, 0f, 0.85f); // Màu vàng sáng
        slashObj.transform.localScale = new Vector3(attackRange * 1.2f, attackRange * 1.2f, 1f);

        // Lấy đúng sorting layer của Player
        if (playerSr != null)
        {
            sr.sortingLayerID = playerSr.sortingLayerID;
        }
        sr.sortingOrder = 100; // Hiển thị trên cùng

        // Tự xóa sau 0.15 giây
        StartCoroutine(FadeOutAndDestroy(slashObj, sr, 0.15f));
    }

    private IEnumerator FadeOutAndDestroy(GameObject obj, SpriteRenderer sr, float duration)
    {
        float elapsed = 0f;
        Color startColor = sr.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / duration);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }
        Destroy(obj);
    }

    // Vẽ vùng tấn công trong Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
