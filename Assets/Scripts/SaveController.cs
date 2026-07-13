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
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        inventoryController = FindAnyObjectByType<InventoryController>();
        
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

    public void SaveGame()
    {
        Player player = FindAnyObjectByType<Player>();
        GameManager gm = GameManager.Instance;

        SaveData saveData = new SaveData
        {
            playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position,
            mapBoundry = FindAnyObjectByType<CinemachineConfiner2D>().BoundingShape2D.gameObject.name,
            inventorySaveData = inventoryController.GetInventoryItems(),

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
                : 0f
        };

        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
        Debug.Log($"Game saved! Level: {saveData.currentLevel}, Score: {saveData.score}");
    }

    public void LoadGame()
    {
        if (File.Exists(saveLocation))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));
            
            GameObject.FindGameObjectWithTag("Player").transform.position = saveData.playerPosition;
            
            FindAnyObjectByType<CinemachineConfiner2D>().BoundingShape2D = GameObject.Find(saveData.mapBoundry).GetComponent<PolygonCollider2D>();
            
            inventoryController.SetInventoryItems(saveData.inventorySaveData);

            // Restore dungeon progress
            Player player = FindAnyObjectByType<Player>();
            if (player != null)
            {
                player.maxHealth = saveData.playerMaxHealth;
                player.currentHealth = saveData.playerHealth;
                if (player.healthBar != null)
                {
                    player.healthBar.SetMaxHealth(saveData.playerMaxHealth);
                    player.healthBar.SetHealth(saveData.playerHealth);
                }
            }

            GameManager gm = GameManager.Instance;
            if (gm != null)
            {
                gm.currentLevel = saveData.currentLevel;
                gm.score = saveData.score;
            }

            Debug.Log($"Game loaded! Level: {saveData.currentLevel}, Score: {saveData.score}");
        }
        else
        {
            SaveGame(); 
        }
    }
}
