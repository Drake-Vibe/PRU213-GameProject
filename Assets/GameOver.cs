using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public void RestartGame()
    {
        Debug.Log("Restarting game...");
        
        // Reset save load flag so we start a clean game
        PlayerPrefs.SetInt("ShouldLoadSave", 0);
        PlayerPrefs.Save();

        LevelManager lm = LevelManager.Instance;
        if (lm != null)
        {
            lm.StartGame(); // Loads UI-Default with loading screen
        }
        else
        {
            SceneManager.LoadScene("UI-Default");
        }
    }

    public void LoadMenu()
    {
        Debug.Log("Returning to main menu...");
        
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
