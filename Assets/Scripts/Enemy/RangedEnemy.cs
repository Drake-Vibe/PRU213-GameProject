using UnityEngine;

/// <summary>
/// Ranged enemy that shoots at the player when line-of-sight is clear.
/// Uses raycasting to detect player visibility through walls.
/// Moves toward the player when visible, stops and shoots.
/// </summary>
public class RangedEnemy : BaseEnemy
{
    [Header("Ranged AI")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float preferredDistance = 4f; // stop moving when this close

    [Header("Shooting")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float fireRate = 1f; // bullets per second
    [SerializeField] private float bulletDamage = 15f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private float timeSinceLastShot = 0f;
    private bool canSeePlayer = false;

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

        timeSinceLastShot += Time.deltaTime;

        float distanceToPlayer = Vector2.Distance(transform.position, targetPlayer.transform.position);

        if (distanceToPlayer <= detectionRange)
        {
            CheckLineOfSight();
            LookAtPlayer();

            if (canSeePlayer)
            {
                // Move toward player if too far, stop if close enough
                if (distanceToPlayer > preferredDistance)
                {
                    MoveTowardPlayer();
                }
                else
                {
                    StopMoving();
                }

                // Shoot
                if (timeSinceLastShot >= 1f / fireRate)
                {
                    timeSinceLastShot = 0f;
                    Fire();
                }
            }
            else
            {
                StopMoving();
            }
        }
        else
        {
            StopMoving();
        }
    }

    /// <summary>
    /// Raycast to check if the player is visible (no walls blocking).
    /// Similar to Soul Knight's GunnyEnemy.DoIfSeePlayer().
    /// </summary>
    private void CheckLineOfSight()
    {
        Vector2 direction = (targetPlayer.transform.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, targetPlayer.transform.position);

        string[] layers = new string[] { Layers.PLAYER, Layers.MAP_WALL };
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            direction,
            distance,
            LayerMask.GetMask(layers)
        );

        canSeePlayer = hit.collider != null && hit.collider.CompareTag(Tags.PLAYER);

        // Debug visualization
        Color debugColor = canSeePlayer ? Color.red : Color.blue;
        Debug.DrawRay(transform.position, direction * distance, debugColor);
    }

    private void LookAtPlayer()
    {
        Vector2 direction = targetPlayer.transform.position - transform.position;
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = direction.x < 0;
        }
    }

    private void MoveTowardPlayer()
    {
        Vector2 direction = (targetPlayer.transform.position - transform.position).normalized;

        if (rb != null)
        {
            rb.linearVelocity = direction * moveSpeed;
        }

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
    /// Fire a bullet toward the player.
    /// </summary>
    private void Fire()
    {
        if (bulletPrefab == null) return;

        Vector2 direction = (targetPlayer.transform.position - transform.position).normalized;
        Quaternion rotation = Quaternion.FromToRotation(Vector2.up, direction);

        GameObject bulletObj = Instantiate(bulletPrefab, transform.position, rotation);
        Bullet bullet = bulletObj.GetComponent<Bullet>();

        if (bullet != null)
        {
            bullet.SetDamage(bulletDamage);
            bullet.SetOwnerTag(Tags.ENEMY);
        }

        // Tag the bullet as enemy bullet
        bulletObj.tag = Tags.ENEMY_BULLET;
    }
}
