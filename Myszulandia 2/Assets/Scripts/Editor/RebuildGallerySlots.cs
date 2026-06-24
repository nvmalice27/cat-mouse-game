using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class RebuildGallerySlots
{
    const int TotalSlots = 21;  // 3 cols × 7 rows

    [MenuItem("CatMouse/Rebuild Gallery Slots (Gallery scene)")]
    public static void Rebuild()
    {
        var galleryUI = Object.FindObjectOfType<GalleryUI>();
        if (galleryUI == null)
        {
            Debug.LogError("Nie znaleziono GalleryUI. Otwórz scenę Gallery.");
            return;
        }

        var contentGO = GameObject.Find("Content");
        if (contentGO == null)
        {
            Debug.LogError("Nie znaleziono GO 'Content'.");
            return;
        }

        // Delete all existing GallerySlot children
        var toDelete = new List<GameObject>();
        for (int i = 0; i < contentGO.transform.childCount; i++)
            toDelete.Add(contentGO.transform.GetChild(i).gameObject);
        foreach (var child in toDelete)
            Object.DestroyImmediate(child);

        // Update Content height: 7 rows × 200 + 6 gaps × 20 + top/bot padding 2×20 = 1560
        var contentRT = contentGO.GetComponent<RectTransform>();
        if (contentRT != null)
            contentRT.sizeDelta = new Vector2(contentRT.sizeDelta.x, 1560);

        var lockedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Placeholders/gallery_locked.png");

        var slotComponents = new GallerySlot[TotalSlots];
        for (int i = 0; i < TotalSlots; i++)
        {
            var slotGO  = new GameObject($"GallerySlot_{i + 1}");
            slotGO.transform.SetParent(contentGO.transform, false);
            var slotImg = slotGO.AddComponent<Image>();
            slotImg.color = new Color(0.15f, 0.12f, 0.22f, 0.95f);

            // Mouse image
            var mouseImgGO = new GameObject("MouseImage");
            mouseImgGO.transform.SetParent(slotGO.transform, false);
            var mouseImgRT  = mouseImgGO.AddComponent<RectTransform>();
            mouseImgRT.anchorMin = new Vector2(0.1f, 0.35f);
            mouseImgRT.anchorMax = new Vector2(0.9f, 0.95f);
            mouseImgRT.offsetMin = Vector2.zero;
            mouseImgRT.offsetMax = Vector2.zero;
            var mouseImg = mouseImgGO.AddComponent<Image>();
            mouseImg.preserveAspect = true;

            // Locked overlay
            var overlayGO  = new GameObject("LockedOverlay");
            overlayGO.transform.SetParent(slotGO.transform, false);
            var overlayRT  = overlayGO.AddComponent<RectTransform>();
            overlayRT.anchorMin = Vector2.zero;
            overlayRT.anchorMax = Vector2.one;
            overlayRT.offsetMin = Vector2.zero;
            overlayRT.offsetMax = Vector2.zero;
            var overlayImg = overlayGO.AddComponent<Image>();
            overlayImg.color = new Color(0f, 0f, 0f, 0.75f);
            if (lockedSprite != null) overlayImg.sprite = lockedSprite;

            // Name text
            var nameGO = new GameObject("NameText");
            nameGO.transform.SetParent(slotGO.transform, false);
            var nameRT = nameGO.AddComponent<RectTransform>();
            nameRT.anchorMin = new Vector2(0, 0.18f);
            nameRT.anchorMax = new Vector2(1, 0.38f);
            nameRT.offsetMin = new Vector2(4, 0);
            nameRT.offsetMax = new Vector2(-4, 0);
            var nameTMP      = nameGO.AddComponent<TextMeshProUGUI>();
            nameTMP.text     = "???";
            nameTMP.fontSize = 14;
            nameTMP.color    = Color.white;
            nameTMP.alignment = TextAlignmentOptions.Center;

            // Hint text
            var hintGO = new GameObject("HintText");
            hintGO.transform.SetParent(slotGO.transform, false);
            var hintRT = hintGO.AddComponent<RectTransform>();
            hintRT.anchorMin = new Vector2(0, 0f);
            hintRT.anchorMax = new Vector2(1, 0.18f);
            hintRT.offsetMin = new Vector2(4, 0);
            hintRT.offsetMax = new Vector2(-4, 0);
            var hintTMP      = hintGO.AddComponent<TextMeshProUGUI>();
            hintTMP.text     = "";
            hintTMP.fontSize = 10;
            hintTMP.color    = new Color(0.8f, 0.8f, 0.8f);
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

        // Re-wire mouseTypes[] from generated assets
        MouseTypeSOGenerator.WireGallery();

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log($"✓ Galeria przebudowana: {TotalSlots} slotów (3×7), slots[] i mouseTypes[] zaktualizowane.");
    }

    static void SetRef(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        var sp = so.FindProperty(field);
        if (sp != null) { sp.objectReferenceValue = value; so.ApplyModifiedProperties(); }
        else Debug.LogWarning($"Pole '{field}' nie znalezione na {target.GetType().Name}");
    }
}
