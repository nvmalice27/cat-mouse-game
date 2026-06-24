using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public static class GallerySceneSetup
{
    const int SlotCount = 15;

    [MenuItem("CatMouse/Setup Gallery Scene")]
    public static void Setup()
    {
        // EventSystem
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // Camera
        if (Object.FindObjectOfType<Camera>() == null)
        {
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic      = true;
            cam.orthographicSize  = 5f;
            cam.clearFlags        = CameraClearFlags.SolidColor;
            cam.backgroundColor   = new Color(0.08f, 0.06f, 0.12f);
            cam.depth             = -1;
            camGO.AddComponent<AudioListener>();
            camGO.transform.position = new Vector3(0, 0, -10);
        }

        // ── MANAGERS ────────────────────────────────────────────────────────
        var mgr = new GameObject("GameManager");
        mgr.AddComponent<GameManager>();
        mgr.AddComponent<SaveManager>();
        mgr.AddComponent<EconomyManager>();
        mgr.AddComponent<MouseStateManager>();
        mgr.AddComponent<InventoryManager>();
        mgr.AddComponent<DayNightManager>();

        // ── BACKGROUND ──────────────────────────────────────────────────────
        var bg = new GameObject("Background");
        bg.AddComponent<SpriteRenderer>().sortingOrder = -10;

        // ── UI CANVAS ────────────────────────────────────────────────────────
        var canvasGO = new GameObject("UICanvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Full-screen gallery panel
        var galleryPanel = MakePanel("GalleryPanel", canvasGO.transform,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        // Title + close button
        MakeTMP("Title", galleryPanel.transform, "Galeria Myszek", new Vector2(0, -40), 36);

        var galleryUI = galleryPanel.AddComponent<GalleryUI>();

        var closeGO = new GameObject("CloseButton");
        closeGO.transform.SetParent(galleryPanel.transform, false);
        var closeRT = closeGO.AddComponent<RectTransform>();
        closeRT.anchorMin        = new Vector2(1f, 1f);
        closeRT.anchorMax        = new Vector2(1f, 1f);
        closeRT.pivot            = new Vector2(1f, 1f);
        closeRT.anchoredPosition = new Vector2(-20, -20);
        closeRT.sizeDelta        = new Vector2(110, 44);
        var closeImg = closeGO.AddComponent<Image>();
        closeImg.color = new Color(0.4f, 0.1f, 0.1f, 0.9f);
        var closeBtn = closeGO.AddComponent<Button>();
        MakeTMPChild("Label", closeGO.transform, "Wstecz", 18);
        var closeMethod = typeof(GalleryUI).GetMethod("Close");
        if (closeMethod != null)
        {
            var action = System.Delegate.CreateDelegate(
                typeof(UnityEngine.Events.UnityAction), galleryUI, closeMethod)
                as UnityEngine.Events.UnityAction;
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(closeBtn.onClick, action);
        }

        // Scroll view for the 15 slots (3 columns × 5 rows)
        var scrollGO = new GameObject("ScrollView");
        scrollGO.transform.SetParent(galleryPanel.transform, false);
        var scrollRT = scrollGO.AddComponent<RectTransform>();
        scrollRT.anchorMin        = Vector2.zero;
        scrollRT.anchorMax        = Vector2.one;
        scrollRT.offsetMin        = new Vector2(30, 30);
        scrollRT.offsetMax        = new Vector2(-30, -100);
        var scrollRect = scrollGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;

        // Viewport
        var viewportGO = new GameObject("Viewport");
        viewportGO.transform.SetParent(scrollGO.transform, false);
        var viewportRT = viewportGO.AddComponent<RectTransform>();
        viewportRT.anchorMin = Vector2.zero;
        viewportRT.anchorMax = Vector2.one;
        viewportRT.offsetMin = Vector2.zero;
        viewportRT.offsetMax = Vector2.zero;
        viewportGO.AddComponent<Image>().color = new Color(0, 0, 0, 0.01f);
        var mask = viewportGO.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        scrollRect.viewport = viewportRT;

        // Content with grid layout
        var contentGO = new GameObject("Content");
        contentGO.transform.SetParent(viewportGO.transform, false);
        var contentRT = contentGO.AddComponent<RectTransform>();
        contentRT.anchorMin        = new Vector2(0, 1);
        contentRT.anchorMax        = new Vector2(1, 1);
        contentRT.pivot            = new Vector2(0.5f, 1f);
        contentRT.anchoredPosition = Vector2.zero;
        contentRT.sizeDelta        = new Vector2(0, 1200);
        var grid = contentGO.AddComponent<GridLayoutGroup>();
        grid.cellSize        = new Vector2(280, 200);
        grid.spacing         = new Vector2(20, 20);
        grid.padding         = new RectOffset(20, 20, 20, 20);
        grid.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        scrollRect.content = contentRT;

        // Locked sprite (placeholder — dark square)
        var lockedSprite = MakeLockedSprite();

        // Build 15 gallery slots
        var slotComponents = new GallerySlot[SlotCount];
        for (int i = 0; i < SlotCount; i++)
        {
            var slotGO = new GameObject($"GallerySlot_{i + 1}");
            slotGO.transform.SetParent(contentGO.transform, false);
            var slotImg = slotGO.AddComponent<Image>();
            slotImg.color = new Color(0.15f, 0.12f, 0.22f, 0.95f);

            // Mouse image (top portion)
            var mouseImgGO = new GameObject("MouseImage");
            mouseImgGO.transform.SetParent(slotGO.transform, false);
            var mouseImgRT = mouseImgGO.AddComponent<RectTransform>();
            mouseImgRT.anchorMin        = new Vector2(0.1f, 0.35f);
            mouseImgRT.anchorMax        = new Vector2(0.9f, 0.95f);
            mouseImgRT.offsetMin        = Vector2.zero;
            mouseImgRT.offsetMax        = Vector2.zero;
            var mouseImg = mouseImgGO.AddComponent<Image>();
            mouseImg.preserveAspect = true;

            // Locked overlay
            var overlayGO = new GameObject("LockedOverlay");
            overlayGO.transform.SetParent(slotGO.transform, false);
            var overlayRT = overlayGO.AddComponent<RectTransform>();
            overlayRT.anchorMin = Vector2.zero;
            overlayRT.anchorMax = Vector2.one;
            overlayRT.offsetMin = Vector2.zero;
            overlayRT.offsetMax = Vector2.zero;
            var overlayImg = overlayGO.AddComponent<Image>();
            overlayImg.color  = new Color(0f, 0f, 0f, 0.75f);
            overlayImg.sprite = lockedSprite;

            // Name text
            var nameGO = new GameObject("NameText");
            nameGO.transform.SetParent(slotGO.transform, false);
            var nameRT = nameGO.AddComponent<RectTransform>();
            nameRT.anchorMin        = new Vector2(0, 0.18f);
            nameRT.anchorMax        = new Vector2(1, 0.38f);
            nameRT.offsetMin        = new Vector2(4, 0);
            nameRT.offsetMax        = new Vector2(-4, 0);
            var nameTMP = nameGO.AddComponent<TextMeshProUGUI>();
            nameTMP.text      = "???";
            nameTMP.fontSize  = 14;
            nameTMP.color     = Color.white;
            nameTMP.alignment = TextAlignmentOptions.Center;

            // Hint text
            var hintGO = new GameObject("HintText");
            hintGO.transform.SetParent(slotGO.transform, false);
            var hintRT = hintGO.AddComponent<RectTransform>();
            hintRT.anchorMin        = new Vector2(0, 0f);
            hintRT.anchorMax        = new Vector2(1, 0.18f);
            hintRT.offsetMin        = new Vector2(4, 0);
            hintRT.offsetMax        = new Vector2(-4, 0);
            var hintTMP = hintGO.AddComponent<TextMeshProUGUI>();
            hintTMP.text      = "";
            hintTMP.fontSize  = 10;
            hintTMP.color     = new Color(0.8f, 0.8f, 0.8f);
            hintTMP.alignment = TextAlignmentOptions.Center;

            // GallerySlot component
            var slot = slotGO.AddComponent<GallerySlot>();
            SetField(slot, "mouseImage",    mouseImg);
            SetField(slot, "lockedOverlay", overlayImg);
            SetField(slot, "nameText",      (Object)nameTMP);
            SetField(slot, "hintText",      (Object)hintTMP);
            SetFieldSprite(slot, "lockedSprite", lockedSprite);

            slotComponents[i] = slot;
        }

        // Wire GalleryUI.slots array
        var gallerySO  = new SerializedObject(galleryUI);
        var slotsProp  = gallerySO.FindProperty("slots");
        slotsProp.arraySize = SlotCount;
        for (int i = 0; i < SlotCount; i++)
            slotsProp.GetArrayElementAtIndex(i).objectReferenceValue = slotComponents[i];
        // mouseTypes[] left empty — fill when MouseTypeSO assets are created
        gallerySO.ApplyModifiedProperties();

        // ── SAVE ─────────────────────────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("✓ Gallery scene built! Przypisz MouseTypeSO assets do GalleryUI.mouseTypes[] gdy bedziesz tworzyć dane myszek.");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    static void SetField(Object target, string fieldName, Object value)
    {
        var so = new SerializedObject(target);
        var sp = so.FindProperty(fieldName);
        if (sp != null) { sp.objectReferenceValue = value; so.ApplyModifiedProperties(); }
        else Debug.LogWarning($"Field '{fieldName}' not found on {target.GetType().Name}");
    }

    static void SetFieldSprite(Object target, string fieldName, Sprite value)
    {
        var so = new SerializedObject(target);
        var sp = so.FindProperty(fieldName);
        if (sp != null) { sp.objectReferenceValue = value; so.ApplyModifiedProperties(); }
        else Debug.LogWarning($"Sprite field '{fieldName}' not found on {target.GetType().Name}");
    }

    static Sprite MakeLockedSprite()
    {
        string dir  = "Assets/Art/Placeholders";
        string path = $"{dir}/gallery_locked.png";

        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder("Assets/Art", "Placeholders");

        var tex    = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        var pixels = new Color[64 * 64];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(0.1f, 0.05f, 0.15f);
        tex.SetPixels(pixels);
        tex.Apply();
        System.IO.File.WriteAllBytes(
            System.IO.Path.Combine(Application.dataPath.Replace("Assets", ""), path),
            tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path);

        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        imp.textureType       = TextureImporterType.Sprite;
        imp.spritePixelsPerUnit = 100;
        imp.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static GameObject MakePanel(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt        = go.AddComponent<RectTransform>();
        rt.anchorMin  = anchorMin;
        rt.anchorMax  = anchorMax;
        rt.pivot      = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta  = size;
        return go;
    }

    static TextMeshProUGUI MakeTMP(string name, Transform parent, string text, Vector2 pos, float fontSize = 18)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = new Vector2(0.5f, 1f);
        rt.anchorMax        = new Vector2(0.5f, 1f);
        rt.sizeDelta        = new Vector2(600, 55);
        rt.anchoredPosition = pos;
        var tmp      = go.AddComponent<TextMeshProUGUI>();
        tmp.text     = text;
        tmp.fontSize = fontSize;
        tmp.color    = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        return tmp;
    }

    static void MakeTMPChild(string name, Transform parent, string text, float fontSize)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin        = Vector2.zero;
        rt.anchorMax        = Vector2.one;
        rt.offsetMin        = Vector2.zero;
        rt.offsetMax        = Vector2.zero;
        var tmp      = go.AddComponent<TextMeshProUGUI>();
        tmp.text     = text;
        tmp.fontSize = fontSize;
        tmp.color    = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
    }
}
