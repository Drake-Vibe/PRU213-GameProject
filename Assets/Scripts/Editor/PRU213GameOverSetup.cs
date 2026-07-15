using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Auto-setup for the GameOver scene. Generates Canvas, title text, and restart/exit buttons.
/// Buttons are built using the minimalist floating text design.
/// Run: Tools → PRU213 Setup → 💀 Setup Game Over Scene
/// </summary>
public class PRU213GameOverSetup : EditorWindow
{
    [MenuItem("Tools/PRU213 Setup/💀 Setup Game Over Scene", priority = 21)]
    public static void SetupGameOverScene()
    {
        if (!EditorUtility.DisplayDialog(
            "Setup Game Over Scene",
            "Sẽ tự động tạo:\n" +
            "• Canvas GameOver (Background đỏ tối vương quyền)\n" +
            "• Chữ lớn phát sáng \"YOU DIED\" (Game Over)\n" +
            "• Nút RESTART (Khởi động lại về UI-Default)\n" +
            "• Nút EXIT (Thoát về màn hình MainMenu)\n" +
            "• EventSystem hỗ trợ chuột/phím\n\n" +
            "Vui lòng chạy trong scene GameOver trống!",
            "Tạo!", "Hủy"))
        {
            return;
        }

        // Create Canvas
        GameObject canvasObj = new GameObject("GameOverCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // Add GameOver script component
        GameOver gameOverScript = canvasObj.AddComponent<GameOver>();

        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform, false);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.12f, 0.02f, 0.02f, 0.95f); // Very dark crimson red
        
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Title Text
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "YOU DIED";
        titleText.fontSize = 96;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(0.85f, 0.15f, 0.15f, 1f); // Dark blood red
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0, 150);
        titleRect.sizeDelta = new Vector2(800, 120);

        // Buttons
        float buttonY = -50f;
        float buttonSpacing = 70f;

        GameObject restartBtnObj = CreateTextButton(canvasObj.transform, "RestartButton", "RESTART", new Vector2(0, buttonY));
        GameObject exitBtnObj = CreateTextButton(canvasObj.transform, "ExitButton", "EXIT TO MENU", new Vector2(0, buttonY - buttonSpacing));

        // Connect button events programmatically via Editor tools
        Button restartBtn = restartBtnObj.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(restartBtn.onClick, gameOverScript.RestartGame);

        Button exitBtn = exitBtnObj.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(exitBtn.onClick, gameOverScript.LoadMenu);

        // Create EventSystem
        CreateEventSystem();

        // Mark dirty to save changes
        EditorUtility.SetDirty(canvasObj);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("✅ Game Over Setup Done!",
            "Đã tạo xong giao diện GameOver!\nĐừng quên thêm scene này vào Build Settings.", "OK");
    }

    private static GameObject CreateTextButton(Transform parent, string name, string label, Vector2 position)
    {
        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        // Transparent image for raycasting click boundaries
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = Color.clear;

        Button btn = btnObj.AddComponent<Button>();

        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = position;
        btnRect.sizeDelta = new Vector2(350, 55);

        // Label text object
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 28;
        tmp.alignment = TextAlignmentOptions.Center;

        // Target the text component for state color changes
        btn.targetGraphic = tmp;

        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.65f, 0.65f, 0.7f, 1f);      // Muted dark silver-blue
        cb.highlightedColor = new Color(1f, 0.85f, 0.3f, 1f);   // Bright gold on hover
        cb.pressedColor = new Color(0.8f, 0.65f, 0.2f, 1f);     // Muted gold on click
        cb.selectedColor = new Color(1f, 0.85f, 0.3f, 1f);
        cb.disabledColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        btn.colors = cb;

        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        return btnObj;
    }

    private static void CreateEventSystem()
    {
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("✓ EventSystem created for GameOver scene");
        }
    }
}
