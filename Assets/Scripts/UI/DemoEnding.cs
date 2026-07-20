using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Controller for the Demo Ending screen shown after clearing the Boss Fight.
/// Displays final score and handles returning to the Main Menu.
/// </summary>
public class DemoEnding : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;

    private void Start()
    {
        // Display final player stats (Score only, no levels completed)
        GameManager gm = GameManager.Instance;
        if (gm != null)
        {
            if (scoreText != null)
            {
                scoreText.text = $"FINAL SCORE: {gm.score}";
            }
        }
        else
        {
            if (scoreText != null) scoreText.text = "FINAL SCORE: 0";
        }

        // Clean up persistent instances that are no longer needed
        Player.DestroyInstance();
        PlayerHUD.DestroyInstance();
        MenuController.DestroyInstance();

        // Unlock cursor for UI interaction
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    /// <summary>
    /// Return to Main Menu.
    /// </summary>
    public void LoadMenu()
    {
        Debug.Log("Returning to main menu from Demo Ending screen...");

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
