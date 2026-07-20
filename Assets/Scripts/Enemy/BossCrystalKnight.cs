using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossCrystalKnight : MonoBehaviour
{
    [Header("Boss Stats")]
    [SerializeField] private int maxHealth = 500;
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float detectionRadius = 14f;
    [SerializeField] private int meleeDamage = 20;
    [SerializeField] private bool isActive = false;
    [SerializeField] private int scoreValue = 100;

    [Header("Attack Ranges")]
    [SerializeField] private float meleeAttackRange = 2.8f;
    [SerializeField] private float meleeHitboxRadius = 3.5f;

    [Header("Skill Cooldowns (Seconds)")]
    [SerializeField] private float meleeCooldown = 1.8f;
    [SerializeField] private float teleportCooldown = 5f;
    [SerializeField] private float lightningCooldown = 6f;

    private int currentHealth;
    private bool isDead = false;
    private bool isExecutingSkill = false;

    private float lastMeleeTime = 0f;
    private float lastTeleportTime = 0f;
    private float lastLightningTime = 0f;

    private Transform playerTransform;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Color originalColor = Color.white;

    // Sprite Animation Collections
    private Sprite[] idleSprites;
    private Sprite[] attackSprites;
    private Sprite[] teleportSprites;
    private Sprite deathSprite;
    private Sprite[] explosionSprites;

    private Animator animator;

    private float animTimer = 0f;
    private int animFrame = 0;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsActive => isActive;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        currentHealth = maxHealth;
        LoadBossSprites();
    }

    private void LoadBossSprites()
    {
#if UNITY_EDITOR
        idleSprites = LoadAllSpritesAtPath("Assets/Art/Sprites/GameAssets/Boss Sprite/Crystal Knight idle.png");
        attackSprites = LoadAllSpritesAtPath("Assets/Art/Sprites/GameAssets/Boss Sprite/Crystal Knight attack.png");
        teleportSprites = LoadAllSpritesAtPath("Assets/Art/Sprites/GameAssets/Boss Sprite/Crystal Knight Tele.png");
        
        Sprite[] deathArr = LoadAllSpritesAtPath("Assets/Art/Sprites/GameAssets/Boss Sprite/Crystal Knight death.png");
        if (deathArr != null && deathArr.Length > 0) deathSprite = deathArr[0];

        List<Sprite> expList = new List<Sprite>();
        Sprite[] exp1 = LoadAllSpritesAtPath("Assets/Art/Sprites/GameAssets/Boss Sprite/Crystal Knight explosion 1 .png");
        Sprite[] exp2 = LoadAllSpritesAtPath("Assets/Art/Sprites/GameAssets/Boss Sprite/Crystal Knight explosion 2 .png");
        Sprite[] exp3 = LoadAllSpritesAtPath("Assets/Art/Sprites/GameAssets/Boss Sprite/Crystal Knight explosion 3 .png");
        if (exp1 != null) expList.AddRange(exp1);
        if (exp2 != null) expList.AddRange(exp2);
        if (exp3 != null) expList.AddRange(exp3);
        explosionSprites = expList.ToArray();
#endif

        if (idleSprites != null && idleSprites.Length > 0 && spriteRenderer != null)
        {
            spriteRenderer.sprite = idleSprites[0];
        }
    }

#if UNITY_EDITOR
    private Sprite[] LoadAllSpritesAtPath(string path)
    {
        Object[] assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(path);
        List<Sprite> list = new List<Sprite>();
        foreach (Object a in assets)
        {
            if (a is Sprite s) list.Add(s);
        }
        return list.ToArray();
    }
