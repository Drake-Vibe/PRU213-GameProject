using UnityEngine;

/// <summary>
/// Melee weapon (sword, axe, etc.) that attacks with an animation-driven collider.
/// Uses a child object with a collider that enables/disables during attack.
/// Usage: Attach to a melee weapon GameObject. Create a child "AttackZone" with a Collider2D (IsTrigger).
/// </summary>
public class MeleeWeapon : BaseWeapon
{
    [Header("Melee Config")]
    [SerializeField] private Collider2D attackCollider;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float attackDuration = 0.2f;

    private float timeSinceLastAttack = 0f;
    private bool isAttacking = false;
    private float attackTimer = 0f;
    private System.Collections.Generic.List<GameObject> hitEnemies = new System.Collections.Generic.List<GameObject>();

    /// <summary>
    /// Whether the weapon is currently in an attack swing.
    /// Used by enemies to check if they should take damage.
    /// </summary>
    public bool IsAttacking => isAttacking;

    protected override Vector3 GetPositionOffset()
    {
        return new Vector3(0.3f, 0.2f, -1f);
    }

    protected override Quaternion GetRotationOffset()
    {
        return Quaternion.Euler(0f, 0f, -90f);
    }

    protected override void Awake()
    {
        base.Awake();
        if (attackCollider != null)
            attackCollider.enabled = false;
    }

    protected override void Update()
    {
        base.Update();

        if (parentEntity == null) return;

        timeSinceLastAttack += Time.deltaTime;

        // Attack when attack key is pressed
        if (IsAttackPressed() && !isAttacking && timeSinceLastAttack >= attackCooldown)
        {
            StartAttack();
        }

        // Handle attack duration and rotate the sword smoothly to represent a 3-phase swing
        if (isAttacking)
        {
            attackTimer += Time.deltaTime;
            float t = Mathf.Clamp01(attackTimer / attackDuration);

            if (t < 0.25f) // Phase 1: Wind-up (pull back from 0 to -60 degrees, no damage)
            {
                float p = t / 0.25f;
                attackRotationOffset = Mathf.Lerp(0f, -60f, p);
                if (attackCollider != null) attackCollider.enabled = false;
            }
            else if (t < 0.8f) // Phase 2: Slash (vung chém từ -60 to 60 degrees, trigger damage)
            {
                float p = (t - 0.25f) / 0.55f;
                attackRotationOffset = Mathf.Lerp(-60f, 60f, p);
                if (attackCollider != null) attackCollider.enabled = true;
            }
            else // Phase 3: Recovery (return from 60 to 0 degrees, no damage)
            {
                float p = (t - 0.8f) / 0.2f;
                attackRotationOffset = Mathf.Lerp(60f, 0f, p);
                if (attackCollider != null) attackCollider.enabled = false;
            }

            if (attackTimer >= attackDuration)
            {
                EndAttack();
            }
        }
    }

    private void StartAttack()
    {
        isAttacking = true;
        attackTimer = 0f;
        timeSinceLastAttack = 0f;
        hitEnemies.Clear(); // Clear the list of hit targets for this new swing

        if (attackCollider != null)
            attackCollider.enabled = false; // Start disabled during wind-up phase
    }

    private void EndAttack()
    {
        isAttacking = false;
        attackTimer = 0f;
        attackRotationOffset = 0f; // Return to the normal aim rotation

        if (attackCollider != null)
            attackCollider.enabled = false;
    }

    /// <summary>
    /// Called when the attack collider hits an enemy (forwarded via MeleeAttackZone).
    /// </summary>
    public void OnAttackTriggerEnter2D(Collider2D collision)
    {
        if (!isAttacking) return;

        GameObject target = collision.gameObject;
        
        // Find enemy scripts on the target or its parent
        BaseEnemy baseEnemy = target.GetComponent<BaseEnemy>();
        if (baseEnemy == null) baseEnemy = target.GetComponentInParent<BaseEnemy>();

        EnemyHealth enemyHealth = null;
        if (baseEnemy == null)
        {
            enemyHealth = target.GetComponent<EnemyHealth>();
            if (enemyHealth == null) enemyHealth = target.GetComponentInParent<EnemyHealth>();
        }

        bool isEnemy = collision.CompareTag(Tags.ENEMY) || baseEnemy != null || enemyHealth != null;

        if (isEnemy)
        {
            // Resolve the root target object to prevent hitting multiple child colliders of the same enemy
            GameObject rootTarget = target;
            if (baseEnemy != null) rootTarget = baseEnemy.gameObject;
            else if (enemyHealth != null) rootTarget = enemyHealth.gameObject;

            if (!hitEnemies.Contains(rootTarget))
            {
                hitEnemies.Add(rootTarget); // Mark as hit to prevent double damage in this swing

                // Apply knockback to the enemy using the existing EnemyChase component before dealing damage
                if (parentEntity != null)
                {
                    Vector2 knockbackDir = (rootTarget.transform.position - parentEntity.transform.position).normalized;
                    EnemyChase enemyChase = rootTarget.GetComponent<EnemyChase>();
                    if (enemyChase == null) enemyChase = rootTarget.GetComponentInParent<EnemyChase>();
                    if (enemyChase != null)
                    {
                        enemyChase.ApplyKnockback(knockbackDir * 12f, 0.25f);
                    }
                }

                // Deal damage using the found component
                if (baseEnemy != null)
                {
                    baseEnemy.TakeDamage(damage);
                }
                else if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage((int)damage);
                }
            }
        }
    }

    private bool IsAttackPressed()
    {
        KeyCode attackKey = SettingsManager.CurrentSettings != null ? SettingsManager.CurrentSettings.attack : KeyCode.Mouse0;
        if (attackKey == KeyCode.Mouse0) return Input.GetMouseButtonDown(0);
        if (attackKey == KeyCode.Mouse1) return Input.GetMouseButtonDown(1);
        if (attackKey == KeyCode.Mouse2) return Input.GetMouseButtonDown(2);
        return Input.GetKeyDown(attackKey);
    }
}
