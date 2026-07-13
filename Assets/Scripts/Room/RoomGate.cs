using UnityEngine;
using System;

/// <summary>
/// A gate/barrier in a dungeon room.
/// Detects when the player walks through and notifies the RoomController.
/// Can be opened (disabled collider) or closed (enabled collider).
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class RoomGate : MonoBehaviour
{
    /// <summary>
    /// Event fired when the player enters this gate's trigger zone.
    /// </summary>
    public event Action OnPlayerEnter;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer gateVisual;

    private Collider2D gateCollider;
    private bool hasTriggered = false;

    private void Awake()
    {
        gateCollider = GetComponent<Collider2D>();
    }

    /// <summary>
    /// Close the gate - enable the barrier collider and show visual.
    /// </summary>
    public void Close()
    {
        if (gateCollider != null)
            gateCollider.enabled = true;

        if (gateVisual != null)
            gateVisual.enabled = true;

        gameObject.SetActive(true);
    }

    /// <summary>
    /// Open the gate - disable the barrier collider and hide visual.
    /// </summary>
    public void Open()
    {
        if (gateCollider != null)
            gateCollider.enabled = false;

        if (gateVisual != null)
            gateVisual.enabled = false;

        // Optionally fully disable the gate
        // gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasTriggered) return;

        if (collision.CompareTag(Tags.PLAYER))
        {
            hasTriggered = true;
            OnPlayerEnter?.Invoke();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasTriggered) return;

        if (collision.collider.CompareTag(Tags.PLAYER))
        {
            hasTriggered = true;
            OnPlayerEnter?.Invoke();
        }
    }
}
