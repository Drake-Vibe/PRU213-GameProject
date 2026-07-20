using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Auto-setup for the DemoEnding scene. Generates Canvas, score display, thank you text,
/// exit button, and links everything programmatically.
/// Run: Tools → PRU213 Setup → 🎉 Setup Demo Ending Scene
/// </summary>
public class PRU213DemoEndingSetup : EditorWindow
{
    [MenuItem("Tools/PRU213 Setup/🎉 Setup Demo Ending Scene", priority = 22)]
    public static void SetupDemoEndingScene()
    {
        if (!EditorUtility.DisplayDialog(
            "Setup Demo Ending Scene",
            "Sẽ tự động tạo:\n" +
            "• Canvas DemoEnding (Background xám tối sang trọng)\n" +
            "• Chữ lớn phát sáng \"DEMO COMPLETED\"\n" +
            "• Dòng chữ cảm ơn chân thành từ đội ngũ phát triển\n" +
            "• Text hiển thị Điểm Số cuối cùng (không hiển thị số màn)\n" +
            "• Nút EXIT TO MENU quay về MainMenu chính\n" +
            "• EventSystem hỗ trợ chuột/phím\n" +
            "• Tự động liên kết nhạc nền '8 - End Theme.ogg' vào LevelManager\n\n" +
            "Vui lòng chạy trong scene DemoEnding trống!",
            "Tạo!", "Hủy"))
        {
            return;
        }

        // Clean up any existing duplicate UI elements in the scene
        GameObject oldCanvasEnding = GameObject.Find("DemoEndingCanvas");
        if (oldCanvasEnding != null) DestroyImmediate(oldCanvasEnding);

        GameObject oldCanvasComplete = GameObject.Find("DemoCompleteCanvas");
        if (oldCanvasComplete != null) DestroyImmediate(oldCanvasComplete);

        GameObject oldEventSystem = GameObject.Find("EventSystem");
        if (oldEventSystem != null) DestroyImmediate(oldEventSystem);

        // Create Canvas
        GameObject canvasObj = new GameObject("DemoEndingCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 90;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // Add DemoEnding script component
        DemoEnding demoScript = canvasObj.AddComponent<DemoEnding>();

        // Background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform, false);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.08f, 0.08f, 0.1f, 0.96f); // Sleek dark grey/blue
        
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Title Text
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "DEMO COMPLETED";
        titleText.fontSize = 80;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = new Color(1f, 0.82f, 0f, 1f); // Bright Gold
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0, 180);
        titleRect.sizeDelta = new Vector2(1000, 100);

        // Subtitle Text (Thank you)
        GameObject subObj = new GameObject("ThankYouText");
        subObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI subText = subObj.AddComponent<TextMeshProUGUI>();
        subText.text = "Cảm ơn bạn đã trải nghiệm bản chơi thử (DEMO) của chúng tôi!\nHy vọng bạn đã có những giây phút thư giãn tuyệt vời.";
        subText.fontSize = 28;
        subText.alignment = TextAlignmentOptions.Center;
        subText.color = new Color(0.85f, 0.85f, 0.88f, 1f); // Muted off-white
        
        RectTransform subRect = subObj.GetComponent<RectTransform>();
        subRect.anchorMin = new Vector2(0.5f, 0.5f);
        subRect.anchorMax = new Vector2(0.5f, 0.5f);
        subRect.anchoredPosition = new Vector2(0, 60);
        subRect.sizeDelta = new Vector2(1200, 100);

        // Stats - Score Text
        GameObject scoreObj = new GameObject("ScoreText");
        scoreObj.transform.SetParent(canvasObj.transform, false);
        TextMeshProUGUI scoreText = scoreObj.AddComponent<TextMeshProUGUI>();
        scoreText.text = "FINAL SCORE: 0000";
        scoreText.fontSize = 36;
        scoreText.alignment = TextAlignmentOptions.Center;
        scoreText.color = new Color(0.9f, 0.72f, 0.1f, 1f); // Pale Gold
        
        RectTransform scoreRect = scoreObj.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0.5f, 0.5f);
        scoreRect.anchorMax = new Vector2(0.5f, 0.5f);
        scoreRect.anchoredPosition = new Vector2(0, -60);
        scoreRect.sizeDelta = new Vector2(800, 60);

        // Link references to DemoEnding script
        SerializedObject demoSo = new SerializedObject(demoScript);
        demoSo.FindProperty("scoreText").objectReferenceValue = scoreText;
        demoSo.ApplyModifiedProperties();

        // Exit Button
        GameObject exitBtnObj = CreateTextButton(canvasObj.transform, "ExitButton", "EXIT TO MENU", new Vector2(0, -180));

        // Connect button events programmatically via Editor tools
        Button exitBtn = exitBtnObj.GetComponent<Button>();
        UnityEditor.Events.UnityEventTools.AddPersistentListener(exitBtn.onClick, demoScript.LoadMenu);

        // Create EventSystem
        CreateEventSystem();

        // Automatically configure LevelManager's demo BGM if it is present
        LevelManager lm = Object.FindAnyObjectByType<LevelManager>();
        if (lm != null)
        {
            SerializedObject lmSo = new SerializedObject(lm);
            AudioClip endMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Art/Sprites/GameAssets/Ninja Adventure - Asset Pack/Audio/Musics/8 - End Theme.ogg");
            if (endMusic != null)
            {
                lmSo.FindProperty("demoCompleteMusic").objectReferenceValue = endMusic;
                lmSo.ApplyModifiedProperties();
                Debug.Log("✓ Assigned '8 - End Theme.ogg' to LevelManager's demoCompleteMusic");
            }
            else
            {
                Debug.LogWarning("⚠️ Could not find '8 - End Theme.ogg' BGM asset. Music assignment skipped.");
            }
        }

        // Mark dirty to save changes
        EditorUtility.SetDirty(canvasObj);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("✅ Demo Ending Setup Done!",
            "Đã tạo xong giao diện DemoEnding và liên kết nhạc nền thành công!\nĐừng quên thêm scene này vào Build Settings ngay dưới Boss Fight.", "OK");
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
        btnRect.sizeDelta = new Vector2(400, 60);

        // Label text object
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 30;
        tmp.alignment = TextAlignmentOptions.Center;

        // Target the text component for state color changes
        btn.targetGraphic = tmp;

        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.7f, 0.7f, 0.75f, 1f);      // Muted light grey
        cb.highlightedColor = new Color(1f, 0.82f, 0f, 1f);    // Bright gold on hover
        cb.pressedColor = new Color(0.8f, 0.65f, 0f, 1f);      // Darker gold on click
        cb.selectedColor = new Color(1f, 0.82f, 0f, 1f);
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
            Debug.Log("✓ EventSystem created for DemoEnding scene");
        }
    }
}
