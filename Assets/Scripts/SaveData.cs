using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]

public class SaveData : MonoBehaviour
{
    public Vector3 playerPosition;
    public string mapBoundry;
    public List<InventorySaveData> inventorySaveData;
}
