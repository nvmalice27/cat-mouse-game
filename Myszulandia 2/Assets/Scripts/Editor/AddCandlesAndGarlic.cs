using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class AddCandlesAndGarlic
{
    [MenuItem("CatMouse/Add Candles to Room + Garlic to Kitchen")]
    public static void AddAll()
    {
        AddCandles();
        AddGarlic();
        Debug.Log("✓ Świeczki dodane do Room, czosnek dodany do Kitchen. Przypisz sprite'y w Inspektorze.");
    }

    static void AddCandles()
    {
        string path = FindScenePath("Room");
        if (path == null) { Debug.LogWarning("AddCandlesAndGarlic: nie znaleziono sceny Room."); return; }

        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

        // Nie duplikuj jeśli już istnieje
        var existing = FindInScene(scene, "Candles");
        if (existing != null) { Debug.Log("Room: Candles już istnieje — pomijam."); return; }

        var go = new GameObject("Candles");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 2;
        go.AddComponent<CircleCollider2D>();
        var candle = go.AddComponent<CandleObject>();

        // Wire SpriteRenderer reference via SerializedObject
        var so = new SerializedObject(candle);
        so.FindProperty("sr").objectReferenceValue = sr;
        so.ApplyModifiedProperties();

        go.transform.position = new Vector3(1.5f, -0.5f, 0f);

        EditorSceneManager.SaveScene(scene);
        Debug.Log("✓ Room: dodano obiekt 'Candles'. Przypisz sprite'y Lit/Unlit w Inspektorze.");
    }

    static void AddGarlic()
    {
        string path = FindScenePath("Kitchen");
        if (path == null) { Debug.LogWarning("AddCandlesAndGarlic: nie znaleziono sceny Kitchen."); return; }

        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

        var existing = FindInScene(scene, "Garlic");
        if (existing != null) { Debug.Log("Kitchen: Garlic już istnieje — pomijam."); return; }

        var go = new GameObject("Garlic");
        go.AddComponent<SpriteRenderer>().sortingOrder = 2;
        go.AddComponent<CircleCollider2D>();
        go.AddComponent<GarlicObject>();

        go.transform.position = new Vector3(2f, -1.5f, 0f);

        EditorSceneManager.SaveScene(scene);
        Debug.Log("✓ Kitchen: dodano obiekt 'Garlic'. Przypisz sprite w Inspektorze.");
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
