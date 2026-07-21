using UnityEngine;

/// <summary>
/// Singleton GameManager that tracks global game state: score, game state, pause.
/// Persists across scenes using DontDestroyOnLoad.
/// Integrates with PRU213's existing PauseController.
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance => _instance;

    [Header("Game State")]
    public int score = 0;
    public int currentLevel = 1;

    [Header("Potion Recovery System")]
    public int enemiesKilledStreak = 0;
    public int killStreakForPotion = 3; // Every 3 kills = +1 HP & MP Potion

    private bool isPlaying = true;
    public bool IsPlaying => isPlaying;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Add score points (called when enemies die).
    /// </summary>
    public void AddScore(int points)
    {
        score += points;
        Debug.Log($"Score: {score} (+{points})");
    }

    /// <summary>
    /// Called when any enemy is killed to track kill streaks and grant potion rewards.
    /// </summary>
    public void OnEnemyKilled()
    {
        enemiesKilledStreak++;
        Debug.Log($"[GameManager] Enemy killed. Current kill streak: {enemiesKilledStreak}/{killStreakForPotion}");

        if (enemiesKilledStreak >= killStreakForPotion)
        {
            enemiesKilledStreak = 0;
            Player player = FindAnyObjectByType<Player>();
            if (player != null)
            {
                player.AddPotions(1, 1);
                Debug.Log($"[GameManager] Kill streak reward! Granted 1 HP Potion and 1 MP Potion.");
            }
        }
    }

    /// <summary>
    /// Reset game state for a new run.
    /// </summary>
    public void ResetGame()
    {
        score = 0;
        currentLevel = 1;
        enemiesKilledStreak = 0;
        isPlaying = true;
    }

    public void PauseGame()
    {
        isPlaying = false;
        Time.timeScale = 0f;
        PauseController.SetPause(true);
    }

    public void ResumeGame()
    {
        isPlaying = true;
        Time.timeScale = 1f;
        PauseController.SetPause(false);
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
