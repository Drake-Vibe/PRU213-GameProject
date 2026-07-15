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

        // Ensure Prefabs folder exists
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Generated"))
            AssetDatabase.CreateFolder("Assets/Prefabs", "Generated");

        string prefabPath = "Assets/Prefabs/Generated";

        // Create prefabs
        CreateBulletPrefab(prefabPath);
        CreateEnemyBulletPrefab(prefabPath);
        CreateGunPrefab(prefabPath);
        CreateSwordPrefab(prefabPath);
        CreateMeleeEnemyPrefab(prefabPath);
        CreateRangedEnemyPrefab(prefabPath);
        CreateRoomGatePrefab(prefabPath);
        CreateNextLevelPortalPrefab(prefabPath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("✅ All prefabs created in: " + prefabPath);
    }

    // ========================================================
    // STEP 4: Setup Current Scene
    // ========================================================
    [MenuItem("Tools/PRU213 Setup/Step 4 - Setup Current Scene", priority = 13)]
    public static void SetupCurrentScene()
    {
        Debug.Log("=== PRU213 Setup: Setting up current scene ===");

        SetupPlayerInScene();
        CreateGameManagerInScene();
        CreateLevelManagerInScene();
        CreateHUDInScene();
        PlaceStarterWeaponInScene();

        // Mark scene dirty so all UI and player references are saved
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("✅ Scene setup complete!");
    }

    // ========================================================
    // PREFAB CREATION METHODS
    // ========================================================

    private static void CreateBulletPrefab(string path)
    {
        if (AssetExists(path + "/Bullet.prefab")) return;

        GameObject bullet = new GameObject("Bullet");

        // SpriteRenderer
        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.color = new Color(1f, 0.87f, 0.27f, 1f); // Yellow
        sr.sortingOrder = 5;
        // Try to find a small sprite
        sr.sprite = FindSpriteAsset("flask_blue") ?? FindSpriteAsset("coin_anim_f0");

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

        SavePrefab(bullet, path + "/Bullet.prefab");
        Debug.Log("  ✓ Bullet prefab created");
    }

    private static void CreateEnemyBulletPrefab(string path)
    {
        if (AssetExists(path + "/EnemyBullet.prefab")) return;

        GameObject bullet = new GameObject("EnemyBullet");

        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.color = new Color(1f, 0.27f, 0.27f, 1f); // Red
        sr.sortingOrder = 5;
        sr.sprite = FindSpriteAsset("flask_red") ?? FindSpriteAsset("coin_anim_f0");

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

        SavePrefab(bullet, path + "/EnemyBullet.prefab");
        Debug.Log("  ✓ EnemyBullet prefab created");
    }

    private static void CreateGunPrefab(string path)
    {
        GameObject gun = new GameObject("Gun");

        SpriteRenderer sr = gun.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 3;
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Game Assets/Ninja Adventure - Asset Pack/Items/Weapons/MagicWand/Sprite.png");

        BoxCollider2D col = gun.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(0.5f, 0.5f);

        Gun gunScript = gun.AddComponent<Gun>();
        gunScript.damage = 5f;
        gunScript.weaponName = "Basic Gun";

        // Link bullet prefab
        GameObject bulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Generated/Bullet.prefab");
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
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Game Assets/Ninja Adventure - Asset Pack/Items/Weapons/Sword/Sprite.png");

        // Create AttackZone child
        GameObject attackZone = new GameObject("AttackZone");
        attackZone.transform.SetParent(sword.transform);
        attackZone.transform.localPosition = new Vector3(0.4f, 0f, 0f);
        attackZone.tag = "Sword";

        BoxCollider2D atkCol = attackZone.AddComponent<BoxCollider2D>();
        atkCol.isTrigger = true;
        atkCol.size = new Vector2(0.8f, 0.5f);

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
        GameObject enemyBulletPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Generated/EnemyBullet.prefab");
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
        if (Object.FindAnyObjectByType<LevelManager>() != null)
        {
            Debug.Log("  ✓ LevelManager already exists");
            return;
        }

        GameObject lm = new GameObject("LevelManager");
        lm.AddComponent<LevelManager>();
        Debug.Log("  ✓ LevelManager created");
    }

    private static void CreateHUDInScene()
    {
        // Check if HUD already exists
        if (Object.FindAnyObjectByType<GameHUD>() != null)
        {
            Debug.Log("  ✓ GameHUD already exists");
            return;
        }

        // Create Canvas
        GameObject canvas = new GameObject("HUD_Canvas");
        Canvas c = canvas.AddComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 100;

        CanvasScaler scaler = canvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvas.AddComponent<GraphicRaycaster>();

        // Score Text
        GameObject scoreObj = CreateTMPText(canvas.transform, "ScoreText", "SCORE: 0",
            new Vector2(150, -40), TextAlignmentOptions.Left, 24, Color.white);
        RectTransform scoreRect = scoreObj.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0, 1);
        scoreRect.anchorMax = new Vector2(0, 1);
        scoreRect.pivot = new Vector2(0, 1);

        // Level Text
        GameObject levelObj = CreateTMPText(canvas.transform, "LevelText", "FLOOR 1",
            new Vector2(0, -40), TextAlignmentOptions.Center, 28, new Color(1f, 0.85f, 0.3f));
        RectTransform levelRect = levelObj.GetComponent<RectTransform>();
        levelRect.anchorMin = new Vector2(0.5f, 1);
        levelRect.anchorMax = new Vector2(0.5f, 1);
        levelRect.pivot = new Vector2(0.5f, 1);

        // Weapon Info Panel
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
        GameHUD hud = canvas.AddComponent<GameHUD>();
        SerializedObject so = new SerializedObject(hud);
        so.FindProperty("scoreText").objectReferenceValue = scoreObj.GetComponent<TextMeshProUGUI>();
        so.FindProperty("levelText").objectReferenceValue = levelObj.GetComponent<TextMeshProUGUI>();
        so.FindProperty("weaponNameText").objectReferenceValue = weaponNameObj.GetComponent<TextMeshProUGUI>();
        so.FindProperty("weaponDamageText").objectReferenceValue = weaponDmgObj.GetComponent<TextMeshProUGUI>();
        so.FindProperty("weaponInfoPanel").objectReferenceValue = weaponPanel;
        so.ApplyModifiedProperties();

        // Automatically setup Player HealthBar in the HUD Canvas
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        Player playerScript = playerObj != null ? playerObj.GetComponent<Player>() : null;
        CreatePlayerHealthBarInHUD(canvas, playerScript);

        Debug.Log("  ✓ HUD created");
    }

    private static void CreatePlayerHealthBarInHUD(GameObject canvas, Player player)
    {
        // Check if HealthBar component already exists in the scene
        HealthBar hbScript = Object.FindAnyObjectByType<HealthBar>();
        if (hbScript != null)
        {
            if (player != null) player.healthBar = hbScript;
            return;
        }

        // Create HealthBar UI Container
        GameObject hbObj = new GameObject("HealthBar");
        hbObj.transform.SetParent(canvas.transform, false);
        RectTransform hbRect = hbObj.AddComponent<RectTransform>();
        hbRect.anchorMin = new Vector2(0, 1);
        hbRect.anchorMax = new Vector2(0, 1);
        hbRect.pivot = new Vector2(0, 1);
        hbRect.anchoredPosition = new Vector2(150, -80); // Placed below score text
        hbRect.sizeDelta = new Vector2(250, 25);

        // Add HealthBar script
        HealthBar healthBar = hbObj.AddComponent<HealthBar>();

        // Create Slider
        GameObject sliderObj = new GameObject("Slider");
        sliderObj.transform.SetParent(hbObj.transform, false);
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 100;
        slider.value = 100;
        slider.wholeNumbers = true;
        
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = Vector2.zero;
        sliderRect.anchorMax = Vector2.one;
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = Vector2.zero;

        // Background Image
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderObj.transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform faRect = fillArea.AddComponent<RectTransform>();
        faRect.anchorMin = Vector2.zero;
        faRect.anchorMax = Vector2.one;
        faRect.offsetMin = new Vector2(2, 2);
        faRect.offsetMax = new Vector2(-2, -2);

        // Fill Image
        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = Color.green;
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        slider.fillRect = fillRect;

        // Setup Gradient
        Gradient grad = new Gradient();
        GradientColorKey[] gck = new GradientColorKey[2];
        gck[0].color = Color.red;
        gck[0].time = 0.0f;
        gck[1].color = Color.green;
        gck[1].time = 1.0f;
        
        GradientAlphaKey[] gak = new GradientAlphaKey[2];
        gak[0].alpha = 1.0f;
        gak[0].time = 0.0f;
        gak[1].alpha = 1.0f;
        gak[1].time = 1.0f;
        grad.SetKeys(gck, gak);

        // Assign slider & fill to HealthBar
        healthBar.healthSlider = slider;
        healthBar.fill = fillImg;
        healthBar.gradient = grad;

        // Connect player to health bar
        if (player != null)
        {
            player.healthBar = healthBar;
            EditorUtility.SetDirty(player);
        }

        EditorUtility.SetDirty(healthBar);
        Debug.Log("  ✓ Player HealthBar UI added to HUD");
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

        GameObject gunPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Generated/Gun.prefab");
        if (gunPrefab != null)
        {
            GameObject gun = (GameObject)PrefabUtility.InstantiatePrefab(gunPrefab);
            gun.transform.position = player.transform.position + new Vector3(1.5f, 0, 0);
            Debug.Log("  ✓ Starter Gun placed near player");
        }

        GameObject swordPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Generated/Sword.prefab");
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

    private static bool AssetExists(string path)
    {
        return AssetDatabase.LoadAssetAtPath<Object>(path) != null;
    }

    private static void SavePrefab(GameObject obj, string path)
    {
        PrefabUtility.SaveAsPrefabAsset(obj, path);
        Object.DestroyImmediate(obj);
    }
}
