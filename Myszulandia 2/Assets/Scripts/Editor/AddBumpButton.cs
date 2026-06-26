using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public static class AddBumpButton
{
    static readonly string[] Scenes = { "Room", "Bathroom", "Kitchen" };

    [MenuItem("CatMouse/Add Bumpcorz Button (all scenes)")]
    public static void AddAll()
    {
        foreach (var sceneName in Scenes)
        {
            string path = FindScenePath(sceneName);
            if (path == null) { Debug.LogWarning($"AddBumpButton: nie znaleziono sceny {sceneName}."); continue; }

            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            var panel = FindInScene(scene, "ActionMenuPanel");
            if (panel == null) { Debug.LogWarning($"{sceneName}: nie znaleziono ActionMenuPanel."); continue; }

            var actionMenu = panel.GetComponent<MouseActionMenu>();
            if (actionMenu == null) { Debug.LogWarning($"{sceneName}: brak MouseActionMenu."); continue; }

            // Sprawdź czy Bumpcorz już istnieje
            if (panel.transform.Find("Bumpcorz") != null)
            {
                Debug.Log($"{sceneName}: Bumpcorz już istnieje — pomijam.");
                continue;
            }

            // Utwórz przycisk Bumpcorz
            var bGO = new GameObject("Bumpcorz");
            bGO.transform.SetParent(panel.transform, false);
            var bRT = bGO.AddComponent<RectTransform>();
            bRT.sizeDelta = new Vector2(60, 60);
            var bImg = bGO.AddComponent<Image>();
            bImg.color = new Color(0.85f, 0.35f, 0.85f);
            var btn = bGO.AddComponent<Button>();

            // Podepnij OnBump przez UnityEvent
            var clickEvent = new UnityEngine.Events.UnityEvent();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                btn.onClick, actionMenu.OnBump);

            // Tekst na przycisku
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(bGO.transform, false);
            var textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text      = "Bump";
            tmp.fontSize  = 11;
            tmp.alignment = TMPro.TextAlignmentOptions.Center;
            tmp.color     = Color.white;

            // Przycisk domyślnie ukryty (widoczny tylko w stanie Rozochocona)
            bGO.SetActive(false);

            // Przepozycjonuj wszystkie przyciski równomiernie
            int count = panel.transform.childCount;
            float startX = -(count - 1) * 31f;
            for (int i = 0; i < count; i++)
            {
                var rt = panel.transform.GetChild(i).GetComponent<RectTransform>();
                if (rt != null) rt.anchoredPosition = new Vector2(startX + i * 62f, 0f);
            }

            // Podepnij bumpButton w MouseActionMenu
            var so = new SerializedObject(actionMenu);
            so.FindProperty("bumpButton").objectReferenceValue = bGO;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(actionMenu);

            EditorSceneManager.SaveScene(scene);
            Debug.Log($"✓ {sceneName}: dodano przycisk Bumpcorz.");
        }
    }

    static GameObject FindInScene(UnityEngine.SceneManagement.Scene scene, string name)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            var found = FindInChildren(root.transform, name);
            if (found != null) return found;
        }
        return null;
    }

    static GameObject FindInChildren(Transform t, string name)
    {
        if (t.name == name) return t.gameObject;
        for (int i = 0; i < t.childCount; i++)
        {
            var found = FindInChildren(t.GetChild(i), name);
            if (found != null) return found;
        }
        return null;
    }

    static string FindScenePath(string sceneName)
    {
        foreach (var s in EditorBuildSettings.scenes)
            if (System.IO.Path.GetFileNameWithoutExtension(s.path) == sceneName) return s.path;
        foreach (var guid in AssetDatabase.FindAssets($"{sceneName} t:Scene"))
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(p) == sceneName) return p;
        }
        return null;
    }
}
