using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Controls the pause sub-menu (opened via Tab or Escape).
/// Updates the player status tab and settings tab (Save, Settings, Exit).
/// </summary>
public class MenuController : MonoBehaviour
{
    public static MenuController Instance { get; private set; }

    [Header("UI Canvas")]
    public GameObject menuCanvas;

    [Header("Player Details Page")]
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI playerHealthText;
    public TextMeshProUGUI playerShieldText;
    public TextMeshProUGUI playerManaText;

    [Header("Settings Page Buttons")]
    public Button saveButton;
    public Button settingsButton;
    public Button exitButton;

    private Player player;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        player = FindAnyObjectByType<Player>();

        if (menuCanvas != null)
        {
            menuCanvas.SetActive(false);
        }

        // Setup Button Listeners
        if (saveButton != null)
        {
            saveButton.onClick.RemoveAllListeners();
            saveButton.onClick.AddListener(OnSavePressed);
        }
        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(OnSettingsPressed);
        }
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(OnExitPressed);
        }
    }

    private void Update()
    {
        // Toggle menu when pressing Escape or Tab
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        if (menuCanvas == null) return;

        // If another screen is already pausing the game (like Loading Screen), ignore
        if (!menuCanvas.activeSelf && Time.timeScale == 0f && !menuCanvas.activeInHierarchy)
        {
            return;
        }

        bool isOpening = !menuCanvas.activeSelf;
        menuCanvas.SetActive(isOpening);

        // Pause / unpause the game time
        Time.timeScale = isOpening ? 0f : 1f;
        PauseController.SetPause(isOpening);

        if (isOpening)
        {
            UpdatePlayerPage();
        }
    }

    private void UpdatePlayerPage()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<Player>();
        }

        if (player == null) return;

        if (playerNameText != null)
        {
            playerNameText.text = "ALEXANDER"; // Default player name
        }

        if (playerHealthText != null)
        {
            playerHealthText.text = $"{player.currentHealth}/{player.maxHealth}";
        }

        if (playerShieldText != null)
        {
            playerShieldText.text = $"{player.currentArmor}/{player.maxArmor}";
        }

        if (playerManaText != null)
        {
            playerManaText.text = $"{player.currentEnergy}/{player.maxEnergy}";
        }
    }

    public void OnSavePressed()
    {
        SaveController saveCtrl = FindAnyObjectByType<SaveController>();
        if (saveCtrl != null)
        {
            saveCtrl.SaveGame();
            Debug.Log("Game saved!");
        }
    }

    public void OnSettingsPressed()
    {
        SettingsManager settingsMgr = null;
        SettingsManager[] managers = Resources.FindObjectsOfTypeAll<SettingsManager>();
        if (managers != null && managers.Length > 0)
        {
            settingsMgr = managers[0];
        }

        if (settingsMgr != null)
        {
            if (settingsMgr.gameObject.activeSelf)
            {
                settingsMgr.gameObject.SetActive(false);
            }
            else
            {
                settingsMgr.OpenSettings();
            }
        }
        else
        {
            Debug.LogWarning("MenuController: No SettingsManager found in scene!");
        }
    }

    public void OnExitPressed()
    {
        // Exit to main menu
        Time.timeScale = 1f;
        PauseController.SetPause(false);
        
        LevelManager lm = LevelManager.Instance;
        if (lm != null)
        {
            lm.ReturnToMenu();
        }
        else
        {
            SceneManager.LoadScene("GameMainMenu");
        }
    }
}
