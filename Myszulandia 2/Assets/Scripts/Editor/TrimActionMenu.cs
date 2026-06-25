using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

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

            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);

            var panel = GameObject.Find("ActionMenuPanel");
            if (panel == null) { EditorSceneManager.CloseScene(scene, false); continue; }

            // Remove extra buttons
            int removed = 0;
            foreach (string btnName in ExtraButtons)
            {
                var child = panel.transform.Find(btnName);
                if (child == null) continue;
                Object.DestroyImmediate(child.gameObject);
                removed++;
            }

            // Re-centre the 2 remaining buttons
            int remaining = panel.transform.childCount;
            float totalW  = remaining * 60f + (remaining - 1) * 2f;
            float startX  = -totalW / 2f + 30f;
            for (int i = 0; i < remaining; i++)
            {
                var rt = panel.transform.GetChild(i).GetComponent<RectTransform>();
                if (rt != null) rt.anchoredPosition = new Vector2(startX + i * 62f, 0f);
            }

            EditorSceneManager.SaveScene(scene);
            EditorSceneManager.CloseScene(scene, false);

            totalRemoved += removed;
            Debug.Log($"✓ {sceneName}: usunięto {removed} przycisk(i).");
        }

        Debug.Log($"✓ Gotowe — usunięto łącznie {totalRemoved} przycisków.");
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
