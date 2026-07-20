using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Cinemachine;
public class SaveController : MonoBehaviour
{
    private string saveLocation;
    private InventoryController inventoryController;
    void Start()
    {
        saveLocation = @"D:\PRU213-GameProject\Save File\saveData.json";
        inventoryController = FindAnyObjectByType<InventoryController>();

        // Clean up old save file from persistentDataPath to avoid confusion
        string oldSavePath = Path.Combine(Application.persistentDataPath, "saveData.json");
        if (File.Exists(oldSavePath))
        {
            try
            {
                File.Delete(oldSavePath);
                Debug.Log($"Cleaned up old save file at {oldSavePath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Failed to delete old save file: {ex.Message}");
            }
        }
        
        if (PlayerPrefs.GetInt("ShouldLoadSave", 0) == 1)
        {
            LoadGame();
            PlayerPrefs.SetInt("ShouldLoadSave", 0);
            PlayerPrefs.Save();
        }
        else
        {
            Debug.Log("Starting fresh game (LoadGame skipped).");
        }
    }

    public void DeleteSaveFile()
    {
        if (File.Exists(saveLocation))
        {
            try
            {
                File.Delete(saveLocation);
                Debug.Log($"Save file deleted at {saveLocation}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to delete save file: {ex.Message}");
            }
        }
    }

    public void SaveGame()
    {
        Player player = FindAnyObjectByType<Player>();
        GameManager gm = GameManager.Instance;

        CinemachineConfiner2D confiner = FindAnyObjectByType<CinemachineConfiner2D>();

        SaveData saveData = new SaveData
        {
            savedSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
            playerPosition = player != null ? player.transform.position : Vector3.zero,
            mapBoundry = (confiner != null && confiner.BoundingShape2D != null) ? confiner.BoundingShape2D.gameObject.name : "",
            inventorySaveData = inventoryController != null ? inventoryController.GetInventoryItems() : new List<InventorySaveData>(),

            // Dungeon progress
            currentLevel = gm != null ? gm.currentLevel : 1,
            score = gm != null ? gm.score : 0,
            playerHealth = player != null ? player.currentHealth : 100,
            playerMaxHealth = player != null ? player.maxHealth : 100,

            // Weapon
            equippedWeaponName = player != null && player.currentWeapon != null
                ? player.currentWeapon.GetComponent<BaseWeapon>()?.weaponName ?? ""
                : "",
            equippedWeaponDamage = player != null && player.currentWeapon != null
                ? player.currentWeapon.GetComponent<BaseWeapon>()?.damage ?? 0f
                : 0f,

            // Potions
            healthPotions = player != null ? player.healthPotions : 3,
            manaPotions = player != null ? player.manaPotions : 3
        };

        try
        {
            string dir = Path.GetDirectoryName(saveLocation);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
            Debug.Log($"Game saved! Level: {saveData.currentLevel}, Score: {saveData.score}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to save game: {ex.Message}");
        }
    }

    public void LoadGame()
    {
        if (File.Exists(saveLocation))
        {
            try
            {
                SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));
                
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    playerObj.transform.position = saveData.playerPosition;
                }
                
                CinemachineConfiner2D confiner = FindAnyObjectByType<CinemachineConfiner2D>();
                if (confiner != null && !string.IsNullOrEmpty(saveData.mapBoundry))
                {
                    GameObject boundryObj = GameObject.Find(saveData.mapBoundry);
                    if (boundryObj != null)
                    {
                        confiner.BoundingShape2D = boundryObj.GetComponent<PolygonCollider2D>();
                    }
                }
                
                if (inventoryController != null)
                {
                    inventoryController.SetInventoryItems(saveData.inventorySaveData);
                }

                // Restore dungeon progress
                Player player = FindAnyObjectByType<Player>();
                if (player != null)
                {
                    player.maxHealth = saveData.playerMaxHealth;
                    player.currentHealth = saveData.playerHealth;
                    player.healthPotions = saveData.healthPotions;
                    player.manaPotions = saveData.manaPotions;
                    player.currentArmor = player.maxArmor;
                    player.currentEnergy = player.maxEnergy;
                }

                // Restore Equipped Weapon
                if (player != null && !string.IsNullOrEmpty(saveData.equippedWeaponName))
                {
                    RestorePlayerWeapon(player, saveData.equippedWeaponName);
                }

                Debug.Log($"Game loaded! Level: {saveData.currentLevel}, Score: {saveData.score}, Weapon: {saveData.equippedWeaponName}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error loading game: {ex.Message}");
            }
        }
        else
        {
            SaveGame(); 
        }
    }

    private void RestorePlayerWeapon(Player player, string targetWeaponName)
    {
        if (player == null || string.IsNullOrEmpty(targetWeaponName)) return;

        // Check if player ALREADY has a weapon attached
        BaseWeapon[] childWeapons = player.GetComponentsInChildren<BaseWeapon>(true);
        BaseWeapon equippedWeapon = null;

        foreach (var bw in childWeapons)
        {
            if (bw.weaponName.Equals(targetWeaponName, System.StringComparison.OrdinalIgnoreCase) ||
                bw.gameObject.name.Contains(targetWeaponName))
            {
                equippedWeapon = bw;
                break;
            }
        }

        // If not found in player children, search scene for ground weapon matching targetWeaponName
        if (equippedWeapon == null)
        {
            BaseWeapon[] sceneWeapons = Object.FindObjectsByType<BaseWeapon>(FindObjectsInactive.Include);
            foreach (var bw in sceneWeapons)
            {
                if (!bw.IsEquipped && (bw.weaponName.Equals(targetWeaponName, System.StringComparison.OrdinalIgnoreCase) ||
                    bw.gameObject.name.Contains(targetWeaponName)))
                {
                    equippedWeapon = bw;
                    break;
                }
            }
        }

        if (equippedWeapon != null)
        {
            // Attach weapon to player
            equippedWeapon.gameObject.SetActive(true);
            equippedWeapon.AttachTo(player.gameObject);
            player.currentWeapon = equippedWeapon.gameObject;

            // Remove any duplicate unequipped weapons of the SAME name on the ground to prevent duplication
            BaseWeapon[] remainingWeapons = Object.FindObjectsByType<BaseWeapon>(FindObjectsInactive.Include);
            foreach (var bw in remainingWeapons)
            {
                if (!bw.IsEquipped && (bw.weaponName.Equals(targetWeaponName, System.StringComparison.OrdinalIgnoreCase) ||
                    bw.gameObject.name.Contains(targetWeaponName)))
                {
                    Object.Destroy(bw.gameObject);
                }
            }
            Debug.Log($"[SaveController] Successfully restored equipped weapon: {targetWeaponName}");
        }
    }
}
