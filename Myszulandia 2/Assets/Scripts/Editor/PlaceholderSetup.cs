using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;

public static class PlaceholderSetup
{
    const string PlaceholderDir    = "Assets/Art/Placeholders";
    const string PlaceholderDirAbs = "Assets/Art/Placeholders";

    [MenuItem("CatMouse/Add Camera")]
    public static void AddCamera()
    {
        if (Object.FindObjectOfType<Camera>() != null)
        {
            Debug.Log("Kamera już istnieje na scenie.");
            return;
        }
        var camGO = new GameObject("Main Camera");
        camGO.tag = "MainCamera";
        var cam = camGO.AddComponent<Camera>();
        cam.orthographic     = true;
        cam.orthographicSize = 5f;
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = new Color(0.2f, 0.2f, 0.2f);
        cam.depth            = -1;
        camGO.AddComponent<AudioListener>();
        camGO.transform.position = new Vector3(0, 0, -10);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("✓ Kamera dodana!");
    }

    [MenuItem("CatMouse/Setup Placeholders + InventoryUI")]
    public static void Setup()
    {
        EnsureFolder("Assets/Art", "Placeholders");

        // ── sprites for scene objects ──────────────────────────────────────
        var sprBackground = MakeSprite("bg",         new Color(0.53f, 0.81f, 0.98f), 1920, 1080);
        var sprCat        = MakeSprite("cat",        new Color(1.00f, 0.55f, 0.00f),   80,  80);
        var sprMouse      = MakeSprite("mouse",      new Color(0.70f, 0.70f, 0.70f),   60,  60);
        var sprDirty      = MakeSprite("dirty",      new Color(0.40f, 0.25f, 0.05f),   60,  60);
        var sprBed        = MakeSprite("bed",        new Color(0.55f, 0.27f, 0.07f),  120,  70);
        var sprBedMade    = MakeSprite("bed_made",   new Color(0.80f, 0.60f, 0.40f),  120,  70);
        var sprPillow     = MakeSprite("pillow",     new Color(0.96f, 0.87f, 0.70f),   60,  40);
        var sprBike       = MakeSprite("bike",       new Color(0.80f, 0.10f, 0.10f),  100,  70);
        var sprRadio      = MakeSprite("radio",      new Color(0.50f, 0.00f, 0.50f),   80,  50);
        var sprPhone      = MakeSprite("phone",      new Color(0.20f, 0.20f, 0.20f),   50,  90);
        var sprDoor       = MakeSprite("door",       new Color(0.29f, 0.18f, 0.10f),   70, 120);
        var sprCrumb      = MakeSprite("crumb",      new Color(1.00f, 0.85f, 0.00f),   24,  24);
        var sprSock       = MakeSprite("sock",       new Color(1.00f, 0.71f, 0.76f),   30,  50);

        // ── sprites for inventory icons (UI) ───────────────────────────────
        var icoSock      = MakeSprite("ico_sock",       new Color(1.00f, 0.71f, 0.76f), 48, 48);
        var icoCrumb     = MakeSprite("ico_crumb",      new Color(1.00f, 0.85f, 0.00f), 48, 48);
        var icoMealGood  = MakeSprite("ico_meal_good",  new Color(0.20f, 0.80f, 0.20f), 48, 48);
        var icoMealBad   = MakeSprite("ico_meal_bad",   new Color(0.85f, 0.15f, 0.15f), 48, 48);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // ── assign to scene objects ────────────────────────────────────────
        AssignSprite("Background", sprBackground);
        AssignSprite("Cat",        sprCat);
        AssignSprite("Mouse",      sprMouse);

        var mouseGO = GameObject.Find("Mouse");
        if (mouseGO != null)
        {
            var overlay = mouseGO.transform.Find("DirtyOverlay");
            if (overlay != null) overlay.GetComponent<SpriteRenderer>().sprite = sprDirty;

            var mouseCtrl = mouseGO.GetComponent<MouseController>();
            if (mouseCtrl != null)
            {
                SetField(mouseCtrl, "normalSprite", sprMouse);
                SetField(mouseCtrl, "hungrySprite", sprMouse);
            }
        }

        AssignSprite("Bed",    sprBed);
        AssignSprite("Pillow", sprPillow);
        AssignSprite("Bike",   sprBike);
        AssignSprite("Radio",  sprRadio);
        AssignSprite("Phone",  sprPhone);
        AssignSprite("Door",   sprDoor);

        var bedObj = GameObject.Find("Bed")?.GetComponent<BedObject>();
        if (bedObj != null) { SetField(bedObj, "madeSprite", sprBedMade); SetField(bedObj, "unmadeSprite", sprBed); }

        for (int i = 1; i <= 3; i++) AssignSprite($"Crumb_{i}", sprCrumb);
        for (int i = 1; i <= 3; i++) AssignSprite($"Sock_{i}",  sprSock);

        // ── fix all zero-size BoxCollider2Ds ───────────────────────────────
        FixAllColliders();

        // ── create InventorySlot prefab ────────────────────────────────────
        string prefabPath = "Assets/Prefabs/UI/InventorySlot.prefab";
        var slotGO = new GameObject("InventorySlot");

        var slotRT = slotGO.AddComponent<RectTransform>();
        slotRT.sizeDelta = new Vector2(60, 60);

        var slotImg  = slotGO.AddComponent<Image>();
        slotImg.color = new Color(0.15f, 0.15f, 0.2f, 0.9f);

        var slotDD = slotGO.AddComponent<DragDropItem>();

        // icon child
        var iconGO = new GameObject("Icon");
        iconGO.transform.SetParent(slotGO.transform, false);
        var iconRT   = iconGO.AddComponent<RectTransform>();
        iconRT.sizeDelta        = new Vector2(44, 44);
        iconRT.anchoredPosition = new Vector2(0, 4);
        var iconImg  = iconGO.AddComponent<Image>();
        iconImg.preserveAspect = true;

        // quantity text child
        var qtyGO = new GameObject("Qty");
        qtyGO.transform.SetParent(slotGO.transform, false);
        var qtyRT  = qtyGO.AddComponent<RectTransform>();
        qtyRT.sizeDelta        = new Vector2(60, 20);
        qtyRT.anchoredPosition = new Vector2(0, -22);
        var qtyTMP = qtyGO.AddComponent<TextMeshProUGUI>();
        qtyTMP.text      = "";
        qtyTMP.fontSize  = 13;
        qtyTMP.color     = Color.white;
        qtyTMP.alignment = TextAlignmentOptions.Center;

        // save prefab
        EnsureFolder("Assets/Prefabs", "UI");
        var prefab = PrefabUtility.SaveAsPrefabAsset(slotGO, prefabPath);
        Object.DestroyImmediate(slotGO);

        // ── wire InventoryUI ───────────────────────────────────────────────
        var invGO  = GameObject.Find("InventoryPanel");
        if (invGO != null)
        {
            // add horizontal layout + container child
            var container = invGO.transform.Find("SlotsContainer");
            if (container == null)
            {
                var cGO = new GameObject("SlotsContainer");
                cGO.transform.SetParent(invGO.transform, false);
                var cRT = cGO.AddComponent<RectTransform>();
                cRT.anchorMin = Vector2.zero;
                cRT.anchorMax = Vector2.one;
                cRT.offsetMin = new Vector2(5, 5);
                cRT.offsetMax = new Vector2(-5, -5);
                var hlg = cGO.AddComponent<HorizontalLayoutGroup>();
                hlg.spacing           = 6;
                hlg.childAlignment    = TextAnchor.MiddleLeft;
                hlg.childForceExpandWidth  = false;
                hlg.childForceExpandHeight = false;
                hlg.childControlWidth  = true;
                hlg.childControlHeight = true;
                container = cGO.transform;
            }

            var invUI = invGO.GetComponent<InventoryUI>();
            if (invUI != null)
            {
                SetField(invUI, "slotPrefab",    prefab);
                SetField(invUI, "container",     container);
                SetField(invUI, "crumbSprite",   icoCrumb);
                SetField(invUI, "sockSprite",    icoSock);
                SetField(invUI, "mealGoodSprite", icoMealGood);
                SetField(invUI, "mealBadSprite",  icoMealBad);
            }
        }

        // ── save scene ─────────────────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("✓ Placeholders + InventoryUI gotowe! Możesz teraz wcisnąć Play.");
    }

