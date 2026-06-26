using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;

public static class AddHUDStateText
{
    static readonly string[] Scenes = { "Room", "Bathroom", "Kitchen" };

    [MenuItem("CatMouse/Add State Label to HUD (all scenes)")]
    public static void AddAll()
    {
        foreach (var sceneName in Scenes)
        {
            string path = FindScenePath(sceneName);
            if (path == null) { Debug.LogWarning($"Nie znaleziono sceny: {sceneName}"); continue; }

            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            var hudGO = FindInScene(scene, "HUD");
            if (hudGO == null) { Debug.LogWarning($"{sceneName}: brak HUD"); continue; }

            var ctrl = hudGO.GetComponent<HUDController>();
            if (ctrl == null) { Debug.LogWarning($"{sceneName}: brak HUDController"); continue; }

            // Expand HUD height if needed
            var rt = hudGO.GetComponent<RectTransform>();
            if (rt != null && rt.sizeDelta.y < 175f)
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, 175f);

            // Add stateText label at y = -160
            var so   = new SerializedObject(ctrl);
            var prop = so.FindProperty("stateText");

            if (prop.objectReferenceValue == null)
            {
                var go  = new GameObject("stateText");
                go.transform.SetParent(hudGO.transform, false);
                var goRT = go.AddComponent<RectTransform>();
                goRT.anchoredPosition = new Vector2(130f, -160f);
                goRT.sizeDelta        = new Vector2(240f, 24f);
                var tmp = go.AddComponent<TextMeshProUGUI>();
                tmp.text      = "Stan: Normal";
                tmp.fontSize  = 14;
                tmp.color     = new Color(1f, 1f, 0.4f);
                tmp.alignment = TextAlignmentOptions.MidlineLeft;
                prop.objectReferenceValue = tmp;
            }

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(ctrl);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"✓ {sceneName}: dodano etykietę stanu.");
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
