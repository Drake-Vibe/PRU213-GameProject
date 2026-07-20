using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Controls the Soul Knight-style HUD showing Health, Armor (Shield), and Energy (Mana).
/// Automatically queries player stats and updates sliders and texts in real-time.
/// </summary>
public class PlayerHUD : MonoBehaviour
{
    public static PlayerHUD Instance { get; private set; }

    [Header("Player Reference")]
    [SerializeField] private Player player;

    [Header("Health Bar UI")]
    public Slider healthSlider;
    public TextMeshProUGUI healthText;

    [Header("Shield Bar UI")]
    public Slider shieldSlider;
    public TextMeshProUGUI shieldText;

    [Header("Energy Bar UI")]
    public Slider energySlider;
    public TextMeshProUGUI energyText;

    [Header("Hotbar UI")]
    public TextMeshProUGUI hpPotionText;
    public TextMeshProUGUI mpPotionText;

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
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // Re-find player after scene load
        player = FindAnyObjectByType<Player>();

        // Enable / Disable canvas based on scene
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.enabled = (scene.name != "GameMainMenu" && scene.name != "GameOver" && scene.name != "DemoEnding");
        }
    }

    private void Start()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<Player>();
        }
    }

    private void Update()
    {
        if (player == null)
        {
            player = FindAnyObjectByType<Player>();
            if (player == null) return;
        }

        UpdateHealthUI();
        UpdateShieldUI();
        UpdateEnergyUI();
        UpdateHotbarUI();
    }

    private void UpdateHotbarUI()
    {
        if (hpPotionText != null)
        {
            hpPotionText.text = $"{player.healthPotions}";
        }
        if (mpPotionText != null)
        {
            mpPotionText.text = $"{player.manaPotions}";
        }
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = player.maxHealth;
            healthSlider.value = player.currentHealth;
        }
        if (healthText != null)
        {
            healthText.text = $"{player.currentHealth}/{player.maxHealth}";
        }
    }

    private void UpdateShieldUI()
    {
        if (shieldSlider != null)
        {
            shieldSlider.maxValue = player.maxArmor;
            shieldSlider.value = player.currentArmor;
        }
        if (shieldText != null)
        {
            shieldText.text = $"{player.currentArmor}/{player.maxArmor}";
        }
    }

    private void UpdateEnergyUI()
    {
        if (energySlider != null)
        {
            energySlider.maxValue = player.maxEnergy;
            energySlider.value = player.currentEnergy;
        }
        if (energyText != null)
        {
            energyText.text = $"{player.currentEnergy}/{player.maxEnergy}";
        }
    }
}
