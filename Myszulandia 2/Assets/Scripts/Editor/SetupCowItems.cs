using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public static class SetupCowItems
{
    const string ScenePath_Room     = "Assets/_Scenes/Room.unity";
    const string ScenePath_Kitchen  = "Assets/_Scenes/Kitchen.unity";
    const string ScenePath_Bathroom = "Assets/_Scenes/Bathroom.unity";

    [MenuItem("CatMouse/Setup Cow Items (Step 3+4)")]
    static void Run()
    {
        // 1. Create placeholder sprites
        Sprite cowEarsScene = GetOrCreateSprite("cow_ears");
        Sprite cowScene     = GetOrCreateSprite("cow");
        Sprite cowEarsIco   = GetOrCreateSprite("ico_cow_ears");
        Sprite cowIco       = GetOrCreateSprite("ico_cow");

        if (cowEarsScene == null || cowScene == null || cowEarsIco == null || cowIco == null)
        {
            Debug.LogError("SetupCowItems: nie udało się stworzyć sprite'ów. Sprawdź Assets/Art/Placeholders/.");
            return;
        }

        // 2. Add GameObjects to Room scene
        AddObjectsToRoom(cowEarsScene, cowScene);

        // 3. Assign inventory sprites in all three scenes
        AssignInventorySprites(ScenePath_Room,     cowEarsIco, cowIco);
        AssignInventorySprites(ScenePath_Kitchen,  cowEarsIco, cowIco);
        AssignInventorySprites(ScenePath_Bathroom, cowEarsIco, cowIco);

        Debug.Log("✓ SetupCowItems: gotowe! Sprawdź scenę Room i InventoryUI.");
    }

    static Sprite GetOrCreateSprite(string name)
    {
        string assetPath = $"Assets/Art/Placeholders/{name}.png";
        Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        if (existing != null) return existing;

        // Create 32×32 black square PNG
        Texture2D tex = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[32 * 32];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.black;
        tex.SetPixels(pixels);
        tex.Apply();
        byte[] png = tex.EncodeToPNG();
        Object.DestroyImmediate(tex);

        string fullPath = Path.Combine(Application.dataPath, "..", assetPath);
        File.WriteAllBytes(Path.GetFullPath(fullPath), png);
        AssetDatabase.ImportAsset(assetPath);

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
        if (importer == null) { Debug.LogError($"SetupCowItems: nie znaleziono importera dla {assetPath}"); return null; }
        importer.textureType       = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 100;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    static void AddObjectsToRoom(Sprite cowEarsSprite, Sprite cowSprite)
    {
        var scene = EditorSceneManager.OpenScene(ScenePath_Room, OpenSceneMode.Single);

        // Guard: don't add duplicates
        if (GameObject.Find("CowEarsPickup") == null)
            CreatePickup("CowEarsPickup", typeof(CowEarsObject), cowEarsSprite, new Vector3(-1f, 1f, 0f));

        if (GameObject.Find("CowPickup") == null)
            CreatePickup("CowPickup", typeof(CowObject), cowSprite, new Vector3(1f, 1f, 0f));

        EditorSceneManager.SaveScene(scene);
    }

    static void CreatePickup(string name, System.Type scriptType, Sprite sprite, Vector3 position)
    {
        var go = new GameObject(name);
        go.transform.position = position;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = sprite;
        sr.sortingOrder = 2;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.32f, 0.32f);

        go.AddComponent(scriptType);
    }

    static void AssignInventorySprites(string scenePath, Sprite cowEarsSprite, Sprite cowSprite)
    {
        bool wasLoaded = false;
        UnityEngine.SceneManagement.Scene scene = default;

        // Check if already loaded
        for (int i = 0; i < UnityEditor.SceneManagement.EditorSceneManager.sceneCount; i++)
        {
            var s = UnityEditor.SceneManagement.EditorSceneManager.GetSceneAt(i);
            if (s.path == scenePath && s.isLoaded) { scene = s; wasLoaded = true; break; }
        }
        if (!wasLoaded)
            scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

        bool changed = false;
        foreach (var root in scene.GetRootGameObjects())
        {
            foreach (var ui in root.GetComponentsInChildren<InventoryUI>(true))
            {
                var so = new SerializedObject(ui);
                SetSprite(so, "cowEarsSprite", cowEarsSprite, ref changed);
                SetSprite(so, "cowSprite",     cowSprite,     ref changed);
                if (changed) so.ApplyModifiedProperties();
            }
        }

        if (changed) EditorSceneManager.SaveScene(scene);
        if (!wasLoaded) EditorSceneManager.CloseScene(scene, true);
    }

    static void SetSprite(SerializedObject so, string field, Sprite sprite, ref bool changed)
    {
        var prop = so.FindProperty(field);
        if (prop == null) return;
        if (prop.objectReferenceValue != sprite) { prop.objectReferenceValue = sprite; changed = true; }
    }
}
