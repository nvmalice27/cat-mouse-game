using UnityEngine;
using UnityEditor;
using System.IO;

public static class AddContainerObjects
{
    const string PlaceholderDir = "Assets/Art/Placeholders";

    // Uruchom mając otwartą scenę Room
    [MenuItem("CatMouse/Add Containers - Room (Szuflada + Koldra)")]
    public static void AddContainersRoom()
    {
        EnsureFolder("Assets/Art", "Placeholders");

        var sprDrawerClosed = MakeSprite("szuflada_zamknieta", new Color(0.55f, 0.35f, 0.15f), 120,  50);
        var sprDrawerOpen   = MakeSprite("szuflada_otwarta",   new Color(0.72f, 0.52f, 0.28f), 120,  50);
        var sprBlanketOn    = MakeSprite("koldra_zakryta",     new Color(0.30f, 0.58f, 0.72f), 160,  80);
        var sprBlanketOff   = MakeSprite("koldra_odkryta",     new Color(0.55f, 0.82f, 0.92f), 160,  80);
        var sprSockHidden   = MakeSprite("sock_hidden",        new Color(1.00f, 0.71f, 0.76f),  30,  50);
        var sprItemHidden   = MakeSprite("item_hidden",        new Color(1.00f, 0.85f, 0.30f),  40,  40);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        var drawer = MakeContainer("Szuflada", new Vector3(-1.5f, -0.5f, 0),
            sprDrawerClosed, sprDrawerOpen, sortOrder: 2);
        var sockGO = MakeHidden("Sock_Hidden", drawer.transform, sprSockHidden);
        var sock   = sockGO.AddComponent<SockObject>();
        SetIntField(sock, "sockIndex", 3);
        WireHiddenObjects(drawer.GetComponent<ContainerObject>(), new[] { sockGO });

        var blanket = MakeContainer("Koldra", new Vector3(2.0f, 1.3f, 0),
            sprBlanketOn, sprBlanketOff, sortOrder: 3);
        var itemGO = MakeHidden("Hidden_Item", blanket.transform, sprItemHidden);
        WireHiddenObjects(blanket.GetComponent<ContainerObject>(), new[] { itemGO });

        SaveScene();
        Debug.Log("Szuflada + Koldra dodane do sceny Room!");
    }

    // Uruchom mając otwartą scenę Kitchen — tworzy lodówkę z placeholderem
    [MenuItem("CatMouse/Add Containers - Kitchen (Lodowka)")]
    public static void AddContainersKitchen()
    {
        EnsureFolder("Assets/Art", "Placeholders");

        var sprFridgeClosed = MakeSprite("lodowka_zamknieta", new Color(0.90f, 0.95f, 1.00f), 80, 160);
        var sprFridgeOpen   = MakeSprite("lodowka_otwarta",   new Color(0.70f, 0.88f, 0.95f), 80, 160);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        var fridge = MakeContainer("Lodowka", new Vector3(2.5f, 0.0f, 0),
            sprFridgeClosed, sprFridgeOpen, sortOrder: 2);

        SaveScene();
        Debug.Log("Lodowka dodana. Uruchom teraz 'Setup Fridge Contents' zeby podpiac skladniki.");
    }

    // Uruchom mając otwartą scenę Kitchen — podpina istniejące obiekty do lodówki
    [MenuItem("CatMouse/Setup Fridge Contents (Kitchen)")]
    public static void SetupFridgeContents()
    {
        var fridgeGO = GameObject.Find("Lodowka");
        if (fridgeGO == null)
        {
            Debug.LogError("Nie znaleziono 'Lodowka' — najpierw uruchom 'Add Containers - Kitchen'.");
            return;
        }

        var container = fridgeGO.GetComponent<ContainerObject>();
        if (container == null)
        {
            Debug.LogError("'Lodowka' nie ma komponentu ContainerObject.");
            return;
        }

        var drink = GameObject.Find("DrinkPickup");
        var ing1  = GameObject.Find("Ingredient_1");
        var ing2  = GameObject.Find("Ingredient_2");

        if (drink == null) Debug.LogWarning("Nie znaleziono 'DrinkPickup' w scenie.");
        if (ing1  == null) Debug.LogWarning("Nie znaleziono 'Ingredient_1' w scenie.");
        if (ing2  == null) Debug.LogWarning("Nie znaleziono 'Ingredient_2' w scenie.");

        // Przenieś pod lodówkę i ukryj
        Reparent(drink, fridgeGO.transform);
        Reparent(ing1,  fridgeGO.transform);
        Reparent(ing2,  fridgeGO.transform);

        // Podpnij jako hiddenObjects
        var hidden = BuildArray(drink, ing1, ing2);
        WireHiddenObjects(container, hidden);

        // Upewnij się że IngredientSource ma KitchenScene
        WireIngredientSourceIfNeeded(ing1);
        WireIngredientSourceIfNeeded(ing2);

        SaveScene();
        Debug.Log("Lodowka okablowana: DrinkPickup + Ingredient_1 + Ingredient_2 schowane w srodku.");
    }

