using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class AddBadStateGallerySlots
{
    [MenuItem("CatMouse/Add Bad State Gallery Slots (Gallery scene)")]
    public static void AddSlots()
    {
        var galleryUI = Object.FindObjectOfType<GalleryUI>();
        if (galleryUI == null)
        {
            Debug.LogError("Nie znaleziono GalleryUI. Otwórz scenę Gallery i spróbuj ponownie.");
            return;
        }

        // Find Content grid
        var contentGO = GameObject.Find("Content");
        if (contentGO == null)
        {
            Debug.LogError("Nie znaleziono GO 'Content' w scenie Gallery.");
            return;
        }

        // Update Content height to fit 6 rows: 6×200 + 5×20 + 2×20 padding = 1340
        var contentRT = contentGO.GetComponent<RectTransform>();
        if (contentRT != null)
            contentRT.sizeDelta = new Vector2(contentRT.sizeDelta.x, 1340);

        // Load locked sprite
        var lockedSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Placeholders/gallery_locked.png");

        // Read current slots from serialized object
        var gallerySO  = new SerializedObject(galleryUI);
        var slotsProp  = gallerySO.FindProperty("slots");
        int existing   = slotsProp.arraySize; // should be 15

        if (existing >= 18)
        {
            Debug.LogWarning($"GalleryUI.slots ma już {existing} wpisów — nie trzeba dodawać.");
            return;
        }

        var newSlots = new GallerySlot[18 - existing];

        for (int i = existing; i < 18; i++)
        {
            var slotGO  = new GameObject($"GallerySlot_{i + 1}");
            slotGO.transform.SetParent(contentGO.transform, false);
            var slotImg = slotGO.AddComponent<Image>();
            slotImg.color = new Color(0.15f, 0.12f, 0.22f, 0.95f);

            // Mouse image
            var mouseImgGO = new GameObject("MouseImage");
            mouseImgGO.transform.SetParent(slotGO.transform, false);
            var mouseImgRT  = mouseImgGO.AddComponent<RectTransform>();
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
            var overlayImg  = overlayGO.AddComponent<Image>();
            overlayImg.color  = new Color(0f, 0f, 0f, 0.75f);
            if (lockedSprite != null) overlayImg.sprite = lockedSprite;

            // Name text
            var nameGO = new GameObject("NameText");
            nameGO.transform.SetParent(slotGO.transform, false);
            var nameRT = nameGO.AddComponent<RectTransform>();
            nameRT.anchorMin        = new Vector2(0, 0.18f);
            nameRT.anchorMax        = new Vector2(1, 0.38f);
            nameRT.offsetMin        = new Vector2(4, 0);
            nameRT.offsetMax        = new Vector2(-4, 0);
            var nameTMP      = nameGO.AddComponent<TextMeshProUGUI>();
            nameTMP.text     = "???";
            nameTMP.fontSize = 14;
            nameTMP.color    = Color.white;
            nameTMP.alignment = TextAlignmentOptions.Center;

            // Hint text
            var hintGO = new GameObject("HintText");
            hintGO.transform.SetParent(slotGO.transform, false);
            var hintRT = hintGO.AddComponent<RectTransform>();
            hintRT.anchorMin        = new Vector2(0, 0f);
            hintRT.anchorMax        = new Vector2(1, 0.18f);
            hintRT.offsetMin        = new Vector2(4, 0);
            hintRT.offsetMax        = new Vector2(-4, 0);
            var hintTMP      = hintGO.AddComponent<TextMeshProUGUI>();
            hintTMP.text     = "";
            hintTMP.fontSize = 10;
            hintTMP.color    = new Color(0.8f, 0.8f, 0.8f);
            hintTMP.alignment = TextAlignmentOptions.Center;

            // GallerySlot component
            var slot = slotGO.AddComponent<GallerySlot>();
            SetRef(slot, "mouseImage",    mouseImg);
            SetRef(slot, "lockedOverlay", overlayImg);
            SetRef(slot, "nameText",      (Object)nameTMP);
            SetRef(slot, "hintText",      (Object)hintTMP);
            if (lockedSprite != null) SetRef(slot, "lockedSprite", (Object)lockedSprite);

            newSlots[i - existing] = slot;
        }

        // Expand slots array to 18
        slotsProp.arraySize = 18;
        for (int i = existing; i < 18; i++)
            slotsProp.GetArrayElementAtIndex(i).objectReferenceValue = newSlots[i - existing];

        gallerySO.ApplyModifiedProperties();
        EditorUtility.SetDirty(galleryUI);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log($"✓ Dodano {18 - existing} nowych slotów galerii (Smutna / Zlowroga / Sciekla). slots[] = 18.");
    }

    static void SetRef(Object target, string field, Object value)
    {
        var so = new SerializedObject(target);
        var sp = so.FindProperty(field);
        if (sp != null) { sp.objectReferenceValue = value; so.ApplyModifiedProperties(); }
        else Debug.LogWarning($"Pole '{field}' nie znalezione na {target.GetType().Name}");
    }
}
