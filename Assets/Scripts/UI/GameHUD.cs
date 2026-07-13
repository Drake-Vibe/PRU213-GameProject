using UnityEngine;
using TMPro;

/// <summary>
/// In-game HUD displaying health, score, weapon info, and dungeon floor.
/// Updates every frame from GameManager and Player state.
/// </summary>
public class GameHUD : MonoBehaviour
{
    [Header("Score & Level")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Weapon Info")]
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI weaponDamageText;
    [SerializeField] private GameObject weaponInfoPanel;

    private Player player;

    private void Start()
    {
        player = FindAnyObjectByType<Player>();

        if (weaponInfoPanel != null)
            weaponInfoPanel.SetActive(false);
    }

    private void Update()
    {
        UpdateScoreDisplay();
        UpdateLevelDisplay();
        UpdateWeaponDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText == null) return;

        GameManager gm = GameManager.Instance;
        if (gm != null)
        {
            scoreText.text = $"SCORE: {gm.score}";
        }
    }

    private void UpdateLevelDisplay()
    {
        if (levelText == null) return;

        GameManager gm = GameManager.Instance;
        if (gm != null)
        {
            levelText.text = $"FLOOR {gm.currentLevel}";
        }
    }

    private void UpdateWeaponDisplay()
    {
        if (player == null || weaponInfoPanel == null) return;

        if (player.currentWeapon != null)
        {
            BaseWeapon weapon = player.currentWeapon.GetComponent<BaseWeapon>();
            if (weapon != null)
            {
                weaponInfoPanel.SetActive(true);

                if (weaponNameText != null)
                    weaponNameText.text = weapon.weaponName;

                if (weaponDamageText != null)
                    weaponDamageText.text = $"DMG: {weapon.damage}";
            }
        }
        else
        {
            weaponInfoPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Show temporary weapon pickup info.
    /// </summary>
    public void ShowWeaponPickupInfo(string name, float damage, float damageDiff)
    {
        if (weaponInfoPanel == null) return;

        weaponInfoPanel.SetActive(true);

        string prefix = damageDiff >= 0 ? "+" : "";

        if (weaponNameText != null)
            weaponNameText.text = $"{name} (Q)";

        if (weaponDamageText != null)
            weaponDamageText.text = $"{damage} DMG ({prefix}{damageDiff})";
    }
}
