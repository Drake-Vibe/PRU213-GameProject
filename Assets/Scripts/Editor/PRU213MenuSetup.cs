using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Auto-setup cho Main Menu scene: tạo UI buttons, settings panel, cutscene canvas, loading screen.
/// Run: Tools → PRU213 Setup → Setup Main Menu Scene
/// </summary>
public class PRU213MenuSetup : EditorWindow
{
    [MenuItem("Tools/PRU213 Setup/🎮 Setup Main Menu Scene", priority = 20)]
    public static void SetupMainMenuScene()
    {
        if (!EditorUtility.DisplayDialog(
            "Setup Main Menu",
            "Sẽ tự động tạo:\n" +
            "• Menu Canvas (4 nút: Start, Load, Setting, Quit)\n" +
            "• Settings Panel (Audio sliders + Key bindings)\n" +
            "• Cutscene Canvas (slideshow system)\n" +
            "• Loading Screen overlay\n" +
            "• Quit Confirmation popup\n\n" +
            "Chạy trong MainMenu scene!",
            "Tạo!", "Hủy"))
        {
            return;
        }

        CreateMenuCanvas();
        CreateSettingsPanel();
        CreateCutsceneCanvas();
        CreateLoadingScreen();
        LinkMainMenuReferences();
        CreateEventSystem();

        // Mark scene dirty so Unity knows to save all generated GameObjects and references
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("✅ Main Menu Setup Done!",
            "Đã tạo xong:\n" +
            "• 4 menu buttons\n" +
            "• Settings panel (Audio + Key bindings)\n" +
            "• Cutscene canvas (thêm images trong Inspector)\n" +
            "• Loading screen với progress bar + tips\n\n" +
            "Bạn cần:\n" +
            "1. Thêm hình/text vào CutsceneManager → Pages\n" +
            "2. Tùy chỉnh màu sắc/font nếu muốn\n" +
            "3. Đảm bảo GameManager + LevelManager tồn tại",
            "OK");
    }

    // ========================================
    // MENU CANVAS
    // ========================================
    private static void CreateMenuCanvas()
    {
        // Check if already exists
        MainMenu existingMenu = Object.FindAnyObjectByType<MainMenu>();
        if (existingMenu != null)
        {
            Debug.Log("MainMenu already exists, skipping canvas creation.");
            return;
        }

        GameObject canvas = CreateCanvas("MenuCanvas", 90);

        // Background image setup (using Assets/Image/bg.jpg)
        GameObject bg = CreateImage(canvas.transform, "Background", Color.white);
        StretchFull(bg);
        Image bgImg = bg.GetComponent<Image>();
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Image/bg.jpg");
        if (bgSprite != null)
        {
            bgImg.sprite = bgSprite;
        }
        else
        {
            Debug.LogWarning("Could not find background image at Assets/Image/bg.jpg. Using solid dark color instead.");
            bgImg.color = new Color(0.05f, 0.05f, 0.15f, 0.95f);
        }

        // Title
        GameObject title = CreateTMPText(canvas.transform, "TitleText", "DUNGEON CRAWLER",
            new Vector2(0, 200), TextAlignmentOptions.Center, 64, new Color(0.9f, 0.75f, 0.3f));
        RectTransform titleRect = title.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.sizeDelta = new Vector2(800, 100);

        // Subtitle
        CreateTMPText(canvas.transform, "SubtitleText", "A Soul Knight-Inspired Adventure",
            new Vector2(0, 140), TextAlignmentOptions.Center, 20, new Color(0.7f, 0.7f, 0.8f));

        // Buttons container
        float buttonY = 40f;
        float buttonSpacing = 65f;

        CreateMenuButton(canvas.transform, "StartButton", ">  START", 
            new Vector2(0, buttonY), new Color(0.2f, 0.7f, 0.3f));
        CreateMenuButton(canvas.transform, "LoadButton", "LOAD", 
            new Vector2(0, buttonY - buttonSpacing), new Color(0.3f, 0.5f, 0.8f));
        CreateMenuButton(canvas.transform, "SettingsButton", "SETTINGS", 
            new Vector2(0, buttonY - buttonSpacing * 2), new Color(0.5f, 0.5f, 0.6f));
        CreateMenuButton(canvas.transform, "QuitButton", "QUIT", 
            new Vector2(0, buttonY - buttonSpacing * 3), new Color(0.7f, 0.25f, 0.25f));

        // No Save File Message
        GameObject noSaveMsg = CreateTMPText(canvas.transform, "NoSaveMessage",
            "[!] No save file found!", new Vector2(0, -280),
            TextAlignmentOptions.Center, 22, new Color(1f, 0.5f, 0.3f));
        noSaveMsg.SetActive(false);

        // Quit Confirmation Panel
        CreateQuitConfirmPanel(canvas.transform);

        // Add MainMenu script
        MainMenu menuScript = canvas.AddComponent<MainMenu>();

        // Link buttons via SerializedObject
        SerializedObject so = new SerializedObject(menuScript);
        so.FindProperty("gameSceneName").stringValue = "UI-Default";
        so.FindProperty("afterCutsceneScene").stringValue = "UI-Default";
        so.FindProperty("startButton").objectReferenceValue = canvas.transform.Find("StartButton").GetComponent<Button>();
        so.FindProperty("loadButton").objectReferenceValue = canvas.transform.Find("LoadButton").GetComponent<Button>();
        so.FindProperty("settingsButton").objectReferenceValue = canvas.transform.Find("SettingsButton").GetComponent<Button>();
        so.FindProperty("quitButton").objectReferenceValue = canvas.transform.Find("QuitButton").GetComponent<Button>();
        so.FindProperty("noSaveFileMessage").objectReferenceValue = noSaveMsg;
        so.FindProperty("confirmQuitPanel").objectReferenceValue = canvas.transform.Find("QuitConfirmPanel").gameObject;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(menuScript);

        Debug.Log("✓ Menu Canvas created");
    }

