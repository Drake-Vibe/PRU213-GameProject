using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Manages level loading with extended loading screen (5-10s), fake progress bar,
/// and random loading tips. Singleton that persists across scenes.
/// </summary>
public class LevelManager : MonoBehaviour
{
    private static LevelManager _instance;
    public static LevelManager Instance => _instance;

    [Header("Loading Screen")]
    public GameObject loadingScreen;
    public Slider progressBar;
    public TextMeshProUGUI progressText;
    public TextMeshProUGUI tipsText;
    public Image loadingArt; // Optional: background art during loading

    [Header("Loading Config")]
    [Tooltip("Minimum time the loading screen stays visible (seconds).")]
    [SerializeField] private float minLoadTime = 7f; // 5-10s, default 7
    [Tooltip("Maximum time the loading screen stays visible (seconds).")]
    [SerializeField] private float maxLoadTime = 10f;

    [Header("Loading Tips")]
    [SerializeField] private string[] loadingTips = new string[]
    {
        "Nhấn Q để nhặt vũ khí trên mặt đất",
        "Giết hết quái trong phòng để mở cửa tiếp theo",
        "Tấn công cận chiến gây nhiều sát thương hơn súng",
        "Enemy bắn xa sẽ tránh bạn nếu bạn đến gần",
        "Lưu game thường xuyên để không mất tiến trình",
        "Hãy nhớ tìm vũ khí mạnh hơn trước khi vào boss room",
        "Bạn có thể đổi phím điều khiển trong Settings",
        "Giữ khoảng cách với enemy cận chiến",
        "Portal cuối dungeon sẽ đưa bạn đến floor tiếp theo",
        "Health bar của enemy chỉ hiện khi chúng bị thương"
    };

