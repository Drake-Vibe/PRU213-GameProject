using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

/// <summary>
/// Manages the Settings UI panel.
/// Handles audio volume sliders and key binding rebinding.
/// 
/// Setup in Unity:
/// 1. Create Settings Panel (child of MainMenu Canvas)
/// 2. Add 3 Sliders for audio (Master, Music, SFX)
/// 3. Add buttons for each key binding (clicking starts rebind)
/// 4. Add "Reset Defaults" and "Back" buttons
/// </summary>
public class SettingsManager : MonoBehaviour
{
    [Header("Audio Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Audio Labels")]
    [SerializeField] private TextMeshProUGUI masterVolumeLabel;
    [SerializeField] private TextMeshProUGUI musicVolumeLabel;
    [SerializeField] private TextMeshProUGUI sfxVolumeLabel;

    [Header("Key Binding Buttons")]
    [SerializeField] private Button moveUpButton;
    [SerializeField] private Button moveDownButton;
    [SerializeField] private Button moveLeftButton;
    [SerializeField] private Button moveRightButton;
    [SerializeField] private Button attackButton;
    [SerializeField] private Button pickupButton;
    [SerializeField] private Button menuButton;
    [SerializeField] private Button interactButton;

    [Header("Key Binding Labels")]
    [SerializeField] private TextMeshProUGUI moveUpLabel;
    [SerializeField] private TextMeshProUGUI moveDownLabel;
    [SerializeField] private TextMeshProUGUI moveLeftLabel;
    [SerializeField] private TextMeshProUGUI moveRightLabel;
    [SerializeField] private TextMeshProUGUI attackLabel;
    [SerializeField] private TextMeshProUGUI pickupLabel;
    [SerializeField] private TextMeshProUGUI menuLabel;
    [SerializeField] private TextMeshProUGUI interactLabel;

    [Header("Buttons")]
    [SerializeField] private Button resetDefaultsButton;
    [SerializeField] private Button backButton;

    [Header("Rebind UI")]
    [SerializeField] private GameObject rebindOverlay;
    [SerializeField] private TextMeshProUGUI rebindPromptText;

    // Current settings
    private SettingsData settings = new SettingsData();

    // Rebinding state
    private bool isRebinding = false;
    private Action<KeyCode> currentRebindCallback;

    // Singleton-like access for other scripts to read settings
    private static SettingsData _currentSettings;
    public static SettingsData CurrentSettings
    {
        get
        {
            if (_currentSettings == null)
            {
                _currentSettings = new SettingsData();
                _currentSettings.Load();
            }
            return _currentSettings;
        }
    }

    private void Start()
    {
        settings.Load();
        _currentSettings = settings;

        SetupSliders();
        SetupKeyBindButtons();
        UpdateAllLabels();

        if (resetDefaultsButton != null)
            resetDefaultsButton.onClick.AddListener(ResetDefaults);

        if (backButton != null)
            backButton.onClick.AddListener(CloseSettings);

        if (rebindOverlay != null)
            rebindOverlay.SetActive(false);
    }

