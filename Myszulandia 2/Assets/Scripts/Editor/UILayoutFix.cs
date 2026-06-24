using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public static class UILayoutFix
{
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
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log($"✓ Ustawiono indeksy dla {fixed_} skarpetkach.");
    }

    [MenuItem("CatMouse/Fix UI Positions (ta scena)")]
    public static void FixPositions()
    {
        int fixed_ = 0;

        // HUD — lewy górny róg
        var hud = GameObject.Find("HUD");
        if (hud != null)
        {
            var rt = hud.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0f, 1f);
            rt.anchorMax        = new Vector2(0f, 1f);
            rt.pivot            = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(10f, -10f);
            rt.sizeDelta        = new Vector2(260f, 65f);
            EditorUtility.SetDirty(hud);
            fixed_++;
        }

        // InventoryPanel — dół, wyśrodkowany
        var inv = GameObject.Find("InventoryPanel");
        if (inv != null)
        {
            var rt = inv.GetComponent<RectTransform>();
            rt.anchorMin        = new Vector2(0.5f, 0f);
            rt.anchorMax        = new Vector2(0.5f, 0f);
            rt.pivot            = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 10f);
            rt.sizeDelta        = new Vector2(560f, 100f);
            EditorUtility.SetDirty(inv);
            fixed_++;
        }

        // SlotsContainer wewnątrz InventoryPanel — rozciągnij na cały panel
        var container = GameObject.Find("SlotsContainer");
        if (container != null)
        {
            var rt = container.GetComponent<RectTransform>();
            rt.anchorMin  = Vector2.zero;
            rt.anchorMax  = Vector2.one;
            rt.offsetMin  = new Vector2(6f, 6f);
            rt.offsetMax  = new Vector2(-6f, -6f);
            EditorUtility.SetDirty(container);
        }

        if (fixed_ == 0)
        {
            Debug.LogWarning("Nie znaleziono HUD ani InventoryPanel — sprawdź czy jesteś na właściwej scenie.");
            return;
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log($"✓ Poprawiono {fixed_} panele UI. Sprawdź w Game view.");
    }
}
