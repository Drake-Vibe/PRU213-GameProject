using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;
using TMPro;

/// <summary>
/// Unity Editor script that automates ALL setup tasks for the PRU213 Soul Knight project.
/// Run from menu: Tools → PRU213 Setup → Run Full Setup
/// </summary>
public class PRU213AutoSetup : EditorWindow
{
    [MenuItem("Tools/PRU213 Setup/🚀 Run Full Setup (All Steps)", priority = 0)]
    public static void RunFullSetup()
    {
        if (!EditorUtility.DisplayDialog(
            "PRU213 Auto Setup",
            "Sẽ tự động:\n" +
            "• Thêm Tags & Layers\n" +
            "• Cấu hình Physics2D\n" +
            "• Tạo tất cả Prefabs\n" +
            "• Setup Player\n" +
            "• Tạo GameManager & LevelManager\n" +
            "• Tạo HUD UI\n" +
            "• Tạo Scene DungeonLevel1 cơ bản\n\n" +
            "Bạn có muốn tiếp tục?",
            "Chạy!", "Hủy"))
        {
            return;
        }

        SetupTagsAndLayers();
        SetupPhysics2D();
        CreateAllPrefabs();
        SetupCurrentScene();

        EditorUtility.DisplayDialog("✅ Hoàn Tất!", 
            "Setup xong!\n\n" +
            "Việc còn lại:\n" +
            "1. Mở scene DungeonLevel1\n" +
            "2. Vẽ tilemap rooms (File → Open Scene)\n" +
            "3. Kéo prefabs vào rooms\n" +
            "4. File → Build Settings → Add scenes\n" +
            "5. Nhấn Play để test!", 
            "OK");
    }

    [MenuItem("Tools/PRU213 Setup/🔧 Fix Menu TabController References Only (Keep UI Positions)", priority = 5)]
    public static void FixMenuTabControllerOnly()
    {
        TabController tabCtrl = Object.FindAnyObjectByType<TabController>();
        if (tabCtrl == null)
        {
            EditorUtility.DisplayDialog("Error", "TabController component not found in the current scene!", "OK");
            return;
        }

        // Find Tab Buttons
        GameObject playerTabObj = null;
        GameObject settingsTabObj = null;

        // Search for tab gameobjects in children of tabCtrl's parent or tabCtrl itself
        Transform searchRoot = tabCtrl.transform.parent != null ? tabCtrl.transform.parent : tabCtrl.transform;
        foreach (Transform child in searchRoot.GetComponentsInChildren<Transform>(true))
        {
            string nameUpper = child.name.ToUpper();
            if (playerTabObj == null && nameUpper.Contains("PLAYER") && (nameUpper.Contains("TAB") || nameUpper.Contains("BTN") || nameUpper.Contains("BUTTON")))
            {
                playerTabObj = child.gameObject;
            }
            if (settingsTabObj == null && nameUpper.Contains("SETTING") && (nameUpper.Contains("TAB") || nameUpper.Contains("BTN") || nameUpper.Contains("BUTTON")))
            {
                settingsTabObj = child.gameObject;
            }
        }

        // Fallback search by text labels
        if (playerTabObj == null || settingsTabObj == null)
        {
            foreach (TextMeshProUGUI tmp in searchRoot.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                string txt = tmp.text.ToUpper().Trim();
                GameObject targetObj = tmp.gameObject;
                if (targetObj.GetComponent<Button>() == null && targetObj.transform.parent != null)
                {
                    targetObj = targetObj.transform.parent.gameObject;
                }

                if (playerTabObj == null && (txt == "PLAYER" || tmp.gameObject.name.ToUpper().Contains("PLAYER")))
                {
                    playerTabObj = targetObj;
                }
                if (settingsTabObj == null && (txt == "SETTINGS" || txt == "SETTING" || tmp.gameObject.name.ToUpper().Contains("SETTING")))
                {
                    settingsTabObj = targetObj;
                }
            }
        }

        // Find Pages
        GameObject playerPg = null;
        GameObject settingsPg = null;

        foreach (Transform child in searchRoot.GetComponentsInChildren<Transform>(true))
        {
            // Ignore tab buttons
            if (child.gameObject == playerTabObj || child.gameObject == settingsTabObj) continue;

            string nameUpper = child.name.ToUpper();
            if (playerPg == null && nameUpper.Contains("PLAYER") && (nameUpper.Contains("PAGE") || nameUpper.Contains("PANEL")))
            {
                playerPg = child.gameObject;
            }
            if (settingsPg == null && nameUpper.Contains("SETTING") && (nameUpper.Contains("PAGE") || nameUpper.Contains("PANEL") || nameUpper.Contains("MENU")))
            {
                settingsPg = child.gameObject;
            }
        }

        if (playerTabObj == null || settingsTabObj == null || playerPg == null || settingsPg == null)
        {
            // Print diagnostics
            string msg = $"Could not identify all references automatically:\n" +
                         $"- Player Tab: {(playerTabObj != null ? playerTabObj.name : "MISSING")}\n" +
                         $"- Settings Tab: {(settingsTabObj != null ? settingsTabObj.name : "MISSING")}\n" +
                         $"- Player Page: {(playerPg != null ? playerPg.name : "MISSING")}\n" +
                         $"- Settings Page: {(settingsPg != null ? settingsPg.name : "MISSING")}\n\n" +
                         $"Please make sure names of tab buttons contain 'Player' / 'Setting' and page panels contain 'Player' / 'Setting'.";
            EditorUtility.DisplayDialog("Diagnostics", msg, "OK");
            return;
        }

        // Apply references to TabController without changing UI layout/positions
        var newTabImages = new System.Collections.Generic.List<UnityEngine.UI.Image>();
        newTabImages.Add(playerTabObj.GetComponentInChildren<UnityEngine.UI.Image>(true) ?? playerTabObj.GetComponent<UnityEngine.UI.Image>());
        newTabImages.Add(settingsTabObj.GetComponentInChildren<UnityEngine.UI.Image>(true) ?? settingsTabObj.GetComponent<UnityEngine.UI.Image>());

        var newPages = new System.Collections.Generic.List<GameObject>();
        newPages.Add(playerPg);
        newPages.Add(settingsPg);

        SerializedObject soTab = new SerializedObject(tabCtrl);
        
        SerializedProperty tabImagesProp = soTab.FindProperty("tabImages");
        tabImagesProp.ClearArray();
        tabImagesProp.arraySize = newTabImages.Count;
        for (int i = 0; i < newTabImages.Count; i++)
        {
            tabImagesProp.GetArrayElementAtIndex(i).objectReferenceValue = newTabImages[i];
        }

        SerializedProperty pagesProp = soTab.FindProperty("pages");
        pagesProp.ClearArray();
        pagesProp.arraySize = newPages.Count;
        for (int i = 0; i < newPages.Count; i++)
        {
            pagesProp.GetArrayElementAtIndex(i).objectReferenceValue = newPages[i];
        }

        soTab.ApplyModifiedProperties();
        EditorUtility.SetDirty(tabCtrl);

        // Bind Button onClick handlers persistent calls (Player=0, Settings=1) WITHOUT repositioning
        UpdateTabButtonTrigger(playerTabObj, 0);
        UpdateTabButtonTrigger(settingsTabObj, 1);

        // Save active scene changes
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(tabCtrl.gameObject.scene);

        EditorUtility.DisplayDialog("✅ Success", 
            $"Successfully fixed TabController references while keeping all positions intact!\n\n" +
            $"- TabController: {tabCtrl.name}\n" +
            $"- Player Tab: {playerTabObj.name} -> Page: {playerPg.name}\n" +
            $"- Settings Tab: {settingsTabObj.name} -> Page: {settingsPg.name}", 
            "OK");
    }

    [MenuItem("Tools/PRU213 Setup/🔄 Setup All Scenes in Build", priority = 2)]
    public static void SetupAllScenesInBuild()
    {
        Debug.Log("=== PRU213 Setup: Setting up all scenes ===");
        var originalSetup = UnityEditor.SceneManagement.EditorSceneManager.GetSceneManagerSetup();
        
        string[] scenes = {
            "Assets/Scenes/UI-Default.unity",
            "Assets/Scenes/Level12.unity",
            "Assets/Scenes/Level13.unity",
            "Assets/Scenes/Level14.unity",
            "Assets/Scenes/GameMainMenu.unity",
            "Assets/Scenes/GameOver.unity"
        };

        foreach (string scenePath in scenes)
        {
            if (System.IO.File.Exists(scenePath))
            {
                var activeScene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
                if (activeScene.IsValid())
                {
                    Debug.Log($"Configuring scene: {scenePath}...");
                    SetupCurrentScene();
                    
                    // For Level 12 specifically, re-run its preplaced setup so it links GatedDoor as exitPortal
                    if (scenePath.EndsWith("Level12.unity"))
                    {
                        SetupLevel12();
                    }
                    // For Level 13 specifically, re-run its preplaced setup
                    if (scenePath.EndsWith("Level13.unity"))
                    {
                        SetupLevel13();
                    }
                    // For Level 14 specifically, re-run its boss room setup
                    if (scenePath.EndsWith("Level14.unity"))
                    {
                        SetupLevel14();
                    }

                    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(activeScene);
                }
            }
        }

        if (originalSetup != null && originalSetup.Length > 0)
        {
            UnityEditor.SceneManagement.EditorSceneManager.RestoreSceneManagerSetup(originalSetup);
        }
    }

    [MenuItem("Tools/PRU213 Setup/🔧 Repair Loading Screen in Current Scene", priority = 3)]
    public static void RepairLoadingScreenInCurrentScene()
    {
        Debug.Log("=== PRU213 Setup: Repairing Loading Screen in Current Scene ===");
        PRU213MenuSetup.CreateLoadingScreen();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("✅ Loading Screen successfully repaired in current scene!");
    }

    [MenuItem("Tools/PRU213 Setup/🔧 Repair Loading Screen in ALL Scenes", priority = 4)]
    public static void RepairLoadingScreenInAllScenes()
    {
        Debug.Log("=== PRU213 Setup: Repairing Loading Screen in ALL Scenes ===");
        var originalSetup = UnityEditor.SceneManagement.EditorSceneManager.GetSceneManagerSetup();
        
        string[] scenes = {
            "Assets/Scenes/UI-Default.unity",
            "Assets/Scenes/Level12.unity",
            "Assets/Scenes/Level13.unity",
            "Assets/Scenes/Level14.unity",
            "Assets/Scenes/GameMainMenu.unity",
            "Assets/Scenes/GameOver.unity"
        };

        foreach (string scenePath in scenes)
        {
            if (System.IO.File.Exists(scenePath))
            {
                var activeScene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
                if (activeScene.IsValid())
                {
                    Debug.Log($"Repairing loading screen in: {scenePath}...");
                    PRU213MenuSetup.CreateLoadingScreen();
                    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(activeScene);
                }
            }
        }

        if (originalSetup != null && originalSetup.Length > 0)
        {
            UnityEditor.SceneManagement.EditorSceneManager.RestoreSceneManagerSetup(originalSetup);
        }
        Debug.Log("✅ Loading Screen successfully repaired in ALL scenes!");
    }

    // ========================================================
    // STEP 1: Tags & Layers
    // ========================================================
    [MenuItem("Tools/PRU213 Setup/Step 1 - Tags & Layers", priority = 10)]
    public static void SetupTagsAndLayers()
    {
        Debug.Log("=== PRU213 Setup: Adding Tags & Layers ===");

        // Add Tags
        string[] tagsToAdd = { "Enemy", "Bullet", "EnemyBullet", "Sword", "Wall", "Gate_Start", "Gate_End" };
        foreach (string tag in tagsToAdd)
        {
            AddTag(tag);
        }

        // Add Layers
        Dictionary<int, string> layersToAdd = new Dictionary<int, string>
        {
            { 6, "Player" },
            { 7, "Enemy" },
            { 8, "Bullet" },
            { 9, "EnemyBullet" },
            { 10, "MapWall" }
        };

        foreach (var kvp in layersToAdd)
        {
            AddLayer(kvp.Key, kvp.Value);
        }

        Debug.Log("✅ Tags & Layers setup complete!");
    }

    // ========================================================
    // STEP 2: Physics2D Collision Matrix
    // ========================================================
    [MenuItem("Tools/PRU213 Setup/Step 2 - Physics2D Collision", priority = 11)]
    public static void SetupPhysics2D()
    {
        Debug.Log("=== PRU213 Setup: Configuring Physics2D ===");

        int playerLayer = LayerMask.NameToLayer("Player");
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        int bulletLayer = LayerMask.NameToLayer("Bullet");
        int enemyBulletLayer = LayerMask.NameToLayer("EnemyBullet");
        int mapWallLayer = LayerMask.NameToLayer("MapWall");

        if (playerLayer < 0 || enemyLayer < 0 || bulletLayer < 0 || enemyBulletLayer < 0 || mapWallLayer < 0)
        {
            Debug.LogWarning("⚠️ Some layers not found! Run Step 1 first.");
            return;
        }

        // Disable collisions that shouldn't happen
        Physics2D.IgnoreLayerCollision(bulletLayer, bulletLayer, true);         // Bullets don't hit bullets
        Physics2D.IgnoreLayerCollision(bulletLayer, enemyBulletLayer, true);    // Player bullets don't hit enemy bullets
        Physics2D.IgnoreLayerCollision(enemyBulletLayer, enemyBulletLayer, true); // Enemy bullets don't hit each other
        Physics2D.IgnoreLayerCollision(bulletLayer, playerLayer, true);         // Player bullets don't hit player
        Physics2D.IgnoreLayerCollision(enemyBulletLayer, enemyLayer, true);    // Enemy bullets don't hit enemies
        Physics2D.IgnoreLayerCollision(enemyLayer, enemyLayer, true);          // Enemies don't push each other

        Debug.Log("✅ Physics2D collision matrix configured!");
    }

