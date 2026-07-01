using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class WireMouseController
{
    static readonly string[] GameScenes = { "Room", "Bathroom", "Kitchen" };

    // Same order as CollectibleIndex in MouseStateManager (indices 0–27)
    static readonly string[] TypeNames = {
        "Heppi", "Czonstkujaca", "Grobol", "Obrazona", "Paczurowa",
        "Smrodliwa", "Makapaka", "Pirat", "Niewyspana", "WesolaPoPobudce",
        "Myszkujaca", "Tanczaca", "Pachnaca", "Czonstkowa", "Pumpuzka",
        "Roztopiona", "Spankowa", "Czosnkowa", "Zakochana", "Rozochocona",
        "Glodna", "Chcaca", "Smutna", "Zrozpaczona", "Zlowroga",
        "Sciekla", "ScieklaII", "Krowka"
    };

    [MenuItem("CatMouse/Wire MouseController sprites (all game scenes)")]
    public static void Wire()
    {
        // Pre-load all SO assets
        var assets = new MouseTypeSO[TypeNames.Length];
        for (int i = 0; i < TypeNames.Length; i++)
        {
            string path = $"Assets/Data/MouseTypes/{TypeNames[i]}.asset";
            assets[i] = AssetDatabase.LoadAssetAtPath<MouseTypeSO>(path);
            if (assets[i] == null)
                Debug.LogWarning($"Brak assetu: {path}");
        }

        foreach (var sceneName in GameScenes)
        {
            string path = FindScenePath(sceneName);
            if (path == null) { Debug.LogWarning($"Nie znaleziono sceny: {sceneName}"); continue; }

            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

            MouseController ctrl = null;
            foreach (var root in scene.GetRootGameObjects())
            {
                ctrl = root.GetComponentInChildren<MouseController>(true);
                if (ctrl != null) break;
            }

            if (ctrl == null) { Debug.LogWarning($"{sceneName}: brak MouseController."); continue; }

            var so   = new SerializedObject(ctrl);
            var prop = so.FindProperty("mouseTypes");
            prop.arraySize = assets.Length;
            for (int i = 0; i < assets.Length; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = assets[i];
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(ctrl);

            EditorSceneManager.SaveScene(scene);
            Debug.Log($"✓ {sceneName}: MouseController.mouseTypes[] podpięte ({assets.Length} assetów).");
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
