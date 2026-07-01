using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public static class PatchFoodMenu
{
    [MenuItem("CatMouse/Patch: 4 typy jedzenia + sprite'y")]
    public static void Patch()
    {
        ImportAndSetSprite("Assets/Art/Placeholders/ico_meal_good_2.png");
        ImportAndSetSprite("Assets/Art/Placeholders/ico_meal_bad_2.png");

        var mealGood2 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Placeholders/ico_meal_good_2.png");
        var mealBad2  = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Placeholders/ico_meal_bad_2.png");

        // ── InventoryUI: przypisz nowe sprite'y ───────────────────────────────
        var invUI = Object.FindObjectOfType<InventoryUI>();
        if (invUI != null)
        {
            var so = new SerializedObject(invUI);
            SetSprite(so, "mealGood2Sprite", mealGood2);
            SetSprite(so, "mealBad2Sprite",  mealBad2);
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(invUI);
            Debug.Log("✓ InventoryUI: przypisano mealGood2Sprite i mealBad2Sprite.");
        }
        else
        {
            Debug.LogWarning("Nie znaleziono InventoryUI na scenie.");
        }

        // ── FoodSubPanel: przebuduj przyciski ─────────────────────────────────
        // GameObject.Find pomija nieaktywne — szukamy przez wszystkie Transform
        GameObject foodSub = null;
        foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
        {
            if (t.name == "FoodSubPanel" && t.hideFlags == HideFlags.None)
            { foodSub = t.gameObject; break; }
        }
        if (foodSub == null)
        {
            Debug.LogWarning("Nie znaleziono FoodSubPanel. Upewnij się że scena Room jest otwarta.");
            return;
        }

        var allPhoneUIs = Resources.FindObjectsOfTypeAll<PhoneUI>();
        PhoneUI phoneUI = allPhoneUIs.Length > 0 ? allPhoneUIs[0] : null;
        if (phoneUI == null)
        {
            Debug.LogWarning("Nie znaleziono PhoneUI na scenie.");
            return;
        }

        // Przesuń tytuł w górę
        var title = foodSub.transform.Find("Title");
        if (title != null)
            (title as RectTransform).anchoredPosition = new Vector2(0, 90);

        // Przesuń istniejące przyciski
        MoveButton(foodSub, "Dobre jedzenie (50)", new Vector2(0, 45));
        MoveButton(foodSub, "Złe jedzenie (50)",   new Vector2(0, -5));

        // Dodaj 2 nowe przyciski (tylko jeśli jeszcze nie ma)
        if (foodSub.transform.Find("Dobre jedzenie 2 (50)") == null)
            AddButton(foodSub, "Dobre jedzenie 2 (50)", new Vector2(0, -55), phoneUI, "OrderGoodFood2");

        if (foodSub.transform.Find("Złe jedzenie 2 (50)") == null)
            AddButton(foodSub, "Złe jedzenie 2 (50)",   new Vector2(0,-105), phoneUI, "OrderBadFood2");

        EditorUtility.SetDirty(foodSub);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("✓ FoodSubPanel: przebudowano do 4 typów jedzenia. Zapisano scenę.");
    }

    static void ImportAndSetSprite(string path)
    {
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp != null && imp.textureType != TextureImporterType.Sprite)
        {
            imp.textureType        = TextureImporterType.Sprite;
            imp.spritePixelsPerUnit = 100;
            imp.filterMode         = FilterMode.Point;
            imp.SaveAndReimport();
        }
    }

    static void SetSprite(SerializedObject so, string field, Sprite sprite)
    {
        var prop = so.FindProperty(field);
        if (prop != null) prop.objectReferenceValue = sprite;
        else Debug.LogWarning($"Pole '{field}' nie znalezione.");
    }

    static void MoveButton(GameObject parent, string buttonName, Vector2 newPos)
    {
        var t = parent.transform.Find(buttonName);
        if (t == null) { Debug.LogWarning($"Przycisk '{buttonName}' nie znaleziony."); return; }
        (t as RectTransform).anchoredPosition = newPos;
    }

    static void AddButton(GameObject parent, string label, Vector2 pos, PhoneUI target, string methodName)
    {
        var go = new GameObject(label);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta        = new Vector2(220, 36);
        rt.anchoredPosition = pos;
        var img   = go.AddComponent<Image>();
        img.color = new Color(0.22f, 0.22f, 0.32f, 1f);
        var btn   = go.AddComponent<Button>();

        var lblGO = new GameObject(label + "Label");
        lblGO.transform.SetParent(go.transform, false);
        var lblRT = lblGO.AddComponent<RectTransform>();
        lblRT.sizeDelta        = new Vector2(220, 36);
        lblRT.anchoredPosition = Vector2.zero;
        var tmp      = lblGO.AddComponent<TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 16;
        tmp.color     = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;

        var method = typeof(PhoneUI).GetMethod(methodName);
        if (method != null)
        {
            var action = System.Delegate.CreateDelegate(
                typeof(UnityEngine.Events.UnityAction), target, method)
                as UnityEngine.Events.UnityAction;
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick, action);
        }
        else Debug.LogWarning($"Metoda '{methodName}' nie znaleziona na PhoneUI.");
    }
}