    private void Update()
    {
        if (!isRebinding) return;

        // Listen for any key press during rebinding
        if (Input.anyKeyDown)
        {
            // Find which key was pressed
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    // Don't allow Escape to be bound (used to cancel)
                    if (key == KeyCode.Escape)
                    {
                        CancelRebind();
                        return;
                    }

                    CompleteRebind(key);
                    return;
                }
            }
        }
    }

    // ========================================
    // AUDIO
    // ========================================

    private void SetupSliders()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.minValue = 0f;
            masterVolumeSlider.maxValue = 1f;
            masterVolumeSlider.value = settings.masterVolume;
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 1f;
            musicVolumeSlider.value = settings.musicVolume;
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.minValue = 0f;
            sfxVolumeSlider.maxValue = 1f;
            sfxVolumeSlider.value = settings.sfxVolume;
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }

    private void OnMasterVolumeChanged(float value)
    {
        settings.masterVolume = value;
        AudioListener.volume = value;
        UpdateVolumeLabel(masterVolumeLabel, "Master", value);
        settings.Save();
    }

    private void OnMusicVolumeChanged(float value)
    {
        settings.musicVolume = value;
        UpdateVolumeLabel(musicVolumeLabel, "Music", value);
        settings.Save();
        
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.SetMusicVolume(value);
        }
    }

    private void OnSFXVolumeChanged(float value)
    {
        settings.sfxVolume = value;
        UpdateVolumeLabel(sfxVolumeLabel, "SFX", value);
        settings.Save();
        
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.SetSFXVolume(value);
        }
    }

    private void UpdateVolumeLabel(TextMeshProUGUI label, string name, float value)
    {
        if (label != null)
            label.text = $"{name}: {Mathf.RoundToInt(value * 100)}%";
    }

    // ========================================
    // KEY BINDINGS
    // ========================================

    private void SetupKeyBindButtons()
    {
        BindButton(moveUpButton, "Move Up", k => { settings.moveUp = k; UpdateKeyLabel(moveUpLabel, "Move Up", k); });
        BindButton(moveDownButton, "Move Down", k => { settings.moveDown = k; UpdateKeyLabel(moveDownLabel, "Move Down", k); });
        BindButton(moveLeftButton, "Move Left", k => { settings.moveLeft = k; UpdateKeyLabel(moveLeftLabel, "Move Left", k); });
        BindButton(moveRightButton, "Move Right", k => { settings.moveRight = k; UpdateKeyLabel(moveRightLabel, "Move Right", k); });
        BindButton(attackButton, "Attack", k => { settings.attack = k; UpdateKeyLabel(attackLabel, "Attack", k); });
        BindButton(pickupButton, "Pickup", k => { settings.pickupWeapon = k; UpdateKeyLabel(pickupLabel, "Pickup", k); });
        BindButton(menuButton, "Menu", k => { settings.openMenu = k; UpdateKeyLabel(menuLabel, "Menu", k); });
        BindButton(interactButton, "Interact", k => { settings.interact = k; UpdateKeyLabel(interactLabel, "Interact", k); });
    }

    private void BindButton(Button button, string actionName, Action<KeyCode> callback)
    {
        if (button == null) return;
        button.onClick.AddListener(() => StartRebind(actionName, callback));
    }

    private void StartRebind(string actionName, Action<KeyCode> callback)
    {
        isRebinding = true;
        currentRebindCallback = callback;

        if (rebindOverlay != null)
            rebindOverlay.SetActive(true);

        if (rebindPromptText != null)
            rebindPromptText.text = $"Press any key for '{actionName}'\n\n(ESC to cancel)";

        Debug.Log($"Rebinding: {actionName} - Press any key...");
    }

    private void CompleteRebind(KeyCode key)
    {
        isRebinding = false;

        currentRebindCallback?.Invoke(key);
        currentRebindCallback = null;

        if (rebindOverlay != null)
            rebindOverlay.SetActive(false);

        settings.Save();
        Debug.Log($"Rebound to: {key}");
    }

    private void CancelRebind()
    {
        isRebinding = false;
        currentRebindCallback = null;

        if (rebindOverlay != null)
            rebindOverlay.SetActive(false);

        Debug.Log("Rebind cancelled");
    }

    // ========================================
    // LABELS
    // ========================================

    private void UpdateAllLabels()
    {
        UpdateVolumeLabel(masterVolumeLabel, "Master", settings.masterVolume);
        UpdateVolumeLabel(musicVolumeLabel, "Music", settings.musicVolume);
        UpdateVolumeLabel(sfxVolumeLabel, "SFX", settings.sfxVolume);

        UpdateKeyLabel(moveUpLabel, "Move Up", settings.moveUp);
        UpdateKeyLabel(moveDownLabel, "Move Down", settings.moveDown);
        UpdateKeyLabel(moveLeftLabel, "Move Left", settings.moveLeft);
        UpdateKeyLabel(moveRightLabel, "Move Right", settings.moveRight);
        UpdateKeyLabel(attackLabel, "Attack", settings.attack);
        UpdateKeyLabel(pickupLabel, "Pickup", settings.pickupWeapon);
        UpdateKeyLabel(menuLabel, "Menu", settings.openMenu);
        UpdateKeyLabel(interactLabel, "Interact", settings.interact);
    }

    private void UpdateKeyLabel(TextMeshProUGUI label, string name, KeyCode key)
    {
        if (label != null)
            label.text = FormatKeyName(key);
    }

    /// <summary>
    /// Human-readable key name formatting.
    /// </summary>
    private string FormatKeyName(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.Mouse0: return "LMB";
            case KeyCode.Mouse1: return "RMB";
            case KeyCode.Mouse2: return "MMB";
            case KeyCode.Return: return "Enter";
            case KeyCode.Escape: return "Esc";
            default: return key.ToString();
        }
    }

    // ========================================
    // ACTIONS
    // ========================================

    private void ResetDefaults()
    {
        settings.ResetToDefaults();
        settings.Save();

        // Update UI
        if (masterVolumeSlider != null) masterVolumeSlider.value = settings.masterVolume;
        if (musicVolumeSlider != null) musicVolumeSlider.value = settings.musicVolume;
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = settings.sfxVolume;

        AudioListener.volume = settings.masterVolume;
        UpdateAllLabels();

        Debug.Log("Settings reset to defaults!");
    }

    private void CloseSettings()
    {
        gameObject.SetActive(false);
    }

    public void OpenSettings()
    {
        gameObject.SetActive(true);
    }
}
