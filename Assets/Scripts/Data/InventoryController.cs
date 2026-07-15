using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{   
    private ItemDictionary itemDictionary;
    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount;
    public GameObject[] itemPrefabs;
    void Start()
    {
        // Deleted all UI slot instantiation logic as inventory page is removed
    }

    public List<InventorySaveData> GetInventoryItems()
    {
        // Return empty list as inventory UI grid is removed
        return new List<InventorySaveData>();
    }
    
    public void SetInventoryItems(List<InventorySaveData> inventorySaveData)
    {
        // Do nothing as inventory UI grid is removed
    }
}
