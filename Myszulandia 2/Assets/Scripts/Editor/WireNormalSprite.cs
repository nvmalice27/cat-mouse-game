using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class WireNormalSprite
{
    static readonly string[] Scenes = { "Room", "Bathroom", "Kitchen" };

    [MenuItem("CatMouse/Wire Normal Mouse Sprite (all scenes)")]
    public static void Wire()
    {
        // Find myszump sprite anywhere in Assets/
        Sprite normalSprite = null;
        foreach (var guid in AssetDatabase.FindAssets("myszump t:Sprite"))
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(p) == "myszump")
            {
                normalSprite = AssetDatabase.LoadAssetAtPath<Sprite>(p);
                break;
            }
        }

        if (normalSprite == null)
        {
            Debug.LogError("WireNormalSprite: nie znaleziono sprite'a 'myszump'. Upewnij się że plik jest zaimportowany w Unity.");
            return;
        }

        foreach (var sceneName in Scenes)
        {
            string path = FindScenePath(sceneName);
            if (path == null) { Debug.LogWarning($"Nie znaleziono sceny: {sceneName}"); continue; }

            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            bool wired = false;
            foreach (var root in scene.GetRootGameObjects())
            {
                var ctrl = root.GetComponentInChildren<MouseController>(true);
                if (ctrl == null) continue;

                var so = new SerializedObject(ctrl);
                so.FindProperty("normalSprite").objectReferenceValue = normalSprite;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(ctrl);
                wired = true;
                break;
            }

            EditorSceneManager.SaveScene(scene);
            Debug.Log(wired ? $"✓ {sceneName}: normalSprite podpięty." : $"⚠ {sceneName}: nie znaleziono MouseController.");
        }
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
