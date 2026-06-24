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
        string[] sceneNames = { "Room", "Kitchen", "Bathroom" };
        string originalPath = UnityEditor.SceneManagement.EditorSceneManager
            .GetActiveScene().path;

        foreach (var sceneName in sceneNames)
        {
            // Znajdź ścieżkę sceny po nazwie pliku
            string[] guids = AssetDatabase.FindAssets($"t:Scene {sceneName}");
            string scenePath = null;
            foreach (var guid in guids)
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                if (Path.GetFileNameWithoutExtension(p) == sceneName)
                {
                    scenePath = p;
                    break;
                }
            }
            if (scenePath == null) { Debug.LogWarning($"Scena {sceneName} nie znaleziona"); continue; }

            // Otwórz scenę
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
            Debug.Log($"[FixAll] Naprawiam: {sceneName}...");

            // Napraw CutscenePanel (dzień/noc)
            FixCutscenePanelInternal();

            // Napraw przyciski nawigacji
            AddNavButtonsInternal();

            // Zapisz
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Debug.Log($"[FixAll] ✓ {sceneName} zapisana.");
        }

        // Wróć do oryginalnej sceny
        if (!string.IsNullOrEmpty(originalPath))
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(originalPath);

        Debug.Log("✓ Wszystkie sceny naprawione! CutscenePanel + NavButtons.");
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
        // GameObject.Find ignoruje nieaktywne — szukamy przez komponent
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
        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null) { Debug.LogWarning("Brak Canvas na scenie."); return; }

        // Usuń stare (mogły być wired do GameManager)
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
        Debug.Log($"  NavButtons: dodane z NavHelper.");
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
