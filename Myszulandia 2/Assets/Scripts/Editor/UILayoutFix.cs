using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class UILayoutFix
{
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
        var go = GameObject.Find("CutscenePanel");
        if (go == null) { Debug.LogWarning("Nie znaleziono CutscenePanel."); return; }

        // Musi być aktywny żeby Awake() się odpaliło
        go.SetActive(true);

        // CanvasGroup ukrywa wizualnie zamiast SetActive(false)
        var cg = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();
        cg.alpha          = 0f;
        cg.blocksRaycasts = false;
        cg.interactable   = false;

        EditorUtility.SetDirty(go);
        SaveScene();
        Debug.Log("✓ CutscenePanel naprawiony — teraz aktywny ale niewidoczny.");
    }

    // ── Add Nav Buttons ───────────────────────────────────────────────────────

    [MenuItem("CatMouse/Add Nav Buttons (ta scena)")]
    public static void AddNavButtons()
    {
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null) { Debug.LogWarning("Brak Canvas na scenie."); return; }

        // Nie dodawaj jeśli już są
        if (GameObject.Find("NavButtons") != null)
        {
            Debug.Log("NavButtons już istnieją na tej scenie.");
            return;
        }

        // Kontener — prawy górny róg
        var navGO = new GameObject("NavButtons");
        navGO.transform.SetParent(canvas.transform, false);
        var navRT = navGO.AddComponent<RectTransform>();
        navRT.anchorMin        = new Vector2(1f, 1f);
        navRT.anchorMax        = new Vector2(1f, 1f);
        navRT.pivot            = new Vector2(1f, 1f);
        navRT.anchoredPosition = new Vector2(-10f, -10f);
        navRT.sizeDelta        = new Vector2(230f, 44f);

        var mgr = Object.FindObjectOfType<GameManager>();

        MakeNavBtn("BtnGaleria", navGO.transform, new Vector2(-120f, 0f), "Galeria",
            mgr, "NavigateToGallery");
        MakeNavBtn("BtnMenu",    navGO.transform, new Vector2( 0f,   0f), "Menu",
            mgr, "NavigateToMainMenu");

        EditorUtility.SetDirty(navGO);
        SaveScene();
        Debug.Log("✓ Przyciski nawigacji dodane (prawy górny róg).");
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

        if (target != null)
        {
            var method = target.GetType().GetMethod(methodName);
            if (method != null)
            {
                var action = System.Delegate.CreateDelegate(
                    typeof(UnityEngine.Events.UnityAction), target, method)
                    as UnityEngine.Events.UnityAction;
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick, action);
            }
            else Debug.LogWarning($"Metoda '{methodName}' nie znaleziona na {target.GetType().Name}");
        }
    }

    static void SaveScene()
    {
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
    }
}