    // Naprawia brakujące referencje na istniejącej Lodówce
    [MenuItem("CatMouse/Fix Fridge Ingredient (Kitchen)")]
    public static void FixFridgeIngredient()
    {
        var fridgeGO = GameObject.Find("Lodowka");
        if (fridgeGO == null) { Debug.LogError("Nie znaleziono 'Lodowka'."); return; }

        foreach (Transform child in fridgeGO.transform)
        {
            var src = child.GetComponent<IngredientSource>();
            if (src != null) WireIngredientSourceIfNeeded(child.gameObject);
        }

        SaveScene();
        Debug.Log("Referencje KitchenScene naprawione.");
    }

    // ── helpers ────────────────────────────────────────────────────────────────

    static void Reparent(GameObject go, Transform newParent)
    {
        if (go == null) return;
        go.transform.SetParent(newParent, worldPositionStays: true);
        go.SetActive(false);
    }

    static GameObject[] BuildArray(params GameObject[] items)
    {
        int count = 0;
        foreach (var i in items) if (i != null) count++;
        var arr = new GameObject[count];
        int idx = 0;
        foreach (var i in items) if (i != null) arr[idx++] = i;
        return arr;
    }

    static void WireIngredientSourceIfNeeded(GameObject go)
    {
        if (go == null) return;
        var src = go.GetComponent<IngredientSource>();
        if (src == null) return;

        var so      = new SerializedObject(src);
        var kitProp = so.FindProperty("kitchen");
        if (kitProp.objectReferenceValue != null) return; // już podpięte

        var kitchenScene = Object.FindObjectOfType<KitchenScene>();
        if (kitchenScene == null) { Debug.LogWarning("Nie znaleziono KitchenScene w scenie."); return; }

        kitProp.objectReferenceValue = kitchenScene;
        so.ApplyModifiedProperties();
    }

    static GameObject MakeContainer(string name, Vector3 pos,
        Sprite closedSpr, Sprite openSpr, int sortOrder)
    {
        var go = new GameObject(name);
        go.transform.position = pos;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = closedSpr;
        sr.sortingOrder = sortOrder;

        var col  = go.AddComponent<BoxCollider2D>();
        col.size = closedSpr != null ? closedSpr.bounds.size : new Vector2(1f, 1f);

        var container = go.AddComponent<ContainerObject>();
        SetField(container, "sr",           sr);
        SetField(container, "closedSprite", closedSpr);
        SetField(container, "openSprite",   openSpr);

        return go;
    }

    static GameObject MakeHidden(string name, Transform parent, Sprite spr)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = spr;
        sr.sortingOrder = parent.GetComponent<SpriteRenderer>().sortingOrder + 1;

        var col  = go.AddComponent<BoxCollider2D>();
        col.size = spr != null ? spr.bounds.size : new Vector2(0.5f, 0.5f);

        go.SetActive(false);
        return go;
    }

    static void WireHiddenObjects(ContainerObject container, GameObject[] objs)
    {
        var so = new SerializedObject(container);
        var sp = so.FindProperty("hiddenObjects");
        sp.ClearArray();
        sp.arraySize = objs.Length;
        for (int i = 0; i < objs.Length; i++)
            sp.GetArrayElementAtIndex(i).objectReferenceValue = objs[i];
        so.ApplyModifiedProperties();
    }

    static Sprite MakeSprite(string name, Color color, int w, int h)
    {
        string path    = $"{PlaceholderDir}/{name}.png";
        string absPath = Path.Combine(Application.dataPath.Replace("Assets", ""), path);

        var tex    = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();
        File.WriteAllBytes(absPath, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path);

        var imp = (TextureImporter)AssetImporter.GetAtPath(path);
        imp.textureType         = TextureImporterType.Sprite;
        imp.spritePixelsPerUnit = 100;
        imp.filterMode          = FilterMode.Point;
        imp.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static void SetField(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        var sp = so.FindProperty(field);
        if (sp != null) { sp.objectReferenceValue = value; so.ApplyModifiedProperties(); }
        else Debug.LogWarning($"Pole '{field}' nie znalezione na {target.GetType().Name}");
    }

    static void SetIntField(Object target, string field, int value)
    {
        var so = new SerializedObject(target);
        var sp = so.FindProperty(field);
        if (sp != null) { sp.intValue = value; so.ApplyModifiedProperties(); }
        else Debug.LogWarning($"Pole int '{field}' nie znalezione na {target.GetType().Name}");
    }

    static void SaveScene()
    {
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
    }

    static void EnsureFolder(string parent, string child)
    {
        string full = $"{parent}/{child}";
        if (!AssetDatabase.IsValidFolder(full))
            AssetDatabase.CreateFolder(parent, child);
    }
}