    // ========================================================
    // STEP 3: Create All Prefabs
    // ========================================================
    [MenuItem("Tools/PRU213 Setup/Step 3 - Create All Prefabs", priority = 12)]
    public static void CreateAllPrefabs()
    {
        Debug.Log("=== PRU213 Setup: Creating Prefabs ===");

        // Ensure Prefabs subfolders exist
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Weapons"))
            AssetDatabase.CreateFolder("Assets/Prefabs", "Weapons");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Enemies"))
            AssetDatabase.CreateFolder("Assets/Prefabs", "Enemies");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Environment"))
            AssetDatabase.CreateFolder("Assets/Prefabs", "Environment");

        string weaponsPath = "Assets/Prefabs/Weapons";
        string enemiesPath = "Assets/Prefabs/Enemies";
        string environmentPath = "Assets/Prefabs/Environment";

        // Create prefabs into categorized folders
        CreateBulletPrefab(weaponsPath);
        CreateEnemyBulletPrefab(weaponsPath);
        CreateGunPrefab(weaponsPath);
        CreateSwordPrefab(weaponsPath);
        CreateMeleeEnemyPrefab(enemiesPath);
        CreateRangedEnemyPrefab(enemiesPath);
        CreateEyeBatPrefab(enemiesPath);
        CreateRoomGatePrefab(environmentPath);
        CreateNextLevelPortalPrefab(environmentPath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("✅ Prefabs organized: Weapons=" + weaponsPath + ", Enemies=" + enemiesPath + ", Environment=" + environmentPath);
    }

    // ========================================================
    // STEP 4: Setup Current Scene
    // ========================================================
    [MenuItem("Tools/PRU213 Setup/Step 4 - Setup Current Scene", priority = 13)]
    public static void SetupCurrentScene()
    {
        Debug.Log("=== PRU213 Setup: Setting up current scene ===");

        // Tag the main camera as 'MainCamera' so Camera.main resolves successfully
        Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include);
        bool taggedCamera = false;
        foreach (Camera cam in cameras)
        {
            if (cam.name.ToUpper().Contains("MAIN") || (cam.name.ToUpper().Contains("CAMERA") && !cam.name.ToUpper().Contains("MAP")))
            {
                if (cam.gameObject.tag != "MainCamera")
                {
                    cam.gameObject.tag = "MainCamera";
                    Debug.Log($"  ✓ Tagged camera '{cam.gameObject.name}' as 'MainCamera'");
                }
                taggedCamera = true;

                // Ensure camera is Orthographic and properly Z-positioned for 2D frustum
                cam.orthographic = true;
                if (cam.transform.position.z >= 0f)
                {
                    Vector3 camPos = cam.transform.position;
                    camPos.z = -10f;
                    cam.transform.position = camPos;
                }
                cam.nearClipPlane = 0.1f;
                cam.farClipPlane = 1000f;

                // Add CameraFollow script if not present
                CameraFollow follow = cam.gameObject.GetComponent<CameraFollow>();
                if (follow == null)
                {
                    follow = cam.gameObject.AddComponent<CameraFollow>();
                    Debug.Log($"  ✓ Attached CameraFollow script to camera '{cam.gameObject.name}'");
                }
            }
        }
        if (!taggedCamera)
        {
            Debug.LogWarning("⚠️ No Main Camera found in scene to tag as 'MainCamera'!");
        }

        // Find or Create SpawnPoint in the scene
        GameObject spawnPointObj = GameObject.Find("SpawnPoint");
        if (spawnPointObj == null) spawnPointObj = GameObject.Find("PlayerSpawnPoint");
        if (spawnPointObj == null)
        {
            spawnPointObj = new GameObject("SpawnPoint");
            spawnPointObj.name = "SpawnPoint";
        }

        string currentSceneName = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name;
        if (currentSceneName == "UI-Default")
        {
            spawnPointObj.transform.position = new Vector3(3.52f, 0.03f, 0f);
            Debug.Log("  ✓ SpawnPoint for UI-Default configured at (3.52, 0.03, 0)");
        }
        else if (spawnPointObj.transform.position == Vector3.zero)
        {
            spawnPointObj.transform.position = new Vector3(0f, -2f, 0f);
            Debug.Log("  ✓ Created 'SpawnPoint' at (0, -2, 0) for Player spawn");
        }

        SetupPlayerInScene();
        CreateGameManagerInScene();
        CreateLevelManagerInScene();
        CreateHUDInScene();
        PlaceStarterWeaponInScene();
        SetupPauseMenuInScene();

        // Mark scene dirty so all UI and player references are saved
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("✅ Scene setup complete!");
    }

    // ========================================================
    // PREFAB CREATION METHODS
    // ========================================================

    private static void CreateBulletPrefab(string path)
    {
        // Recreate bullet prefab to ensure correct sprites are loaded
        GameObject bullet = new GameObject("Bullet");

        // SpriteRenderer
        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.color = new Color(1f, 0.87f, 0.27f, 1f); // Yellow
        sr.sortingOrder = 5;
        sr.sortingLayerName = "Player";
        // Find existing valid projectile assets
        sr.sprite = LoadSpriteByDirectPath("Fireball", "Arrow");

        // Rigidbody2D
        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // CircleCollider2D
        CircleCollider2D col = bullet.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.2f;

        // Bullet script
        Bullet bulletScript = bullet.AddComponent<Bullet>();
        bulletScript.speed = 12f;
        bulletScript.lifetime = 3f;

        // Tag & Layer
        bullet.tag = "Bullet";
        bullet.layer = LayerMask.NameToLayer("Bullet");

        // Scale
        bullet.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

        string spriteName = sr.sprite != null ? sr.sprite.name : "NULL";
        SavePrefab(bullet, path + "/Bullet.prefab");
        Debug.Log($"  ✓ Bullet prefab created with sprite: {spriteName}");
    }

    private static void CreateEnemyBulletPrefab(string path)
    {
        // Recreate enemy bullet prefab to ensure correct sprites are loaded
        GameObject bullet = new GameObject("EnemyBullet");

        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.color = new Color(1f, 0.27f, 0.27f, 1f); // Red
        sr.sortingOrder = 5;
        sr.sortingLayerName = "Player";
        sr.sprite = LoadSpriteByDirectPath("EnergyBall", "Arrow");

        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        CircleCollider2D col = bullet.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.2f;

        Bullet bulletScript = bullet.AddComponent<Bullet>();
        bulletScript.speed = 8f;
        bulletScript.lifetime = 4f;

        bullet.tag = "EnemyBullet";
        bullet.layer = LayerMask.NameToLayer("EnemyBullet");
        bullet.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

        string spriteName = sr.sprite != null ? sr.sprite.name : "NULL";
        SavePrefab(bullet, path + "/EnemyBullet.prefab");
        Debug.Log($"  ✓ EnemyBullet prefab created with sprite: {spriteName}");
    }

    private static void CreateGunPrefab(string path)
    {
        GameObject gun = new GameObject("Gun");

        SpriteRenderer sr = gun.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 3;
        sr.sortingLayerName = "Player";
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/GameAssets/Ninja Adventure - Asset Pack/Items/Weapons/MagicWand/Sprite.png");

        BoxCollider2D col = gun.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.5f, 0.5f);

        Gun gunScript = gun.AddComponent<Gun>();
        gunScript.damage = 20f;
        gunScript.energyCost = 5;
        gunScript.weaponName = "Basic Gun";

        // Link bullet prefab
        GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Weapons/Bullet.prefab");
        if (bulletPrefab != null)
        {
            gunScript.bulletPrefab = bulletPrefab;
        }

        gun.AddComponent<WeaponPickup>();

