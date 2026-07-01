using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using TMPro;

public static class RebuildGallerySlots
{
    // 28 slots, 4 cols × 7 rows
    const int TotalSlots    = 28;
    const int Cols          = 4;

    // Slot: 440×200px, 4 cols fill 1920px canvas minus 60px margins and 3×20px gaps
    const float SlotW       = 440f;
    const float SlotH       = 200f;
    const float SpacingX    = 20f;
    const float SpacingY    = 20f;
    const float PadSide     = 20f;
    const float PadVert     = 20f;
    static readonly float ContentH =
        7 * SlotH + 6 * SpacingY + PadVert * 2;   // 1560px

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

        // Ustaw scroll sensitivity na ScrollRect jeśli istnieje
        var scrollRect = contentGO.GetComponentInParent<ScrollRect>();
        if (scrollRect != null) scrollRect.scrollSensitivity = 40f;

        var slotComponents = new GallerySlot[TotalSlots];
        for (int i = 0; i < TotalSlots; i++)
        {
            var slotGO  = new GameObject($"GallerySlot_{i + 1}");
            slotGO.transform.SetParent(contentGO.transform, false);
            var slotImg = slotGO.AddComponent<Image>();
            slotImg.color = new Color(0.15f, 0.12f, 0.22f, 0.95f);

            // Grafika myszy — górne 55% slotu
            var mouseImgGO = new GameObject("MouseImage");
            mouseImgGO.transform.SetParent(slotGO.transform, false);
            var mouseImgRT       = mouseImgGO.AddComponent<RectTransform>();
            mouseImgRT.anchorMin = new Vector2(0.10f, 0.40f);
            mouseImgRT.anchorMax = new Vector2(0.90f, 0.95f);
            mouseImgRT.offsetMin = Vector2.zero;
            mouseImgRT.offsetMax = Vector2.zero;
            var mouseImg = mouseImgGO.AddComponent<Image>();
            mouseImg.preserveAspect = true;

            // Nakładka zablokowania — cały slot
            var overlayGO  = new GameObject("LockedOverlay");
            overlayGO.transform.SetParent(slotGO.transform, false);
            var overlayRT  = overlayGO.AddComponent<RectTransform>();
            overlayRT.anchorMin = Vector2.zero;
            overlayRT.anchorMax = Vector2.one;
            overlayRT.offsetMin = Vector2.zero;
            overlayRT.offsetMax = Vector2.zero;
            var overlayImg = overlayGO.AddComponent<Image>();
            overlayImg.color = new Color(0f, 0f, 0f, 0.70f);
            if (lockedSprite != null) overlayImg.sprite = lockedSprite;

            // Nazwa — środkowe 20% (35–55%)
            var nameGO = new GameObject("NameText");
            nameGO.transform.SetParent(slotGO.transform, false);
            var nameRT       = nameGO.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0f, 0.22f);
            nameRT.anchorMax = new Vector2(1f, 0.42f);
            nameRT.offsetMin = new Vector2(6, 0);
            nameRT.offsetMax = new Vector2(-6, 0);
            var nameTMP      = nameGO.AddComponent<TextMeshProUGUI>();
            nameTMP.text      = "???";
            nameTMP.fontSize  = 15;
            nameTMP.fontStyle = FontStyles.Bold;
            nameTMP.color     = Color.white;
            nameTMP.alignment = TextAlignmentOptions.Center;

            // Podpowiedź — dolne 22%
            var hintGO = new GameObject("HintText");
            hintGO.transform.SetParent(slotGO.transform, false);
            var hintRT       = hintGO.AddComponent<RectTransform>();
            hintRT.anchorMin = new Vector2(0f, 0.01f);
            hintRT.anchorMax = new Vector2(1f, 0.22f);
            hintRT.offsetMin = new Vector2(6, 0);
            hintRT.offsetMax = new Vector2(-6, 0);
            var hintTMP      = hintGO.AddComponent<TextMeshProUGUI>();
            hintTMP.text      = "";
            hintTMP.fontSize  = 11;
            hintTMP.color     = new Color(0.78f, 0.78f, 0.78f);
            hintTMP.alignment = TextAlignmentOptions.Center;

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

        int rows = (TotalSlots + Cols - 1) / Cols;
        Debug.Log($"✓ Galeria przebudowana: {TotalSlots} slotów ({Cols}×{rows} rzędów).");
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
