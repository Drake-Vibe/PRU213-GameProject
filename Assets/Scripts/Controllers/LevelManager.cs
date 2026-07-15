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
    }

    private void PlayMusicForScene(string sceneName)
    {
        if (musicAudioSource == null) return;

        AudioClip targetClip = null;

        if (sceneName == "GameMainMenu")
        {
            targetClip = mainMenuMusic;
        }
        else if (sceneName == "UI-Default")
        {
            targetClip = hubMusic;
        }
        else if (sceneName.StartsWith("Level") || sceneName.StartsWith("Floor"))
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
    }

    /// <summary>
    /// Start a new game — reset state + load with FULL loading screen (5-10s).
    /// </summary>
    public void StartGame()
    {
        GameManager gm = GameManager.Instance;
        if (gm != null)
        {
            gm.ResetGame();
        }

        StartCoroutine(LoadSceneWithFullLoading("UI-Default"));
    }

    /// <summary>
    /// Load a saved game — shorter loading (no fake delay).
    /// </summary>
    public void LoadSavedGame(string sceneName)
    {
        StartCoroutine(LoadSceneQuick(sceneName));
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
        yield return QuickLoadRoutine(SceneManager.LoadSceneAsync(sceneName));
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
    /// Helper to find and disable/enable all other UI Canvases in the active scene.
    /// </summary>
    private void ToggleOtherCanvases(bool active)
    {
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
        {
            // Skip the loading screen canvas itself to keep it visible
            if (loadingScreen != null && (canvas.gameObject == loadingScreen || canvas.transform.IsChildOf(loadingScreen.transform) || loadingScreen.transform.IsChildOf(canvas.transform)))
            {
                continue;
            }

            if (canvas.name == "LoadingCanvas" || canvas.name == "Loading_Canvas")
            {
                continue;
            }

            canvas.gameObject.SetActive(active);
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

