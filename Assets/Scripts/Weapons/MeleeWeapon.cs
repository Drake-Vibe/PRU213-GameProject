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

        // Handle attack duration
        if (isAttacking)
        {
            attackTimer += Time.deltaTime;
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

        if (attackCollider != null)
            attackCollider.enabled = true;
    }

    private void EndAttack()
    {
        isAttacking = false;
        attackTimer = 0f;

        if (attackCollider != null)
            attackCollider.enabled = false;
    }

    /// <summary>
    /// Called when the attack collider hits an enemy.
    /// Handles damage dealing via OnTriggerEnter2D on the attack collider's child object.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isAttacking) return;

        if (collision.CompareTag(Tags.ENEMY))
        {
            BaseEnemy enemy = collision.GetComponent<BaseEnemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
    }
}