    [Header("Background Music")]
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip hubMusic;
    [SerializeField] private AudioClip dungeonMusic;
    [SerializeField] private AudioClip demoCompleteMusic;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Auto-configure AudioSource
        if (musicAudioSource == null)
        {
            musicAudioSource = GetComponent<AudioSource>();
            if (musicAudioSource == null)
            {
                musicAudioSource = gameObject.AddComponent<AudioSource>();
                musicAudioSource.playOnAwake = false;
                musicAudioSource.loop = true;
            }
        }
    }

    private float currentMusicVol = 0.8f;
    private float currentSfxVol = 1f;

    public float CurrentSfxVolume => currentSfxVol;

    public void SetMusicVolume(float volume)
    {
        currentMusicVol = Mathf.Clamp01(volume);
        if (musicAudioSource != null)
        {
            musicAudioSource.volume = currentMusicVol;
        }
    }

    public void SetSFXVolume(float volume)
    {
        currentSfxVol = Mathf.Clamp01(volume);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);

        if (scene.name == "GameMainMenu" || scene.name == "GameOver" || scene.name == "DemoEnding")
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Destroy(player);
            }
        }
        else
        {
            // If we are loading a saved game, skip SpawnPoint teleport
            // and let SaveController restore the saved position instead
            bool isLoadingSave = PlayerPrefs.GetInt("ShouldLoadSave", 0) == 1;

            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (!isLoadingSave)
            {
                GameObject spawnPoint = GameObject.Find("SpawnPoint");
                if (spawnPoint == null) spawnPoint = GameObject.Find("PlayerSpawnPoint");
                if (spawnPoint == null) spawnPoint = GameObject.FindWithTag("Respawn");

                if (player != null && spawnPoint != null)
                {
                    // Temporarily disable character controller or rigidbody to ensure clean teleport
                    Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.linearVelocity = Vector2.zero;
                    }
                    player.transform.position = spawnPoint.transform.position;
                    Debug.Log($"[LevelManager] Teleported persistent Player to {spawnPoint.name} at {spawnPoint.transform.position}");
                }
            }
            else
            {
                Debug.Log("[LevelManager] Skipping SpawnPoint teleport — loading saved position via SaveController.");
            }

            // Clean up duplicate ground weapons in the new scene matching player's equipped weapon
            CleanUpDuplicateWeapons(player);

            // Spawn EventSystem if not found in the scene to ensure UI interaction works
            if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Debug.Log("[LevelManager] Dynamically created EventSystem for UI interaction.");
            }
        }
    }

    private void CleanUpDuplicateWeapons(GameObject playerObj)
    {
        if (playerObj == null) return;
        Player p = playerObj.GetComponent<Player>();
        if (p == null) return;

        // Clean up any child weapons that are not the current equipped one
        BaseWeapon[] childWeapons = playerObj.GetComponentsInChildren<BaseWeapon>(true);
        foreach (var bw in childWeapons)
        {
            if (p.currentWeapon == null || bw.gameObject != p.currentWeapon)
            {
                Debug.Log($"[LevelManager] Destroying unequipped child weapon from Player: {bw.gameObject.name}");
                Destroy(bw.gameObject);
            }
        }

        if (p.currentWeapon == null) return;

        BaseWeapon equippedBw = p.currentWeapon.GetComponent<BaseWeapon>();
        if (equippedBw == null) return;

        string equippedName = equippedBw.weaponName;

        // Search for unequipped weapons in the scene that match equippedName
        BaseWeapon[] sceneWeapons = Object.FindObjectsByType<BaseWeapon>(FindObjectsInactive.Include);
        foreach (var bw in sceneWeapons)
        {
            if (!bw.IsEquipped && (bw.weaponName.Equals(equippedName, System.StringComparison.OrdinalIgnoreCase) || bw.gameObject.name.Contains(equippedName)))
            {
                Debug.Log($"[LevelManager] Removing duplicate ground weapon '{bw.gameObject.name}' since Player is already holding '{equippedName}'.");
                Destroy(bw.gameObject);
            }
        }
    }

    private void PlayMusicForScene(string sceneName)
    {
        if (musicAudioSource == null) return;

        AudioClip targetClip = null;

        if (sceneName == "GameMainMenu")
        {
            targetClip = mainMenuMusic;
        }
        else if (sceneName == "DemoEnding")
        {
            targetClip = demoCompleteMusic;
        }
        else if (sceneName == "UI-Default")
        {
            targetClip = hubMusic;
        }
        else if (sceneName.StartsWith("Level") || sceneName.StartsWith("Floor") || sceneName == "Boss Fight")
        {
            targetClip = dungeonMusic;
        }

        // Only switch and play if it is a new clip
        if (targetClip != null && musicAudioSource.clip != targetClip)
        {
            musicAudioSource.clip = targetClip;
            musicAudioSource.loop = true;
            musicAudioSource.Play();
        }
        else if (targetClip == null)
        {
            // Optional: you can choose to stop or let music keep playing
        }
    }

    private void Start()
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(false);

        // Load saved audio volumes from Settings
        currentMusicVol = PlayerPrefs.GetFloat("musicVolume", 0.8f);
        currentSfxVol = PlayerPrefs.GetFloat("sfxVolume", 1f);
        if (musicAudioSource != null)
        {
            musicAudioSource.volume = currentMusicVol;
        }
    }

    /// <summary>
    /// Start a new game — reset state + load with FULL loading screen (5-10s).
    /// </summary>
    private void CleanupPersistentObjects()
    {
        Player.DestroyInstance();
        PlayerHUD.DestroyInstance();
        MenuController.DestroyInstance();

        GameManager gm = GameManager.Instance;
        if (gm != null)
        {
            gm.ResetGame();
        }
    }

    public void StartGame()
    {
        CleanupPersistentObjects();
        StartCoroutine(LoadSceneWithFullLoading("UI-Default"));
    }

    /// <summary>
    /// Load a saved game with a single loading screen.
    /// If targetScene != UI-Default: Load UI-Default first (to get Player + UI), equip weapon, then load target scene.
    /// If targetScene == UI-Default: Just load UI-Default and restore save data.
    /// </summary>
    public void LoadSavedGame(string targetScene, SaveData saveData)
    {
        CleanupPersistentObjects();
        StartCoroutine(LoadSaveGameTwoStep(targetScene, saveData));
    }

    private IEnumerator LoadSaveGameTwoStep(string targetScene, SaveData saveData)
    {
        ShowLoadingScreen();

        float totalLoadTime = Random.Range(minLoadTime, maxLoadTime);
        float elapsed = 0f;
        float tipTimer = 0f;
        float tipChangeInterval = 3f;
        ShowRandomTip();

        // ── STEP 1: Load UI-Default to create Player + MenuInGame + HUD ──
        Debug.Log("[LevelManager] LoadSaveGame Step 1: Loading UI-Default for Player + UI...");
        AsyncOperation op1 = SceneManager.LoadSceneAsync("UI-Default");
        if (op1 != null) op1.allowSceneActivation = false;

        // Run progress bar while loading UI-Default (first 40% of progress)
        while (op1 != null && op1.progress < 0.9f)
        {
            elapsed += Time.unscaledDeltaTime;
            tipTimer += Time.unscaledDeltaTime;
            if (tipTimer >= tipChangeInterval) { tipTimer = 0f; ShowRandomTip(); }
            float t = Mathf.Clamp01(elapsed / totalLoadTime);
            UpdateProgressBar(EaseInOutCubic(t) * 0.4f);
            yield return null;
        }

        // Activate UI-Default
        if (op1 != null) op1.allowSceneActivation = true;
        while (op1 != null && !op1.isDone) yield return null;

        // Wait a frame for UI-Default to initialize (Player, MenuInGame, HUD etc.)
        yield return null;
        yield return null;

        // ── Equip saved weapon onto Player ──
        if (saveData != null && !string.IsNullOrEmpty(saveData.equippedWeaponName))
        {
            Player player = FindAnyObjectByType<Player>();
            if (player != null)
            {
                RestorePlayerWeaponForLoad(player, saveData.equippedWeaponName);
                Debug.Log($"[LevelManager] Equipped saved weapon '{saveData.equippedWeaponName}' onto Player in UI-Default.");
            }
        }

        // ── STEP 2: If target scene is different from UI-Default, load it ──
        bool needSecondLoad = !string.IsNullOrEmpty(targetScene) && targetScene != "UI-Default";
        
        if (needSecondLoad)
        {
            Debug.Log($"[LevelManager] LoadSaveGame Step 2: Loading target scene '{targetScene}'...");
            AsyncOperation op2 = SceneManager.LoadSceneAsync(targetScene);
            if (op2 != null) op2.allowSceneActivation = false;

            // Continue progress bar (40% → 95%)
            while (op2 != null && op2.progress < 0.9f)
            {
                elapsed += Time.unscaledDeltaTime;
                tipTimer += Time.unscaledDeltaTime;
                if (tipTimer >= tipChangeInterval) { tipTimer = 0f; ShowRandomTip(); }
                float t = Mathf.Clamp01(elapsed / totalLoadTime);
                UpdateProgressBar(0.4f + EaseInOutCubic(t) * 0.55f);
                yield return null;
            }

            // Fill remaining time if loading was faster than minimum
            while (elapsed < totalLoadTime)
            {
                elapsed += Time.unscaledDeltaTime;
                tipTimer += Time.unscaledDeltaTime;
                if (tipTimer >= tipChangeInterval) { tipTimer = 0f; ShowRandomTip(); }
                float t = Mathf.Clamp01(elapsed / totalLoadTime);
                UpdateProgressBar(0.4f + EaseInOutCubic(t) * 0.55f);
                yield return null;
            }

            // Activate target scene
            if (op2 != null) op2.allowSceneActivation = true;
            while (op2 != null && !op2.isDone) yield return null;
        }
        else
        {
            // Fill remaining time for UI-Default only load
            while (elapsed < totalLoadTime)
            {
                elapsed += Time.unscaledDeltaTime;
                tipTimer += Time.unscaledDeltaTime;
                if (tipTimer >= tipChangeInterval) { tipTimer = 0f; ShowRandomTip(); }
                float t = Mathf.Clamp01(elapsed / totalLoadTime);
                UpdateProgressBar(EaseInOutCubic(t));
                yield return null;
            }
        }

        // Final: 100%
        UpdateProgressBar(1f);
        yield return new WaitForSecondsRealtime(0.3f);

        HideLoadingScreen();

        // Wait a frame for the scene to settle
        yield return null;

        // ── STEP 3: Restore all save data (position, HP, inventory, score etc.) ──
        if (saveData != null)
        {
            SaveController sc = SaveController.Instance ?? FindAnyObjectByType<SaveController>();
            if (sc != null)
            {
                sc.RestoreSaveDataPublic(saveData);
            }
            else
            {
                Debug.LogWarning("[LevelManager] No SaveController found to restore save data!");
            }
        }

        // Clear load flags
        PlayerPrefs.SetInt("ShouldLoadSave", 0);
        PlayerPrefs.SetString("SavedSceneName", "");
        PlayerPrefs.Save();

        Debug.Log($"[LevelManager] Save game loaded successfully! Scene: {targetScene}");
    }

    /// <summary>
    /// Helper to find and equip a weapon by name onto the player during save loading.
    /// </summary>
    private void RestorePlayerWeaponForLoad(Player player, string weaponName)
    {
        if (player == null || string.IsNullOrEmpty(weaponName)) return;

        // Check children first
        BaseWeapon[] childWeapons = player.GetComponentsInChildren<BaseWeapon>(true);
        foreach (var bw in childWeapons)
        {
            if (bw.weaponName.Equals(weaponName, System.StringComparison.OrdinalIgnoreCase) ||
                bw.gameObject.name.Contains(weaponName))
            {
                bw.gameObject.SetActive(true);
                bw.AttachTo(player.gameObject);
                player.currentWeapon = bw.gameObject;
                return;
            }
        }

        // Search scene
        BaseWeapon[] sceneWeapons = FindObjectsByType<BaseWeapon>(FindObjectsInactive.Include);
        foreach (var bw in sceneWeapons)
        {
            if (bw.weaponName.Equals(weaponName, System.StringComparison.OrdinalIgnoreCase) ||
                bw.gameObject.name.Contains(weaponName))
            {
                bw.gameObject.SetActive(true);
                bw.AttachTo(player.gameObject);
                player.currentWeapon = bw.gameObject;
                return;
            }
        }
    }

    /// <summary>
    /// Load the next level with full loading screen.
    /// </summary>
    public void NextLevel()
    {
        GameManager gm = GameManager.Instance;
        if (gm != null)
        {
            gm.currentLevel++;
        }

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log("All levels completed! Returning to menu.");
            ReturnToMenu();
            return;
        }

        StartCoroutine(LoadSceneWithFullLoading(nextSceneIndex));
    }

    /// <summary>
    /// Return to main menu (quick load, no extended loading screen).
    /// </summary>
    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        StartCoroutine(LoadSceneQuick("GameMainMenu"));
    }

    /// <summary>
    /// Reload current scene.
    /// </summary>
    public void RetryLevel()
    {
        StartCoroutine(LoadSceneWithFullLoading(SceneManager.GetActiveScene().buildIndex));
    }

    /// <summary>
    /// Load a specific dungeon level by scene name (with full loading screen).
    /// Called by DungeonEntrance when player enters the dungeon from Hub Map.
    /// </summary>
    public void LoadDungeonLevel(string sceneName)
    {
        StartCoroutine(LoadSceneWithFullLoading(sceneName));
    }

    /// <summary>
    /// Return to the Hub/Default Map (Game.unity) from a dungeon floor.
    /// Uses quick load since the hub is a small scene.
    /// </summary>
    public void ReturnToHub()
    {
        Time.timeScale = 1f;
        StartCoroutine(LoadSceneQuick("UI-Default"));
    }

    // ========================================
    // FULL LOADING (5-10s with fake progress + tips)
    // ========================================

    private IEnumerator LoadSceneWithFullLoading(string sceneName)
    {
        yield return FullLoadingRoutine(SceneManager.LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneWithFullLoading(int sceneIndex)
    {
        yield return FullLoadingRoutine(SceneManager.LoadSceneAsync(sceneIndex));
    }

    /// <summary>
    /// Extended loading screen with:
    /// - Fake smooth progress bar (eased curve over minLoadTime ~ maxLoadTime)
    /// - Random loading tips that change every 3 seconds
    /// - Actual scene loading happens in parallel
    /// </summary>
    private IEnumerator FullLoadingRoutine(AsyncOperation operation)
    {
        if (operation == null)
        {
            Debug.LogError("[LevelManager] FullLoadingRoutine: AsyncOperation is null! Check if the target scene exists and is added to Build Settings.");
            yield break;
        }

        // Don't auto-activate scene when loaded — wait for our timer
        operation.allowSceneActivation = false;

        ShowLoadingScreen();

        // Randomize load time between min and max
        float totalLoadTime = Random.Range(minLoadTime, maxLoadTime);
        float elapsed = 0f;
        float tipChangeInterval = 3f;
        float tipTimer = 0f;

        // Show first tip
        ShowRandomTip();

        while (elapsed < totalLoadTime)
        {
            elapsed += Time.unscaledDeltaTime;
            tipTimer += Time.unscaledDeltaTime;

            // Change tip every N seconds
            if (tipTimer >= tipChangeInterval)
            {
                tipTimer = 0f;
                ShowRandomTip();
            }

            // Fake progress with eased curve
            // Fast at start, slow in middle, fast at end
            float t = elapsed / totalLoadTime;
            float fakeProgress = EaseInOutCubic(t);

            // Blend with real progress (real loading is usually very fast)
            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);
            float displayProgress = Mathf.Max(fakeProgress, realProgress * 0.5f);

            // Cap at 95% until scene is actually ready
            if (operation.progress < 0.9f)
                displayProgress = Mathf.Min(displayProgress, 0.95f);

            UpdateProgressBar(displayProgress);

            yield return null;
        }

        // Final: jump to 100%
        UpdateProgressBar(1f);
        yield return new WaitForSecondsRealtime(0.3f); // Brief pause at 100%

        // Now activate the scene
        operation.allowSceneActivation = true;

        // Wait for scene to actually load
        while (!operation.isDone)
        {
            yield return null;
        }

        HideLoadingScreen();
    }

    // ========================================
    // QUICK LOADING (for Load Game / Return to Menu)
    // ========================================

    private IEnumerator LoadSceneQuick(string sceneName)
    {
        if (sceneName == "GameMainMenu")
        {
            var op = SceneManager.LoadSceneAsync(sceneName);
            while (!op.isDone)
            {
                yield return null;
            }
        }
        else
        {
            yield return QuickLoadRoutine(SceneManager.LoadSceneAsync(sceneName));
        }
    }

    private IEnumerator QuickLoadRoutine(AsyncOperation operation)
    {
        ShowLoadingScreen();

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            UpdateProgressBar(progress);
            yield return null;
        }

        HideLoadingScreen();
    }

    // ========================================
    // UI HELPERS
    // ========================================

    private void ShowLoadingScreen()
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        Time.timeScale = 0f; // Freeze game actions/physics
        ToggleOtherCanvases(false); // Hide all other UI Canvases

        UpdateProgressBar(0f);
    }

    private void HideLoadingScreen()
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(false);

        Time.timeScale = 1f; // Resume gameplay
        ToggleOtherCanvases(true); // Restore other UI Canvases
    }

    /// <summary>
    /// Helper to find and disable/enable all other UI Canvases.
    /// Uses FindObjectsInactive.Include to also find DontDestroyOnLoad canvases.
    /// </summary>
    private void ToggleOtherCanvases(bool active)
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include);
        foreach (Canvas canvas in canvases)
        {
            if (canvas == null) continue;

            // Skip the loading screen canvas itself to keep it visible
            if (loadingScreen != null && (canvas.gameObject == loadingScreen || canvas.transform.IsChildOf(loadingScreen.transform) || loadingScreen.transform.IsChildOf(canvas.transform)))
            {
                continue;
            }

            if (canvas.name == "LoadingCanvas" || canvas.name == "Loading_Canvas")
            {
                continue;
            }

            // Skip the LevelManager's own canvas (LoadingCanvas parent)
            if (canvas.gameObject == gameObject)
            {
                continue;
            }

            // In menu/restricted scenes, do not re-enable gameplay canvases (like HUD or In-Game Menu)
            if (active)
            {
                string currentSceneName = SceneManager.GetActiveScene().name;
                if (currentSceneName == "GameMainMenu" || currentSceneName == "GameOver" || currentSceneName == "DemoEnding")
                {
                    if (canvas.name == "HUD_Canvas" || canvas.name.Contains("Menu") || canvas.name.Contains("Settings"))
                    {
                        continue;
                    }
                }
            }

            canvas.enabled = active;
        }
    }

    private void UpdateProgressBar(float progress)
    {
        if (progressBar != null)
            progressBar.value = progress;

        if (progressText != null)
            progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
    }

    private void ShowRandomTip()
    {
        if (tipsText == null || loadingTips == null || loadingTips.Length == 0) return;

        int randomIndex = Random.Range(0, loadingTips.Length);
        tipsText.text = $"TIP: {loadingTips[randomIndex]}";
    }

    /// <summary>
    /// Smooth ease-in-out curve for natural-looking progress.
    /// </summary>
    private float EaseInOutCubic(float t)
    {
        return t < 0.5f
            ? 4f * t * t * t
            : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}