#endif

    private void Start()
    {
        Player player = FindAnyObjectByType<Player>();
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    /// <summary>
    /// Kích hoạt Boss khi Player bước vào vùng Trigger của CombatRoom.
    /// </summary>
    public void ActivateBoss()
    {
        if (isActive || isDead) return;

        isActive = true;
        Debug.Log("👑 BOSS CRYSTAL KNIGHT ACTIVATED!");

        // Show Boss HUD upon entering battle
        if (BossHUD.Instance != null)
        {
            BossHUD.Instance.Initialize("CRYSTAL KNIGHT", currentHealth, maxHealth);
            BossHUD.Instance.Show();
        }
    }

    private void Update()
    {
        if (!isActive || isDead) return;

        // Animate idle / movement ONLY if there is no animator component
        if (animator == null && !isExecutingSkill && spriteRenderer != null && idleSprites != null && idleSprites.Length > 0)
        {
            animTimer += Time.deltaTime;
            if (animTimer >= 0.18f)
            {
                animTimer = 0f;
                animFrame = (animFrame + 1) % idleSprites.Length;
                spriteRenderer.sprite = idleSprites[animFrame];
            }
        }
    }

    private void FixedUpdate()
    {
        if (!isActive || isDead || isExecutingSkill) return;

        if (playerTransform == null)
        {
            Player p = FindAnyObjectByType<Player>();
            if (p != null) playerTransform = p.transform;
            else return;
        }

        float distToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distToPlayer <= detectionRadius)
        {
            // Skill decision tree based on cooldowns
            if (Time.time - lastTeleportTime >= teleportCooldown)
            {
                StartCoroutine(TeleportSkillRoutine());
            }
            else if (Time.time - lastLightningTime >= lightningCooldown)
            {
                StartCoroutine(LightningSkillRoutine());
            }
            else if (distToPlayer <= meleeAttackRange && Time.time - lastMeleeTime >= meleeCooldown)
            {
                StartCoroutine(MeleeAttackRoutine());
            }
            else
            {
                // Move towards player
                Vector2 moveDir = ((Vector2)playerTransform.position - rb.position).normalized;
                rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);
                if (animator != null) animator.SetBool("IsMoving", true);

                if (spriteRenderer != null)
                {
                    if (moveDir.x > 0.01f) spriteRenderer.flipX = false;
                    else if (moveDir.x < -0.01f) spriteRenderer.flipX = true;
                }
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            if (animator != null) animator.SetBool("IsMoving", false);
        }
    }

    // ==========================================
    // ⚔️ MELEE ATTACK (With Attack Animation & Hitbox)
    // ==========================================
    private IEnumerator MeleeAttackRoutine()
    {
        isExecutingSkill = true;
        lastMeleeTime = Time.time;
        rb.linearVelocity = Vector2.zero;
        if (animator != null)
        {
            animator.SetBool("IsMoving", false);
            animator.SetTrigger("Attack");
            
            // Wait for half the attack duration to check hitbox (slash timing)
            yield return new WaitForSeconds(0.3f);
            CheckMeleeHitbox();
            yield return new WaitForSeconds(0.3f); // Wait for animation to finish
        }
        else if (attackSprites != null && attackSprites.Length > 0 && spriteRenderer != null)
        {
            for (int i = 0; i < attackSprites.Length; i++)
            {
                spriteRenderer.sprite = attackSprites[i];
                
                // Trigger damage Hitbox on slash frame (middle of animation)
                if (i == attackSprites.Length / 2)
                {
                    CheckMeleeHitbox();
                }
                yield return new WaitForSeconds(0.12f);
            }
        }
        else
        {
            yield return new WaitForSeconds(0.25f);
            CheckMeleeHitbox();
        }

        yield return new WaitForSeconds(0.2f);
        isExecutingSkill = false;
    }

    private void CheckMeleeHitbox()
    {
        if (playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        if (dist <= meleeHitboxRadius)
        {
            Player p = playerTransform.GetComponent<Player>();
            if (p != null)
            {
                p.TakeDamage(meleeDamage, transform.position);
                Debug.Log($"⚔️ Boss Crystal Knight dealt {meleeDamage} melee damage to Player via Hitbox!");
            }
        }
    }

    // ==========================================
    // 🌀 TELEPORTATION SKILL (With Teleport Animation & Room Bounds)
    // ==========================================
    private IEnumerator TeleportSkillRoutine()
    {
        isExecutingSkill = true;
        lastTeleportTime = Time.time;
        rb.linearVelocity = Vector2.zero;

        // Tạm thời tắt Animator tự động trong lúc làm animation dịch chuyển thủ công
        if (animator != null) animator.enabled = false;

        // 1. Play Teleport Out Animation (Biến mất)
        if (teleportSprites != null && teleportSprites.Length > 0 && spriteRenderer != null)
        {
            for (int i = 0; i < teleportSprites.Length; i++)
            {
                spriteRenderer.sprite = teleportSprites[i];
                float alpha = 1f - ((float)i / teleportSprites.Length);
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return new WaitForSeconds(0.07f);
            }
        }

        if (spriteRenderer != null) spriteRenderer.enabled = false;

        // 2. Dịch chuyển vị trí mới trong giới hạn CombatRoom
        if (playerTransform != null)
        {
            bool teleportClose = Random.value > 0.5f;
            Vector3 targetPos;

            if (teleportClose)
            {
                Vector2 randomDir = Random.insideUnitCircle.normalized * 2.5f;
                targetPos = playerTransform.position + (Vector3)randomDir;
            }
            else
            {
                Vector2 randomDir = Random.insideUnitCircle.normalized * 5.5f;
                targetPos = playerTransform.position + (Vector3)randomDir;
            }

            CombatRoom room = FindAnyObjectByType<CombatRoom>();
            if (room != null)
            {
                BoxCollider2D roomCol = room.GetComponent<BoxCollider2D>();
                if (roomCol != null)
                {
                    Bounds b = roomCol.bounds;
                    targetPos.x = Mathf.Clamp(targetPos.x, b.min.x + 3f, b.max.x - 3f);
                    targetPos.y = Mathf.Clamp(targetPos.y, b.min.y + 3f, b.max.y - 3f);
                }
            }

            transform.position = targetPos;
        }

        yield return new WaitForSeconds(0.15f);

        // 3. Play Teleport In Animation (Hiện ra lại tại vị trí mới)
        if (spriteRenderer != null) spriteRenderer.enabled = true;

        if (teleportSprites != null && teleportSprites.Length > 0 && spriteRenderer != null)
        {
            for (int i = teleportSprites.Length - 1; i >= 0; i--)
            {
                spriteRenderer.sprite = teleportSprites[i];
                float alpha = 1f - ((float)i / teleportSprites.Length);
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return new WaitForSeconds(0.07f);
            }
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
            if (idleSprites != null && idleSprites.Length > 0)
            {
                spriteRenderer.sprite = idleSprites[0];
            }
        }

        // Bật lại Animator khi hoàn tất Dịch chuyển
        if (animator != null) animator.enabled = true;

        yield return new WaitForSeconds(0.2f);
        isExecutingSkill = false;
    }

    // ==========================================
    // ⚡ LIGHTNING SKILL
    // ==========================================
    private IEnumerator LightningSkillRoutine()
    {
        isExecutingSkill = true;
        lastLightningTime = Time.time;
        rb.linearVelocity = Vector2.zero;

        bool useLineAttack = Random.value > 0.5f;

        if (useLineAttack && playerTransform != null)
        {
            Vector3 startPos = transform.position;
            Vector3 dir = (playerTransform.position - startPos).normalized;
            Vector3 endPos = startPos + dir * 14f;

            LightningWarning.CreateLineWarning(startPos, endPos, 1.8f, 0.8f, 25);
        }
        else if (playerTransform != null)
        {
            LightningWarning.CreateBoxWarning(playerTransform.position, new Vector2(3.5f, 3.5f), 0.8f, 30);
        }

        yield return new WaitForSeconds(1.1f);
        isExecutingSkill = false;
    }

    // ==========================================
    // ✨ DAMAGE FLASH & HP HANDLING
    // ==========================================
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        // Auto-activate if attacked before entering trigger
        if (!isActive)
        {
            ActivateBoss();
        }

        currentHealth -= damage;

        if (BossHUD.Instance != null)
        {
            BossHUD.Instance.UpdateHealth(currentHealth, maxHealth);
        }

        StartCoroutine(FlashDamageRoutine());

        if (currentHealth <= 0)
        {
            StartCoroutine(ExplodeOnDeathRoutine());
        }
    }

    private IEnumerator FlashDamageRoutine()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 0.3f, 0.3f, 1f);
            yield return new WaitForSeconds(0.12f);
            spriteRenderer.color = originalColor;
        }
    }

    // ==========================================
    // 💣 DEATH EXPLOSION & ANIMATION
    // ==========================================
    private IEnumerator ExplodeOnDeathRoutine()
    {
        isDead = true;
        isExecutingSkill = true;

        // Add score to GameManager
        GameManager gm = GameManager.Instance;
        if (gm != null)
        {
            gm.AddScore(scoreValue);
        }

        rb.linearVelocity = Vector2.zero;
        if (animator != null) animator.SetTrigger("Die");

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Set Death Pose sprite
        if (deathSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = deathSprite;
        }

        // Rapid flashing & charging for 1.2s
        float explodeTimer = 0f;
        while (explodeTimer < 1.2f)
        {
            explodeTimer += Time.deltaTime;
            if (spriteRenderer != null)
            {
                float t = Mathf.PingPong(explodeTimer * 20f, 1f);
                spriteRenderer.color = Color.Lerp(Color.red, Color.yellow, t);
            }
            yield return null;
        }

        // Play Explosion FX using Crystal Knight explosion 1, 2, 3 sprites!
        if (explosionSprites != null && explosionSprites.Length > 0 && spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
            foreach (Sprite expSprite in explosionSprites)
            {
                spriteRenderer.sprite = expSprite;
                yield return new WaitForSeconds(0.1f);
            }
        }

        // Detonate Area-of-Effect Lightning Box Warning explosion
        LightningWarning.CreateBoxWarning(transform.position, new Vector2(6f, 6f), 0.1f, 40);

        // Hide Boss HUD
        if (BossHUD.Instance != null)
        {
            BossHUD.Instance.Hide();
        }

        // Unlock Exit Portal
        NextLevelPortal portal = FindAnyObjectByType<NextLevelPortal>();
        if (portal != null)
        {
            portal.SetLocked(false);
            portal.gameObject.SetActive(true);
        }

        Debug.Log("💥 Boss Crystal Knight exploded and died!");
        Destroy(gameObject, 0.2f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Phản hồi vật lý va chạm, không gây sát thương va chạm trực tiếp theo yêu cầu
    }

    private void OnDrawGizmosSelected()
    {
        // Vòng tròn đỏ: Vùng gây sát thương đòn chém Melee Hitbox
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeHitboxRadius);

        // Vòng tròn vàng: Vùng phát hiện người chơi
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
