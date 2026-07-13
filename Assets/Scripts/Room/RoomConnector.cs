using UnityEngine;

/// <summary>
/// Connects two dungeon rooms through a passage/corridor.
/// Place this at the doorway between two rooms.
/// When the player walks through, it activates the destination room's RoomController.
/// 
/// Setup in Unity:
/// 1. Place at the doorway between Room A and Room B
/// 2. Add a BoxCollider2D (Is Trigger) spanning the doorway
/// 3. Assign the destination RoomController
/// 4. (Optional) Assign RoomGates on both sides
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class RoomConnector : MonoBehaviour
{
    [Header("Connection")]
    [Tooltip("The RoomController that will be activated when the player passes through.")]
    [SerializeField] private RoomController destinationRoom;

    [Header("Gates (Optional)")]
    [Tooltip("Gate on the source room side. Closes when player enters destination.")]
    [SerializeField] private RoomGate sourceGate;
    [Tooltip("Gate on the destination room side. Closes when room activates.")]
    [SerializeField] private RoomGate destinationGate;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer doorVisual;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasTriggered) return;

        if (collision.CompareTag(Tags.PLAYER))
        {
            hasTriggered = true;
            ActivateDestination();
        }
    }

    private void ActivateDestination()
    {
        Debug.Log($"RoomConnector: Player entered passage to {destinationRoom?.gameObject.name}");

        if (destinationRoom != null)
        {
            destinationRoom.ActivateRoom();
        }
    }

    /// <summary>
    /// Reset the connector so it can trigger again (useful for rooms that can be revisited).
    /// </summary>
    public void ResetConnector()
    {
        hasTriggered = false;
    }

    private void OnDrawGizmos()
    {
        // Draw connection line in editor for visualization
        Gizmos.color = Color.yellow;
        if (destinationRoom != null)
        {
            Gizmos.DrawLine(transform.position, destinationRoom.transform.position);
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }

        // Draw door icon
        Gizmos.color = hasTriggered ? Color.green : Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(1f, 0.5f, 0f));
    }
}
