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
    }

    protected virtual void Start() { }

    protected virtual void Update()
    {
        if (parentEntity == null) return;

        FollowParentAndAimAtMouse();
    }

    /// <summary>
    /// Positions the weapon at the parent entity and rotates it to face the mouse.
    /// Handles flipping when aiming left.
    /// </summary>
    private void FollowParentAndAimAtMouse()
    {
        // Get mouse position in world space
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 weaponPos = (Vector2)transform.position;
        Vector2 aimDirection = mouseWorldPos - weaponPos;

        // Calculate rotation angle
        float rotationDeg = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        // Determine if we need to flip (aiming left)
        bool aimingLeft = rotationDeg > 90f || rotationDeg < -90f;
        float flipY = aimingLeft ? 180f : 0f;
        float localRotation = aimingLeft ? (180f - rotationDeg) : rotationDeg;

        // Apply position and rotation
        Vector3 offset = GetPositionOffset();
        if (aimingLeft) offset.x = -Mathf.Abs(offset.x);

        transform.position = parentEntity.transform.position + offset;
        transform.rotation = Quaternion.Euler(0f, flipY, localRotation) * GetRotationOffset();
    }

    /// <summary>
    /// Detach weapon from parent (drop on ground).
    /// </summary>
    public void Detach()
    {
        parentEntity = null;
    }

    /// <summary>
    /// Attach weapon to a new parent entity.
    /// </summary>
    public void AttachTo(GameObject entity)
    {
        parentEntity = entity;
    }

    /// <summary>
    /// Whether this weapon is currently held by an entity.
    /// </summary>
    public bool IsEquipped => parentEntity != null;
}
