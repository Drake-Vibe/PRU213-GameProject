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

    [Header("Door Barrier & State")]
    [SerializeField] private GameObject solidBarrier;
    [SerializeField] private bool isLocked = false;

    private bool playerInRange = false;

    private void Start()
    {
        if (promptText != null)
        {
            promptText.text = promptMessage;
            MeshRenderer mr = promptText.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                mr.sortingLayerName = "Player";
                mr.sortingOrder = 10;
            }
            promptText.gameObject.SetActive(false);
        }

        UpdateBarrierState();
    }

    public void SetLocked(bool locked)
    {
        isLocked = locked;
        UpdateBarrierState();
    }

    private void UpdateBarrierState()
    {
        if (solidBarrier != null)
        {
            solidBarrier.SetActive(isLocked);
        }
    }

    private void Update()
    {
        // Show/hide prompt (only when unlocked)
        if (promptText != null)
            promptText.gameObject.SetActive(playerInRange && !isLocked);

        // Activate portal
        if (playerInRange && !isLocked && Input.GetKeyDown(activateKey))
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
