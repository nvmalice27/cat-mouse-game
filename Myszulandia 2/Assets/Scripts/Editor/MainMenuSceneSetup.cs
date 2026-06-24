using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public static class MainMenuSceneSetup
{
    [MenuItem("CatMouse/Setup MainMenu Scene")]
    public static void Setup()
    {
        // EventSystem
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // Camera
        if (Object.FindObjectOfType<Camera>() == null)
        {
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic      = true;
            cam.orthographicSize  = 5f;
            cam.clearFlags        = CameraClearFlags.SolidColor;
            cam.backgroundColor   = new Color(0.13f, 0.10f, 0.18f);
            cam.depth             = -1;
            camGO.AddComponent<AudioListener>();
            camGO.transform.position = new Vector3(0, 0, -10);
        }

        // ── MANAGERS ────────────────────────────────────────────────────────
        var mgr = new GameObject("GameManager");
        mgr.AddComponent<GameManager>();
        mgr.AddComponent<SaveManager>();
        mgr.AddComponent<EconomyManager>();
        mgr.AddComponent<MouseStateManager>();
        mgr.AddComponent<InventoryManager>();
        mgr.AddComponent<DayNightManager>();

        // ── BACKGROUND ──────────────────────────────────────────────────────
        var bg = new GameObject("Background");
        bg.AddComponent<SpriteRenderer>().sortingOrder = -10;

        // ── UI CANVAS ────────────────────────────────────────────────────────
        var canvasGO = new GameObject("UICanvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Root menu panel
        var rootPanel = MakePanel("MenuRoot", canvasGO.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(320, 500));

        // Title
        MakeTMP("Title", rootPanel.transform, "Myszulandia 2", new Vector2(0, 180), 42);
        MakeTMP("Subtitle", rootPanel.transform, "Opieka nad myszką", new Vector2(0, 135), 22);

        // MainMenuUI component
        var menuUI = rootPanel.AddComponent<MainMenuUI>();

        // Buttons
        var btnNewGame  = MakeMenuButton("NowáGra",    rootPanel.transform, new Vector2(0,  50), menuUI, "OnNewGame");
        var btnContinue = MakeMenuButton("Kontynuuj",  rootPanel.transform, new Vector2(0,   0), menuUI, "OnContinue");
        var btnGallery  = MakeMenuButton("Galeria",    rootPanel.transform, new Vector2(0, -50), menuUI, "OnGallery");
        var btnQuit     = MakeMenuButton("Wyjdź",      rootPanel.transform, new Vector2(0,-100), menuUI, "OnQuit");

        // Continue button starts hidden (MainMenuUI.Start() handles it)
        btnContinue.SetActive(false);
        SetField(menuUI, "continueButton", btnContinue);

        // ── SAVE ─────────────────────────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("✓ MainMenu scene built!");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    static void SetField(Object target, string fieldName, Object value)
    {
        var so = new SerializedObject(target);
        var sp = so.FindProperty(fieldName);
        if (sp != null) { sp.objectReferenceValue = value; so.ApplyModifiedProperties(); }
        else Debug.LogWarning($"Field '{fieldName}' not found on {target.GetType().Name}");
    }

    static GameObject MakePanel(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt        = go.AddComponent<RectTransform>();
        rt.anchorMin  = anchorMin;
        rt.anchorMax  = anchorMax;
        rt.pivot      = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta  = size;
        return go;
    }

    static TextMeshProUGUI MakeTMP(string name, Transform parent, string text, Vector2 pos, float fontSize = 18)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta        = new Vector2(400, 55);
        rt.anchoredPosition = pos;
        var tmp      = go.AddComponent<TextMeshProUGUI>();
        tmp.text     = text;
        tmp.fontSize = fontSize;
        tmp.color    = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        return tmp;
    }

    static GameObject MakeMenuButton(string label, Transform parent, Vector2 pos, Object target, string methodName)
    {
        var go = new GameObject(label);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta        = new Vector2(260, 44);
        rt.anchoredPosition = pos;
        var img = go.AddComponent<Image>();
        img.color = new Color(0.22f, 0.18f, 0.32f, 0.95f);
        var btn = go.AddComponent<Button>();

        var lblGO = new GameObject("Label");
        lblGO.transform.SetParent(go.transform, false);
        var lblRT = lblGO.AddComponent<RectTransform>();
        lblRT.sizeDelta        = new Vector2(260, 44);
        lblRT.anchoredPosition = Vector2.zero;
        var tmp = lblGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 20;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        var method = target.GetType().GetMethod(methodName);
        if (method != null)
        {
            var action = System.Delegate.CreateDelegate(
                typeof(UnityEngine.Events.UnityAction), target, method)
                as UnityEngine.Events.UnityAction;
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick, action);
        }
        else Debug.LogWarning($"Method '{methodName}' not found on {target.GetType().Name}");

        return go;
    }
}
