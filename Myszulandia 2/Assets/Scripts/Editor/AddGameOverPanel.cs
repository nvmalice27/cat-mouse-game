using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class AddGameOverPanel
{
    [MenuItem("CatMouse/Dodaj GameOver Panel (aktywna scena)")]
    public static void Add()
    {
        if (GameObject.Find("GameOverPanel") != null)
        {
            Debug.Log("GameOverPanel już istnieje na scenie.");
            return;
        }

        var canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null) { Debug.LogError("Brak Canvas na scenie."); return; }

        var gameOverUI = canvas.gameObject.AddComponent<GameOverUI>();

        // ── Panel — czarne tło na cały ekran ─────────────────────────────────
        var panelGO = new GameObject("GameOverPanel");
        panelGO.transform.SetParent(canvas.transform, false);
        var panelRT = panelGO.AddComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;
        var panelImg = panelGO.AddComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.88f);

        // ── Napis "Koniec gry" — wyśrodkowany ────────────────────────────────
        var titleGO = new GameObject("TitleText");
        titleGO.transform.SetParent(panelGO.transform, false);
        var titleRT = titleGO.AddComponent<RectTransform>();
        titleRT.anchorMin        = new Vector2(0.5f, 0.5f);
        titleRT.anchorMax        = new Vector2(0.5f, 0.5f);
        titleRT.sizeDelta        = new Vector2(700f, 120f);
        titleRT.anchoredPosition = new Vector2(0f, 60f);
        var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
        titleTMP.text      = "Koniec gry";
        titleTMP.fontSize  = 72;
        titleTMP.color     = Color.white;
        titleTMP.alignment = TextAlignmentOptions.Center;

        // ── Przyciski — prawy górny róg ───────────────────────────────────────
        var buttonsGO = new GameObject("ButtonsPanel");
        buttonsGO.transform.SetParent(panelGO.transform, false);
        var buttonsRT = buttonsGO.AddComponent<RectTransform>();
        buttonsRT.anchorMin        = new Vector2(1f, 1f);
        buttonsRT.anchorMax        = new Vector2(1f, 1f);
        buttonsRT.pivot            = new Vector2(1f, 1f);
        buttonsRT.sizeDelta        = new Vector2(260f, 110f);
        buttonsRT.anchoredPosition = new Vector2(-20f, -20f);

        MakeButton("Menu główne", buttonsGO.transform, new Vector2(0f, -10f), gameOverUI, "OnMainMenu");

        // ── Podepnij panel do GameOverUI ──────────────────────────────────────
        var so   = new SerializedObject(gameOverUI);
        var prop = so.FindProperty("panel");
        prop.objectReferenceValue = panelGO;
        so.ApplyModifiedProperties();

        panelGO.SetActive(false);

        EditorUtility.SetDirty(canvas.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("✓ GameOverPanel dodany i zapisany.");
    }

    static void MakeButton(string label, Transform parent, Vector2 pos, GameOverUI target, string methodName)
    {
        var go = new GameObject(label);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta        = new Vector2(240f, 44f);
        rt.anchoredPosition = pos;
        var img = go.AddComponent<Image>();
        img.color = new Color(0.18f, 0.18f, 0.25f, 1f);
        var btn = go.AddComponent<Button>();

        var lblGO = new GameObject("Label");
        lblGO.transform.SetParent(go.transform, false);
        var lblRT = lblGO.AddComponent<RectTransform>();
        lblRT.anchorMin = Vector2.zero;
        lblRT.anchorMax = Vector2.one;
        lblRT.offsetMin = Vector2.zero;
        lblRT.offsetMax = Vector2.zero;
        var tmp      = lblGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 18;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        var method = typeof(GameOverUI).GetMethod(methodName);
        if (method != null)
        {
            var action = System.Delegate.CreateDelegate(
                typeof(UnityEngine.Events.UnityAction), target, method)
                as UnityEngine.Events.UnityAction;
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick, action);
        }
    }
}
