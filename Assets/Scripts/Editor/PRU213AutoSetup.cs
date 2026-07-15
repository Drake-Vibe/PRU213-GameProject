using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
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
            }
        }
        if (!taggedCamera)
        {
            Debug.LogWarning("⚠️ No Main Camera found in scene to tag as 'MainCamera'!");
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
        col.radius = 0.1f;

        // Bullet script
        Bullet bulletScript = bullet.AddComponent<Bullet>();
        bulletScript.speed = 12f;
        bulletScript.lifetime = 3f;

        // Tag & Layer
        bullet.tag = "Bullet";
        bullet.layer = LayerMask.NameToLayer("Bullet");

        // Scale
        bullet.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

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
        col.radius = 0.1f;

        Bullet bulletScript = bullet.AddComponent<Bullet>();
        bulletScript.speed = 8f;
        bulletScript.lifetime = 4f;

        bullet.tag = "EnemyBullet";
        bullet.layer = LayerMask.NameToLayer("EnemyBullet");
        bullet.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

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
        gunScript.damage = 5f;
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
        attackZone.transform.localPosition = new Vector3(0.4f, 0f, 0f);
        attackZone.tag = "Sword";

        BoxCollider2D atkCol = attackZone.AddComponent<BoxCollider2D>();
        atkCol.isTrigger = true;
        atkCol.size = new Vector2(0.8f, 0.5f);

        // Attach routing helper
        attackZone.AddComponent<MeleeAttackZone>();

        // Pickup collider on sword itself
        BoxCollider2D pickupCol = sword.AddComponent<BoxCollider2D>();
        pickupCol.isTrigger = true;
        pickupCol.size = new Vector2(0.5f, 0.5f);

        MeleeWeapon meleeScript = sword.AddComponent<MeleeWeapon>();
        meleeScript.damage = 8f;
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

        Debug.Log("  ✓ Soul Knight HUD & Potions Hotbar created successfully");
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
                GameObject targetObj = tmpText.gameObject;
                if (targetObj.GetComponent<UnityEngine.EventSystems.EventTrigger>() == null && targetObj.transform.parent != null)
                {
                    targetObj = targetObj.transform.parent.gameObject;
                }

                if (txt == "PLAYER") playerTabObj = targetObj;
                else if (txt == "IVENTORY" || txt == "INVENTORY") invTabObj = targetObj;
                else if (txt == "MAP") mapTabObj = targetObj;
                else if (txt == "SETTINGS") settingsTabObj = targetObj;
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
}