    private static void CreateQuitConfirmPanel(Transform parent)
    {
        GameObject panel = new GameObject("QuitConfirmPanel");
        panel.transform.SetParent(parent, false);

        Image panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0, 0, 0, 0.85f);
        StretchFull(panel);

        CreateTMPText(panel.transform, "QuitPrompt", "Are you sure you want to quit?",
            new Vector2(0, 40), TextAlignmentOptions.Center, 28, Color.white);

        CreateMenuButton(panel.transform, "QuitYesButton", "YES",
            new Vector2(-100, -40), new Color(0.7f, 0.25f, 0.25f), new Vector2(150, 50));

        CreateMenuButton(panel.transform, "QuitNoButton", "NO",
            new Vector2(100, -40), new Color(0.3f, 0.6f, 0.3f), new Vector2(150, 50));

        panel.SetActive(false);
    }

    // ========================================
    // SETTINGS PANEL
    // ========================================
    private static void CreateSettingsPanel()
    {
        if (Object.FindAnyObjectByType<SettingsManager>() != null)
        {
            Debug.Log("SettingsManager already exists, skipping.");
            return;
        }

        // Find menu canvas
        Canvas menuCanvas = null;
        foreach (Canvas c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (c.gameObject.name == "MenuCanvas")
            {
                menuCanvas = c;
                break;
            }
        }

        Transform parent = menuCanvas != null ? menuCanvas.transform : null;
        if (parent == null)
        {
            Debug.LogWarning("MenuCanvas not found for Settings panel");
            return;
        }

        GameObject settingsPanel = new GameObject("SettingsPanel");
        settingsPanel.transform.SetParent(parent, false);

        Image bg = settingsPanel.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.08f, 0.18f, 0.97f);
        StretchFull(settingsPanel);

        // Title
        CreateTMPText(settingsPanel.transform, "SettingsTitle", "SETTINGS",
            new Vector2(0, 280), TextAlignmentOptions.Center, 42, new Color(0.9f, 0.85f, 0.6f));

        // --- Audio Section ---
        CreateTMPText(settingsPanel.transform, "AudioHeader", "-- AUDIO --",
            new Vector2(-300, 200), TextAlignmentOptions.Left, 26, new Color(0.7f, 0.8f, 1f));

        CreateVolumeSlider(settingsPanel.transform, "MasterVolume", "Master: 100%", new Vector2(0, 155));
        CreateVolumeSlider(settingsPanel.transform, "MusicVolume", "Music: 80%", new Vector2(0, 110));
        CreateVolumeSlider(settingsPanel.transform, "SFXVolume", "SFX: 100%", new Vector2(0, 65));

        // --- Controls Section ---
        CreateTMPText(settingsPanel.transform, "ControlsHeader", "-- CONTROLS --",
            new Vector2(-300, 10), TextAlignmentOptions.Left, 26, new Color(0.7f, 0.8f, 1f));

        float keyY = -35f;
        float keySpacing = 38f;
        CreateKeyBindRow(settingsPanel.transform, "MoveUp", "Move Up", "[W]", new Vector2(0, keyY));
        CreateKeyBindRow(settingsPanel.transform, "MoveDown", "Move Down", "[S]", new Vector2(0, keyY - keySpacing));
        CreateKeyBindRow(settingsPanel.transform, "MoveLeft", "Move Left", "[A]", new Vector2(0, keyY - keySpacing * 2));
        CreateKeyBindRow(settingsPanel.transform, "MoveRight", "Move Right", "[D]", new Vector2(0, keyY - keySpacing * 3));
        CreateKeyBindRow(settingsPanel.transform, "Attack", "Attack", "[LMB]", new Vector2(0, keyY - keySpacing * 4));
        CreateKeyBindRow(settingsPanel.transform, "Pickup", "Pickup", "[Q]", new Vector2(0, keyY - keySpacing * 5));
        CreateKeyBindRow(settingsPanel.transform, "Menu", "Menu", "[Tab]", new Vector2(0, keyY - keySpacing * 6));
        CreateKeyBindRow(settingsPanel.transform, "Interact", "Interact", "[E]", new Vector2(0, keyY - keySpacing * 7));

        // Buttons
        CreateMenuButton(settingsPanel.transform, "ResetDefaultsButton", "Reset Defaults",
            new Vector2(-120, -350), new Color(0.6f, 0.4f, 0.2f), new Vector2(200, 45));
        CreateMenuButton(settingsPanel.transform, "BackButton", "← Back",
            new Vector2(120, -350), new Color(0.4f, 0.4f, 0.5f), new Vector2(200, 45));

        // Rebind Overlay
        GameObject rebindOverlay = new GameObject("RebindOverlay");
        rebindOverlay.transform.SetParent(settingsPanel.transform, false);
        Image rebindBg = rebindOverlay.AddComponent<Image>();
        rebindBg.color = new Color(0, 0, 0, 0.9f);
        StretchFull(rebindOverlay);

        CreateTMPText(rebindOverlay.transform, "RebindPrompt", "Press any key...\n\n(ESC to cancel)",
            Vector2.zero, TextAlignmentOptions.Center, 30, Color.white);

        rebindOverlay.SetActive(false);

        // Add SettingsManager
        SettingsManager sm = settingsPanel.AddComponent<SettingsManager>();

        // Link references
        SerializedObject so = new SerializedObject(sm);
        so.FindProperty("masterVolumeSlider").objectReferenceValue = settingsPanel.transform.Find("MasterVolumeSlider")?.GetComponent<Slider>();
        so.FindProperty("musicVolumeSlider").objectReferenceValue = settingsPanel.transform.Find("MusicVolumeSlider")?.GetComponent<Slider>();
        so.FindProperty("sfxVolumeSlider").objectReferenceValue = settingsPanel.transform.Find("SFXVolumeSlider")?.GetComponent<Slider>();

        so.FindProperty("masterVolumeLabel").objectReferenceValue = settingsPanel.transform.Find("MasterVolumeLabel")?.GetComponent<TextMeshProUGUI>();
        so.FindProperty("musicVolumeLabel").objectReferenceValue = settingsPanel.transform.Find("MusicVolumeLabel")?.GetComponent<TextMeshProUGUI>();
        so.FindProperty("sfxVolumeLabel").objectReferenceValue = settingsPanel.transform.Find("SFXVolumeLabel")?.GetComponent<TextMeshProUGUI>();

        so.FindProperty("moveUpButton").objectReferenceValue = settingsPanel.transform.Find("MoveUpButton")?.GetComponent<Button>();
        so.FindProperty("moveDownButton").objectReferenceValue = settingsPanel.transform.Find("MoveDownButton")?.GetComponent<Button>();
        so.FindProperty("moveLeftButton").objectReferenceValue = settingsPanel.transform.Find("MoveLeftButton")?.GetComponent<Button>();
        so.FindProperty("moveRightButton").objectReferenceValue = settingsPanel.transform.Find("MoveRightButton")?.GetComponent<Button>();
        so.FindProperty("attackButton").objectReferenceValue = settingsPanel.transform.Find("AttackButton")?.GetComponent<Button>();
        so.FindProperty("pickupButton").objectReferenceValue = settingsPanel.transform.Find("PickupButton")?.GetComponent<Button>();
        so.FindProperty("menuButton").objectReferenceValue = settingsPanel.transform.Find("MenuButton")?.GetComponent<Button>();
        so.FindProperty("interactButton").objectReferenceValue = settingsPanel.transform.Find("InteractButton")?.GetComponent<Button>();

        so.FindProperty("moveUpLabel").objectReferenceValue = settingsPanel.transform.Find("MoveUpLabel")?.GetComponent<TextMeshProUGUI>();
        so.FindProperty("moveDownLabel").objectReferenceValue = settingsPanel.transform.Find("MoveDownLabel")?.GetComponent<TextMeshProUGUI>();
        so.FindProperty("moveLeftLabel").objectReferenceValue = settingsPanel.transform.Find("MoveLeftLabel")?.GetComponent<TextMeshProUGUI>();
        so.FindProperty("moveRightLabel").objectReferenceValue = settingsPanel.transform.Find("MoveRightLabel")?.GetComponent<TextMeshProUGUI>();
        so.FindProperty("attackLabel").objectReferenceValue = settingsPanel.transform.Find("AttackLabel")?.GetComponent<TextMeshProUGUI>();
        so.FindProperty("pickupLabel").objectReferenceValue = settingsPanel.transform.Find("PickupLabel")?.GetComponent<TextMeshProUGUI>();
        so.FindProperty("menuLabel").objectReferenceValue = settingsPanel.transform.Find("MenuLabel")?.GetComponent<TextMeshProUGUI>();
        so.FindProperty("interactLabel").objectReferenceValue = settingsPanel.transform.Find("InteractLabel")?.GetComponent<TextMeshProUGUI>();

        so.FindProperty("resetDefaultsButton").objectReferenceValue = settingsPanel.transform.Find("ResetDefaultsButton")?.GetComponent<Button>();
        so.FindProperty("backButton").objectReferenceValue = settingsPanel.transform.Find("BackButton")?.GetComponent<Button>();

        so.FindProperty("rebindOverlay").objectReferenceValue = rebindOverlay;
        so.FindProperty("rebindPromptText").objectReferenceValue = rebindOverlay.transform.Find("RebindPrompt")?.GetComponent<TextMeshProUGUI>();

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(sm);

        settingsPanel.SetActive(false);
        Debug.Log("✓ Settings Panel created");
    }

    // ========================================
    // CUTSCENE CANVAS
    // ========================================
    private static void CreateCutsceneCanvas()
    {
        if (Object.FindAnyObjectByType<CutsceneManager>() != null)
        {
            Debug.Log("CutsceneManager already exists, skipping.");
            return;
        }

        GameObject canvas = CreateCanvas("CutsceneCanvas", 95);

        // Background Image (fullscreen)
        GameObject bgImage = CreateImage(canvas.transform, "BackgroundImage", Color.black);
        StretchFull(bgImage);

        // Narrative Text
        GameObject narText = CreateTMPText(canvas.transform, "NarrativeText", "",
            new Vector2(0, -250), TextAlignmentOptions.Center, 28, Color.white);
        RectTransform narRect = narText.GetComponent<RectTransform>();
        narRect.anchorMin = new Vector2(0.1f, 0f);
        narRect.anchorMax = new Vector2(0.9f, 0.3f);
        narRect.offsetMin = Vector2.zero;
        narRect.offsetMax = Vector2.zero;

        // Fade Overlay (CanvasGroup)
        GameObject fadeObj = CreateImage(canvas.transform, "FadeOverlay", Color.black);
        StretchFull(fadeObj);
        CanvasGroup fadeGroup = fadeObj.AddComponent<CanvasGroup>();
        fadeGroup.alpha = 1f;
        fadeGroup.blocksRaycasts = false;

        // Skip Indicator
        GameObject skipInd = new GameObject("SkipIndicator");
        skipInd.transform.SetParent(canvas.transform, false);
        RectTransform skipRect = skipInd.AddComponent<RectTransform>();
        skipRect.anchorMin = new Vector2(1, 0);
        skipRect.anchorMax = new Vector2(1, 0);
        skipRect.pivot = new Vector2(1, 0);
        skipRect.anchoredPosition = new Vector2(-30, 30);
        skipRect.sizeDelta = new Vector2(200, 50);

        Image skipBg = skipInd.AddComponent<Image>();
        skipBg.color = new Color(0, 0, 0, 0.6f);

        // Skip text
        GameObject skipTextObj = CreateTMPText(skipInd.transform, "SkipText", "Hold SPACE to skip...",
            Vector2.zero, TextAlignmentOptions.Center, 16, new Color(0.8f, 0.8f, 0.8f));
        RectTransform stRect = skipTextObj.GetComponent<RectTransform>();
        stRect.anchorMin = Vector2.zero;
        stRect.anchorMax = Vector2.one;
        stRect.offsetMin = new Vector2(5, 0);
        stRect.offsetMax = new Vector2(-30, 0);

        // Skip progress fill
        GameObject skipFill = new GameObject("SkipProgressFill");
        skipFill.transform.SetParent(skipInd.transform, false);
        Image fillImg = skipFill.AddComponent<Image>();
        fillImg.color = new Color(0.3f, 0.8f, 0.3f, 0.7f);
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillAmount = 0f;
        RectTransform fillRect = skipFill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        skipFill.transform.SetAsFirstSibling(); // Behind text

        skipInd.SetActive(false);

        // Add CutsceneManager
        CutsceneManager cm = canvas.AddComponent<CutsceneManager>();
        SerializedObject so = new SerializedObject(cm);
        so.FindProperty("backgroundImage").objectReferenceValue = bgImage.GetComponent<Image>();
        so.FindProperty("narrativeText").objectReferenceValue = narText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("fadeOverlay").objectReferenceValue = fadeGroup;
        so.FindProperty("skipIndicator").objectReferenceValue = skipInd;
        so.FindProperty("skipProgressFill").objectReferenceValue = fillImg;
        so.FindProperty("skipText").objectReferenceValue = skipTextObj.GetComponent<TextMeshProUGUI>();

        // Automatically populate the 6 cutscene pages
        SerializedProperty pagesProp = so.FindProperty("pages");
        pagesProp.ClearArray();
        
        string[] cutsceneTexts = new string[]
        {
            "Sau nhiều thế kỷ yên bình, mặt đất nứt toác và một tháp đá đen khổng lồ trỗi dậy từ lòng lục địa: Ngục Tối Vực Sâu (The Abyssal Dungeon).",
            "Quái vật tràn ra tàn phá làng mạc và vương quốc, biến rừng rậm và sông ngòi thành những vùng đất chết tha hóa.",
            "Ngục tối thực chất là phong ấn thần thánh giam giữ Cự Long Hủy Diệt (The Devouring Dragon). Nhưng giờ đây, phong ấn ấy đang dần sụp đổ.",
            "Một nhóm người bình thường may mắn sống sót, thức tỉnh với những dấu ấn hình rồng rực cháy trên tay - phước lành của các vị thần đã khuất.",
            "Bị dẫn dắt bởi số phận và thù hận, những người anh hùng lên đường tiến vào lòng Ngục Tối Vực Sâu để tìm kiếm câu trả lời.",
            "Càng tiến sâu xuống các tầng ngục, quái vật càng mạnh mẽ hơn, và Cự Long cổ xưa dưới lòng đất cũng đang dần thức giấc."
        };

        for (int i = 0; i < 6; i++)
        {
            pagesProp.InsertArrayElementAtIndex(i);
            SerializedProperty pageElement = pagesProp.GetArrayElementAtIndex(i);
            
            pageElement.FindPropertyRelative("narrativeText").stringValue = cutsceneTexts[i];
            pageElement.FindPropertyRelative("duration").floatValue = 10f;
            
            Sprite cutsceneSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Image/cutscene{i + 1}.png");
            if (cutsceneSprite != null)
            {
                pageElement.FindPropertyRelative("backgroundImage").objectReferenceValue = cutsceneSprite;
            }
            else
            {
                Debug.LogWarning($"Could not find cutscene image at Assets/Image/cutscene{i + 1}.png");
            }
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(cm);

        canvas.SetActive(false);
        Debug.Log("✓ Cutscene Canvas created");
    }

    // ========================================
    // LOADING SCREEN
    // ========================================
    private static void CreateLoadingScreen()
    {
        // Create or find LevelManager
        LevelManager lm = Object.FindAnyObjectByType<LevelManager>();
        if (lm == null)
        {
            GameObject lmObj = new GameObject("LevelManager");
            lm = lmObj.AddComponent<LevelManager>();
        }

        // Check if loading screen already exists
        if (lm.loadingScreen != null)
        {
            Debug.Log("Loading screen already exists.");
            return;
        }

        GameObject canvas = CreateCanvas("LoadingCanvas", 99);

        // Background
        GameObject bg = CreateImage(canvas.transform, "LoadingBG", new Color(0.05f, 0.05f, 0.12f, 1f));
        StretchFull(bg);

        // Title
        CreateTMPText(canvas.transform, "LoadingTitle", "LOADING...",
            new Vector2(0, 80), TextAlignmentOptions.Center, 42, new Color(0.9f, 0.8f, 0.4f));

        // Progress Bar Background
        GameObject barBg = new GameObject("ProgressBarBG");
        barBg.transform.SetParent(canvas.transform, false);
        Image barBgImg = barBg.AddComponent<Image>();
        barBgImg.color = new Color(0.15f, 0.15f, 0.2f, 1f);
        RectTransform barBgRect = barBg.GetComponent<RectTransform>();
        barBgRect.anchorMin = new Vector2(0.15f, 0.45f);
        barBgRect.anchorMax = new Vector2(0.85f, 0.48f);
        barBgRect.offsetMin = Vector2.zero;
        barBgRect.offsetMax = Vector2.zero;

        // Progress Bar Slider
        GameObject sliderObj = new GameObject("ProgressSlider");
        sliderObj.transform.SetParent(canvas.transform, false);
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 0;
        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.15f, 0.45f);
        sliderRect.anchorMax = new Vector2(0.85f, 0.48f);
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = Vector2.zero;

        // Fill area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = Vector2.zero;
        fillAreaRect.anchorMax = Vector2.one;
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.3f, 0.75f, 0.4f, 1f); // Green
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        slider.fillRect = fillRect;

        // Progress % text
        GameObject progressText = CreateTMPText(canvas.transform, "ProgressText", "0%",
            new Vector2(0, -20), TextAlignmentOptions.Center, 24, Color.white);
        RectTransform ptRect = progressText.GetComponent<RectTransform>();
        ptRect.anchorMin = new Vector2(0.5f, 0.4f);
        ptRect.anchorMax = new Vector2(0.5f, 0.4f);

        // Tips text
        GameObject tipsText = CreateTMPText(canvas.transform, "TipsText", "Loading...",
            new Vector2(0, -80), TextAlignmentOptions.Center, 18, new Color(0.6f, 0.7f, 0.8f));
        RectTransform ttRect = tipsText.GetComponent<RectTransform>();
        ttRect.anchorMin = new Vector2(0.15f, 0.25f);
        ttRect.anchorMax = new Vector2(0.85f, 0.35f);
        ttRect.offsetMin = Vector2.zero;
        ttRect.offsetMax = Vector2.zero;

        // Link to LevelManager
        SerializedObject so = new SerializedObject(lm);
        so.FindProperty("loadingScreen").objectReferenceValue = canvas;
        so.FindProperty("progressBar").objectReferenceValue = slider;
        so.FindProperty("progressText").objectReferenceValue = progressText.GetComponent<TextMeshProUGUI>();
        so.FindProperty("tipsText").objectReferenceValue = tipsText.GetComponent<TextMeshProUGUI>();
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(lm);

        canvas.SetActive(false);
        Debug.Log("✓ Loading Screen created");
    }

    // ========================================
    // LINK REFERENCES
    // ========================================
    private static void LinkMainMenuReferences()
    {
        MainMenu menu = Object.FindAnyObjectByType<MainMenu>();
        CutsceneManager cm = Object.FindAnyObjectByType<CutsceneManager>(FindObjectsInactive.Include);
        SettingsManager sm = Object.FindAnyObjectByType<SettingsManager>(FindObjectsInactive.Include);

        if (menu == null) return;

        SerializedObject so = new SerializedObject(menu);
        if (cm != null) so.FindProperty("cutsceneManager").objectReferenceValue = cm;
        if (sm != null) so.FindProperty("settingsManager").objectReferenceValue = sm;
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(menu);

        Debug.Log("✓ MainMenu references linked");
    }

    // ========================================
    // UTILITY METHODS
    // ========================================
    private static GameObject CreateCanvas(string name, int sortOrder)
    {
        GameObject canvasObj = new GameObject(name);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortOrder;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();
        return canvasObj;
    }

    private static GameObject CreateImage(Transform parent, string name, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        Image img = obj.AddComponent<Image>();
        img.color = color;
        return obj;
    }

    private static void StretchFull(GameObject obj)
    {
        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect == null) rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static GameObject CreateTMPText(Transform parent, string name, string text,
        Vector2 position, TextAlignmentOptions alignment, float fontSize, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = color;

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(600, 50);

        return obj;
    }

    private static void CreateMenuButton(Transform parent, string name, string label,
        Vector2 position, Color bgColor, Vector2? size = null)
    {
        Vector2 btnSize = size ?? new Vector2(320, 55);

        GameObject btnObj = new GameObject(name);
        btnObj.transform.SetParent(parent, false);

        // Make background fully transparent (only acts as click region)
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = Color.clear;

        Button btn = btnObj.AddComponent<Button>();

        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = position;
        btnRect.sizeDelta = btnSize;

        // Button label text
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(btnObj.transform, false);
        TextMeshProUGUI tmp = labelObj.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 28; // Slightly larger for text-only look
        tmp.alignment = TextAlignmentOptions.Center;
        
        // Target the text component for button state color transitions
        btn.targetGraphic = tmp;
        
        ColorBlock cb = btn.colors;
        cb.normalColor = new Color(0.65f, 0.65f, 0.7f, 1f);      // Muted dark silver-blue
        cb.highlightedColor = new Color(1f, 0.85f, 0.3f, 1f);   // Bright glowing gold on hover
        cb.pressedColor = new Color(0.8f, 0.65f, 0.2f, 1f);     // Dark gold on click
        cb.selectedColor = new Color(1f, 0.85f, 0.3f, 1f);      // Stay gold when active/selected
        cb.disabledColor = new Color(0.3f, 0.3f, 0.3f, 1f);
        btn.colors = cb;

        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
    }

    private static void CreateVolumeSlider(Transform parent, string name, string label, Vector2 position)
    {
        // Label
        GameObject labelObj = CreateTMPText(parent, name + "Label", label,
            new Vector2(-200, position.y), TextAlignmentOptions.Left, 18, new Color(0.8f, 0.8f, 0.9f));
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.5f, 0.5f);
        labelRect.anchorMax = new Vector2(0.5f, 0.5f);
        labelRect.sizeDelta = new Vector2(200, 30);

        // Slider
        GameObject sliderObj = new GameObject(name + "Slider");
        sliderObj.transform.SetParent(parent, false);
        Slider slider = sliderObj.AddComponent<Slider>();
        slider.minValue = 0;
        slider.maxValue = 1;
        slider.value = 1;

        RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 0.5f);
        sliderRect.anchorMax = new Vector2(0.5f, 0.5f);
        sliderRect.anchoredPosition = new Vector2(100, position.y);
        sliderRect.sizeDelta = new Vector2(350, 20);

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderObj.transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.3f);
        RectTransform bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Fill
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderObj.transform, false);
        RectTransform faRect = fillArea.AddComponent<RectTransform>();
        faRect.anchorMin = Vector2.zero;
        faRect.anchorMax = Vector2.one;
        faRect.offsetMin = Vector2.zero;
        faRect.offsetMax = Vector2.zero;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.3f, 0.7f, 0.9f);
        RectTransform fillRect = fill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        slider.fillRect = fillRect;
    }

    private static void CreateKeyBindRow(Transform parent, string action, string displayName, string defaultKey, Vector2 position)
    {
        // Label showing current key
        CreateTMPText(parent, action + "Label", $"{displayName}: {defaultKey}",
            new Vector2(-100, position.y), TextAlignmentOptions.Left, 16, new Color(0.7f, 0.7f, 0.8f));

        // Rebind button
        CreateMenuButton(parent, action + "Button", "Rebind",
            new Vector2(250, position.y), new Color(0.3f, 0.35f, 0.45f), new Vector2(100, 30));
    }

    private static void CreateEventSystem()
    {
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            Debug.Log("✓ EventSystem created (required for UI interaction)");
        }
    }
}
