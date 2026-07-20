using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHUD : MonoBehaviour
{
    public static BossHUD Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject bossPanel;
    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;

    private int maxHP = 500;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (bossPanel != null)
        {
            bossPanel.SetActive(false);
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
        Hide();
    }

    private void Start()
    {
        if (bossPanel != null)
        {
            bossPanel.SetActive(false);
        }
    }

    public void Initialize(string bossName, int currentHealth, int maxHealth)
    {
        maxHP = maxHealth;

        if (bossNameText != null)
        {
            bossNameText.text = bossName;
        }

        UpdateHealth(currentHealth, maxHealth);

        if (bossPanel != null)
        {
            bossPanel.SetActive(true);
        }
    }

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        maxHP = maxHealth;
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = Mathf.Max(0, currentHealth);
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.Max(0, currentHealth)} / {maxHealth}";
        }
    }

    public void Show()
    {
        if (bossPanel != null) bossPanel.SetActive(true);
    }

    public void Hide()
    {
        if (bossPanel != null) bossPanel.SetActive(false);
    }

    public void BindReferences(GameObject panel, TextMeshProUGUI nameTxt, Slider hpSlider, TextMeshProUGUI hpTxt)
    {
        bossPanel = panel;
        bossNameText = nameTxt;
        healthSlider = hpSlider;
        healthText = hpTxt;
    }
}