        SavePrefab(gun, path + "/Gun.prefab");
        Debug.Log("  ✓ Gun prefab created");
    }

    private static void CreateSwordPrefab(string path)
    {
        GameObject sword = new GameObject("Sword");

        SpriteRenderer sr = sword.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 3;
        sr.sortingLayerName = "Player";
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/GameAssets/Ninja Adventure - Asset Pack/Items/Weapons/Sword/Sprite.png");

        // Create AttackZone child
        GameObject attackZone = new GameObject("AttackZone");
        attackZone.transform.SetParent(sword.transform);
        attackZone.transform.localPosition = new Vector3(0f, 2.8f, 0f);
        attackZone.tag = "Sword";

        BoxCollider2D atkCol = attackZone.AddComponent<BoxCollider2D>();
        atkCol.isTrigger = true;
        atkCol.size = new Vector2(4.5f, 4.5f);

        // Attach routing helper
        attackZone.AddComponent<MeleeAttackZone>();

        // Pickup collider on sword itself
        BoxCollider2D pickupCol = sword.AddComponent<BoxCollider2D>();
        pickupCol.isTrigger = true;
        pickupCol.size = new Vector2(0.5f, 0.5f);

        MeleeWeapon meleeScript = sword.AddComponent<MeleeWeapon>();
        meleeScript.damage = 15f;
        meleeScript.weaponName = "Sword";
        // Set attackCollider via serialized field
        SerializedObject so = new SerializedObject(meleeScript);
        so.FindProperty("attackCollider").objectReferenceValue = atkCol;
        so.FindProperty("attackCooldown").floatValue = 0.5f;
        so.FindProperty("attackDuration").floatValue = 0.2f;
        so.ApplyModifiedProperties();

        sword.AddComponent<WeaponPickup>();

        SavePrefab(sword, path + "/Sword.prefab");
        Debug.Log("  ✓ Sword prefab created");
    }

    private static void CreateMeleeEnemyPrefab(string path)
    {
        if (AssetExists(path + "/MeleeEnemy.prefab")) return;

        GameObject enemy = new GameObject("MeleeEnemy");

        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 2;
        sr.sortingLayerName = "Player";
        sr.sprite = FindSpriteAsset("chort_idle_anim_f0") 
                  ?? FindSpriteAsset("goblin_idle_anim_f0")
                  ?? FindSpriteAsset("big_demon_idle_anim_f0");

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.isTrigger = false;
        // Auto-size will happen based on sprite

        MeleeEnemy meleeScript = enemy.AddComponent<MeleeEnemy>();
        meleeScript.maxHealth = 30;
        meleeScript.scoreValue = 10;
        // Set serialized fields
        SerializedObject so = new SerializedObject(meleeScript);
        so.FindProperty("moveSpeed").floatValue = 3f;
        so.FindProperty("detectionRange").floatValue = 8f;
        so.FindProperty("contactDamage").floatValue = 20f;
        so.ApplyModifiedProperties();

        enemy.tag = "Enemy";
        enemy.layer = LayerMask.NameToLayer("Enemy");

        // Create healthbar
        CreateEnemyHealthBar(enemy, 30);

        SavePrefab(enemy, path + "/MeleeEnemy.prefab");
        Debug.Log("  ✓ MeleeEnemy prefab created");
    }

    private static void CreateRangedEnemyPrefab(string path)
    {
        if (AssetExists(path + "/RangedEnemy.prefab")) return;

        GameObject enemy = new GameObject("RangedEnemy");

        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 2;
        sr.sortingLayerName = "Player";
        sr.sprite = FindSpriteAsset("wizzard_m_idle_anim_f0") 
                  ?? FindSpriteAsset("necromancer_idle_anim_f0")
                  ?? FindSpriteAsset("orc_shaman_idle_anim_f0");

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        BoxCollider2D col = enemy.AddComponent<BoxCollider2D>();
        col.isTrigger = false;

        RangedEnemy rangedScript = enemy.AddComponent<RangedEnemy>();
        rangedScript.maxHealth = 20;
        rangedScript.scoreValue = 15;

        // Set serialized fields
        SerializedObject so = new SerializedObject(rangedScript);
        so.FindProperty("moveSpeed").floatValue = 2f;
        so.FindProperty("detectionRange").floatValue = 10f;
        so.FindProperty("preferredDistance").floatValue = 5f;
        so.FindProperty("fireRate").floatValue = 1f;
        so.FindProperty("bulletDamage").floatValue = 15f;

        // Link enemy bullet prefab
        GameObject enemyBulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Weapons/EnemyBullet.prefab");
        if (enemyBulletPrefab != null)
        {
            so.FindProperty("bulletPrefab").objectReferenceValue = enemyBulletPrefab;
        }
        so.ApplyModifiedProperties();

        enemy.tag = "Enemy";
        enemy.layer = LayerMask.NameToLayer("Enemy");

        CreateEnemyHealthBar(enemy, 20);

        SavePrefab(enemy, path + "/RangedEnemy.prefab");
        Debug.Log("  ✓ RangedEnemy prefab created");
    }

    private static void CreateEyeBatPrefab(string path)
    {
        string prefabPath = path + "/EyeBat.prefab";
        if (AssetExists(prefabPath)) return;

        // 1) Setup Animations first
        string animFolder = "Assets/Animations/EyeBat";
        if (!AssetDatabase.IsValidFolder("Assets/Animations"))
            AssetDatabase.CreateFolder("Assets", "Animations");
        if (!AssetDatabase.IsValidFolder(animFolder))
            AssetDatabase.CreateFolder("Assets/Animations", "EyeBat");

        // Load 4 frames from the flying creature folder
        string baseFolder = "Assets/Art/Sprites/GameAssets/v1.1 dungeon crawler 16X16 pixel pack/enemies/flying creature";
        List<Sprite> frames = new List<Sprite>();
        for (int i = 0; i <= 3; i++)
        {
            string framePath = $"{baseFolder}/fly_anim_f{i}.png";
            Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(framePath);
            if (sp == null)
            {
                // Fallback to sub-assets if imported as multiple
                Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(framePath);
                foreach (var asset in subAssets)
                {
                    if (asset is Sprite s)
                    {
                        sp = s;
                        break;
                    }
                }
            }
            if (sp != null) frames.Add(sp);
        }

        if (frames.Count == 0)
        {
            Debug.LogError("❌ CreateEyeBatPrefab: Could not load fly animation frames!");
            return;
        }

        // Create Fly Clip
        AnimationClip flyClip = new AnimationClip { frameRate = 8f };
        var binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };
        var keys = new ObjectReferenceKeyframe[frames.Count];
        for (int i = 0; i < frames.Count; i++)
        {
            keys[i] = new ObjectReferenceKeyframe { time = i / 8f, value = frames[i] };
        }
        AnimationUtility.SetObjectReferenceCurve(flyClip, binding, keys);
        var settings = AnimationUtility.GetAnimationClipSettings(flyClip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(flyClip, settings);

        string clipPath = animFolder + "/EyeBat_Fly.anim";
        if (AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath) != null)
            AssetDatabase.DeleteAsset(clipPath);
        AssetDatabase.CreateAsset(flyClip, clipPath);

        // Load Death Explosion frames for EyeBat
        string fxFolder = "Assets/Art/Sprites/GameAssets/v1.1 dungeon crawler 16X16 pixel pack/effects (new)";
        List<Sprite> deathFrames = new List<Sprite>();
        for (int i = 0; i <= 3; i++)
        {
            string expPath = $"{fxFolder}/enemy_afterdead_explosion_anim_f{i}.png";
            Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>(expPath);
            if (sp != null) deathFrames.Add(sp);
        }

        AnimationClip deathClip = new AnimationClip { frameRate = 10f };
        if (deathFrames.Count > 0)
        {
            var dKeys = new ObjectReferenceKeyframe[deathFrames.Count];
            for (int i = 0; i < deathFrames.Count; i++)
            {
                dKeys[i] = new ObjectReferenceKeyframe { time = i / 10f, value = deathFrames[i] };
            }
            AnimationUtility.SetObjectReferenceCurve(deathClip, binding, dKeys);
            var dSettings = AnimationUtility.GetAnimationClipSettings(deathClip);
            dSettings.loopTime = false;
            AnimationUtility.SetAnimationClipSettings(deathClip, dSettings);

            string deathClipPath = animFolder + "/EyeBat_Death.anim";
            if (AssetDatabase.LoadAssetAtPath<AnimationClip>(deathClipPath) != null)
                AssetDatabase.DeleteAsset(deathClipPath);
            AssetDatabase.CreateAsset(deathClip, deathClipPath);
        }

        // Create Animator Controller
        string controllerPath = animFolder + "/EyeBatAnimator.controller";
        var controller = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(controllerPath);
        if (controller == null)
        {
            controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        }

        // Clear parameter & states
        while (controller.parameters.Length > 0) controller.RemoveParameter(0);
        var sm = controller.layers[0].stateMachine;
        foreach (var t in sm.anyStateTransitions.ToList()) sm.RemoveAnyStateTransition(t);
        foreach (var cs in sm.states.ToList()) sm.RemoveState(cs.state);

        // Add parameters
        controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Hurt", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);

        // Create states
        var sIdle = sm.AddState("Idle");
        sIdle.motion = flyClip;
        var sRun = sm.AddState("Run");
        sRun.motion = flyClip;
        var sHurt = sm.AddState("Hurt");
        sHurt.motion = flyClip;
        var sDeath = sm.AddState("Death");
        sDeath.motion = (deathFrames.Count > 0) ? deathClip : flyClip;
        sm.defaultState = sIdle;

        // Transitions
        var toRun = sIdle.AddTransition(sRun);
        toRun.hasExitTime = false; toRun.duration = 0f;
        toRun.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0f, "IsMoving");

        var toIdle = sRun.AddTransition(sIdle);
        toIdle.hasExitTime = false; toIdle.duration = 0f;
        toIdle.AddCondition(UnityEditor.Animations.AnimatorConditionMode.IfNot, 0f, "IsMoving");

        var toHurt = sm.AddAnyStateTransition(sHurt);
        toHurt.hasExitTime = false; toHurt.duration = 0f;
        toHurt.canTransitionToSelf = false;
        toHurt.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0f, "Hurt");

        var hurtBack = sHurt.AddTransition(sIdle);
        hurtBack.hasExitTime = true; hurtBack.exitTime = 0.9f; hurtBack.duration = 0f;

        var toDeath = sm.AddAnyStateTransition(sDeath);
        toDeath.hasExitTime = false; toDeath.duration = 0f;
        toDeath.canTransitionToSelf = false;
        toDeath.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0f, "Die");

        EditorUtility.SetDirty(controller);

        // 2) Build Prefab GameObject
        GameObject enemy = new GameObject("EyeBat");
        enemy.tag = "Enemy";
        enemy.layer = LayerMask.NameToLayer("Enemy");

        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 2;
        sr.sortingLayerName = "Player";
        sr.sprite = frames[0];

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        CircleCollider2D col = enemy.AddComponent<CircleCollider2D>();
        col.isTrigger = false;
        col.radius = 0.25f;

        // AI Scripts
        EnemyChase chase = enemy.AddComponent<EnemyChase>();
        SerializedObject chaseSO = new SerializedObject(chase);
        chaseSO.FindProperty("speed").floatValue = 4f; // slightly faster than zombie
        chaseSO.FindProperty("detectionRadius").floatValue = 5f;
        chaseSO.ApplyModifiedProperties();

        EnemyHealth health = enemy.AddComponent<EnemyHealth>();
        SerializedObject healthSO = new SerializedObject(health);
        healthSO.FindProperty("maxHealth").intValue = 30;
        healthSO.FindProperty("deathAnimationDuration").floatValue = 0.5f;
        healthSO.ApplyModifiedProperties();

        Animator anim = enemy.AddComponent<Animator>();
        anim.runtimeAnimatorController = controller;

        // Scale up since 16x16 is small
        enemy.transform.localScale = new Vector3(5f, 5f, 1f);

        SavePrefab(enemy, prefabPath);
        Debug.Log("  ✓ EyeBat prefab created successfully!");
    }

    private static void CreateRoomGatePrefab(string path)
    {
        if (AssetExists(path + "/RoomGate.prefab")) return;

        GameObject gate = new GameObject("RoomGate");

        SpriteRenderer sr = gate.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 1;
        sr.sortingLayerName = "Player";
        sr.sprite = FindSpriteAsset("doors_leaf_closed") ?? FindSpriteAsset("crate");
        sr.color = new Color(0.6f, 0.4f, 0.2f, 1f); // Brown

        BoxCollider2D col = gate.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(2f, 0.5f);

        RoomGate gateScript = gate.AddComponent<RoomGate>();
        SerializedObject so = new SerializedObject(gateScript);
        so.FindProperty("gateVisual").objectReferenceValue = sr;
        so.ApplyModifiedProperties();

        gate.tag = "Gate_Start";

        SavePrefab(gate, path + "/RoomGate.prefab");
        Debug.Log("  ✓ RoomGate prefab created");
    }

    private static void CreateNextLevelPortalPrefab(string path)
    {
        if (AssetExists(path + "/NextLevelPortal.prefab")) return;

        GameObject portal = new GameObject("NextLevelPortal");

        SpriteRenderer sr = portal.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 0;
        sr.sortingLayerName = "Player";
        sr.sprite = FindSpriteAsset("floor_ladder") ?? FindSpriteAsset("hole");
        sr.color = new Color(0.5f, 0.8f, 1f, 1f); // Light blue

        BoxCollider2D col = portal.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1f, 1f);

        NextLevelPortal portalScript = portal.AddComponent<NextLevelPortal>();

        // Create prompt text child
        GameObject textObj = new GameObject("PromptText");
        textObj.transform.SetParent(portal.transform);
        textObj.transform.localPosition = new Vector3(0f, 1.5f, 0f);

        TextMeshPro tmp = textObj.AddComponent<TextMeshPro>();
        tmp.text = "Press ENTER to continue";
        tmp.fontSize = 3f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        // Link prompt text
        SerializedObject so = new SerializedObject(portalScript);
        so.FindProperty("promptText").objectReferenceValue = tmp;
        so.ApplyModifiedProperties();

        SavePrefab(portal, path + "/NextLevelPortal.prefab");
        Debug.Log("  ✓ NextLevelPortal prefab created");
    }

    // ========================================================
    // SCENE SETUP METHODS
    // ========================================================

    private static void SetupPlayerInScene()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("⚠️ No Player found in scene. Skipping player setup.");
            return;
        }

        // Set layer
        int playerLayer = LayerMask.NameToLayer("Player");
        if (playerLayer >= 0)
            player.layer = playerLayer;

        // Ensure Player script exists
        Player playerScript = player.GetComponent<Player>();
        if (playerScript == null)
        {
            playerScript = player.AddComponent<Player>();
            Debug.Log("  ✓ Attached Player.cs script component to Player GameObject");
        }

        // Setup default health values
        playerScript.maxHealth = 100;
        playerScript.currentHealth = 100;
        EditorUtility.SetDirty(playerScript);

        Debug.Log("  ✓ Player setup complete");
    }

    private static void CreateGameManagerInScene()
    {
        if (Object.FindAnyObjectByType<GameManager>() != null)
        {
            Debug.Log("  ✓ GameManager already exists");
            return;
        }

        GameObject gm = new GameObject("GameManager");
        gm.AddComponent<GameManager>();
        Debug.Log("  ✓ GameManager created");
    }

    private static void CreateLevelManagerInScene()
    {
        LevelManager lmScript = Object.FindAnyObjectByType<LevelManager>();
        bool isNew = false;

        if (lmScript == null)
        {
            GameObject lmObj = new GameObject("LevelManager");
            lmScript = lmObj.AddComponent<LevelManager>();
            isNew = true;
        }

        // Load background music clips from Ninja Adventure assets
        AudioClip mainMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Art/Sprites/GameAssets/Ninja Adventure - Asset Pack/Audio/Musics/1 - Adventure Begin.ogg");
        AudioClip hubMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Art/Sprites/GameAssets/Ninja Adventure - Asset Pack/Audio/Musics/33 - Calm Village.ogg");
        AudioClip dungeonMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Art/Sprites/GameAssets/Ninja Adventure - Asset Pack/Audio/Musics/21 - Dungeon.ogg");

        SerializedObject so = new SerializedObject(lmScript);
        if (mainMusic != null) so.FindProperty("mainMenuMusic").objectReferenceValue = mainMusic;
        if (hubMusic != null) so.FindProperty("hubMusic").objectReferenceValue = hubMusic;
        if (dungeonMusic != null) so.FindProperty("dungeonMusic").objectReferenceValue = dungeonMusic;
        so.ApplyModifiedProperties();

        // Check if loadingScreen is missing and repair it if necessary
        if (lmScript.loadingScreen == null)
        {
            // Call the public creation method from menu setup to create and link the LoadingCanvas
            PRU213MenuSetup.CreateLoadingScreen();
            Debug.Log("  ✓ LevelManager Loading Screen was missing and has been successfully repaired!");
        }

        if (isNew)
        {
            Debug.Log("  ✓ LevelManager created with default BGM clips");
        }
        else
        {
            Debug.Log("  ✓ LevelManager updated with BGM clips while keeping loading screen");
        }
    }

    private static void CreateHUDInScene()
    {
        // 1. Destroy existing HUD_Canvas or related HUD gameobjects to prevent duplicates
        GameObject oldCanvas = GameObject.Find("HUD_Canvas");
        if (oldCanvas != null)
        {
            Object.DestroyImmediate(oldCanvas);
        }
        
        PlayerHUD oldPHUD = Object.FindAnyObjectByType<PlayerHUD>();
        if (oldPHUD != null)
        {
            Object.DestroyImmediate(oldPHUD.gameObject);
        }

        GameHUD oldGHUD = Object.FindAnyObjectByType<GameHUD>();
        if (oldGHUD != null)
        {
            Object.DestroyImmediate(oldGHUD.gameObject);
        }

        // 2. Create HUD Canvas
        GameObject canvas = new GameObject("HUD_Canvas");
        Canvas c = canvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 100;

        CanvasScaler scaler = canvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvas.AddComponent<GraphicRaycaster>();

        // 3. Level Text (center of screen at the top)
        GameObject levelObj = CreateTMPText(canvas.transform, "LevelText", "FLOOR 1",
            new Vector2(0, -40), TextAlignmentOptions.Center, 28, new Color(1f, 0.85f, 0.3f));
        RectTransform levelRect = levelObj.GetComponent<RectTransform>();
        levelRect.anchorMin = new Vector2(0.5f, 1);
        levelRect.anchorMax = new Vector2(0.5f, 1);
        levelRect.pivot = new Vector2(0.5f, 1);

        // 4. Weapon Info Panel (bottom right of screen)
        GameObject weaponPanel = new GameObject("WeaponInfoPanel");
        weaponPanel.transform.SetParent(canvas.transform, false);
        RectTransform panelRect = weaponPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1, 0);
        panelRect.anchorMax = new Vector2(1, 0);
        panelRect.pivot = new Vector2(1, 0);
        panelRect.anchoredPosition = new Vector2(-20, 20);
        panelRect.sizeDelta = new Vector2(250, 70);

        Image panelImg = weaponPanel.AddComponent<Image>();
        panelImg.color = new Color(0, 0, 0, 0.5f);

        GameObject weaponNameObj = CreateTMPText(weaponPanel.transform, "WeaponNameText", "Weapon",
            new Vector2(10, -10), TextAlignmentOptions.Left, 18, Color.white);
        RectTransform wnRect = weaponNameObj.GetComponent<RectTransform>();
        wnRect.anchorMin = new Vector2(0, 1);
        wnRect.anchorMax = new Vector2(1, 1);
        wnRect.pivot = new Vector2(0, 1);
        wnRect.sizeDelta = new Vector2(-20, 30);

        GameObject weaponDmgObj = CreateTMPText(weaponPanel.transform, "WeaponDamageText", "DMG: 0",
            new Vector2(10, -40), TextAlignmentOptions.Left, 14, new Color(0.8f, 0.8f, 0.8f));
        RectTransform wdRect = weaponDmgObj.GetComponent<RectTransform>();
        wdRect.anchorMin = new Vector2(0, 1);
        wdRect.anchorMax = new Vector2(1, 1);
        wdRect.pivot = new Vector2(0, 1);
        wdRect.sizeDelta = new Vector2(-20, 25);

        // Add GameHUD script
        GameHUD gameHUD = canvas.AddComponent<GameHUD>();
        SerializedObject soGameHUD = new SerializedObject(gameHUD);
        soGameHUD.FindProperty("levelText").objectReferenceValue = levelObj.GetComponent<TextMeshProUGUI>();
        soGameHUD.FindProperty("weaponNameText").objectReferenceValue = weaponNameObj.GetComponent<TextMeshProUGUI>();
        soGameHUD.FindProperty("weaponDamageText").objectReferenceValue = weaponDmgObj.GetComponent<TextMeshProUGUI>();
        soGameHUD.FindProperty("weaponInfoPanel").objectReferenceValue = weaponPanel;
        soGameHUD.ApplyModifiedProperties();

        // 5. Soul Knight Style HUD Panel
        GameObject phudPanel = new GameObject("PlayerHUD_Panel");
        phudPanel.transform.SetParent(canvas.transform, false);
        RectTransform phudRect = phudPanel.AddComponent<RectTransform>();
        phudRect.anchorMin = new Vector2(0, 1);
        phudRect.anchorMax = new Vector2(0, 1);
        phudRect.pivot = new Vector2(0, 1);
        phudRect.anchoredPosition = new Vector2(40, -40); // Top-left corner
        phudRect.sizeDelta = new Vector2(440, 170); // Size of HUD box

        // Background of HUD box
        Image phudImg = phudPanel.AddComponent<Image>();
        phudImg.color = new Color(0.24f, 0.20f, 0.16f, 0.85f); // Brown board color

        Outline phudOutline = phudPanel.AddComponent<Outline>();
        phudOutline.effectColor = new Color(0.12f, 0.10f, 0.08f, 1f);
        phudOutline.effectDistance = new Vector2(3, 3);

        // Load Sprites
        Sprite barSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/GameAssets/HealthBar/Bar.png");
        Sprite heartSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/GameAssets/HealthBar/Heart.png");
        Sprite shieldSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/GameAssets/HealthBar/Shield.png");
        Sprite manaSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/GameAssets/HealthBar/Mana.png");

        // Health row (Red)
        var healthUI = CreateHUDBarRow(phudPanel.transform, "Health", heartSprite, barSprite, new Color(0.85f, 0.15f, 0.15f), new Vector2(0, -10));

        // Shield row (Grey/Silver)
        var shieldUI = CreateHUDBarRow(phudPanel.transform, "Shield", shieldSprite, barSprite, new Color(0.6f, 0.65f, 0.7f), new Vector2(0, -60));

        // Energy/Mana row (Blue)
        var energyUI = CreateHUDBarRow(phudPanel.transform, "Energy", manaSprite, barSprite, new Color(0.1f, 0.45f, 0.85f), new Vector2(0, -110));

        // Add PlayerHUD script
        PlayerHUD playerHUD = canvas.AddComponent<PlayerHUD>();
        playerHUD.healthSlider = healthUI.Item1;
        playerHUD.healthText = healthUI.Item2;
        playerHUD.shieldSlider = shieldUI.Item1;
        playerHUD.shieldText = shieldUI.Item2;
        playerHUD.energySlider = energyUI.Item1;
        playerHUD.energyText = energyUI.Item2;

        // 6. Create Potions Hotbar Panel (positioned below the PlayerHUD_Panel)
        GameObject hotbarPanel = new GameObject("PlayerHotbar_Panel");
        hotbarPanel.transform.SetParent(canvas.transform, false);
        RectTransform hotbarRect = hotbarPanel.AddComponent<RectTransform>();
        hotbarRect.anchorMin = new Vector2(0, 1);
        hotbarRect.anchorMax = new Vector2(0, 1);
        hotbarRect.pivot = new Vector2(0, 1);
        hotbarRect.anchoredPosition = new Vector2(40, -225); // Just below HUD Panel
        hotbarRect.sizeDelta = new Vector2(240, 70);

        Image hotbarImg = hotbarPanel.AddComponent<Image>();
        hotbarImg.color = new Color(0.24f, 0.20f, 0.16f, 0.85f); // Brown board color

        Outline hotbarOutline = hotbarPanel.AddComponent<Outline>();
        hotbarOutline.effectColor = new Color(0.12f, 0.10f, 0.08f, 1f);
        hotbarOutline.effectDistance = new Vector2(2, 2);

        // HP Potion Slot
        GameObject hSlot = new GameObject("HPSlot");
        hSlot.transform.SetParent(hotbarPanel.transform, false);
        RectTransform hRect = hSlot.AddComponent<RectTransform>();
        hRect.anchorMin = new Vector2(0, 0.5f);
        hRect.anchorMax = new Vector2(0.5f, 0.5f);
        hRect.pivot = new Vector2(0.5f, 0.5f);
        hRect.anchoredPosition = new Vector2(60, 0);
        hRect.sizeDelta = new Vector2(100, 50);

        GameObject hIconObj = new GameObject("HPIcon");
        hIconObj.transform.SetParent(hSlot.transform, false);
        RectTransform hiRect = hIconObj.AddComponent<RectTransform>();
        hiRect.anchorMin = new Vector2(0, 0.5f);
        hiRect.anchorMax = new Vector2(0, 0.5f);
        hiRect.pivot = new Vector2(0, 0.5f);
        hiRect.anchoredPosition = new Vector2(10, 0);
        hiRect.sizeDelta = new Vector2(28, 28);
        Image hIconImg = hIconObj.AddComponent<Image>();
        hIconImg.sprite = heartSprite;
        hIconImg.preserveAspect = true;

        GameObject hTextObj = CreateTMPText(hSlot.transform, "HPCountText", "[1] 3",
            new Vector2(45, 0), TextAlignmentOptions.Left, 16, Color.white);
        RectTransform htRect = hTextObj.GetComponent<RectTransform>();
        htRect.anchorMin = new Vector2(0, 0.5f);
        htRect.anchorMax = new Vector2(1, 0.5f);
        htRect.pivot = new Vector2(0, 0.5f);
        htRect.sizeDelta = new Vector2(-45, 30);

        // MP Potion Slot
        GameObject mSlot = new GameObject("MPSlot");
        mSlot.transform.SetParent(hotbarPanel.transform, false);
        RectTransform mRect = mSlot.AddComponent<RectTransform>();
        mRect.anchorMin = new Vector2(0.5f, 0.5f);
        mRect.anchorMax = new Vector2(1, 0.5f);
        mRect.pivot = new Vector2(0.5f, 0.5f);
        mRect.anchoredPosition = new Vector2(60, 0);
        mRect.sizeDelta = new Vector2(100, 50);

        GameObject mIconObj = new GameObject("MPIcon");
        mIconObj.transform.SetParent(mSlot.transform, false);
        RectTransform miRect = mIconObj.AddComponent<RectTransform>();
        miRect.anchorMin = new Vector2(0, 0.5f);
        miRect.anchorMax = new Vector2(0, 0.5f);
        miRect.pivot = new Vector2(0, 0.5f);
        miRect.anchoredPosition = new Vector2(10, 0);
        miRect.sizeDelta = new Vector2(28, 28);
        Image mIconImg = mIconObj.AddComponent<Image>();
        mIconImg.sprite = manaSprite;
        mIconImg.preserveAspect = true;

        GameObject mTextObj = CreateTMPText(mSlot.transform, "MPCountText", "[2] 3",
            new Vector2(45, 0), TextAlignmentOptions.Left, 16, Color.white);
        RectTransform mtRect = mTextObj.GetComponent<RectTransform>();
        mtRect.anchorMin = new Vector2(0, 0.5f);
        mtRect.anchorMax = new Vector2(1, 0.5f);
        mtRect.pivot = new Vector2(0, 0.5f);
        mtRect.sizeDelta = new Vector2(-45, 30);

        // Link counts to PlayerHUD
        playerHUD.hpPotionText = hTextObj.GetComponent<TextMeshProUGUI>();
        playerHUD.mpPotionText = mTextObj.GetComponent<TextMeshProUGUI>();

        // Connect Player to HUD
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        Player playerScript = playerObj != null ? playerObj.GetComponent<Player>() : null;
        if (playerScript != null)
        {
            SerializedObject soPHUD = new SerializedObject(playerHUD);
            soPHUD.FindProperty("player").objectReferenceValue = playerScript;
            soPHUD.ApplyModifiedProperties();
        }

        // 7. Create Boss HUD Panel at bottom center of screen
        CreateBossHUDInCanvas(canvas);

        Debug.Log("  ✓ Soul Knight HUD, Potions Hotbar & Boss HUD created successfully");
    }

    private static (Slider, TextMeshProUGUI) CreateHUDBarRow(Transform parent, string name, Sprite iconSprite, Sprite barSprite, Color fillColor, Vector2 anchoredPos)
    {
        // 1. Create Row GameObject
        GameObject row = new GameObject(name + "_Row");
        row.transform.SetParent(parent, false);
        RectTransform rowRect = row.AddComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0, 1);
        rowRect.anchorMax = new Vector2(0, 1);
        rowRect.pivot = new Vector2(0, 1);
        rowRect.anchoredPosition = anchoredPos;
        rowRect.sizeDelta = new Vector2(420, 45);

        // 2. Icon
        GameObject iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(row.transform, false);
        RectTransform iconRect = iconObj.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0, 0.5f);
        iconRect.anchorMax = new Vector2(0, 0.5f);
        iconRect.pivot = new Vector2(0, 0.5f);
        iconRect.anchoredPosition = new Vector2(15, 0);
        iconRect.sizeDelta = new Vector2(32, 32);
        Image iconImg = iconObj.AddComponent<Image>();
        iconImg.sprite = iconSprite;
        iconImg.preserveAspect = true;

        // 3. Slider Container
        GameObject sliderObj = new GameObject("Slider");
        sliderObj.transform.SetParent(row.transform, false);
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0, 0.5f);
        sliderRect.anchorMax = new Vector2(1, 0.5f);
        sliderRect.pivot = new Vector2(0, 0.5f);
        sliderRect.anchoredPosition = new Vector2(65, 0);
        sliderRect.sizeDelta = new Vector2(-85, 26);

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = 100;
        slider.wholeNumbers = true;

        // Slider Background Image
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.sprite = barSprite;
        bgImg.type = Image.Type.Sliced;
        bgImg.color = new Color(0.12f, 0.12f, 0.12f, 0.9f); // Dark background

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform faRect = fillArea.AddComponent<RectTransform>();
        faRect.anchorMin = Vector2.zero;
        faRect.anchorMax = Vector2.one;
        faRect.offsetMin = new Vector2(2, 2);
        faRect.offsetMax = new Vector2(-2, -2);

        // Fill Image
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        Image fillImg = fillObj.AddComponent<Image>();
        fillImg.sprite = barSprite;
        fillImg.type = Image.Type.Sliced;
        fillImg.color = fillColor;

        slider.fillRect = fillRect;

        // Text display on top of slider
        GameObject textObj = CreateTMPText(sliderObj.transform, "ValueText", "0/0",
            Vector2.zero, TextAlignmentOptions.Center, 18, Color.white);
        RectTransform tRect = textObj.GetComponent<RectTransform>();
        tRect.anchorMin = Vector2.zero;
        tRect.anchorMax = Vector2.one;
        tRect.offsetMin = Vector2.zero;
        tRect.offsetMax = Vector2.zero;

        // Add outline to text for readability
        TextMeshProUGUI tmp = textObj.GetComponent<TextMeshProUGUI>();
        tmp.fontStyle = FontStyles.Bold;
        tmp.outlineColor = Color.black;
        tmp.outlineWidth = 0.2f;

        return (slider, tmp);
    }

    private static void PlaceStarterWeaponInScene()
    {
        // Place a gun near the player as starter weapon
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // Check if there's already a weapon in scene
        if (Object.FindAnyObjectByType<BaseWeapon>() != null)
        {
            Debug.Log("  ✓ Weapon already exists in scene");
            return;
        }

        GameObject gunPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Weapons/Gun.prefab");
        if (gunPrefab != null)
        {
            GameObject gun = (GameObject)PrefabUtility.InstantiatePrefab(gunPrefab);
            gun.transform.position = player.transform.position + new Vector3(1.5f, 0, 0);
            Debug.Log("  ✓ Starter Gun placed near player");
        }

        GameObject swordPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Weapons/Sword.prefab");
        if (swordPrefab != null)
        {
            GameObject sword = (GameObject)PrefabUtility.InstantiatePrefab(swordPrefab);
            sword.transform.position = player.transform.position + new Vector3(-1.5f, 0, 0);
            Debug.Log("  ✓ Starter Sword placed near player");
        }
    }

    // ========================================================
    // HELPER METHODS
    // ========================================================

    private static void CreateEnemyHealthBar(GameObject enemy, int maxHealth)
    {
        // Create a simple Canvas + Slider for enemy health bar
        GameObject canvasObj = new GameObject("HealthBarCanvas");
        canvasObj.transform.SetParent(enemy.transform);
        canvasObj.transform.localPosition = new Vector3(0, 0.8f, 0);

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 10;

        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(100, 20);
        canvasRect.localScale = new Vector3(0.01f, 0.01f, 1f);

        // Create Slider
        // Since creating a proper UI Slider programmatically is complex,
        // we'll use a simple approach with a background and fill
        GameObject sliderObj = new GameObject("HealthSlider");
        sliderObj.transform.SetParent(canvasObj.transform, false);

        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = maxHealth;
        slider.value = maxHealth;
        slider.wholeNumbers = true;

        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = Vector2.zero;
        sliderRect.anchorMax = Vector2.one;
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = Vector2.zero;

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderObj.transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = new Vector2(2, 2);
        fillAreaRect.offsetMax = new Vector2(-2, -2);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.9f, 0.2f, 0.2f, 1f); // Red
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        slider.fillRect = fillRect;
        slider.targetGraphic = fillImg;

        // Link healthbar to enemy script
        BaseEnemy enemyScript = enemy.GetComponent<BaseEnemy>();
        if (enemyScript != null)
        {
            enemyScript.healthbar = slider;
        }
    }

    private static GameObject CreateTMPText(Transform parent, string name, string text,
        Vector2 position, TextAlignmentOptions alignment, float fontSize, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = color;

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(300, 40);

        return obj;
    }

    private static void AddTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
        SerializedProperty tagsProp = tagManager.FindProperty("tags");

        // Check if tag already exists
        for (int i = 0; i < tagsProp.arraySize; i++)
        {
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
            {
                Debug.Log($"  Tag '{tag}' already exists");
                return;
            }
        }

        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
        Debug.Log($"  ✓ Tag '{tag}' added");
    }

    private static void AddLayer(int layerIndex, string layerName)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
        SerializedProperty layersProp = tagManager.FindProperty("layers");

        SerializedProperty layerProp = layersProp.GetArrayElementAtIndex(layerIndex);
        if (!string.IsNullOrEmpty(layerProp.stringValue) && layerProp.stringValue != layerName)
        {
            Debug.LogWarning($"  ⚠️ Layer {layerIndex} already has '{layerProp.stringValue}', skipping '{layerName}'");
            return;
        }

        layerProp.stringValue = layerName;
        tagManager.ApplyModifiedProperties();
        Debug.Log($"  ✓ Layer {layerIndex}: '{layerName}' added");
    }

    private static Sprite FindSpriteAsset(string name)
    {
        string[] guids = AssetDatabase.FindAssets(name + " t:Sprite");
        if (guids.Length == 0)
        {
            // Try finding as a tile/asset
            guids = AssetDatabase.FindAssets(name);
        }

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite != null) return sprite;

            // Try loading sub-assets (for sprite sheets)
            Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (Object sub in subAssets)
            {
                if (sub is Sprite s && s.name.Contains(name))
                    return s;
            }
        }
        return null;
    }

    private static Sprite LoadSpriteByDirectPath(string primaryName, string fallbackName)
    {
        Sprite sp = FindSpriteAsset(primaryName);
        if (sp != null) return sp;

        // Try direct paths for Fireball
        if (primaryName == "Fireball")
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Sprites/GameAssets/Ninja Adventure - Asset Pack/FX/Projectile/Fireball.png");
            if (assets != null)
            {
                foreach (Object asset in assets)
                {
                    if (asset is Sprite s) return s;
                }
            }
        }
        
        // Try direct paths for EnergyBall
        if (primaryName == "EnergyBall")
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Sprites/GameAssets/Ninja Adventure - Asset Pack/FX/Projectile/EnergyBall.png");
            if (assets != null)
            {
                foreach (Object asset in assets)
                {
                    if (asset is Sprite s) return s;
                }
            }
        }

        // Try direct paths for Arrow fallback
        Object[] fallbackAssets = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Sprites/GameAssets/Ninja Adventure - Asset Pack/FX/Projectile/Arrow.png");
        if (fallbackAssets != null)
        {
            foreach (Object asset in fallbackAssets)
            {
                if (asset is Sprite s) return s;
            }
        }

        return FindSpriteAsset(fallbackName);
    }

