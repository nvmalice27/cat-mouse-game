using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class TrimActionMenu
{
    static readonly string[] ExtraButtons = { "Pogłaszcz", "Pogłaskcz", "Pobaw się" };
    static readonly string[] Scenes = { "Room", "Bathroom", "Kitchen" };

    [MenuItem("CatMouse/Remove Extra Action Buttons (all scenes)")]
    public static void RemoveExtraButtons()
    {
        int totalRemoved = 0;

        foreach (var sceneName in Scenes)
        {
            string path = FindScenePath(sceneName);
            if (path == null) { Debug.LogWarning($"TrimActionMenu: nie znaleziono sceny {sceneName}."); continue; }

            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            // Search all root objects in this scene for ActionMenuPanel
            var panel = FindInScene(scene, "ActionMenuPanel");
            if (panel == null) { Debug.LogWarning($"{sceneName}: nie znaleziono ActionMenuPanel."); continue; }

            int removed = 0;
            foreach (string btnName in ExtraButtons)
            {
                var child = panel.transform.Find(btnName);
                if (child == null) continue;
                Object.DestroyImmediate(child.gameObject);
                removed++;
            }

            // Re-centre the remaining buttons
            int remaining = panel.transform.childCount;
            if (remaining > 0)
            {
                float startX = -(remaining - 1) * 31f;
                for (int i = 0; i < remaining; i++)
                {
                    var rt = panel.transform.GetChild(i).GetComponent<RectTransform>();
                    if (rt != null) rt.anchoredPosition = new Vector2(startX + i * 62f, 0f);
                }
            }

            EditorSceneManager.SaveScene(scene);
            totalRemoved += removed;
            Debug.Log($"✓ {sceneName}: usunięto {removed} przycisk(i).");
        }

        Debug.Log($"✓ Gotowe — usunięto łącznie {totalRemoved} przycisków.");
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
