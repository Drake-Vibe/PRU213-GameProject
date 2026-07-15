using UnityEngine;

/// <summary>
/// Bullet projectile that travels in a direction and deals damage on contact.
/// Destroys itself when hitting walls, enemies, or after a timeout.
/// Usage: Create a prefab with Rigidbody2D (Kinematic), Collider2D (IsTrigger), and this script.
/// </summary>
public class Bullet : MonoBehaviour
{
    [Header("Bullet Config")]
    public float speed = 12f;
    public float lifetime = 3f; // auto-destroy after this many seconds

    private float damage = 2f;
    private string ownerTag = Tags.PLAYER; // who shot this bullet
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Auto-destroy after lifetime to prevent memory leaks
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        // Move in the "up" direction of the bullet's rotation
        transform.Translate(Vector3.up * speed * Time.fixedDeltaTime, Space.Self);
    }

    public void SetDamage(float dmg)
    {
        damage = dmg;
    }

    public float GetDamage()
    {
        return damage;
    }

    /// <summary>
    /// Set who fired this bullet to avoid self-damage.
    /// Use Tags.PLAYER for player bullets, Tags.ENEMY for enemy bullets.
    /// </summary>
    public void SetOwnerTag(string tag)
    {
        ownerTag = tag;
    }

    public string GetOwnerTag()
    {
        return ownerTag;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        string otherTag = collision.gameObject.tag;

        // Don't hit the owner
        if (otherTag == ownerTag) return;

        // Ignore collisions with other bullets, dropped weapons, or items
        if (otherTag == "Bullet" || otherTag == "EnemyBullet" ||
            otherTag == "Item" || otherTag == "Weapon" || otherTag == "WeaponPickup")
        {
            return;
        }

        // Determine if this is the target of the bullet
        bool isTarget = (ownerTag == Tags.PLAYER && otherTag == Tags.ENEMY) ||
                        (ownerTag == Tags.ENEMY && otherTag == Tags.PLAYER);

        // Destroy the bullet if it hits a solid obstacle (non-trigger) or the intended target
        if (!collision.isTrigger || isTarget)
        {
            Destroy(gameObject);
        }
    }
}
