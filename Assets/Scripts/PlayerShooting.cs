using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletForce = 20f;
    public float fireRate = 0.5f;

    private float nextFireTime = 0f;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        // Use Mouse from InputSystem
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("PlayerShooting: Bullet Prefab or Fire Point is not assigned!");
            return;
        }

        // Determine mouse position in world space
        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Vector2 firePointPos = firePoint.position;
        Vector2 lookDir = mousePos - firePointPos;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;

        // Instantiate bullet with correct rotation
        GameObject bullet = Instantiate(bulletPrefab, firePointPos, Quaternion.Euler(0f, 0f, angle));
        
        // Add force to the bullet
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(lookDir.normalized * bulletForce, ForceMode2D.Impulse);
        }
    }
}
