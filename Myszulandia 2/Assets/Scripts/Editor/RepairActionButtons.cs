using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor.Events;

public static class RepairActionButtons
{
    static readonly string[] Scenes = { "Room", "Bathroom", "Kitchen" };

    [MenuItem("CatMouse/Repair Action Buttons (all scenes)")]
    public static void RepairAll()
    {
        foreach (var sceneName in Scenes)
        {
            string path = FindScenePath(sceneName);
            if (path == null) { Debug.LogWarning($"Nie znaleziono: {sceneName}"); continue; }

            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            var panel = FindInScene(scene, "ActionMenuPanel");
            if (panel == null) { Debug.LogWarning($"{sceneName}: brak ActionMenuPanel"); continue; }

            var menu = panel.GetComponent<MouseActionMenu>();
            if (menu == null) { Debug.LogWarning($"{sceneName}: brak MouseActionMenu"); continue; }

            bool changed = false;
            int repaired = 0;

            // Sprawdź każdy przycisk-dziecko panelu
            foreach (Transform child in panel.transform)
            {
                var btn = child.GetComponent<Button>();
                if (btn == null) continue;

                // Pobierz nazwę przycisku z tekstu TMPro
                var label = child.GetComponentInChildren<TextMeshProUGUI>();
                string btnName = label != null ? label.text.Trim() : child.name;

                string targetMethod = btnName switch
                {
                    "Przytul"  => "OnHug",
                    "Pocałuj"  => "OnKiss",
                    "Bump"     => "OnBump",
                    _          => null
                };

                if (targetMethod == null) continue;

                // Sprawdź czy listener już istnieje
                bool alreadyWired = false;
                for (int i = 0; i < btn.onClick.GetPersistentEventCount(); i++)
                {
                    if (btn.onClick.GetPersistentTarget(i) == (Object)menu &&
                        btn.onClick.GetPersistentMethodName(i) == targetMethod)
                    {
                        alreadyWired = true;
                        break;
                    }
                }

                if (!alreadyWired)
                {
                    // Wyczyść stare listenery i dodaj właściwy
                    while (btn.onClick.GetPersistentEventCount() > 0)
                        UnityEventTools.RemovePersistentListener(btn.onClick, 0);

                    var m = typeof(MouseActionMenu).GetMethod(targetMethod);
                    if (m != null)
                    {
                        var action = System.Delegate.CreateDelegate(
                            typeof(UnityEngine.Events.UnityAction), menu, m)
                            as UnityEngine.Events.UnityAction;
                        UnityEventTools.AddVoidPersistentListener(btn.onClick, action);
                        EditorUtility.SetDirty(btn.gameObject);
                        repaired++;
                        changed = true;
                        Debug.Log($"  ✓ {sceneName}/{btnName} → podpięto {targetMethod}");
                    }
                }
                else
                {
                    Debug.Log($"  ✓ {sceneName}/{btnName} → już podpięty");
                }
            }

            if (changed)
            {
                EditorSceneManager.SaveScene(scene);
                Debug.Log($"✓ {sceneName}: naprawiono {repaired} przycisków i zapisano.");
            }
            else
            {
                Debug.Log($"✓ {sceneName}: wszystkie przyciski poprawnie podpięte.");
            }
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
            var f = FindInChildren(t.GetChild(i), name);
            if (f != null) return f;
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
