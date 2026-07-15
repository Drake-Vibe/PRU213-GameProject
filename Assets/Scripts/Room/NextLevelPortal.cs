using UnityEngine;
using TMPro;

/// <summary>
/// Portal that takes the player to the next dungeon level.
/// Shows a prompt when the player is nearby, and loads the next scene on keypress.
/// </summary>
public class NextLevelPortal : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshPro promptText;
    [SerializeField] private string promptMessage = "Press ENTER to continue";

    [Header("Config")]
    [SerializeField] private KeyCode activateKey = KeyCode.Return;

    private bool playerInRange = false;

    private void Start()
    {
        if (promptText != null)
        {
            promptText.text = promptMessage;
            promptText.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        // Show/hide prompt
        if (promptText != null)
            promptText.gameObject.SetActive(playerInRange);

        // Activate portal
        if (playerInRange && Input.GetKeyDown(activateKey))
        {
            LoadNextLevel();
        }
    }

    private void LoadNextLevel()
    {
        Debug.Log("Loading next level...");

        LevelManager levelManager = FindAnyObjectByType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.NextLevel();
        }
        else
        {
            Debug.LogWarning("NextLevelPortal: No LevelManager found in scene!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Tags.PLAYER))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag(Tags.PLAYER))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(Tags.PLAYER))
        {
            playerInRange = false;
        }
    }
}
