using UnityEngine;

/// <summary>
/// Melee enemy that chases the player and deals contact damage.
/// Moves directly toward the player when in detection range.
/// </summary>
public class MeleeEnemy : BaseEnemy
{
    [Header("Melee AI")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float contactDamage = 20f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    protected override void Start()
    {
        base.Start();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    protected override void Update()
    {
        base.Update();

        if (targetPlayer == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, targetPlayer.transform.position);

        if (distanceToPlayer <= detectionRange)
        {
            ChasePlayer();
        }
        else
        {
            StopMoving();
        }
    }

    private void ChasePlayer()
    {
        Vector2 direction = (targetPlayer.transform.position - transform.position).normalized;

        // Move toward player
        if (rb != null)
        {
            rb.linearVelocity = direction * moveSpeed;
        }

        // Flip sprite to face player
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = direction.x < 0;
        }

        // Set animation
        if (animator != null)
        {
            animator.SetBool("move", true);
        }
    }

    private void StopMoving()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        if (animator != null)
        {
            animator.SetBool("move", false);
        }
    }

    /// <summary>
    /// Deal contact damage to player.
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(Tags.PLAYER))
        {
            Player player = collision.collider.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage((int)contactDamage);
            }
        }
    }
}
