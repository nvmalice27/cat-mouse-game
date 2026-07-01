using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class PatchGameOverPanel
{
    static readonly string[] Scenes = { "Room", "Kitchen", "Bathroom" };

    [MenuItem("CatMouse/Patch: GameOver — tylko Menu główne")]
    public static void Patch()
    {
        foreach (var sceneName in Scenes)
        {
            var guids = AssetDatabase.FindAssets($"{sceneName} t:Scene");
            string path = null;
            foreach (var guid in guids)
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                if (System.IO.Path.GetFileNameWithoutExtension(p) == sceneName) { path = p; break; }
            }
            if (path == null) { Debug.LogWarning($"Nie znaleziono sceny: {sceneName}"); continue; }

            var scene  = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            bool dirty = false;

            // Usuń przycisk "Nowy dzień" jeśli istnieje
            var nowyDzien = GameObject.Find("Nowy dzień");
            if (nowyDzien != null)
            {
                Object.DestroyImmediate(nowyDzien);
                dirty = true;
                Debug.Log($"✓ {sceneName}: usunięto przycisk 'Nowy dzień'.");
            }

            // Wycentruj "Menu główne" w ButtonsPanel
            var menuBtn = GameObject.Find("Menu główne");
            if (menuBtn != null)
            {
                var rt = menuBtn.GetComponent<RectTransform>();
                if (rt != null) { rt.anchoredPosition = new Vector2(0f, -10f); dirty = true; }
            }

            if (dirty)
            {
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"✓ {sceneName}: zapisano.");
            }
            else
            {
                Debug.Log($"{sceneName}: brak zmian.");
            }
        }
    }
}
