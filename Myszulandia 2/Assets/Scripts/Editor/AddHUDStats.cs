using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

public static class AddHUDStats
{
    static readonly string[] Scenes = { "Room", "Bathroom", "Kitchen" };

    [MenuItem("CatMouse/Add Need Stats to HUD (all scenes)")]
    public static void AddAll()
    {
        foreach (var sceneName in Scenes)
        {
            string path = FindScenePath(sceneName);
            if (path == null) { Debug.LogWarning($"AddHUDStats: nie znaleziono sceny {sceneName}."); continue; }

            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            // Find HUD and HUDController
            var hudGO = FindInScene(scene, "HUD");
            if (hudGO == null) { Debug.LogWarning($"{sceneName}: nie znaleziono obiektu HUD."); continue; }

            var hudCtrl = hudGO.GetComponent<HUDController>();
            if (hudCtrl == null) { Debug.LogWarning($"{sceneName}: brak HUDController."); continue; }

            // Expand HUD panel height to fit 5 lines
            var hudRT = hudGO.GetComponent<RectTransform>();
            if (hudRT != null) hudRT.sizeDelta = new Vector2(hudRT.sizeDelta.x, 155f);

            var so = new SerializedObject(hudCtrl);

            // Add each stat text if not already present
            AddStatText(hudGO, so, "hungerText",    "Głód:  0/100",  -76f);
            AddStatText(hudGO, so, "attentionText", "Uwaga: 0/100", -105f);
            AddStatText(hudGO, so, "dirtText",      "Brud:  0/100", -134f);

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(hudCtrl);

            EditorSceneManager.SaveScene(scene);
            Debug.Log($"✓ {sceneName}: dodano liczniki potrzeb do HUD.");
        }
    }

    static void AddStatText(GameObject hud, SerializedObject so, string fieldName, string defaultText, float y)
    {
        // Don't add if already wired
        var prop = so.FindProperty(fieldName);
        if (prop != null && prop.objectReferenceValue != null) return;

        // Don't add if child with that name exists
        if (hud.transform.Find(fieldName) != null)
        {
            // Just wire existing
            var existing = hud.transform.Find(fieldName).GetComponent<TMP_Text>();
            if (existing != null && prop != null) prop.objectReferenceValue = existing;
            return;
        }

        var go  = new GameObject(fieldName);
        go.transform.SetParent(hud.transform, false);
        var rt  = go.AddComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(130f, y);
        rt.sizeDelta        = new Vector2(240f, 24f);

        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = defaultText;
        tmp.fontSize  = 14;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;

        if (prop != null) prop.objectReferenceValue = tmp;
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
