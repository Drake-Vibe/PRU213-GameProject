using UnityEngine;

/// <summary>
/// Ranged weapon that fires bullets. Handles fire rate and spawning bullet prefabs.
/// Inherits mouse-aim rotation from BaseWeapon.
/// Usage: Attach to a Gun GameObject, assign bulletPrefab in inspector.
/// </summary>
public class Gun : BaseWeapon
{
    [Header("Gun Config")]
    public GameObject bulletPrefab;
    public int energyCost = 1;

    [SerializeField]
    private float fireRate = 3f; // bullets per second

    private float timeSinceLastShot = 0f;

    protected override Quaternion GetRotationOffset()
    {
        // Gun sprite typically points up, rotate -90 so it points right (along aim direction)
        return Quaternion.Euler(0f, 0f, -90f);
    }

    protected override void Update()
    {
        base.Update();

        if (parentEntity == null) return;

        timeSinceLastShot += Time.deltaTime;

        // Fire when mouse button is held and fire rate allows
        if (Input.GetMouseButton(0) && timeSinceLastShot >= 1f / fireRate)
        {
            Player player = parentEntity.GetComponent<Player>();
            if (player != null)
            {
                if (player.currentEnergy < energyCost) return; // Not enough energy
                player.currentEnergy -= energyCost;
            }

            timeSinceLastShot = 0f;
            Fire();
        }
    }

    /// <summary>
    /// Override in subclasses for custom fire behavior (shotgun, burst, etc.)
    /// </summary>
    protected virtual void Fire()
    {
        if (bulletPrefab == null)
        {
            Debug.LogWarning("Gun: No bulletPrefab assigned!");
            return;
        }

        // Spawn bullet slightly ahead of the gun muzzle
        Vector3 muzzleOffset = transform.rotation * Vector3.up * 0.5f;
        Vector3 bulletSpawnPos = transform.position + muzzleOffset;

        GameObject bulletObj = Instantiate(bulletPrefab, bulletSpawnPos, transform.rotation);
        Bullet bullet = bulletObj.GetComponent<Bullet>();

        if (bullet != null)
        {
            bullet.SetDamage(damage);
            bullet.SetOwnerTag(Tags.PLAYER);
        }

        bulletObj.SetActive(true);
    }
}