private static bool AssetExists(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Object>(path) != null;
    }

    private static void SetupPauseMenuInScene()
    {
        MenuController mc = Object.FindAnyObjectByType<MenuController>();
        if (mc == null)
        {
            Debug.LogWarning("⚠️ No MenuController found in scene to setup pause menu.");
            return;
        }

        GameObject menuCanvas = mc.menuCanvas;
        if (menuCanvas == null)
        {
            menuCanvas = GameObject.Find("MenuCanvas");
            if (menuCanvas == null) menuCanvas = GameObject.Find("PauseMenu");
            if (menuCanvas == null)
            {
                Debug.LogWarning("⚠️ Could not find menuCanvas for MenuController.");
                return;
            }
            mc.menuCanvas = menuCanvas;
        }

        // Set MenuCanvas sortingOrder to 200 (so HUD at 100 will draw behind/under it!)
        Canvas canvasComp = menuCanvas.GetComponent<Canvas>();
        if (canvasComp == null) canvasComp = menuCanvas.GetComponentInParent<Canvas>();
        if (canvasComp != null)
        {
            canvasComp.sortingOrder = 200;
            Debug.Log("  ✓ Set MenuCanvas sortingOrder to 200 (HUD will draw behind/under it)");
        }

        // 1. Destroy MapCamera if it exists (map feature removed)
        GameObject mapCamObj = GameObject.Find("MapCamera");
        if (mapCamObj != null)
        {
            Object.DestroyImmediate(mapCamObj);
            Debug.Log("  ✓ Removed MapCamera (map feature removed)");
        }

        // 2. Configure Pause Menu Pages and Delete Inventory + Map
        Transform playerPage = null;
        Transform settingsPage = null;

        TabController tabCtrl = menuCanvas.GetComponent<TabController>();
        if (tabCtrl == null) tabCtrl = menuCanvas.GetComponentInChildren<TabController>();

        if (tabCtrl != null)
        {
            // Find all TextMeshProUGUI components under menuCanvas to identify tab buttons
            TextMeshProUGUI[] allTexts = tabCtrl.transform.parent != null
                ? tabCtrl.transform.parent.GetComponentsInChildren<TextMeshProUGUI>(true)
                : tabCtrl.GetComponentsInChildren<TextMeshProUGUI>(true);

            GameObject playerTabObj = null;
            GameObject invTabObj = null;
            GameObject mapTabObj = null;
            GameObject settingsTabObj = null;
            // Note: mapTabObj and invTabObj are still scanned so we can delete them

            foreach (var tmpText in allTexts)
            {
                string txt = tmpText.text.ToUpper().Trim();
                string objName = tmpText.gameObject.name.ToUpper();
                string parentName = tmpText.transform.parent != null ? tmpText.transform.parent.gameObject.name.ToUpper() : "";

                GameObject targetObj = tmpText.gameObject;
                if (targetObj.GetComponent<UnityEngine.EventSystems.EventTrigger>() == null && 
                    targetObj.GetComponent<Button>() == null && 
                    targetObj.transform.parent != null)
                {
                    targetObj = targetObj.transform.parent.gameObject;
                }

                if (txt == "PLAYER" || objName.Contains("PLAYER") || parentName.Contains("PLAYER"))
                {
                    playerTabObj = targetObj;
                }
                else if (txt == "IVENTORY" || txt == "INVENTORY" || objName.Contains("INVENTORY") || parentName.Contains("INVENTORY"))
                {
                    invTabObj = targetObj;
                }
                else if (txt == "MAP" || objName.Contains("MAP") || parentName.Contains("MAP"))
                {
                    mapTabObj = targetObj;
                }
                else if (txt == "SETTINGS" || txt == "SETTING" || objName.Contains("SETTING") || parentName.Contains("SETTING"))
                {
                    settingsTabObj = targetObj;
                }
            }

            // Also find the pages
            GameObject playerPg = null;
            GameObject invPg = null;
            GameObject mapPg = null;
            GameObject settingsPg = null;

            Transform pagesParent = menuCanvas.transform.Find("Pages");
            if (pagesParent == null)
            {
                foreach (Transform t in menuCanvas.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "Pages") { pagesParent = t; break; }
                }
            }

            if (pagesParent != null)
            {
                foreach (Transform child in pagesParent)
                {
                    string nameUpper = child.name.ToUpper();
                    if (nameUpper.Contains("PLAYER")) playerPg = child.gameObject;
                    else if (nameUpper.Contains("MAP")) mapPg = child.gameObject;
                    else if (nameUpper.Contains("SETTING")) settingsPg = child.gameObject;
                    else if (nameUpper.Contains("INVENTORY") || nameUpper.Contains("IVENTORY")) invPg = child.gameObject;
                }
            }

            // Fallback: If not found under pages parent, search globally in menuCanvas
            if (playerPg == null || settingsPg == null)
            {
                foreach (Transform child in menuCanvas.GetComponentsInChildren<Transform>(true))
                {
                    // Ignore buttons/texts to avoid wrong assignments
                    if (child.GetComponent<Button>() != null || child.GetComponent<TextMeshProUGUI>() != null) continue;

                    string nameUpper = child.name.ToUpper();
                    if (playerPg == null && nameUpper.Contains("PLAYER") && (nameUpper.Contains("PAGE") || nameUpper.Contains("PANEL"))) playerPg = child.gameObject;
                    if (settingsPg == null && nameUpper.Contains("SETTING") && (nameUpper.Contains("PAGE") || nameUpper.Contains("PANEL") || nameUpper.Contains("MENU"))) settingsPg = child.gameObject;
                    if (mapPg == null && nameUpper.Contains("MAP") && (nameUpper.Contains("PAGE") || nameUpper.Contains("PANEL"))) mapPg = child.gameObject;
                    if (invPg == null && (nameUpper.Contains("INVENTORY") || nameUpper.Contains("IVENTORY")) && (nameUpper.Contains("PAGE") || nameUpper.Contains("PANEL"))) invPg = child.gameObject;
                }
            }

            Debug.Log($"[PauseMenu Setup] playerTabObj: {(playerTabObj != null ? playerTabObj.name : "NULL")}, settingsTabObj: {(settingsTabObj != null ? settingsTabObj.name : "NULL")}");
            Debug.Log($"[PauseMenu Setup] playerPg: {(playerPg != null ? playerPg.name : "NULL")}, settingsPg: {(settingsPg != null ? settingsPg.name : "NULL")}");

            // If inventory elements are found, delete them
            if (invTabObj != null)
            {
                Object.DestroyImmediate(invTabObj);
            }
            if (invPg != null)
            {
                Object.DestroyImmediate(invPg);
            }

            // Delete Map tab and page if found
            if (mapTabObj != null)
            {
                Object.DestroyImmediate(mapTabObj);
                Debug.Log("  ✓ Removed Map tab button");
            }
            if (mapPg != null)
            {
                Object.DestroyImmediate(mapPg);
                Debug.Log("  ✓ Removed Map page");
            }

            // Rebuild TabController arrays with only Player, Settings
            var newTabImages = new System.Collections.Generic.List<UnityEngine.UI.Image>();
            if (playerTabObj != null) newTabImages.Add(playerTabObj.GetComponentInChildren<UnityEngine.UI.Image>(true) ?? playerTabObj.GetComponent<UnityEngine.UI.Image>());
            if (settingsTabObj != null) newTabImages.Add(settingsTabObj.GetComponentInChildren<UnityEngine.UI.Image>(true) ?? settingsTabObj.GetComponent<UnityEngine.UI.Image>());

            var newPages = new System.Collections.Generic.List<GameObject>();
            if (playerPg != null) newPages.Add(playerPg);
            if (settingsPg != null) newPages.Add(settingsPg);

            SerializedObject soTab = new SerializedObject(tabCtrl);
            SerializedProperty tabImagesProp = soTab.FindProperty("tabImages");
            tabImagesProp.ClearArray();
            tabImagesProp.arraySize = newTabImages.Count;
            for (int i = 0; i < newTabImages.Count; i++)
            {
                tabImagesProp.GetArrayElementAtIndex(i).objectReferenceValue = newTabImages[i];
            }

            SerializedProperty pagesProp = soTab.FindProperty("pages");
            pagesProp.ClearArray();
            pagesProp.arraySize = newPages.Count;
            for (int i = 0; i < newPages.Count; i++)
            {
                pagesProp.GetArrayElementAtIndex(i).objectReferenceValue = newPages[i];
            }

            soTab.ApplyModifiedProperties();
            EditorUtility.SetDirty(tabCtrl);

            // Re-position the remaining 2 tab buttons horizontally and evenly!
            if (playerTabObj != null && settingsTabObj != null)
            {
                RectTransform rPlayer = playerTabObj.GetComponent<RectTransform>();
                RectTransform rSettings = settingsTabObj.GetComponent<RectTransform>();

                // Spacing: Player at X = -100, Settings at X = 100
                rPlayer.anchoredPosition = new Vector2(0f, rPlayer.anchoredPosition.y);
                rSettings.anchoredPosition = new Vector2(100f, rSettings.anchoredPosition.y);

                // Re-bind EventTriggers to match new 2-tab index layout (Player=0, Settings=1)
                UpdateTabButtonTrigger(playerTabObj, 0);
                UpdateTabButtonTrigger(settingsTabObj, 1);
            }

            playerPage = playerPg != null ? playerPg.transform : null;
            settingsPage = settingsPg != null ? settingsPg.transform : null;
        }

        // A. Set up PLAYER PAGE (Health, Shield, Mana, Name)
        TextMeshProUGUI healthValText = null;
        TextMeshProUGUI manaValText = null;
        TextMeshProUGUI shieldValText = null;
        TextMeshProUGUI nameValText = null;

        if (playerPage != null)
        {
            foreach (TextMeshProUGUI tmp in playerPage.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (tmp.name == "HealthCurent") healthValText = tmp;
                else if (tmp.name == "ManaCurent") manaValText = tmp;
                else if (tmp.name == "ShieldCurent") shieldValText = tmp;
                else if (tmp.name.ToUpper().Contains("NAME") && !tmp.name.ToUpper().Contains("LABEL")) nameValText = tmp;
            }

            // Create Shield display if it doesn't exist
            if (shieldValText == null)
            {
                Transform manaLabelTrans = playerPage.Find("Mana");
                Transform manaValTrans = playerPage.Find("ManaCurent");

                if (manaLabelTrans != null && manaValTrans != null)
                {
                    // Duplicate Label
                    GameObject shieldLabelObj = Object.Instantiate(manaLabelTrans.gameObject, playerPage);
                    shieldLabelObj.name = "Shield";
                    TextMeshProUGUI shieldLabel = shieldLabelObj.GetComponent<TextMeshProUGUI>();
                    if (shieldLabel != null) shieldLabel.text = "SHIELD:";

                    // Duplicate Value
                    GameObject shieldValObj = Object.Instantiate(manaValTrans.gameObject, playerPage);
                    shieldValObj.name = "ShieldCurent";
                    shieldValText = shieldValObj.GetComponent<TextMeshProUGUI>();
                    if (shieldValText != null)
                    {
                        shieldValText.text = "5/5";
                        shieldValText.color = new Color(0.7f, 0.7f, 0.7f); // Grey
                    }

                    // Position Shield label and value between Health and Mana
                    Transform healthLabelTrans = playerPage.Find("Health");
                    Transform healthValTrans = playerPage.Find("HealthCurent");

                    RectTransform rShieldLabel = shieldLabelObj.GetComponent<RectTransform>();
                    RectTransform rShieldVal = shieldValObj.GetComponent<RectTransform>();

                    RectTransform rManaLabel = manaLabelTrans.GetComponent<RectTransform>();
                    RectTransform rManaVal = manaValTrans.GetComponent<RectTransform>();

                    if (healthLabelTrans != null && healthValTrans != null)
                    {
                        RectTransform rHealthLabel = healthLabelTrans.GetComponent<RectTransform>();
                        RectTransform rHealthVal = healthValTrans.GetComponent<RectTransform>();

                        float midY = (rHealthLabel.anchoredPosition.y + rManaLabel.anchoredPosition.y) * 0.5f;
                        rShieldLabel.anchoredPosition = new Vector2(rManaLabel.anchoredPosition.x, midY);
                        rShieldVal.anchoredPosition = new Vector2(rManaVal.anchoredPosition.x, midY);
                    }
                    else
                    {
                        rShieldLabel.anchoredPosition = rManaLabel.anchoredPosition + new Vector2(0f, 40f);
                        rShieldVal.anchoredPosition = rManaVal.anchoredPosition + new Vector2(0f, 40f);
                    }
                }
            }
        }

        // (Map page has been removed - no setup needed)

        // D. Set up SETTINGS PAGE (Save, Settings, Exit)
        if (settingsPage != null)
        {
            Button saveBtn = null;
            Button settingsBtn = null;
            Button exitBtn = null;

            foreach (Button btn in settingsPage.GetComponentsInChildren<Button>(true))
            {
                string btnNameUpper = btn.name.ToUpper();
                if (btnNameUpper.Contains("SAVE")) saveBtn = btn;
                else if (btnNameUpper.Contains("LOAD") || btnNameUpper.Contains("SETTING")) settingsBtn = btn;
                else if (btnNameUpper.Contains("EXIT")) exitBtn = btn;
            }

            if (saveBtn != null)
            {
                saveBtn.gameObject.name = "SaveButton";
                TextMeshProUGUI btnText = saveBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = "SAVE";
            }

            if (settingsBtn != null)
            {
                settingsBtn.gameObject.name = "SettingsButton";
                TextMeshProUGUI btnText = settingsBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = "SETTINGS";
            }

            // Create Exit Button if missing
            if (exitBtn == null && settingsBtn != null)
            {
                GameObject exitBtnObj = Object.Instantiate(settingsBtn.gameObject, settingsPage);
                exitBtnObj.name = "ExitButton";
                exitBtn = exitBtnObj.GetComponent<Button>();
                TextMeshProUGUI btnText = exitBtn.GetComponentInChildren<TextMeshProUGUI>();
                if (btnText != null) btnText.text = "EXIT";
            }

            if (saveBtn != null && settingsBtn != null && exitBtn != null)
            {
                RectTransform rSave = saveBtn.GetComponent<RectTransform>();
                RectTransform rSettings = settingsBtn.GetComponent<RectTransform>();
                RectTransform rExit = exitBtn.GetComponent<RectTransform>();

                rSave.anchoredPosition = new Vector2(-530f, -20f);
                rSettings.anchoredPosition = new Vector2(0f, -20f);
                rExit.anchoredPosition = new Vector2(530f, -20f);

                rSave.sizeDelta = new Vector2(500f, 150f);
                rSettings.sizeDelta = new Vector2(500f, 150f);
                rExit.sizeDelta = new Vector2(500f, 150f);
            }

            mc.saveButton = saveBtn;
            mc.settingsButton = settingsBtn;
            mc.exitButton = exitBtn;
        }

        mc.playerNameText = nameValText;
        mc.playerHealthText = healthValText;
        mc.playerShieldText = shieldValText;
        mc.playerManaText = manaValText;

        SerializedObject so = new SerializedObject(mc);
        so.FindProperty("playerNameText").objectReferenceValue = nameValText;
        so.FindProperty("playerHealthText").objectReferenceValue = healthValText;
        so.FindProperty("playerShieldText").objectReferenceValue = shieldValText;
        so.FindProperty("playerManaText").objectReferenceValue = manaValText;

        so.FindProperty("saveButton").objectReferenceValue = mc.saveButton;
        so.FindProperty("settingsButton").objectReferenceValue = mc.settingsButton;
        so.FindProperty("exitButton").objectReferenceValue = mc.exitButton;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(mc);

        Debug.Log("  ✓ Pause sub-menu UI fully configured, positioned and linked!");
    }

    private static void SavePrefab(GameObject obj, string path)
    {
        PrefabUtility.SaveAsPrefabAsset(obj, path);
        Object.DestroyImmediate(obj);
    }

    private static void UpdateTabButtonTrigger(GameObject btnObj, int correctIndex)
    {
        if (btnObj == null) return;
        
        // A. Check Button onClick
        Button btn = btnObj.GetComponent<Button>();
        if (btn == null) btn = btnObj.GetComponentInParent<Button>();
        if (btn == null) btn = btnObj.GetComponentInChildren<Button>(true);
        if (btn != null)
        {
            SerializedObject soBtn = new SerializedObject(btn);
            SerializedProperty onClickCalls = soBtn.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
            if (onClickCalls != null)
            {
                for (int i = 0; i < onClickCalls.arraySize; i++)
                {
                    SerializedProperty call = onClickCalls.GetArrayElementAtIndex(i);
                    SerializedProperty methodName = call.FindPropertyRelative("m_MethodName");
                    if (methodName != null && methodName.stringValue == "ActiveTab")
                    {
                        SerializedProperty intArg = call.FindPropertyRelative("m_Arguments.m_IntArgument");
                        if (intArg != null)
                        {
                            intArg.intValue = correctIndex;
                            Debug.Log($"  ✓ Configured Button onClick on '{btnObj.name}' to call ActiveTab({correctIndex})");
                        }
                    }
                }
            }
            soBtn.ApplyModifiedProperties();
            EditorUtility.SetDirty(btn);
        }

        // B. Check EventTrigger delegates
        UnityEngine.EventSystems.EventTrigger trigger = btnObj.GetComponent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null) trigger = btnObj.GetComponentInParent<UnityEngine.EventSystems.EventTrigger>();
        if (trigger == null) trigger = btnObj.GetComponentInChildren<UnityEngine.EventSystems.EventTrigger>(true);
        if (trigger != null)
        {
            SerializedObject so = new SerializedObject(trigger);
            SerializedProperty delegates = so.FindProperty("m_Delegates");
            if (delegates != null)
            {
                for (int i = 0; i < delegates.arraySize; i++)
                {
                    SerializedProperty entry = delegates.GetArrayElementAtIndex(i);
                    SerializedProperty callback = entry.FindPropertyRelative("callback");
                    if (callback != null)
                    {
                        SerializedProperty persistentCalls = callback.FindPropertyRelative("m_PersistentCalls.m_Calls");
                        if (persistentCalls != null)
                        {
                            for (int j = 0; j < persistentCalls.arraySize; j++)
                            {
                                SerializedProperty call = persistentCalls.GetArrayElementAtIndex(j);
                                SerializedProperty methodName = call.FindPropertyRelative("m_MethodName");
                                if (methodName != null && methodName.stringValue == "ActiveTab")
                                {
                                    SerializedProperty intArg = call.FindPropertyRelative("m_Arguments.m_IntArgument");
                                    if (intArg != null)
                                    {
                                        intArg.intValue = correctIndex;
                                        Debug.Log($"  ✓ Configured EventTrigger on '{btnObj.name}' to call ActiveTab({correctIndex})");
                                    }
                                }
                            }
                        }
                    }
                }
            }
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(trigger);
        }
    }

    [MenuItem("Tools/PRU213 Setup/🛡️ Setup Level 12 (15-20 Zombies)", priority = 30)]
    public static void SetupLevel12()
    {
        Debug.Log("=== PRU213 Setup: Setting up Level 12 ===");

        // 1. Open Level12 scene
        string scenePath = "Assets/Scenes/Level12.unity";
        UnityEditor.SceneManagement.SceneSetup[] originalSetup = null;
        if (System.IO.File.Exists(scenePath))
        {
            originalSetup = UnityEditor.SceneManagement.EditorSceneManager.GetSceneManagerSetup();
            var activeScene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
            if (!activeScene.IsValid())
            {
                Debug.LogError($"❌ Could not open scene {scenePath}");
                return;
            }
        }
        else
        {
            Debug.LogError($"❌ Scene not found: {scenePath}");
            return;
        }

        // 2. Perform standard setup (Camera, Player, GameManager, LevelManager, HUD)
        SetupCurrentScene();

        // 3. Find or Create SpawnPoint in the scene
        GameObject spawnPointObj = GameObject.Find("SpawnPoint");
        if (spawnPointObj == null) spawnPointObj = GameObject.Find("PlayerSpawnPoint");
        if (spawnPointObj == null)
        {
            spawnPointObj = new GameObject("SpawnPoint");
            spawnPointObj.transform.position = new Vector3(0f, -2f, 0f);
            Debug.Log("  ✓ Created 'SpawnPoint' at (0, -2, 0) for Player spawn");
        }
        Vector3 playerPos = spawnPointObj.transform.position;

        // 4. Find all existing enemies in the scene by Tag or Name to track (and auto-configure components)
        List<GameObject> existingEnemies = new List<GameObject>();
        
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
        foreach (var go in allObjects)
        {
            if (go == null) continue;
            bool isEnemy = go.CompareTag("Enemy") || 
                           go.name.ToUpper().Contains("ZOMBIE") || 
                           go.name.ToUpper().Contains("ENEMY");
            
            // Make sure it is an instantiated scene object and not a prefab asset
            if (isEnemy && go.scene.name != null && !existingEnemies.Contains(go))
            {
                existingEnemies.Add(go);
                
                // Auto-configure components if missing
                Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
                if (rb == null)
                {
                    rb = go.AddComponent<Rigidbody2D>();
                    rb.gravityScale = 0f;
                    rb.freezeRotation = true;
                }
                
                EnemyChase chase = go.GetComponent<EnemyChase>();
                if (chase == null)
                {
                    chase = go.AddComponent<EnemyChase>();
                }
                SerializedObject chaseSO = new SerializedObject(chase);
                chaseSO.FindProperty("detectionRadius").floatValue = 5f;
                chaseSO.ApplyModifiedProperties();
                
                EnemyHealth health = go.GetComponent<EnemyHealth>();
                if (health == null)
                {
                    health = go.AddComponent<EnemyHealth>();
                }

                // Make sure tag is set to Enemy
                if (go.tag != "Enemy")
                {
                    go.tag = "Enemy";
                }
            }
        }

        Debug.Log($"  ✓ Found and configured {existingEnemies.Count} existing Zombies/Enemies in the scene to track");

        // 6. Find or Create CombatRoom GameObject
        GameObject combatRoomObj = GameObject.Find("CombatRoom_Level12");
        if (combatRoomObj == null)
        {
            combatRoomObj = new GameObject("CombatRoom_Level12");
            combatRoomObj.transform.position = playerPos;
        }

        // Configure BoxCollider2D (Is Trigger) on the CombatRoom object
        BoxCollider2D col = combatRoomObj.GetComponent<BoxCollider2D>();
        if (col == null)
        {
            col = combatRoomObj.AddComponent<BoxCollider2D>();
        }
        col.isTrigger = true;
        col.size = new Vector2(40f, 40f); // Large enough to cover the room

        // Configure CombatRoom script
        CombatRoom combatRoom = combatRoomObj.GetComponent<CombatRoom>();
        if (combatRoom == null)
        {
            combatRoom = combatRoomObj.AddComponent<CombatRoom>();
        }

        combatRoom.triggerOnPlayerEnter = true;
        combatRoom.keepPlayerInside = false; // No locking of entry doors
        combatRoom.enemyPrefab = null; // We are using pre-placed enemies
        
        // Assign preplaced enemies
        combatRoom.prePlacedEnemies = existingEnemies;

        // 7. Setup GatedDoor as the exit portal
        List<GameObject> exitGates = new List<GameObject>();
        
        // Ensure GatedDoor prefab exists
        string gatedDoorPrefabPath = "Assets/Prefabs/Environment/GatedDoor.prefab";
        GameObject gatedDoorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(gatedDoorPrefabPath);
        if (gatedDoorPrefab == null)
        {
            CreateGatedDoorPrefab();
            gatedDoorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(gatedDoorPrefabPath);
        }

        GameObject doorObj = GameObject.Find("GatedDoor");
        if (doorObj == null)
        {
            NextLevelPortal existingPortal = Object.FindAnyObjectByType<NextLevelPortal>();
            if (existingPortal != null)
            {
                doorObj = existingPortal.gameObject;
            }
        }
        if (doorObj == null)
        {
            if (gatedDoorPrefab != null)
            {
                doorObj = PrefabUtility.InstantiatePrefab(gatedDoorPrefab) as GameObject;
                doorObj.transform.position = playerPos + new Vector3(0f, 12f, 0f);
                doorObj.name = "GatedDoor";
                Debug.Log("  ✓ Instantiated GatedDoor prefab in Level12");
            }
        }

        if (doorObj != null)
        {
            combatRoom.exitPortal = doorObj;
            Debug.Log($"  ✓ Linked GatedDoor '{doorObj.name}' as the CombatRoom exitPortal");
        }
        else
        {
            Debug.LogWarning("⚠️ No GatedDoor prefab found or created to set up as exit!");
        }

        combatRoom.gates = null;

        // 8. Save Scene
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        Debug.Log("✅ Setup for Level 12 complete and saved!");

        // Restore original scene setup in editor if needed
        if (originalSetup != null)
        {
            UnityEditor.SceneManagement.EditorSceneManager.RestoreSceneManagerSetup(originalSetup);
        }
    }

    [MenuItem("Tools/PRU213 Setup/⚔️ Setup Level 13 (Zombies & FlyingEyes)", priority = 30)]
    public static void SetupLevel13()
    {
        Debug.Log("=== PRU213 Setup: Setting up Level 13 ===");

        // 1. Open Level13 scene
        string scenePath = "Assets/Scenes/Level13.unity";
        UnityEditor.SceneManagement.SceneSetup[] originalSetup = null;
        if (System.IO.File.Exists(scenePath))
        {
            originalSetup = UnityEditor.SceneManagement.EditorSceneManager.GetSceneManagerSetup();
            var activeScene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
            if (!activeScene.IsValid())
            {
                Debug.LogError($"❌ Could not open scene {scenePath}");
                return;
            }
        }
        else
        {
            Debug.LogError($"❌ Scene not found: {scenePath}");
            return;
        }

        // 2. Perform standard setup (Camera, Player, GameManager, LevelManager, HUD)
        SetupCurrentScene();

        // 3. Find or Create SpawnPoint in the scene
        GameObject spawnPointObj = GameObject.Find("SpawnPoint");
        if (spawnPointObj == null) spawnPointObj = GameObject.Find("PlayerSpawnPoint");
        if (spawnPointObj == null)
        {
            spawnPointObj = new GameObject("SpawnPoint");
            spawnPointObj.transform.position = new Vector3(0f, -2f, 0f);
            Debug.Log("  ✓ Created 'SpawnPoint' at (0, -2, 0) for Player spawn");
        }
        Vector3 playerPos = spawnPointObj.transform.position;

        // 4. Clean up previously auto-spawned enemies to allow clean regeneration
        GameObject[] initialObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
        foreach (var go in initialObjects)
        {
            if (go != null && go.name.Contains("_Spawned_"))
            {
                Object.DestroyImmediate(go);
            }
        }

        // 5. Find all remaining existing enemies in the scene to track (and auto-configure components)
        List<GameObject> existingEnemies = new List<GameObject>();
        Transform enemiesHolder = GetOrCreateEnemiesHolder();
        
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
        foreach (var go in allObjects)
        {
            if (go == null) continue;
            bool isEnemy = go.CompareTag("Enemy") || 
                           go.name.ToUpper().Contains("ZOMBIE") || 
                           go.name.ToUpper().Contains("EYEBAT") || 
                           go.name.ToUpper().Contains("ENEMY");
            
            // Make sure it is an instantiated scene object and not a prefab asset
            if (isEnemy && go.scene.name != null && !existingEnemies.Contains(go))
            {
                existingEnemies.Add(go);
                go.transform.SetParent(enemiesHolder);
                
                // Auto-configure components if missing
                Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
                if (rb == null)
                {
                    rb = go.AddComponent<Rigidbody2D>();
                }
                rb.gravityScale = 0f;
                rb.freezeRotation = true;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

                // Ensure Collider2D exists and is NOT Trigger so enemies collide with walls
                Collider2D enemyCol = go.GetComponent<Collider2D>();
                if (enemyCol == null)
                {
                    BoxCollider2D boxCol = go.AddComponent<BoxCollider2D>();
                    boxCol.isTrigger = false;
                    boxCol.size = go.name.Contains("EyeBat") ? new Vector2(0.25f, 0.25f) : new Vector2(0.15f, 0.17f);
                }
                else
                {
                    enemyCol.isTrigger = false;
                }
                
                EnemyChase chase = go.GetComponent<EnemyChase>();
                if (chase == null)
                {
                    chase = go.AddComponent<EnemyChase>();
                }
                SerializedObject chaseSO = new SerializedObject(chase);
                chaseSO.FindProperty("speed").floatValue = go.name.Contains("EyeBat") ? 4f : 3f;
                chaseSO.FindProperty("detectionRadius").floatValue = 5f;
                chaseSO.ApplyModifiedProperties();
                
                EnemyHealth health = go.GetComponent<EnemyHealth>();
                if (health == null)
                {
                    health = go.AddComponent<EnemyHealth>();
                }

                // Make sure tag is set to Enemy
                if (go.tag != "Enemy")
                {
                    go.tag = "Enemy";
                }
            }
        }

        // 6. If no enemies exist in the scene, let's instantiate 6 Zombies and 6 EyeBats around the spawn area
        if (existingEnemies.Count == 0)
        {
            GameObject zombiePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Zombie.prefab");
            GameObject eyeBatPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/EyeBat.prefab");

            Vector3[] spawnOffsets = {
                new Vector3(-4f, 2f, 0f),
                new Vector3(4f, 2f, 0f),
                new Vector3(-2f, 4f, 0f),
                new Vector3(2f, 4f, 0f),
                new Vector3(-6f, -1f, 0f),
                new Vector3(6f, -1f, 0f),
                new Vector3(-5f, 5f, 0f),
                new Vector3(5f, 5f, 0f),
                new Vector3(-3f, 3f, 0f),
                new Vector3(3f, 3f, 0f),
            };

            for (int i = 0; i < spawnOffsets.Length; i++)
            {
                GameObject enemyPrefab = (i % 2 == 0) ? zombiePrefab : eyeBatPrefab;
                if (enemyPrefab != null)
                {
                    GameObject enemyObj = PrefabUtility.InstantiatePrefab(enemyPrefab) as GameObject;
                    enemyObj.transform.position = spawnPointObj.transform.position + spawnOffsets[i];
                    enemyObj.transform.SetParent(enemiesHolder);
                    enemyObj.name = enemyPrefab.name + "_Spawned_" + i;
                    existingEnemies.Add(enemyObj);
                }
            }
            Debug.Log($"  ✓ Spawned {existingEnemies.Count} initial enemies (Zombies & EyeBats) in Level13");
        }
        else
        {
            Debug.Log($"  ✓ Found and configured {existingEnemies.Count} existing Zombies/Enemies in the scene to track");
        }

        // 6. Find or Create CombatRoom GameObject
        GameObject combatRoomObj = GameObject.Find("CombatRoom_Level13");
        if (combatRoomObj == null)
        {
            combatRoomObj = new GameObject("CombatRoom_Level13");
            combatRoomObj.transform.position = playerPos;
        }

        // Configure BoxCollider2D (Is Trigger) on the CombatRoom object
        BoxCollider2D col = combatRoomObj.GetComponent<BoxCollider2D>();
        if (col == null)
        {
            col = combatRoomObj.AddComponent<BoxCollider2D>();
        }
        col.isTrigger = true;
        col.size = new Vector2(40f, 40f); // Large enough to cover the room

        // Configure CombatRoom script
        CombatRoom combatRoom = combatRoomObj.GetComponent<CombatRoom>();
        if (combatRoom == null)
        {
            combatRoom = combatRoomObj.AddComponent<CombatRoom>();
        }

        combatRoom.triggerOnPlayerEnter = true;
        combatRoom.keepPlayerInside = false; // No locking of entry doors
        combatRoom.enemyPrefab = null; // We are using pre-placed enemies
        
        // Assign preplaced enemies
        combatRoom.prePlacedEnemies = existingEnemies;

        // 7. Setup GatedDoor as the exit portal
        string gatedDoorPrefabPath = "Assets/Prefabs/Environment/GatedDoor.prefab";
        GameObject gatedDoorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(gatedDoorPrefabPath);
        if (gatedDoorPrefab == null)
        {
            CreateGatedDoorPrefab();
            gatedDoorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(gatedDoorPrefabPath);
        }

        GameObject doorObj = GameObject.Find("GatedDoor");
        if (doorObj == null)
        {
            NextLevelPortal existingPortal = Object.FindAnyObjectByType<NextLevelPortal>();
            if (existingPortal != null)
            {
                doorObj = existingPortal.gameObject;
            }
        }
        if (doorObj == null)
        {
            if (gatedDoorPrefab != null)
            {
                doorObj = PrefabUtility.InstantiatePrefab(gatedDoorPrefab) as GameObject;
                doorObj.transform.position = playerPos + new Vector3(0f, 12f, 0f);
                doorObj.name = "GatedDoor";
                Debug.Log("  ✓ Instantiated GatedDoor prefab in Level13");
            }
        }

        if (doorObj != null)
        {
            combatRoom.exitPortal = doorObj;
            Debug.Log($"  ✓ Linked GatedDoor '{doorObj.name}' as the CombatRoom exitPortal");
        }
        else
        {
            Debug.LogWarning("⚠️ No GatedDoor prefab found or created to set up as exit!");
        }

        combatRoom.gates = null;

        // 8. Save Scene
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        Debug.Log("✅ Setup for Level 13 complete and saved!");

        // Restore original scene setup in editor if needed
        if (originalSetup != null)
        {
            UnityEditor.SceneManagement.EditorSceneManager.RestoreSceneManagerSetup(originalSetup);
        }
    }

    [MenuItem("Tools/PRU213 Setup/👑 Setup Level 14 & Boss (Crystal Knight)", priority = 31)]
    public static void SetupLevel14()
    {
        Debug.Log("=== PRU213 Setup: Setting up Level 14 (Boss Room) ===");

        // 1. Open Level14 scene
        string scenePath = "Assets/Scenes/Level14.unity";
        UnityEditor.SceneManagement.SceneSetup[] originalSetup = null;
        if (System.IO.File.Exists(scenePath))
        {
            originalSetup = UnityEditor.SceneManagement.EditorSceneManager.GetSceneManagerSetup();
            var activeScene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
            if (!activeScene.IsValid())
            {
                Debug.LogError($"❌ Could not open scene {scenePath}");
                return;
            }
        }
        else
        {
            Debug.LogError($"❌ Scene not found: {scenePath}");
            return;
        }

        // 2. Perform standard setup (Camera, Player, GameManager, LevelManager, HUD)
        SetupCurrentScene();

        // 3. Find or Create SpawnPoint in the scene
        GameObject spawnPointObj = GameObject.Find("SpawnPoint");
        if (spawnPointObj == null) spawnPointObj = GameObject.Find("PlayerSpawnPoint");
        if (spawnPointObj == null)
        {
            spawnPointObj = new GameObject("SpawnPoint");
            spawnPointObj.transform.position = new Vector3(0f, -4f, 0f);
            Debug.Log("  ✓ Created 'SpawnPoint' at (0, -4, 0) for Player spawn in Level14");
        }
        Vector3 playerPos = spawnPointObj.transform.position;

        // 4. Ensure Boss Prefab exists
        GameObject bossPrefab = CreateBossCrystalKnightPrefab();

        // 5. Instantiate or Find Boss in scene
        BossCrystalKnight existingBoss = Object.FindAnyObjectByType<BossCrystalKnight>();
        GameObject bossObj = null;
        if (existingBoss != null)
        {
            bossObj = existingBoss.gameObject;
        }
        else if (bossPrefab != null)
        {
            bossObj = PrefabUtility.InstantiatePrefab(bossPrefab) as GameObject;
            bossObj.transform.position = playerPos + new Vector3(0f, 8f, 0f);
            bossObj.name = "BossCrystalKnight";
            Debug.Log("  ✓ Instantiated Boss Crystal Knight in Level14");
        }

        // 6. Find or Create CombatRoom GameObject for Level14 (Preserve position & collider if existing)
        GameObject combatRoomObj = GameObject.Find("CombatRoom_Level14");
        bool isNewRoom = false;
        if (combatRoomObj == null)
        {
            combatRoomObj = new GameObject("CombatRoom_Level14");
            combatRoomObj.transform.position = playerPos;
            isNewRoom = true;
        }

        BoxCollider2D col = combatRoomObj.GetComponent<BoxCollider2D>();
        if (col == null)
        {
            col = combatRoomObj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(40f, 40f);
        }
        else if (isNewRoom)
        {
            col.isTrigger = true;
            col.size = new Vector2(40f, 40f);
        }

        CombatRoom combatRoom = combatRoomObj.GetComponent<CombatRoom>();
        if (combatRoom == null)
        {
            combatRoom = combatRoomObj.AddComponent<CombatRoom>();
        }

        combatRoom.triggerOnPlayerEnter = true;
        combatRoom.keepPlayerInside = false;
        if (bossObj != null)
        {
            combatRoom.prePlacedEnemies = new List<GameObject> { bossObj };
        }

        // 7. Setup GatedDoor as exit (Preserve position if existing)
        string gatedDoorPrefabPath = "Assets/Prefabs/Environment/GatedDoor.prefab";
        GameObject gatedDoorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(gatedDoorPrefabPath);
        GameObject doorObj = GameObject.Find("GatedDoor");
        if (doorObj == null)
        {
            NextLevelPortal existingPortal = Object.FindAnyObjectByType<NextLevelPortal>();
            if (existingPortal != null) doorObj = existingPortal.gameObject;
        }
        if (doorObj == null && gatedDoorPrefab != null)
        {
            doorObj = PrefabUtility.InstantiatePrefab(gatedDoorPrefab) as GameObject;
            doorObj.transform.position = playerPos + new Vector3(0f, 15f, 0f);
            doorObj.name = "GatedDoor";
            Debug.Log("  ✓ Instantiated GatedDoor prefab in Level14");
        }
        else if (doorObj != null)
        {
            Debug.Log($"  ✓ Preserved existing GatedDoor position at {doorObj.transform.position}");
        }

        if (doorObj != null)
        {
            combatRoom.exitPortal = doorObj;
        }

        // 8. Save Scene
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        Debug.Log("✅ Setup for Level 14 (Boss Room) complete and saved!");

        if (originalSetup != null)
        {
            UnityEditor.SceneManagement.EditorSceneManager.RestoreSceneManagerSetup(originalSetup);
        }
    }

    private static BossHUD CreateBossHUDInCanvas(GameObject canvas)
    {
        // Check if BossHUD panel already exists
        Transform existingPanel = canvas.transform.Find("BossHUD_Panel");
        if (existingPanel != null)
        {
            Object.DestroyImmediate(existingPanel.gameObject);
        }

        GameObject bossPanel = new GameObject("BossHUD_Panel");
        bossPanel.transform.SetParent(canvas.transform, false);
        RectTransform bossRect = bossPanel.AddComponent<RectTransform>();
        bossRect.anchorMin = new Vector2(0.5f, 0f);
        bossRect.anchorMax = new Vector2(0.5f, 0f);
        bossRect.pivot = new Vector2(0.5f, 0f);
        bossRect.anchoredPosition = new Vector2(0f, 30f);
        bossRect.sizeDelta = new Vector2(650f, 65f);

        Image bossBg = bossPanel.AddComponent<Image>();
        bossBg.color = new Color(0.15f, 0.12f, 0.10f, 0.9f);

        Outline bossOutline = bossPanel.AddComponent<Outline>();
        bossOutline.effectColor = new Color(0.85f, 0.7f, 0.2f, 1f);
        bossOutline.effectDistance = new Vector2(3, 3);

        GameObject nameObj = CreateTMPText(bossPanel.transform, "BossNameText", "CRYSTAL KNIGHT",
            new Vector2(0, -10), TextAlignmentOptions.Center, 18, new Color(1f, 0.85f, 0.3f));
        RectTransform nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(1, 1);
        nameRect.pivot = new Vector2(0.5f, 1);
        nameRect.sizeDelta = new Vector2(0, 25);

        GameObject sliderObj = new GameObject("BossHealthSlider");
        sliderObj.transform.SetParent(bossPanel.transform, false);
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.05f, 0f);
        sliderRect.anchorMax = new Vector2(0.95f, 0f);
        sliderRect.pivot = new Vector2(0.5f, 0f);
        sliderRect.anchoredPosition = new Vector2(0f, 10f);
        sliderRect.sizeDelta = new Vector2(0, 24);

        UnityEngine.UI.Slider slider = sliderObj.AddComponent<UnityEngine.UI.Slider>();

        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(sliderObj.transform, false);
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.05f, 0.05f, 1f);

        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.sizeDelta = Vector2.zero;

        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform, false);
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        Image fillImg = fillObj.AddComponent<Image>();
        fillImg.color = new Color(0.9f, 0.15f, 0.15f, 1f);

        slider.fillRect = fillRect;

        GameObject hpTextObj = CreateTMPText(sliderObj.transform, "BossHPText", "500 / 500",
            Vector2.zero, TextAlignmentOptions.Center, 14, Color.white);
        RectTransform hpTextRect = hpTextObj.GetComponent<RectTransform>();
        hpTextRect.anchorMin = Vector2.zero;
        hpTextRect.anchorMax = Vector2.one;
        hpTextRect.sizeDelta = Vector2.zero;

        BossHUD bossHUD = canvas.GetComponent<BossHUD>();
        if (bossHUD == null) bossHUD = canvas.AddComponent<BossHUD>();

        bossHUD.BindReferences(bossPanel, nameObj.GetComponent<TextMeshProUGUI>(), slider, hpTextObj.GetComponent<TextMeshProUGUI>());
        return bossHUD;
    }

    [MenuItem("Tools/PRU213 Setup/👑 Force Generate Boss Animator & Prefab", priority = 28)]
    public static void ForceGenerateBossAnimatorAndPrefab()
    {
        Debug.Log("=== PRU213 Setup: Generating Boss Animator & Prefab ===");
        CreateBossCrystalKnightPrefab();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("✅ Assets/Animations/Boss folder and BossCrystalKnight prefab generated!");
    }

    private static GameObject CreateBossCrystalKnightPrefab()
    {
        string path = "Assets/Prefabs/Enemies/BossCrystalKnight.prefab";
        
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Enemies"))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            AssetDatabase.CreateFolder("Assets/Prefabs", "Enemies");
        }

        RuntimeAnimatorController bossController = CreateBossAnimatorController();

        GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existingPrefab != null)
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
            Animator exAnim = prefabRoot.GetComponent<Animator>();
            if (exAnim == null) exAnim = prefabRoot.AddComponent<Animator>();
            exAnim.runtimeAnimatorController = bossController;
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
            Debug.Log("  ✓ Updated existing Boss Crystal Knight prefab with new AnimatorController at " + path);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        GameObject bossObj = new GameObject("BossCrystalKnight");
        bossObj.tag = "Enemy";

        SpriteRenderer sr = bossObj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 4;

        Object[] sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Sprites/GameAssets/Boss Sprite/Crystal Knight.png");
        foreach (Object s in sprites)
        {
            if (s is Sprite sprite && s.name == "Crystal Knight_0")
            {
                sr.sprite = sprite;
                break;
            }
        }

        Rigidbody2D rb = bossObj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        BoxCollider2D boxCol = bossObj.AddComponent<BoxCollider2D>();
        boxCol.isTrigger = false;
        boxCol.size = new Vector2(0.6f, 0.6f);

        bossObj.transform.localScale = new Vector3(4f, 4f, 1f);

        Animator anim = bossObj.AddComponent<Animator>();
        anim.runtimeAnimatorController = CreateBossAnimatorController();

        bossObj.AddComponent<BossCrystalKnight>();
        bossObj.AddComponent<EnemyHealth>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(bossObj, path);
        Object.DestroyImmediate(bossObj);
        Debug.Log("  ✓ Boss Crystal Knight prefab created at " + path);
        return prefab;
    }

    private static RuntimeAnimatorController CreateBossAnimatorController()
    {
        string animFolder = "Assets/Animations/Boss";
        if (!AssetDatabase.IsValidFolder("Assets/Animations"))
            AssetDatabase.CreateFolder("Assets", "Animations");
        if (!AssetDatabase.IsValidFolder(animFolder))
            AssetDatabase.CreateFolder("Assets/Animations", "Boss");

        var binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };

        // 1. Idle Clip
        Sprite[] idleSprites = LoadAllSpritesFromAsset("Assets/Art/Sprites/GameAssets/Boss Sprite/Crystal Knight idle.png");
        AnimationClip idleClip = new AnimationClip { frameRate = 6f };
        if (idleSprites.Length > 0)
        {
            var keys = new ObjectReferenceKeyframe[idleSprites.Length];
            for (int i = 0; i < idleSprites.Length; i++) keys[i] = new ObjectReferenceKeyframe { time = i / 6f, value = idleSprites[i] };
            AnimationUtility.SetObjectReferenceCurve(idleClip, binding, keys);
            var settings = AnimationUtility.GetAnimationClipSettings(idleClip); settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(idleClip, settings);
        }
        string idlePath = animFolder + "/Boss_Idle.anim";
        if (AssetDatabase.LoadAssetAtPath<AnimationClip>(idlePath) != null) AssetDatabase.DeleteAsset(idlePath);
        AssetDatabase.CreateAsset(idleClip, idlePath);

        // 2. Attack Clip
        Sprite[] attackSprites = LoadAllSpritesFromAsset("Assets/Art/Sprites/GameAssets/Boss Sprite/Crystal Knight attack.png");
        AnimationClip attackClip = new AnimationClip { frameRate = 8f };
        if (attackSprites.Length > 0)
        {
            var keys = new ObjectReferenceKeyframe[attackSprites.Length];
            for (int i = 0; i < attackSprites.Length; i++) keys[i] = new ObjectReferenceKeyframe { time = i / 8f, value = attackSprites[i] };
            AnimationUtility.SetObjectReferenceCurve(attackClip, binding, keys);
            var settings = AnimationUtility.GetAnimationClipSettings(attackClip); settings.loopTime = false;
            AnimationUtility.SetAnimationClipSettings(attackClip, settings);
        }
        string attackPath = animFolder + "/Boss_Attack.anim";
        if (AssetDatabase.LoadAssetAtPath<AnimationClip>(attackPath) != null) AssetDatabase.DeleteAsset(attackPath);
        AssetDatabase.CreateAsset(attackClip, attackPath);

        // 3. Teleport Clip (Out + In sequence)
        List<Sprite> teleFrames = new List<Sprite>();
        Sprite[] teleSprites = LoadAllSpritesFromAsset("Assets/Art/Sprites/GameAssets/Boss Sprite/Crystal Knight Tele.png");
        if (teleSprites.Length > 0)
        {
            teleFrames.AddRange(teleSprites);
            for (int i = teleSprites.Length - 1; i >= 0; i--)
            {
                teleFrames.Add(teleSprites[i]);
            }
        }

        AnimationClip teleClip = new AnimationClip { frameRate = 10f };
        if (teleFrames.Count > 0)
        {
            var keys = new ObjectReferenceKeyframe[teleFrames.Count];
            for (int i = 0; i < teleFrames.Count; i++) keys[i] = new ObjectReferenceKeyframe { time = i / 10f, value = teleFrames[i] };
            AnimationUtility.SetObjectReferenceCurve(teleClip, binding, keys);
            var settings = AnimationUtility.GetAnimationClipSettings(teleClip); settings.loopTime = false;
            AnimationUtility.SetAnimationClipSettings(teleClip, settings);
        }
        string telePath = animFolder + "/Boss_Teleport.anim";
        if (AssetDatabase.LoadAssetAtPath<AnimationClip>(telePath) != null) AssetDatabase.DeleteAsset(telePath);
        AssetDatabase.CreateAsset(teleClip, telePath);

        // 4. Death Clip
        List<Sprite> deathFrames = new List<Sprite>();
        Sprite[] dPose = LoadAllSpritesFromAsset("Assets/Art/Sprites/GameAssets/Boss Sprite/Crystal Knight death.png");
        if (dPose.Length > 0) deathFrames.Add(dPose[0]);
        deathFrames.AddRange(LoadAllSpritesFromAsset("Assets/Art/Sprites/GameAssets/Boss Sprite/Crystal Knight explosion 1 .png"));
        deathFrames.AddRange(LoadAllSpritesFromAsset("Assets/Art/Sprites/GameAssets/Boss Sprite/Crystal Knight explosion 2 .png"));
        deathFrames.AddRange(LoadAllSpritesFromAsset("Assets/Art/Sprites/GameAssets/Boss Sprite/Crystal Knight explosion 3 .png"));

        AnimationClip deathClip = new AnimationClip { frameRate = 6f };
        if (deathFrames.Count > 0)
        {
            var keys = new ObjectReferenceKeyframe[deathFrames.Count];
            for (int i = 0; i < deathFrames.Count; i++) keys[i] = new ObjectReferenceKeyframe { time = i / 6f, value = deathFrames[i] };
            AnimationUtility.SetObjectReferenceCurve(deathClip, binding, keys);
            var settings = AnimationUtility.GetAnimationClipSettings(deathClip); settings.loopTime = false;
            AnimationUtility.SetAnimationClipSettings(deathClip, settings);
        }
        string deathPath = animFolder + "/Boss_Death.anim";
        if (AssetDatabase.LoadAssetAtPath<AnimationClip>(deathPath) != null) AssetDatabase.DeleteAsset(deathPath);
        AssetDatabase.CreateAsset(deathClip, deathPath);

        // 5. Animator Controller
        string controllerPath = animFolder + "/BossAnimator.controller";
        var controller = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(controllerPath);
        if (controller == null)
        {
            controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        }

        while (controller.parameters.Length > 0) controller.RemoveParameter(0);
        var sm = controller.layers[0].stateMachine;
        foreach (var t in sm.anyStateTransitions.ToList()) sm.RemoveAnyStateTransition(t);
        foreach (var cs in sm.states.ToList()) sm.RemoveState(cs.state);

        controller.AddParameter("IsMoving", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Teleport", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Die", AnimatorControllerParameterType.Trigger);

        var sIdle = sm.AddState("Idle"); sIdle.motion = idleClip;
        var sRun = sm.AddState("Run"); sRun.motion = idleClip;
        var sAttack = sm.AddState("Attack"); sAttack.motion = attackClip;
        var sTele = sm.AddState("Teleport"); sTele.motion = teleClip;
        var sDeath = sm.AddState("Death"); sDeath.motion = deathClip;
        sm.defaultState = sIdle;

        var toRun = sIdle.AddTransition(sRun); toRun.hasExitTime = false; toRun.duration = 0f; toRun.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0f, "IsMoving");
        var toIdle = sRun.AddTransition(sIdle); toIdle.hasExitTime = false; toIdle.duration = 0f; toIdle.AddCondition(UnityEditor.Animations.AnimatorConditionMode.IfNot, 0f, "IsMoving");

        var toAttack = sm.AddAnyStateTransition(sAttack); toAttack.hasExitTime = false; toAttack.duration = 0f; toAttack.canTransitionToSelf = false; toAttack.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0f, "Attack");
        var attackBack = sAttack.AddTransition(sIdle); attackBack.hasExitTime = true; attackBack.exitTime = 0.9f; attackBack.duration = 0f;

        var toTele = sm.AddAnyStateTransition(sTele); toTele.hasExitTime = false; toTele.duration = 0f; toTele.canTransitionToSelf = false; toTele.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0f, "Teleport");
        var teleBack = sTele.AddTransition(sIdle); teleBack.hasExitTime = true; teleBack.exitTime = 0.9f; teleBack.duration = 0f;

        var toDeath = sm.AddAnyStateTransition(sDeath); toDeath.hasExitTime = false; toDeath.duration = 0f; toDeath.canTransitionToSelf = false; toDeath.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0f, "Die");

        EditorUtility.SetDirty(controller);
        return controller;
    }

    private static Sprite[] LoadAllSpritesFromAsset(string path)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
        List<Sprite> list = new List<Sprite>();
        foreach (Object a in assets)
        {
            if (a is Sprite s) list.Add(s);
        }
        return list.ToArray();
    }

    [MenuItem("Tools/PRU213 Setup/🔧 Configure Zombie Prefab", priority = 31)]
    public static void ConfigureZombiePrefab()
    {
        string path = "Assets/Prefabs/Enemies/Zombie.prefab";
        GameObject zombiePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (zombiePrefab == null)
        {
            Debug.LogError($"❌ Zombie prefab not found at {path}");
            return;
        }

        // Open the prefab contents
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
        
        // 1. Set localScale to (5, 5, 1)
        prefabRoot.transform.localScale = new Vector3(5f, 5f, 1f);

        // 2. Set BoxCollider2D size to (0.15, 0.17)
        BoxCollider2D boxCol = prefabRoot.GetComponent<BoxCollider2D>();
        if (boxCol == null)
        {
            boxCol = prefabRoot.AddComponent<BoxCollider2D>();
        }
        boxCol.size = new Vector2(0.15f, 0.17f);

        // Save the updated prefab
        PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        Debug.Log("✅ Zombie prefab successfully updated: Scale set to (5, 5, 1) and BoxCollider2D size set to (0.15, 0.17)!");
    }

    [MenuItem("Tools/PRU213 Setup/🦇 Configure EyeBat Prefab", priority = 31)]
    public static void ConfigureEyeBatPrefab()
    {
        string path = "Assets/Prefabs/Enemies/EyeBat.prefab";
        GameObject eyeBatPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (eyeBatPrefab == null)
        {
            Debug.LogError($"❌ EyeBat prefab not found at {path}");
            return;
        }

        // Open the prefab contents
        GameObject prefabRoot = PrefabUtility.LoadPrefabContents(path);
        
        // 1. Set localScale to (5, 5, 1)
        prefabRoot.transform.localScale = new Vector3(5f, 5f, 1f);

        // 2. Set CircleCollider2D radius to 0.25f
        CircleCollider2D circleCol = prefabRoot.GetComponent<CircleCollider2D>();
        if (circleCol == null)
        {
            circleCol = prefabRoot.AddComponent<CircleCollider2D>();
        }
        circleCol.radius = 0.25f;

        // 3. Remove HealthBarCanvas child if it exists
        Transform canvasTransform = prefabRoot.transform.Find("HealthBarCanvas");
        if (canvasTransform != null)
        {
            Object.DestroyImmediate(canvasTransform.gameObject, true);
            Debug.Log("  ✓ Removed HealthBarCanvas from EyeBat prefab");
        }

        // Save the updated prefab
        PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
        PrefabUtility.UnloadPrefabContents(prefabRoot);

        Debug.Log("✅ EyeBat prefab successfully updated: Scale set to (5, 5, 1), CircleCollider2D radius set to 0.25f, and HealthBarCanvas removed!");
    }

    private static Transform GetOrCreateEnemiesHolder()
    {
        GameObject holder = GameObject.Find("Enemies");
        if (holder == null)
        {
            holder = new GameObject("Enemies");
            holder.transform.position = Vector3.zero;
        }
        return holder.transform;
    }

    [MenuItem("Tools/PRU213 Setup/🧟 Spawn 5 Zombies & 5 EyeBats", priority = 31)]
    public static void SpawnZombiesAndEyeBats()
    {
        Debug.Log("=== PRU213 Setup: Spawning 5 Zombies & 5 EyeBats ===");

        // 1. Find SpawnPoint or Player position as reference
        GameObject spawnPointObj = GameObject.Find("SpawnPoint");
        if (spawnPointObj == null) spawnPointObj = GameObject.Find("PlayerSpawnPoint");
        if (spawnPointObj == null) spawnPointObj = GameObject.FindWithTag("Player");
        
        Vector3 spawnRefPos = Vector3.zero;
        if (spawnPointObj != null)
        {
            spawnRefPos = spawnPointObj.transform.position;
        }
        else
        {
            Debug.LogWarning("⚠️ No SpawnPoint or Player found. Spawning at (0, 0, 0)");
        }

        // 2. Load Prefabs
        GameObject zombiePrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Zombie.prefab");
        GameObject eyeBatPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/EyeBat.prefab");

        if (zombiePrefab == null || eyeBatPrefab == null)
        {
            Debug.LogError("❌ Prefabs not found! Make sure to run 'Step 3 - Create All Prefabs' first.");
            return;
        }

        // 3. Get or Create parent holder to group enemies under
        Transform enemiesHolder = GetOrCreateEnemiesHolder();

        // 4. Define offsets for the 10 enemies
        Vector3[] spawnOffsets = {
            new Vector3(-5f, 3f, 0f),
            new Vector3(5f, 3f, 0f),
            new Vector3(-3f, 5f, 0f),
            new Vector3(3f, 5f, 0f),
            new Vector3(-7f, 0f, 0f),
            new Vector3(7f, 0f, 0f),
            new Vector3(-6f, 6f, 0f),
            new Vector3(6f, 6f, 0f),
            new Vector3(-4f, 4f, 0f),
            new Vector3(4f, 4f, 0f)
        };

        List<GameObject> newEnemies = new List<GameObject>();

        for (int i = 0; i < spawnOffsets.Length; i++)
        {
            GameObject enemyPrefab = (i % 2 == 0) ? zombiePrefab : eyeBatPrefab;
            GameObject enemyObj = PrefabUtility.InstantiatePrefab(enemyPrefab) as GameObject;
            enemyObj.transform.position = spawnRefPos + spawnOffsets[i];
            enemyObj.transform.SetParent(enemiesHolder);
            enemyObj.name = enemyPrefab.name + "_Spawned_" + System.Guid.NewGuid().ToString().Substring(0, 4) + "_" + i;
            
            // Auto-configure components
            Rigidbody2D rb = enemyObj.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = enemyObj.AddComponent<Rigidbody2D>();
            }
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Ensure Collider2D is NOT Trigger so enemies collide with walls
            Collider2D enemyCol = enemyObj.GetComponent<Collider2D>();
            if (enemyCol == null)
            {
                BoxCollider2D boxCol = enemyObj.AddComponent<BoxCollider2D>();
                boxCol.isTrigger = false;
                boxCol.size = enemyPrefab.name.Contains("EyeBat") ? new Vector2(0.25f, 0.25f) : new Vector2(0.15f, 0.17f);
            }
            else
            {
                enemyCol.isTrigger = false;
            }

            EnemyChase chase = enemyObj.GetComponent<EnemyChase>();
            if (chase == null) chase = enemyObj.AddComponent<EnemyChase>();
            SerializedObject chaseSO = new SerializedObject(chase);
            chaseSO.FindProperty("speed").floatValue = enemyPrefab.name.Contains("EyeBat") ? 4f : 3f;
            chaseSO.FindProperty("detectionRadius").floatValue = 5f;
            chaseSO.ApplyModifiedProperties();

            EnemyHealth health = enemyObj.GetComponent<EnemyHealth>();
            if (health == null) health = enemyObj.AddComponent<EnemyHealth>();

            enemyObj.tag = "Enemy";

            newEnemies.Add(enemyObj);
        }

        // 4. Find CombatRoom in the scene to add enemies to
        CombatRoom combatRoom = Object.FindAnyObjectByType<CombatRoom>();
        if (combatRoom != null)
        {
            if (combatRoom.prePlacedEnemies == null)
            {
                combatRoom.prePlacedEnemies = new List<GameObject>();
            }
            combatRoom.prePlacedEnemies.AddRange(newEnemies);
            EditorUtility.SetDirty(combatRoom);
            Debug.Log($"  ✓ Added 10 new spawned enemies to CombatRoom: '{combatRoom.gameObject.name}'");
        }
        else
        {
            Debug.LogWarning("⚠️ No CombatRoom found in the scene to link these enemies to. Please add them manually to your CombatRoom component.");
        }

        // 5. Save Scene changes
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("✅ Spawned 5 Zombies and 5 EyeBats successfully in the active scene!");
    }

    [MenuItem("Tools/PRU213 Setup/🚪 Create Gated Door Prefab", priority = 32)]
    public static void CreateGatedDoorPrefab()
    {
        Debug.Log("=== PRU213 Setup: Creating Gated Door Prefab ===");

        string prefabPath = "Assets/Prefabs/Environment/GatedDoor.prefab";
        string texturePath = "Assets/Art/Sprites/GameAssets/Pixel Crawler - Free Pack/Environment/Structures/Buildings/Props.png";

        // Create root GameObject
        GameObject doorRoot = new GameObject("GatedDoor");

        // Add BoxCollider2D (Is Trigger)
        BoxCollider2D boxCol = doorRoot.AddComponent<BoxCollider2D>();
        boxCol.isTrigger = true;
        boxCol.size = new Vector2(1.0f, 1.0f);
        boxCol.offset = new Vector2(0f, -0.4f);

        // Add NextLevelPortal script
        NextLevelPortal portal = doorRoot.AddComponent<NextLevelPortal>();
        
        // Find prompt text or add it
        GameObject textObj = new GameObject("PromptText");
        textObj.transform.SetParent(doorRoot.transform);
        textObj.transform.localPosition = new Vector3(0f, 2.2f, 0f);
        TMPro.TextMeshPro tmp = textObj.AddComponent<TMPro.TextMeshPro>();
        tmp.text = "Press ENTER to continue";
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.fontSize = 3f;
        MeshRenderer mr = tmp.GetComponent<MeshRenderer>();
        if (mr != null)
        {
            mr.sortingLayerName = "Player";
            mr.sortingOrder = 15;
        }

        // Add permanent solid side pillar colliders (keep player inside doorway)
        GameObject leftPillar = new GameObject("LeftPillarBarrier");
        leftPillar.transform.SetParent(doorRoot.transform);
        leftPillar.transform.localPosition = new Vector3(-0.85f, 0f, 0f);
        BoxCollider2D leftCol = leftPillar.AddComponent<BoxCollider2D>();
        leftCol.isTrigger = false;
        leftCol.size = new Vector2(0.4f, 2.6f);

        GameObject rightPillar = new GameObject("RightPillarBarrier");
        rightPillar.transform.SetParent(doorRoot.transform);
        rightPillar.transform.localPosition = new Vector3(0.85f, 0f, 0f);
        BoxCollider2D rightCol = rightPillar.AddComponent<BoxCollider2D>();
        rightCol.isTrigger = false;
        rightCol.size = new Vector2(0.4f, 2.6f);

        GameObject topArch = new GameObject("TopArchBarrier");
        topArch.transform.SetParent(doorRoot.transform);
        topArch.transform.localPosition = new Vector3(0f, 1.25f, 0f);
        BoxCollider2D topCol = topArch.AddComponent<BoxCollider2D>();
        topCol.isTrigger = false;
        topCol.size = new Vector2(2.1f, 0.4f);

        // Add solid doorway barrier (active when locked)
        GameObject barrierObj = new GameObject("SolidBarrier");
        barrierObj.transform.SetParent(doorRoot.transform);
        barrierObj.transform.localPosition = new Vector3(0f, -0.2f, 0f);
        BoxCollider2D barrierCol = barrierObj.AddComponent<BoxCollider2D>();
        barrierCol.isTrigger = false;
        barrierCol.size = new Vector2(1.3f, 1.5f);

        // Set promptText & solidBarrier fields
        SerializedObject so = new SerializedObject(portal);
        SerializedProperty promptTextProp = so.FindProperty("promptText");
        if (promptTextProp != null)
        {
            promptTextProp.objectReferenceValue = tmp;
        }
        SerializedProperty barrierProp = so.FindProperty("solidBarrier");
        if (barrierProp != null)
        {
            barrierProp.objectReferenceValue = barrierObj;
        }
        so.ApplyModifiedProperties();

        // 6 tiles configuration: Name, X, Y
        var tilesConfig = new (string spriteName, float x, float y)[] {
            ("Props_4", -0.5f, 1f),   ("Props_5", 0.5f, 1f),   // Top Row (Arch)
            ("Props_16", -0.5f, 0f),  ("Props_17", 0.5f, 0f),  // Mid Row
            ("Props_28", -0.5f, -1f), ("Props_29", 0.5f, -1f)  // Bot Row
        };

        foreach (var tile in tilesConfig)
        {
            GameObject child = new GameObject(tile.spriteName);
            child.transform.SetParent(doorRoot.transform);
            child.transform.localPosition = new Vector3(tile.x, tile.y, 0f);

            SpriteRenderer sr = child.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = "Player";
            
            // Arch tiles render above Player, lower tiles render below Player
            if (tile.spriteName == "Props_4" || tile.spriteName == "Props_5")
            {
                sr.sortingOrder = 5;
            }
            else
            {
                sr.sortingOrder = -1;
            }
            
            // Load sub sprite
            Sprite sprite = LoadSubSprite(texturePath, tile.spriteName);
            if (sprite != null)
            {
                sr.sprite = sprite;
            }
            else
            {
                Debug.LogWarning($"⚠️ Could not load sub-sprite '{tile.spriteName}' from {texturePath}");
            }
        }

        // Ensure directories exist
        System.IO.Directory.CreateDirectory("Assets/Prefabs/Environment");

        // Save Prefab
        PrefabUtility.SaveAsPrefabAsset(doorRoot, prefabPath);
        DestroyImmediate(doorRoot);

        Debug.Log($"✅ GatedDoor prefab successfully created and saved to {prefabPath}!");
    }

    private static Sprite LoadSubSprite(string texturePath, string spriteName)
    {
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
        foreach (Object asset in assets)
        {
            if (asset is Sprite && asset.name == spriteName)
            {
                return asset as Sprite;
            }
        }
        return null;
    }
}
