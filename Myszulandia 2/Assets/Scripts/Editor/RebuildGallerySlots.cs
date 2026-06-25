using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

public static class RebuildGallerySlots
{
    // 3 cols × 9 rows = 27 — matches CollectibleIndex size
    const int TotalSlots    = 27;
    const int Cols          = 3;

    // Slot dimensions tuned so 9 rows fit in a 1920×1080 canvas without scrolling.
    // Available viewport height ≈ 950px (canvas 1080 − top offset 100 − bottom 30).
    // 9 rows × 90 + 8 gaps × 10 + padding 40 = 930 ≤ 950 ✓
    const float SlotW       = 560f;
    const float SlotH       = 90f;
    const float SpacingX    = 10f;
    const float SpacingY    = 10f;
    const float PadSide     = 80f;   // wide side padding centres 3 cols in 1860px
    const float PadVert     = 20f;
    static readonly float ContentH =
        TotalSlots / Cols * SlotH + (TotalSlots / Cols - 1) * SpacingY + PadVert * 2;

    [MenuItem("CatMouse/Rebuild Gallery Slots (Gallery scene)")]
    public static void Rebuild()
    {
        // Open Gallery scene automatically if not already loaded
        var galleryUI    = Object.FindObjectOfType<GalleryUI>();
        bool openedScene = false;

        if (galleryUI == null)
        {
            string path = FindScenePath("Gallery");
            if (path == null)
            {
                Debug.LogError("RebuildGallerySlots: Nie znaleziono sceny Gallery. Dodaj ją do Build Settings.");
                return;
            }
            EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            galleryUI    = Object.FindObjectOfType<GalleryUI>();
            openedScene  = true;
            if (galleryUI == null)
            {
                Debug.LogError("RebuildGallerySlots: Scena Gallery nie zawiera GalleryUI.");
                return;
            }
        }

        var contentGO = GameObject.Find("Content");
        if (contentGO == null) { Debug.LogError("Nie znaleziono GO 'Content'."); return; }

        // Remove old slots
        var toDelete = new List<GameObject>();
        for (int i = 0; i < contentGO.transform.childCount; i++)
            toDelete.Add(contentGO.transform.GetChild(i).gameObject);
        foreach (var child in toDelete) Object.DestroyImmediate(child);

        // Resize Content to fit all rows without scrolling
        var contentRT = contentGO.GetComponent<RectTransform>();
        if (contentRT != null) contentRT.sizeDelta = new Vector2(contentRT.sizeDelta.x, ContentH);

        // Update GridLayoutGroup if present
        var grid = contentGO.GetComponent<GridLayoutGroup>();
        if (grid != null)
        {
            grid.cellSize        = new Vector2(SlotW, SlotH);
            grid.spacing         = new Vector2(SpacingX, SpacingY);
            grid.padding         = new RectOffset((int)PadSide, (int)PadSide, (int)PadVert, (int)PadVert);
            grid.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = Cols;
        }

        var lockedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Placeholders/gallery_locked.png");

        var slotComponents = new GallerySlot[TotalSlots];
        for (int i = 0; i < TotalSlots; i++)
        {
            var slotGO  = new GameObject($"GallerySlot_{i + 1}");
            slotGO.transform.SetParent(contentGO.transform, false);
            var slotImg = slotGO.AddComponent<Image>();
            slotImg.color = new Color(0.15f, 0.12f, 0.22f, 0.95f);

            // Small mouse image — left strip (0–18% width, 10–90% height)
            var mouseImgGO = new GameObject("MouseImage");
            mouseImgGO.transform.SetParent(slotGO.transform, false);
            var mouseImgRT       = mouseImgGO.AddComponent<RectTransform>();
            mouseImgRT.anchorMin = new Vector2(0.01f, 0.10f);
            mouseImgRT.anchorMax = new Vector2(0.17f, 0.90f);
            mouseImgRT.offsetMin = Vector2.zero;
            mouseImgRT.offsetMax = Vector2.zero;
            var mouseImg = mouseImgGO.AddComponent<Image>();
            mouseImg.preserveAspect = true;

            // Locked overlay — full slot
            var overlayGO  = new GameObject("LockedOverlay");
            overlayGO.transform.SetParent(slotGO.transform, false);
            var overlayRT  = overlayGO.AddComponent<RectTransform>();
            overlayRT.anchorMin = Vector2.zero;
            overlayRT.anchorMax = Vector2.one;
            overlayRT.offsetMin = Vector2.zero;
            overlayRT.offsetMax = Vector2.zero;
            var overlayImg = overlayGO.AddComponent<Image>();
            overlayImg.color = new Color(0f, 0f, 0f, 0.60f);
            if (lockedSprite != null) overlayImg.sprite = lockedSprite;

            // Name text — right 82%, upper 55%
            var nameGO = new GameObject("NameText");
            nameGO.transform.SetParent(slotGO.transform, false);
            var nameRT       = nameGO.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0.19f, 0.45f);
            nameRT.anchorMax = new Vector2(1.00f, 1.00f);
            nameRT.offsetMin = new Vector2(4, 0);
            nameRT.offsetMax = new Vector2(-4, -2);
            var nameTMP      = nameGO.AddComponent<TextMeshProUGUI>();
            nameTMP.text      = "???";
            nameTMP.fontSize  = 13;
            nameTMP.fontStyle = FontStyles.Bold;
            nameTMP.color     = Color.white;
            nameTMP.alignment = TextAlignmentOptions.MidlineLeft;

            // Hint text — right 82%, lower 45%
            var hintGO = new GameObject("HintText");
            hintGO.transform.SetParent(slotGO.transform, false);
            var hintRT       = hintGO.AddComponent<RectTransform>();
            hintRT.anchorMin = new Vector2(0.19f, 0.00f);
            hintRT.anchorMax = new Vector2(1.00f, 0.48f);
            hintRT.offsetMin = new Vector2(4, 2);
            hintRT.offsetMax = new Vector2(-4, 0);
            var hintTMP      = hintGO.AddComponent<TextMeshProUGUI>();
            hintTMP.text      = "";
            hintTMP.fontSize  = 10;
            hintTMP.color     = new Color(0.78f, 0.78f, 0.78f);
            hintTMP.alignment = TextAlignmentOptions.MidlineLeft;

            var slot = slotGO.AddComponent<GallerySlot>();
            SetRef(slot, "mouseImage",    mouseImg);
            SetRef(slot, "lockedOverlay", overlayImg);
            SetRef(slot, "nameText",      (Object)nameTMP);
            SetRef(slot, "hintText",      (Object)hintTMP);
            if (lockedSprite != null) SetRef(slot, "lockedSprite", (Object)lockedSprite);

            slotComponents[i] = slot;
        }

        // Wire GalleryUI.slots[]
        var gallerySO = new SerializedObject(galleryUI);
        var slotsProp = gallerySO.FindProperty("slots");
        slotsProp.arraySize = TotalSlots;
        for (int i = 0; i < TotalSlots; i++)
            slotsProp.GetArrayElementAtIndex(i).objectReferenceValue = slotComponents[i];
        gallerySO.ApplyModifiedProperties();
        EditorUtility.SetDirty(galleryUI);

        // Wire mouseTypes[]
        MouseTypeSOGenerator.WireGallery();

        EditorSceneManager.SaveScene(galleryUI.gameObject.scene);
        if (openedScene) EditorSceneManager.CloseScene(galleryUI.gameObject.scene, true);

        Debug.Log($"✓ Galeria przebudowana: {TotalSlots} slotów ({Cols}×{TotalSlots / Cols}), bez przewijania.");
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

    static void SetRef(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        var sp = so.FindProperty(field);
        if (sp != null) { sp.objectReferenceValue = value; so.ApplyModifiedProperties(); }
        else Debug.LogWarning($"Pole '{field}' nie znalezione na {target.GetType().Name}");
    }
}
