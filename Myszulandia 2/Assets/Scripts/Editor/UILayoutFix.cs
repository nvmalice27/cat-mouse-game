using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;

public static class UILayoutFix
{
    // ── FIX ALL SCENES (one click) ────────────────────────────────────────────

    [MenuItem("CatMouse/!!! Fix All Scenes (kliknij to) !!!")]
    public static void FixAllScenes()
    {
        string[] sceneNames  = { "Room", "Kitchen", "Bathroom" };
        string originalPath  = UnityEditor.SceneManagement.EditorSceneManager
            .GetActiveScene().path;

        foreach (var sceneName in sceneNames)
        {
            string[] guids = AssetDatabase.FindAssets($"t:Scene {sceneName}");
            string scenePath = null;
            foreach (var guid in guids)
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(p) == sceneName) { scenePath = p; break; }
            }
            if (scenePath == null) { Debug.LogWarning($"Scena {sceneName} nie znaleziona"); continue; }

            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
            Debug.Log($"[FixAll] {sceneName}...");

            FixCutscenePanelInternal();
            AddNavButtonsInternal();
            AddXButtonsToPanelsInternal();

            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Debug.Log($"[FixAll] ✓ {sceneName} zapisana.");
        }

        if (!string.IsNullOrEmpty(originalPath))
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(originalPath);

        Debug.Log("✓ Wszystkie sceny naprawione!");
    }

    // ── Setup Radio Menu (Room scene only) ───────────────────────────────────

    [MenuItem("CatMouse/Setup Radio Menu (Room scene)")]
    public static void SetupRadioMenu()
    {
        if (Object.FindObjectOfType<RadioUI>(true) != null)
        {
            Debug.Log("RadioUI już istnieje na scenie. Usuń ją ręcznie jeśli chcesz przebudować.");
            return;
        }

        var radioObj = Object.FindObjectOfType<RadioObject>(true);
        if (radioObj == null) { Debug.LogWarning("Brak RadioObject — otwórz scenę Room."); return; }

        var canvas = Object.FindObjectOfType<Canvas>(true);
        if (canvas == null) { Debug.LogWarning("Brak Canvas na scenie."); return; }

        // RadioUI manager — persists as always-active GO
        var mgGO = new GameObject("RadioUI");
        mgGO.transform.SetParent(canvas.transform, false);
        mgGO.AddComponent<RectTransform>();
        var radioUI = mgGO.AddComponent<RadioUI>();

        // RadioPanel
        var panelGO = new GameObject("RadioPanel");
        panelGO.transform.SetParent(canvas.transform, false);
        var panelRT = panelGO.AddComponent<RectTransform>();
        panelRT.anchorMin = panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.pivot     = new Vector2(0.5f, 0.5f);
        panelRT.anchoredPosition = Vector2.zero;
        panelRT.sizeDelta = new Vector2(280f, 240f);
        var panelImg = panelGO.AddComponent<Image>();
        panelImg.color = new Color(0.12f, 0.08f, 0.22f, 0.96f);

        // Title
        var titleGO = new GameObject("Title");
        titleGO.transform.SetParent(panelGO.transform, false);
        var titleRT = titleGO.AddComponent<RectTransform>();
        titleRT.anchorMin = new Vector2(0f, 1f); titleRT.anchorMax = new Vector2(1f, 1f);
        titleRT.pivot     = new Vector2(0.5f, 1f);
        titleRT.anchoredPosition = new Vector2(0f, -10f);
        titleRT.sizeDelta = new Vector2(0f, 36f);
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text      = "Radio";
        titleTMP.fontSize  = 20;
        titleTMP.color     = Color.white;
        titleTMP.fontStyle = FontStyles.Bold;
        titleTMP.alignment = TextAlignmentOptions.Center;

        // Music buttons
        Color btnNormal = new Color(0.25f, 0.18f, 0.38f, 1f);
        Color btnBad    = new Color(0.55f, 0.10f, 0.10f, 1f);

        MakeMenuBtn("BtnPop",       panelGO.transform, 58f,  "Pop",         btnNormal, radioUI, "PlayPop");
        MakeMenuBtn("BtnSpokojna",  panelGO.transform, 102f, "Spokojna",    btnNormal, radioUI, "PlaySpokojna");
        MakeMenuBtn("BtnKlasyczna", panelGO.transform, 146f, "Klasyczna",   btnNormal, radioUI, "PlayKlasyczna");
        MakeMenuBtn("BtnMetal",     panelGO.transform, 190f, "Heavy Metal", btnBad,    radioUI, "PlayMetal");

        // X button + Escape
        AddCloseButtonToPanel(panelGO);

        // Wire fields
        SetFieldSO(radioUI,  "panel",   panelGO);
        SetFieldSO(radioObj, "radioUI", radioUI);

        panelGO.SetActive(false);

        EditorUtility.SetDirty(radioUI);
        EditorUtility.SetDirty(radioObj);
        SaveScene();
        Debug.Log("✓ Radio menu gotowe! Kliknij radio w grze żeby otworzyć.");
    }

    // ── Add X Buttons + Escape to all panels ─────────────────────────────────

    [MenuItem("CatMouse/Add X Buttons to Panels (ta scena)")]
    public static void AddXButtonsToPanels()
    {
        AddXButtonsToPanelsInternal();
        SaveScene();
    }

    static void AddXButtonsToPanelsInternal()
    {
        foreach (var c in Object.FindObjectsOfType<PhoneUI>(true))
            AddCloseButtonToPanel(GetSerializedPanel(c, "mainPanel"));

        foreach (var c in Object.FindObjectsOfType<MouseActionMenu>(true))
            AddCloseButtonToPanel(GetSerializedPanel(c, "panel"));

        foreach (var c in Object.FindObjectsOfType<DoorObject>(true))
            AddCloseButtonToPanel(GetSerializedPanel(c, "roomDoorMenu"));

        foreach (var c in Object.FindObjectsOfType<PillowObject>(true))
            AddCloseButtonToPanel(GetSerializedPanel(c, "confirmPanel"));

        foreach (var c in Object.FindObjectsOfType<RadioUI>(true))
            AddCloseButtonToPanel(GetSerializedPanel(c, "panel"));
    }

    static void AddCloseButtonToPanel(GameObject panel)
    {
        if (panel == null) return;
        if (panel.GetComponent<PanelEscapeClose>() != null) return; // już dodany

        var esc = panel.AddComponent<PanelEscapeClose>();
        EditorUtility.SetDirty(panel);

        // X button — prawy górny róg panelu
        var btnGO = new GameObject("BtnClose");
        btnGO.transform.SetParent(panel.transform, false);
        var rt = btnGO.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot     = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-4f, -4f);
        rt.sizeDelta = new Vector2(28f, 28f);
        var img = btnGO.AddComponent<Image>();
        img.color = new Color(0.75f, 0.12f, 0.12f, 0.92f);
        var btn = btnGO.AddComponent<Button>();

        var lblGO = new GameObject("X");
        lblGO.transform.SetParent(btnGO.transform, false);
        var lrt = lblGO.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        var tmp = lblGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = "✕";
        tmp.fontSize  = 16;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        var method = esc.GetType().GetMethod("Close");
        if (method != null)
        {
            var action = System.Delegate.CreateDelegate(
                typeof(UnityEngine.Events.UnityAction), esc, method)
                as UnityEngine.Events.UnityAction;
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick, action);
        }

        EditorUtility.SetDirty(btnGO);
        Debug.Log($"  ✓ X dodany do: {panel.name}");
    }

    // ── Fix Sock Indices ──────────────────────────────────────────────────────

    [MenuItem("CatMouse/Fix Sock Indices (Room scene)")]
    public static void FixSockIndices()
    {
        int fixed_ = 0;
        for (int i = 1; i <= 3; i++)
        {
            var go = GameObject.Find($"Sock_{i}");
            if (go == null) { Debug.LogWarning($"Nie znaleziono Sock_{i}"); continue; }
            var sock = go.GetComponent<SockObject>();
            if (sock == null) { Debug.LogWarning($"Sock_{i} nie ma SockObject"); continue; }
            var so = new SerializedObject(sock);
            var sp = so.FindProperty("sockIndex");
            if (sp != null) { sp.intValue = i - 1; so.ApplyModifiedProperties(); }
            EditorUtility.SetDirty(sock);
            fixed_++;
        }
        SaveScene();
        Debug.Log($"✓ Ustawiono indeksy dla {fixed_} skarpetkach.");
    }

    // ── Fix UI Positions ──────────────────────────────────────────────────────

    [MenuItem("CatMouse/Fix UI Positions (ta scena)")]
    public static void FixPositions()
    {
        int fixed_ = 0;

        var hud = GameObject.Find("HUD");
        if (hud != null)
        {
            var rt = hud.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot            = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(10f, -10f);
            rt.sizeDelta        = new Vector2(260f, 65f);
            EditorUtility.SetDirty(hud);
            fixed_++;
        }

        var inv = GameObject.Find("InventoryPanel");
        if (inv != null)
        {
            var rt = inv.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot            = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 10f);
            rt.sizeDelta        = new Vector2(560f, 100f);
            EditorUtility.SetDirty(inv);
            fixed_++;
        }

        var container = GameObject.Find("SlotsContainer");
        if (container != null)
        {
            var rt = container.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(6f, 6f);
            rt.offsetMax = new Vector2(-6f, -6f);
            EditorUtility.SetDirty(container);
        }

        if (fixed_ == 0) { Debug.LogWarning("Nie znaleziono HUD ani InventoryPanel."); return; }

        SaveScene();
        Debug.Log($"✓ Poprawiono {fixed_} panele UI.");
    }

    // ── Fix Cutscene Panel ────────────────────────────────────────────────────

    [MenuItem("CatMouse/Fix Cutscene Panel (ta scena)")]
    public static void FixCutscenePanel()
    {
        FixCutscenePanelInternal();
        SaveScene();
    }

    static void FixCutscenePanelInternal()
    {
        var cm = Object.FindObjectsOfType<CutsceneManager>(true);
        if (cm == null || cm.Length == 0)
        {
            Debug.LogWarning("Nie znaleziono CutsceneManager na scenie.");
            return;
        }

        var go = cm[0].gameObject;
        go.SetActive(true);

        var cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        cg.alpha          = 0f;
        cg.blocksRaycasts = false;
        cg.interactable   = false;

        EditorUtility.SetDirty(go);
        Debug.Log($"  CutscenePanel ({go.name}): aktywny + CanvasGroup(alpha=0)");
    }

    // ── Add Nav Buttons ───────────────────────────────────────────────────────

    [MenuItem("CatMouse/Add Nav Buttons (ta scena)")]
    public static void AddNavButtons()
    {
        AddNavButtonsInternal();
        SaveScene();
    }

    static void AddNavButtonsInternal()
    {
        var canvas = Object.FindObjectOfType<Canvas>(true);
        if (canvas == null) { Debug.LogWarning("Brak Canvas na scenie."); return; }

        var existing = GameObject.Find("NavButtons");
        if (existing != null) Object.DestroyImmediate(existing);

        var navGO = new GameObject("NavButtons");
        navGO.transform.SetParent(canvas.transform, false);
        var navRT = navGO.AddComponent<RectTransform>();
        navRT.anchorMin        = new Vector2(1f, 1f);
        navRT.anchorMax        = new Vector2(1f, 1f);
        navRT.pivot            = new Vector2(1f, 1f);
        navRT.anchoredPosition = new Vector2(-10f, -10f);
        navRT.sizeDelta        = new Vector2(230f, 44f);

        var helper = navGO.AddComponent<NavHelper>();

        MakeNavBtn("BtnGaleria", navGO.transform, new Vector2(-120f, 0f), "Galeria",
            helper, "GoToGallery");
        MakeNavBtn("BtnMenu",    navGO.transform, new Vector2(  0f,  0f), "Menu",
            helper, "GoToMainMenu");

        EditorUtility.SetDirty(navGO);
        Debug.Log("  NavButtons: dodane z NavHelper.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static void MakeNavBtn(string name, Transform parent, Vector2 pos, string label,
                            Object target, string methodName)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta        = new Vector2(108f, 40f);
        rt.anchoredPosition = pos;
        var img = go.AddComponent<Image>();
        img.color = new Color(0.18f, 0.18f, 0.25f, 0.9f);
        var btn = go.AddComponent<Button>();

        var lblGO = new GameObject("Label");
        lblGO.transform.SetParent(go.transform, false);
        var lblRT = lblGO.AddComponent<RectTransform>();
        lblRT.anchorMin = Vector2.zero; lblRT.anchorMax = Vector2.one;
        lblRT.offsetMin = lblRT.offsetMax = Vector2.zero;
        var tmp = lblGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label; tmp.fontSize = 17; tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        WireButton(btn, target, methodName);
    }

    static void MakeMenuBtn(string name, Transform parent, float yFromTop, string label,
                             Color bgColor, Object target, string methodName)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot     = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -yFromTop);
        rt.sizeDelta = new Vector2(240f, 36f);
        var img = go.AddComponent<Image>();
        img.color = bgColor;
        var btn = go.AddComponent<Button>();

        var lblGO = new GameObject("Label");
        lblGO.transform.SetParent(go.transform, false);
        var lrt = lblGO.AddComponent<RectTransform>();
        lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
        lrt.offsetMin = lrt.offsetMax = Vector2.zero;
        var tmp = lblGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 15;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        WireButton(btn, target, methodName);
    }

    static void WireButton(Button btn, Object target, string methodName)
    {
        if (target == null) return;
        var method = target.GetType().GetMethod(methodName);
        if (method == null) { Debug.LogWarning($"Metoda '{methodName}' nie znaleziona na {target.GetType().Name}"); return; }
        var action = System.Delegate.CreateDelegate(
            typeof(UnityEngine.Events.UnityAction), target, method)
            as UnityEngine.Events.UnityAction;
        UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick, action);
    }

    static GameObject GetSerializedPanel(Object component, string fieldName)
    {
        var so   = new SerializedObject(component);
        var prop = so.FindProperty(fieldName);
        return prop?.objectReferenceValue as GameObject;
    }

    static void SetFieldSO(Object component, string field, Object value)
    {
        var so   = new SerializedObject(component);
        var prop = so.FindProperty(field);
        if (prop != null) { prop.objectReferenceValue = value; so.ApplyModifiedProperties(); }
        EditorUtility.SetDirty(component);
    }

    static void SaveScene()
    {
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
    }
}
