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

    public static void DestroyInstance()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
            Instance = null;
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
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
        player = FindAnyObjectByType<Player>();

        if (menuCanvas != null)
        {
            menuCanvas.SetActive(false);
        }

        Canvas canvas = GetComponent<Canvas>();
        if (canvas == null) canvas = GetComponentInChildren<Canvas>(true);
        if (canvas != null)
        {
            canvas.enabled = (scene.name != "GameMainMenu" && scene.name != "GameOver" && scene.name != "DemoEnding");
        }
    }

    private void Start()
    {
        player = FindAnyObjectByType<Player>();

        if (menuCanvas != null)
        {
            menuCanvas.SetActive(false);

            // Re-order hierarchy so Tabs is drawn last (on top of Pages to avoid raycast blocking)
            Transform tabsTrans = menuCanvas.transform.Find("Tabs");
            if (tabsTrans == null)
            {
                // Fallback search in all children of menuCanvas
                foreach (Transform t in menuCanvas.GetComponentsInChildren<Transform>(true))
                {
                    if (t.name == "Tabs") { tabsTrans = t; break; }
                }
            }

            if (tabsTrans != null)
            {
                // Draw the tabs above the page panels within the menu.
                tabsTrans.SetAsLastSibling();
            }

            // Give the whole in-game menu a single dedicated raycaster on a top sorting layer.
            // The nested menu canvas was authored with an inconsistent render mode, which stopped
            // the root GraphicRaycaster from delivering clicks to the menu's content. Forcing
            // overrideSorting + a GraphicRaycaster here makes BOTH the tabs and the settings-page
            // buttons reliably receive clicks, and keeps the menu above the HUD.
            Canvas menuCanvasComp = menuCanvas.GetComponent<Canvas>();
            if (menuCanvasComp == null) menuCanvasComp = menuCanvas.AddComponent<Canvas>();
            menuCanvasComp.overrideSorting = true;
            menuCanvasComp.sortingOrder = 210; // above HUD (100) and the base menu canvas (200)

            if (menuCanvas.GetComponent<GraphicRaycaster>() == null)
            {
                menuCanvas.AddComponent<GraphicRaycaster>();
            }

            Debug.Log("[MenuController] Menu canvas given a dedicated raycaster (sortingOrder 210) to fix menu clicks.");
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

        // Setup Tab buttons programmatically to ensure robust switching
        SetupTabButtons();
    }

    private void SetupTabButtons()
    {
        TabController tabCtrl = GetComponent<TabController>();
        if (tabCtrl == null) tabCtrl = GetComponentInChildren<TabController>(true);
        if (tabCtrl == null)
        {
            Debug.LogWarning("[MenuController] SetupTabButtons: TabController not found!");
            return;
        }

        // 1. Setup normal Button components
        Button[] allButtons = GetComponentsInChildren<Button>(true);
        Debug.Log($"[MenuController] SetupTabButtons: Found {allButtons.Length} Button components.");
        foreach (var btn in allButtons)
        {
            string gameObjectName = btn.gameObject.name.ToUpper();
            int targetIndex = -1;
            
            // Match by GameObject name first
            if (gameObjectName.Contains("PLAYER")) targetIndex = 0;
            else if (gameObjectName.Contains("SETTING")) targetIndex = 1;

            // Match by TMPro text if name didn't match
            if (targetIndex == -1)
            {
                TextMeshProUGUI txtComp = btn.GetComponentInChildren<TextMeshProUGUI>(true);
                if (txtComp != null)
                {
                    string txt = txtComp.text.ToUpper().Trim();
                    if (txt == "PLAYER") targetIndex = 0;
                    else if (txt == "SETTINGS" || txt == "SETTING") targetIndex = 1;
                }
            }

            if (targetIndex != -1)
            {
                // Only programmatically bind if no listener is set up in Unity Editor
                if (btn.onClick.GetPersistentEventCount() == 0)
                {
                    btn.onClick.RemoveAllListeners();
                    int idx = targetIndex;
                    btn.onClick.AddListener(() => {
                        Debug.Log($"[MenuController] Tab Button clicked (Runtime Fallback): {btn.gameObject.name} -> ActiveTab({idx})");
                        tabCtrl.ActiveTab(idx);
                    });
                    Debug.Log($"[MenuController] Bound Button '{btn.gameObject.name}' to ActiveTab({targetIndex}) (Runtime Fallback)");
                }
                else
                {
                    Debug.Log($"[MenuController] Button '{btn.gameObject.name}' already has editor onClick configured. Keeping it.");
                }
            }
        }

        // 2. Setup EventTriggers dynamically for pointer events
        UnityEngine.EventSystems.EventTrigger[] allTriggers = GetComponentsInChildren<UnityEngine.EventSystems.EventTrigger>(true);
        Debug.Log($"[MenuController] SetupTabButtons: Found {allTriggers.Length} EventTrigger components.");
        foreach (var trigger in allTriggers)
        {
            string gameObjectName = trigger.gameObject.name.ToUpper();
            int targetIndex = -1;

            // Match by GameObject name first
            if (gameObjectName.Contains("PLAYER")) targetIndex = 0;
            else if (gameObjectName.Contains("SETTING")) targetIndex = 1;

            // Match by TMPro text if name didn't match
            if (targetIndex == -1)
            {
                TextMeshProUGUI txtComp = trigger.GetComponentInChildren<TextMeshProUGUI>(true);
                if (txtComp != null)
                {
                    string txt = txtComp.text.ToUpper().Trim();
                    if (txt == "PLAYER") targetIndex = 0;
                    else if (txt == "SETTINGS" || txt == "SETTING") targetIndex = 1;
                }
            }

            if (targetIndex != -1)
            {
                trigger.enabled = true;
                
                // Only programmatically configure trigger if no entry is configured in Unity Editor
                if (trigger.triggers == null || trigger.triggers.Count == 0)
                {
                    if (trigger.triggers == null)
                    {
                        trigger.triggers = new System.Collections.Generic.List<UnityEngine.EventSystems.EventTrigger.Entry>();
                    }
                    var entry = new UnityEngine.EventSystems.EventTrigger.Entry();
                    entry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
                    int idx = targetIndex;
                    entry.callback.AddListener((eventData) => {
                        Debug.Log($"[MenuController] EventTrigger PointerDown (Runtime Fallback): {trigger.gameObject.name} -> ActiveTab({idx})");
                        tabCtrl.ActiveTab(idx);
                    });
                    trigger.triggers.Add(entry);

                    Button btn = trigger.gameObject.GetComponent<Button>();
                    if (btn != null && btn.onClick.GetPersistentEventCount() == 0)
                    {
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => {
                            Debug.Log($"[MenuController] Tab Button onClick (Runtime Fallback): {btn.gameObject.name} -> ActiveTab({idx})");
                            tabCtrl.ActiveTab(idx);
                        });
                    }
                    Debug.Log($"[MenuController] Programmatically configured empty EventTrigger on '{trigger.gameObject.name}' for ActiveTab({targetIndex})");
                }
                else
                {
                    Debug.Log($"[MenuController] EventTrigger '{trigger.gameObject.name}' already has editor triggers configured. Keeping them intact.");
                }
            }
        }
    }

    private void Update()
    {
        // Block menu toggling in restricted scenes (GameMainMenu, GameOver, DemoEnding)
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "GameMainMenu" || currentScene == "GameOver" || currentScene == "DemoEnding")
        {
            if (menuCanvas != null && menuCanvas.activeSelf)
            {
                menuCanvas.SetActive(false);
            }
            return;
        }

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

            // Automatically reset to the Player tab when opening the menu
            TabController tabCtrl = GetComponent<TabController>();
            if (tabCtrl == null) tabCtrl = GetComponentInChildren<TabController>(true);
            if (tabCtrl != null)
            {
                tabCtrl.ActiveTab(0);
            }
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
