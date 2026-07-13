using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;

/// <summary>
/// Main Menu controller with 4 buttons: Start, Load, Setting, Quit.
/// Handles cutscene flow (Start → Cutscene → Loading → Game) and save file checking (Load).
/// 
/// Setup in Unity:
/// 1. Create Canvas with 4 Buttons
/// 2. Assign button onClick in Inspector OR drag references here
/// 3. Create Settings panel as child (managed by SettingsManager)
/// 4. Create Cutscene Canvas (managed by CutsceneManager)
/// </summary>
public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("References")]
    [SerializeField] private CutsceneManager cutsceneManager;
    [SerializeField] private SettingsManager settingsManager;

    [Header("UI Feedback")]
    [SerializeField] private GameObject noSaveFileMessage;
    [SerializeField] private GameObject confirmQuitPanel;

    [Header("Config")]
    [Tooltip("Scene name to load for new game")]
    [SerializeField] private string gameSceneName = "Game";
    [Tooltip("Scene name to load after cutscene (can be same as gameSceneName)")]
    [SerializeField] private string afterCutsceneScene = "Game";

    private string saveFilePath;

    private void Start()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "saveData.json");

        // Setup button listeners
        if (startButton != null)
            startButton.onClick.AddListener(OnStartClicked);

        if (loadButton != null)
            loadButton.onClick.AddListener(OnLoadClicked);

        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        // Subscribe to cutscene completion
        if (cutsceneManager != null)
            cutsceneManager.OnCutsceneComplete += OnCutsceneFinished;

        // Hide UI elements
        if (noSaveFileMessage != null)
            noSaveFileMessage.SetActive(false);

        if (confirmQuitPanel != null)
            confirmQuitPanel.SetActive(false);

        // Apply saved audio settings
        SettingsData settings = SettingsManager.CurrentSettings;
        AudioListener.volume = settings.masterVolume;

        // Ensure time scale is normal
        Time.timeScale = 1f;
    }

    // ========================================
    // BUTTON HANDLERS
    // ========================================

    /// <summary>
    /// Start button: Check for cutscene → play it or go straight to loading.
    /// </summary>
    public void OnStartClicked()
    {
        Debug.Log("Start New Game clicked!");

        // Check if cutscene exists and has pages
        if (cutsceneManager != null && cutsceneManager.HasCutscene)
        {
            Debug.Log("Playing intro cutscene...");
            // Hide menu buttons during cutscene
            SetMenuButtonsActive(false);
            cutsceneManager.PlayCutscene();
        }
        else
        {
            Debug.Log("No cutscene found, going straight to loading...");
            StartNewGame();
        }
    }

    /// <summary>
    /// Called when cutscene finishes (either completed or skipped).
    /// Triggers loading screen → game.
    /// </summary>
    private void OnCutsceneFinished()
    {
        Debug.Log("Cutscene finished! Starting game via loading screen...");
        StartNewGame();
    }

    /// <summary>
    /// Actually start the new game (reset state + load with loading screen).
    /// </summary>
    private void StartNewGame()
    {
        // Reset game state
        GameManager gm = GameManager.Instance;
        if (gm != null)
        {
            gm.ResetGame();
        }

        // Delete old save file for clean start
        if (File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            Debug.Log("Old save file deleted for new game.");
        }

        // Use LevelManager to load with loading screen
        LevelManager lm = LevelManager.Instance;
        if (lm != null)
        {
            lm.StartGame();
        }
        else
        {
            // Fallback: direct load
            SceneManager.LoadScene(afterCutsceneScene);
        }
    }

    /// <summary>
    /// Load button: Check if save file exists, load if yes, show message if no.
    /// </summary>
    public void OnLoadClicked()
    {
        Debug.Log("Load Game clicked!");

        if (File.Exists(saveFilePath))
        {
            Debug.Log("Save file found! Loading game...");

            // Load directly without long loading screen (as per user request)
            LevelManager lm = LevelManager.Instance;
            if (lm != null)
            {
                lm.LoadSavedGame(gameSceneName);
            }
            else
            {
                SceneManager.LoadScene(gameSceneName);
            }
        }
        else
        {
            Debug.Log("No save file found!");
            ShowNoSaveMessage();
        }
    }

    /// <summary>
    /// Settings button: Open settings panel.
    /// </summary>
    public void OnSettingsClicked()
    {
        Debug.Log("Settings clicked!");

        if (settingsManager != null)
        {
            settingsManager.OpenSettings();
        }
        else
        {
            Debug.LogWarning("MainMenu: SettingsManager not assigned!");
        }
    }

    /// <summary>
    /// Quit button: Show confirmation or quit directly.
    /// </summary>
    public void OnQuitClicked()
    {
        Debug.Log("Quit clicked!");

        if (confirmQuitPanel != null)
        {
            confirmQuitPanel.SetActive(true);
        }
        else
        {
            QuitGame();
        }
    }

    // ========================================
    // QUIT CONFIRMATION
    // ========================================

    /// <summary>
    /// Called by "Yes" button in quit confirmation panel.
    /// </summary>
    public void ConfirmQuit()
    {
        QuitGame();
    }

    /// <summary>
    /// Called by "No" button in quit confirmation panel.
    /// </summary>
    public void CancelQuit()
    {
        if (confirmQuitPanel != null)
            confirmQuitPanel.SetActive(false);
    }

    private void QuitGame()
    {
        Debug.Log("Quitting game...");

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // ========================================
    // HELPERS
    // ========================================

    private void ShowNoSaveMessage()
    {
        if (noSaveFileMessage != null)
        {
            noSaveFileMessage.SetActive(true);
            // Auto-hide after 3 seconds
            Invoke(nameof(HideNoSaveMessage), 3f);
        }
    }

    private void HideNoSaveMessage()
    {
        if (noSaveFileMessage != null)
            noSaveFileMessage.SetActive(false);
    }

    private void SetMenuButtonsActive(bool active)
    {
        if (startButton != null) startButton.gameObject.SetActive(active);
        if (loadButton != null) loadButton.gameObject.SetActive(active);
        if (settingsButton != null) settingsButton.gameObject.SetActive(active);
        if (quitButton != null) quitButton.gameObject.SetActive(active);
    }

    private void OnDestroy()
    {
        if (cutsceneManager != null)
            cutsceneManager.OnCutsceneComplete -= OnCutsceneFinished;
    }
}
