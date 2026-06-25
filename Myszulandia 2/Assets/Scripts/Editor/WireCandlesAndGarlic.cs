using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;

public static class WireCandlesAndGarlic
{
    const string PlaceholderDir = "Assets/Art/Placeholders";

    [MenuItem("CatMouse/Wire Candles + Garlic sprites (all scenes)")]
    public static void WireAll()
    {
        EnsureFolder("Assets/Art", "Placeholders");

        // Create placeholder sprites if missing
        var sprCandleLit   = MakeSprite("candle_lit",   new Color(1.00f, 0.80f, 0.10f), 40, 60);
        var sprCandleUnlit = MakeSprite("candle_unlit",  new Color(0.50f, 0.40f, 0.20f), 40, 60);
        var sprGarlic      = MakeSprite("garlic",        new Color(0.90f, 0.90f, 0.70f), 48, 48);
        var icoGarlic      = MakeSprite("ico_garlic",    new Color(0.70f, 0.85f, 0.30f), 48, 48);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Re-load from disk so references are valid after Refresh
        sprCandleLit   = AssetDatabase.LoadAssetAtPath<Sprite>($"{PlaceholderDir}/candle_lit.png");
        sprCandleUnlit = AssetDatabase.LoadAssetAtPath<Sprite>($"{PlaceholderDir}/candle_unlit.png");
        sprGarlic      = AssetDatabase.LoadAssetAtPath<Sprite>($"{PlaceholderDir}/garlic.png");
        icoGarlic      = AssetDatabase.LoadAssetAtPath<Sprite>($"{PlaceholderDir}/ico_garlic.png");

        WireRoom(sprCandleLit, sprCandleUnlit, icoGarlic);
        WireBathroom(icoGarlic);
        WireKitchen(sprGarlic, icoGarlic);

        Debug.Log("✓ Świeczki i czosnek podpięte. Zastąp placeholder sprite'y własnymi grafikami.");
    }

    static void WireRoom(Sprite candleLit, Sprite candleUnlit, Sprite icoGarlic)
    {
        string path = FindScenePath("Room");
        if (path == null) { Debug.LogWarning("Wire: nie znaleziono sceny Room."); return; }

        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

        // Wire CandleObject sprites
        var candleGO = FindInScene(scene, "Candles");
        if (candleGO != null)
        {
            var candle = candleGO.GetComponent<CandleObject>();
            if (candle != null)
            {
                var so = new SerializedObject(candle);
                so.FindProperty("litSprite").objectReferenceValue   = candleLit;
                so.FindProperty("unlitSprite").objectReferenceValue = candleUnlit;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(candle);

                // Also set SpriteRenderer to unlit by default
                var sr = candleGO.GetComponent<SpriteRenderer>();
                if (sr != null) { sr.sprite = candleUnlit; EditorUtility.SetDirty(sr); }
            }
        }
        else Debug.LogWarning("Room: nie znaleziono obiektu 'Candles'.");

        WireInventoryUI(scene, icoGarlic);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("✓ Room: świeczki i InventoryUI podpięte.");
    }

    static void WireBathroom(Sprite icoGarlic)
    {
        string path = FindScenePath("Bathroom");
        if (path == null) return;
        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        WireInventoryUI(scene, icoGarlic);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("✓ Bathroom: InventoryUI podpięte.");
    }

    static void WireKitchen(Sprite garlicSprite, Sprite icoGarlic)
    {
        string path = FindScenePath("Kitchen");
        if (path == null) { Debug.LogWarning("Wire: nie znaleziono sceny Kitchen."); return; }

        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

        // Wire GarlicObject sprite
        var garlicGO = FindInScene(scene, "Garlic");
        if (garlicGO != null)
        {
            var sr = garlicGO.GetComponent<SpriteRenderer>();
            if (sr != null) { sr.sprite = garlicSprite; EditorUtility.SetDirty(sr); }
        }
        else Debug.LogWarning("Kitchen: nie znaleziono obiektu 'Garlic'.");

        WireInventoryUI(scene, icoGarlic);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("✓ Kitchen: czosnek i InventoryUI podpięte.");
    }

    static void WireInventoryUI(UnityEngine.SceneManagement.Scene scene, Sprite icoGarlic)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            var invUI = root.GetComponentInChildren<InventoryUI>(true);
            if (invUI == null) continue;
            var so = new SerializedObject(invUI);
            var prop = so.FindProperty("garlicSprite");
            if (prop != null)
            {
                prop.objectReferenceValue = icoGarlic;
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(invUI);
            }
            break;
        }
    }

    // ── helpers ─────────────────────────────────────────────────────────────

    static Sprite MakeSprite(string name, Color color, int w, int h)
    {
        string path = $"{PlaceholderDir}/{name}.png";
        if (AssetDatabase.LoadAssetAtPath<Sprite>(path) != null) return null; // already exists

        var tex = new Texture2D(w, h);
        var pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();
        System.IO.File.WriteAllBytes(
            System.IO.Path.Combine(Application.dataPath.Replace("Assets", ""), path),
            tex.EncodeToPNG());
        return null; // loaded after Refresh
    }

    static void EnsureFolder(string parent, string child)
    {
        string full = $"{parent}/{child}";
        if (!AssetDatabase.IsValidFolder(full)) AssetDatabase.CreateFolder(parent, child);
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
