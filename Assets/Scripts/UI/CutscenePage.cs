using UnityEngine;

/// <summary>
/// Data for a single cutscene/intro page.
/// Each page displays a background image + narrative text for a set duration.
/// Assign in CutsceneManager's pages array via Inspector.
/// </summary>
[System.Serializable]
public class CutscenePage
{
    [Header("Visual")]
    [Tooltip("Background image for this page. Leave null for black background.")]
    public Sprite backgroundImage;

    [Header("Text")]
    [TextArea(3, 6)]
    [Tooltip("Narrative text displayed on this page.")]
    public string narrativeText = "";

    [Header("Timing")]
    [Tooltip("How long this page is displayed (seconds) before auto-advancing.")]
    public float duration = 10f;

    [Header("Audio (Optional)")]
    [Tooltip("Optional audio clip to play during this page.")]
    public AudioClip pageAudio;
}
