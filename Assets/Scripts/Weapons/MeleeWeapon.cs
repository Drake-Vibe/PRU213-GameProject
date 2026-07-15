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

        // Attack on mouse click
        if (Input.GetMouseButtonDown(0) && !isAttacking && timeSinceLastAttack >= attackCooldown)
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

        if (collision.CompareTag(Tags.ENEMY))
        {
            GameObject target = collision.gameObject;
            if (!hitEnemies.Contains(target))
            {
                hitEnemies.Add(target); // Mark as hit to prevent double damage in this swing

                // Damage BaseEnemy if present
                BaseEnemy baseEnemy = target.GetComponent<BaseEnemy>();
                if (baseEnemy == null) baseEnemy = target.GetComponentInParent<BaseEnemy>();
                if (baseEnemy != null)
                {
                    baseEnemy.TakeDamage(damage);
                }
                else
                {
                    // Damage EnemyHealth if present (for enamy branch compatibility)
                    EnemyHealth enemyHealth = target.GetComponent<EnemyHealth>();
                    if (enemyHealth == null) enemyHealth = target.GetComponentInParent<EnemyHealth>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.TakeDamage((int)damage);
                    }
                }
            }
        }
    }
}
