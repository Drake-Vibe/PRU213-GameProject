using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

/// <summary>
/// Manages cutscene/intro slideshow playback.
/// Features:
/// - Multiple pages with background image + narrative text
/// - Auto-advance after configurable duration (default 10s per page)
/// - Fade in/out transitions between pages
/// - Skip by holding Space for 1.5 seconds (with visual progress indicator)
/// - Callback when cutscene completes (for loading screen trigger)
/// 
/// Setup in Unity:
/// 1. Create a Canvas with this script
/// 2. Add child: Image (fullscreen) for background
/// 3. Add child: TextMeshProUGUI for narrative text
/// 4. Add child: Image + TextMeshProUGUI for skip indicator
/// 5. Add child: CanvasGroup for fade overlay
/// 6. Assign CutscenePage array in Inspector
/// </summary>
public class CutsceneManager : MonoBehaviour
{
    [Header("Pages")]
    [Tooltip("Array of cutscene pages to display in order.")]
    public CutscenePage[] pages;

    [Header("UI References")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private TextMeshProUGUI narrativeText;
    [SerializeField] private CanvasGroup fadeOverlay;

    [Header("Skip UI")]
    [SerializeField] private GameObject skipIndicator;
    [SerializeField] private Image skipProgressFill;
    [SerializeField] private TextMeshProUGUI skipText;

    [Header("Transition Settings")]
    [SerializeField] private float fadeDuration = 0.8f;
    [SerializeField] private float holdToSkipDuration = 1.5f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    // Events
    public event Action OnCutsceneComplete;

    // State
    private int currentPageIndex = -1;
    private bool isPlaying = false;
    private bool isSkipping = false;
    private float skipHoldTime = 0f;
    private Coroutine playCoroutine;

    /// <summary>
    /// Whether cutscene pages are assigned and valid.
    /// </summary>
    public bool HasCutscene => pages != null && pages.Length > 0;

    /// <summary>
    /// Whether the cutscene is currently playing.
    /// </summary>
    public bool IsPlaying => isPlaying;

    private void Start()
    {
        // Hide everything initially
        if (skipIndicator != null)
            skipIndicator.SetActive(false);

        if (!isPlaying)
        {
            gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isPlaying) return;

        HandleSkipInput();
    }

    /// <summary>
    /// Start playing the cutscene from the first page.
    /// </summary>
    public void PlayCutscene()
    {
        if (!HasCutscene)
        {
            Debug.LogWarning("CutsceneManager: No pages assigned!");
            OnCutsceneComplete?.Invoke();
            return;
        }

        gameObject.SetActive(true);
        isPlaying = true;
        currentPageIndex = -1;
        skipHoldTime = 0f;

        // Start with black screen
        if (fadeOverlay != null)
            fadeOverlay.alpha = 1f;

        playCoroutine = StartCoroutine(PlayAllPages());
    }

    /// <summary>
    /// Handle hold-Space-to-skip logic.
    /// Shows a progress indicator while holding, resets when released.
    /// </summary>
    private void HandleSkipInput()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            skipHoldTime += Time.unscaledDeltaTime;

            // Show skip indicator
            if (skipIndicator != null)
                skipIndicator.SetActive(true);

            // Update skip progress
            float progress = Mathf.Clamp01(skipHoldTime / holdToSkipDuration);
            if (skipProgressFill != null)
                skipProgressFill.fillAmount = progress;

            if (skipText != null)
                skipText.text = progress < 1f ? "Hold SPACE to skip..." : "Skipping...";

            // Skip when held long enough
            if (skipHoldTime >= holdToSkipDuration && !isSkipping)
            {
                isSkipping = true;
                SkipCutscene();
            }
        }
        else
        {
            // Reset when released
            skipHoldTime = 0f;

            if (skipIndicator != null)
                skipIndicator.SetActive(false);

            if (skipProgressFill != null)
                skipProgressFill.fillAmount = 0f;
        }
    }

    /// <summary>
    /// Skip the entire cutscene immediately.
    /// </summary>
    private void SkipCutscene()
    {
        Debug.Log("Cutscene skipped!");

        if (playCoroutine != null)
            StopCoroutine(playCoroutine);

        if (audioSource != null)
            audioSource.Stop();

        StartCoroutine(EndCutscene());
    }

    /// <summary>
    /// Coroutine that plays all pages in sequence.
    /// </summary>
    private IEnumerator PlayAllPages()
    {
        for (int i = 0; i < pages.Length; i++)
        {
            if (isSkipping) yield break;

            currentPageIndex = i;
            CutscenePage page = pages[i];

            // Set content
            SetPageContent(page);

            // Play audio if available
            if (page.pageAudio != null && audioSource != null)
            {
                audioSource.clip = page.pageAudio;
                audioSource.Play();
            }

            // Fade in
            yield return StartCoroutine(Fade(1f, 0f, fadeDuration));

            // Wait for page duration
            float elapsed = 0f;
            while (elapsed < page.duration && !isSkipping)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (isSkipping) yield break;

            // Fade out
            yield return StartCoroutine(Fade(0f, 1f, fadeDuration));
        }

        // All pages done
        yield return StartCoroutine(EndCutscene());
    }

    /// <summary>
    /// Set the visual content for a page.
    /// </summary>
    private void SetPageContent(CutscenePage page)
    {
        if (backgroundImage != null)
        {
            if (page.backgroundImage != null)
            {
                backgroundImage.sprite = page.backgroundImage;
                backgroundImage.color = Color.white;
                backgroundImage.gameObject.SetActive(true);
            }
            else
            {
                backgroundImage.gameObject.SetActive(false);
            }
        }

        if (narrativeText != null)
        {
            narrativeText.text = page.narrativeText;
            narrativeText.gameObject.SetActive(!string.IsNullOrEmpty(page.narrativeText));
        }
    }

    /// <summary>
    /// Fade the overlay from startAlpha to endAlpha over duration.
    /// </summary>
    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        if (fadeOverlay == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            // Smooth ease curve
            t = t * t * (3f - 2f * t);
            fadeOverlay.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }
        fadeOverlay.alpha = endAlpha;
    }

    /// <summary>
    /// End the cutscene, fade to black, fire completion event.
    /// </summary>
    private IEnumerator EndCutscene()
    {
        // Fade to black
        yield return StartCoroutine(Fade(fadeOverlay != null ? fadeOverlay.alpha : 0f, 1f, fadeDuration));

        isPlaying = false;
        isSkipping = false;

        // Fire completion event (MainMenu will trigger loading screen)
        OnCutsceneComplete?.Invoke();

        gameObject.SetActive(false);
    }
}
