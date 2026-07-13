using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

/// <summary>
/// Controls a single dungeon room. Tracks enemies and gates.
/// When the player enters, gates close and enemies activate.
/// When all enemies are dead, gates open (room is "solved").
/// </summary>
public class RoomController : MonoBehaviour
{
    [Header("Room Elements")]
    public List<RoomGate> gates = new List<RoomGate>();
    public List<BaseEnemy> enemiesInRoom = new List<BaseEnemy>();

    [Header("Events")]
    public UnityEvent onRoomActivated;
    public UnityEvent onRoomSolved;

    private bool isSolved = false;
    private bool isActivated = false;

    /// <summary>
    /// Whether all enemies have been defeated.
    /// </summary>
    public bool IsSolved => isSolved;

    private void Start()
    {
        // Setup gate enter callbacks
        foreach (RoomGate gate in gates)
        {
            gate.OnPlayerEnter += ActivateRoom;
        }
    }

    private void Update()
    {
        if (isSolved || !isActivated) return;

        // Remove destroyed enemies from the list
        enemiesInRoom.RemoveAll(e => e == null || e.gameObject == null);

        // Check if all enemies are dead
        if (enemiesInRoom.Count == 0)
        {
            SolveRoom();
        }
    }

    /// <summary>
    /// Called when the player enters through a gate.
    /// Closes all gates and activates enemies.
    /// </summary>
    public void ActivateRoom()
    {
        if (isActivated) return;

        isActivated = true;
        Debug.Log($"Room activated: {gameObject.name}");

        // Close gates
        foreach (RoomGate gate in gates)
        {
            gate.Close();
        }

        // Activate enemies - give them the player target
        Player player = FindAnyObjectByType<Player>();
        foreach (BaseEnemy enemy in enemiesInRoom)
        {
            if (enemy != null)
            {
                enemy.gameObject.SetActive(true);
            }
        }

        onRoomActivated?.Invoke();
    }

    /// <summary>
    /// Called when all enemies are defeated. Opens gates.
    /// </summary>
    private void SolveRoom()
    {
        if (isSolved) return;

        isSolved = true;
        Debug.Log($"Room solved: {gameObject.name}");

        // Open gates
        foreach (RoomGate gate in gates)
        {
            gate.Open();
        }

        onRoomSolved?.Invoke();
    }
}
