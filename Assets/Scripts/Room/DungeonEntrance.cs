using UnityEngine;
using TMPro;

/// <summary>
/// Portal placed in the Hub/Default Map (Game.unity) that takes the player into dungeon floors.
/// When the player walks near and presses the interact key, loads the first dungeon level.
/// 
/// Game Flow:
///   MainMenu → (Cutscene) → Loading → Game.unity [HUB MAP] → DungeonEntrance → DungeonLevel1 → Floor 2...
/// 
/// Setup in Unity:
/// 1. Place in the Hub Map scene (Game.unity)
/// 2. Add BoxCollider2D (Is Trigger)
/// 3. Assign a sprite (e.g., door, staircase, portal)
/// 4. Set dungeonSceneName to the first dungeon scene
/// </summary>
public class DungeonEntrance : MonoBehaviour
{
    [Header("Config")]
    [Tooltip("Name of the first dungeon scene to load.")]
    [SerializeField] private string dungeonSceneName = "Level 1";

    [Tooltip("Key to press to enter the dungeon.")]
    [SerializeField] private KeyCode enterKey = KeyCode.Return;

    [Header("UI")]
    [SerializeField] private TextMeshPro promptText;
    [SerializeField] private string promptMessage = "Into The Dungeon\nPlease [Enter]";

    [Header("Visual")]
    [SerializeField] private SpriteRenderer portalVisual;
    [SerializeField] private Color glowColor = new Color(0.5f, 0.3f, 0.8f, 1f); // Purple glow

    private bool playerInRange = false;
    private Color originalColor;

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

        if (portalVisual != null)
            originalColor = portalVisual.color;
    }

    private void Update()
    {
        // Show/hide prompt
        if (promptText != null)
            promptText.gameObject.SetActive(playerInRange);

        // Pulse glow effect when player is near
        if (portalVisual != null && playerInRange)
        {
            float pulse = Mathf.Sin(Time.time * 3f) * 0.3f + 0.7f;
            portalVisual.color = Color.Lerp(originalColor, glowColor, pulse);
        }
        else if (portalVisual != null)
        {
            portalVisual.color = originalColor;
        }

        // Enter dungeon
        if (playerInRange && Input.GetKeyDown(enterKey))
        {
            EnterDungeon();
        }
    }

    private void EnterDungeon()
    {
        Debug.Log($"Entering dungeon: {dungeonSceneName}");

        LevelManager lm = LevelManager.Instance;
        if (lm != null)
        {
            // Use full loading screen when entering dungeon
            GameManager gm = GameManager.Instance;
            if (gm != null)
            {
                gm.currentLevel = 1;
            }
            lm.LoadDungeonLevel(dungeonSceneName);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(dungeonSceneName);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
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

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.5f, 0.3f, 0.8f, 0.5f);
        Gizmos.DrawWireCube(transform.position, new Vector3(1.5f, 1.5f, 0f));
        
        // Draw label
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.2f, 
            $"→ {dungeonSceneName}");
        #endif
    }
}
