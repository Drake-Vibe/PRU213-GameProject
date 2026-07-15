using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Extended save data including dungeon progress, weapon, and score.
/// </summary>
[System.Serializable]
public class SaveData
{
    // Original PRU213 data
    public Vector3 playerPosition;
    public string mapBoundry;
    public List<InventorySaveData> inventorySaveData;

    // New dungeon progress data
    public int currentLevel = 1;
    public int score = 0;
    public int playerHealth = 100;
    public int playerMaxHealth = 100;

    // Weapon data
    public string equippedWeaponName = "";
    public float equippedWeaponDamage = 0f;

    // Potion data
    public int healthPotions = 3;
    public int manaPotions = 3;
}