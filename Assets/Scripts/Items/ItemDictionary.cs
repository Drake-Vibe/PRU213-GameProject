using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class ItemDictionary : MonoBehaviour
{
    public List<Item> itemPrefabs;
    private Dictionary<int, GameObject> itemDictionary;
    public void Awake()
    {
        itemDictionary = new Dictionary<int, GameObject>();
        for (int i = 0; i < itemPrefabs.Count; i++)
        {
            if (itemPrefabs[i].ID != 0)
            {
                itemPrefabs[i].ID = i + 1;
            }
        }
        foreach(Item item in itemPrefabs)
        {
            itemDictionary[item.ID] = item.gameObject;
        }
    }
    public GameObject GetItemPrefab(int itemID)
    {
       itemDictionary.TryGetValue(itemID, out GameObject prefab);
        if (prefab != null)
        {
            Debug.Log($"Item with ID {itemID} not found in dictionary");
        }
        return prefab;
    }
}
