using UnityEngine;

/// <summary>
/// Base class for all weapons. Handles following the parent entity (player)
/// and rotating to face the mouse cursor.
/// Attach to a weapon GameObject that is a child or independent object.
/// </summary>
public class BaseWeapon : MonoBehaviour
{
    [Header("Weapon Config")]
    public GameObject parentEntity;
    public float damage = 1f;
    public string weaponName = "Weapon";
    protected float attackRotationOffset = 0f; // Attack animation rotation offset

    [Header("Sprite")]
    [SerializeField] protected SpriteRenderer spriteRenderer;

    /// <summary>
    /// Override to set the local position offset relative to the parent entity.
    /// </summary>
    protected virtual Vector3 GetPositionOffset()
    {
        return new Vector3(0.5f, 0f, -1f);
    }

    /// <summary>
    /// Override to set additional rotation offset for the weapon sprite.
    /// </summary>
    protected virtual Quaternion GetRotationOffset()
    {
        return Quaternion.identity;
    }

    protected virtual void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // Dynamically offset sprite rendering to rotate around the handle/base of the weapon
        if (spriteRenderer != null && spriteRenderer.transform == transform)
        {
            GameObject visualObj = new GameObject("Visual");
            visualObj.transform.SetParent(transform, false);

            SpriteRenderer newSr = visualObj.AddComponent<SpriteRenderer>();
            newSr.sprite = spriteRenderer.sprite;
            newSr.color = spriteRenderer.color;
            newSr.material = spriteRenderer.material;
            newSr.sortingOrder = spriteRenderer.sortingOrder;
            newSr.sortingLayerName = spriteRenderer.sortingLayerName;
            newSr.flipX = spriteRenderer.flipX;
            newSr.flipY = spriteRenderer.flipY;

            // Offset the sprite renderer so its bottom border is aligned with the pivot (0,0,0)
            if (newSr.sprite != null)
            {
                float offsetY = -newSr.sprite.bounds.min.y;
                visualObj.transform.localPosition = new Vector3(0f, offsetY, 0f);
            }

            Destroy(spriteRenderer);
            spriteRenderer = newSr;
        }
    }

    protected virtual void Start() { }

    protected virtual void Update()
    {
        if (parentEntity == null) return;

        FollowParentAndAimAtMouse();
    }

    /// <summary>
    /// Positions the weapon at a fixed local offset and rotates it to face the mouse.
    /// Rotates around the Y-axis to flip when aiming left to preserve pivot point.
    /// </summary>
    private void FollowParentAndAimAtMouse()
    {
        if (Camera.main == null) return;

        Vector3 mousePos = Input.mousePosition;
        if (float.IsNaN(mousePos.x) || float.IsNaN(mousePos.y) || float.IsInfinity(mousePos.x) || float.IsInfinity(mousePos.y))
        {
            return;
        }

        // Get mouse position in world space
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePos);
        Vector2 weaponPos = (Vector2)transform.position;
        Vector2 aimDirection = mouseWorldPos - weaponPos;

        // Calculate rotation angle
        float rotationDeg = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        // Determine if aiming left to flip the weapon and follow player flip
        bool aimingLeft = rotationDeg > 90f || rotationDeg < -90f;

        // Position weapon dynamically based on aiming direction:
        // - localX: 0.2f (right edge) or -0.2f (left edge) to follow player flip
        // - localY: 0.22f (raised up slightly higher, around chest level)
        float localX = aimingLeft ? -0.2f : 0.2f;
        float localY = 0.22f;
        transform.localPosition = new Vector3(localX, localY, -0.05f);

        float flipY = aimingLeft ? 180f : 0f;
        float localRotation = aimingLeft ? (180f - rotationDeg) : rotationDeg;

        // Reset flipY on the sprite renderer (the 3D rotation handles flipping correctly)
        if (spriteRenderer != null)
        {
            spriteRenderer.flipY = false;
        }

        // Apply rotation around parent Y (for flip) and Z (for aim) plus attack animation offset
        transform.rotation = Quaternion.Euler(0f, flipY, localRotation + attackRotationOffset) * GetRotationOffset();
    }

    /// <summary>
    /// Detach weapon from parent (drop on ground).
    /// </summary>
    public void Detach()
    {
        parentEntity = null;
        transform.SetParent(null);

        // Move to the active scene to prevent it from persisting in DontDestroyOnLoad
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(gameObject, UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        // Re-enable main collider for pickup
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        WeaponPickup pickup = GetComponent<WeaponPickup>();
        if (pickup != null) pickup.enabled = true;
    }

    /// <summary>
    /// Attach weapon to a new parent entity.
    /// </summary>
    public void AttachTo(GameObject entity)
    {
        parentEntity = entity;
        transform.SetParent(entity.transform);

        // Disable main collider so it doesn't interfere with physics or bullets
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        WeaponPickup pickup = GetComponent<WeaponPickup>();
        if (pickup != null) pickup.enabled = false;
    }

    /// <summary>
    /// Whether this weapon is currently held by an entity.
    /// </summary>
    public bool IsEquipped => parentEntity != null;
}