    // ── helpers ────────────────────────────────────────────────────────────

    static Sprite MakeSprite(string name, Color color, int w = 64, int h = 64)
    {
        string path    = $"{PlaceholderDir}/{name}.png";
        string absPath = Path.Combine(Application.dataPath.Replace("Assets",""), path);

        var tex    = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();
        File.WriteAllBytes(absPath, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path);

        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        imp.textureType       = TextureImporterType.Sprite;
        imp.spritePixelsPerUnit = 100;
        imp.filterMode        = FilterMode.Point;
        imp.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static void AssignSprite(string goName, Sprite sprite)
    {
        var go = GameObject.Find(goName);
        if (go == null) { Debug.LogWarning($"Nie znaleziono: {goName}"); return; }
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr != null) sr.sprite = sprite;
    }

    static void SetField(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        var sp = so.FindProperty(field);
        if (sp != null) { sp.objectReferenceValue = value; so.ApplyModifiedProperties(); }
        else Debug.LogWarning($"Pole '{field}' nie znalezione na {target.GetType().Name}");
    }

    static void FixAllColliders()
    {
        foreach (var col in Object.FindObjectsOfType<BoxCollider2D>())
        {
            if (col.size.x < 0.01f || col.size.y < 0.01f)
            {
                var sr = col.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite != null)
                    col.size = sr.sprite.bounds.size;
                else
                    col.size = new Vector2(1f, 1f);
                EditorUtility.SetDirty(col);
            }
        }
        Debug.Log("✓ Kolizje naprawione.");
    }

    static void EnsureFolder(string parent, string child)
    {
        string full = $"{parent}/{child}";
        if (!AssetDatabase.IsValidFolder(full))
            AssetDatabase.CreateFolder(parent, child);
    }
}
