using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Tự động tạo + nối dây một CombatRoom trong scene đang mở.
/// Menu: Tools > Combat > Create Combat Room (auto-wire)
///
/// Nó sẽ:
/// - Tạo object "CombatRoom_Main" tại vị trí Player (tâm phòng tạm).
/// - Gán sẵn: Enemy Prefab (Zombie), các SpawnPoint* có sẵn, object "Gem" (nếu có).
/// - Đặt Total=10, MaxAlive=5.
/// - Tắt object "EnemySpawner" cũ (nếu có).
///
/// Bạn vẫn cần TỰ TAY: di chuyển CombatRoom vào giữa phòng, tạo collider cho Gem,
/// tạo các Gate và kéo vào ô Gates (vì mấy cái này cần đặt đúng vị trí trên bản đồ).
/// </summary>
public static class CombatRoomSetup
{
    [MenuItem("Tools/Combat/Create Combat Room (auto-wire)")]
    public static void CreateCombatRoom()
    {
        // 1) Tạo object CombatRoom
        GameObject go = new GameObject("CombatRoom_Main");
        Undo.RegisterCreatedObjectUndo(go, "Create Combat Room");

        // Đặt tại vị trí Player cho tiện (bạn kéo lại sau)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) go.transform.position = player.transform.position;

        CombatRoom room = go.AddComponent<CombatRoom>();

        // 2) Gán Enemy Prefab (Zombie)
        GameObject zombie = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Zombie.prefab");
        if (zombie != null) room.enemyPrefab = zombie;
        else Debug.LogWarning("CombatRoomSetup: không tìm thấy Assets/Prefabs/Zombie.prefab — nhớ tự gán Enemy Prefab.");

        // 3) Gán các SpawnPoint* có sẵn trong scene
        List<Transform> points = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None)
            .Where(t => t.name.StartsWith("SpawnPoint"))
            .OrderBy(t => t.name)
            .ToList();
        if (points.Count > 0) room.spawnPoints = points.ToArray();
        else Debug.LogWarning("CombatRoomSetup: chưa thấy object tên 'SpawnPoint...' nào — quái sẽ sinh quanh tâm phòng.");

        // 4) Gán Gem nếu có object tên "Gem"
        GameObject gem = GameObject.Find("Gem");
        if (gem != null) room.gem = gem;
        else Debug.Log("CombatRoomSetup: chưa có object 'Gem' — hãy tạo object Gem (có Collider2D) rồi kéo vào ô Gem.");

        // 5) Thông số mặc định
        room.totalEnemiesToKill = 10;
        room.maxAliveEnemies = 5;
        room.spawnInterval = 1.5f;

        // 6) Tắt EnemySpawner cũ
        GameObject oldSpawner = GameObject.Find("EnemySpawner");
        if (oldSpawner != null)
        {
            Undo.RecordObject(oldSpawner, "Disable old EnemySpawner");
            oldSpawner.SetActive(false);
            Debug.Log("CombatRoomSetup: đã TẮT object EnemySpawner cũ.");
        }

        // Đánh dấu scene cần lưu + chọn object mới
        EditorUtility.SetDirty(room);
        EditorSceneManager.MarkSceneDirty(go.scene);
        Selection.activeGameObject = go;

        Debug.Log("<color=lime>CombatRoomSetup: Đã tạo & nối dây CombatRoom_Main.</color> " +
                  "Giờ hãy: (1) kéo nó vào giữa phòng, (2) tạo Collider2D cho Gem, (3) tạo Gate và gán vào ô Gates.");
    }
}
