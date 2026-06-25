using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public static class KitchenSceneSetup
{
    [MenuItem("CatMouse/Setup Kitchen Scene")]
    public static void Setup()
    {
        if (Object.FindObjectOfType<KitchenScene>() != null)
        {
            Debug.LogWarning("Kitchen scene już zbudowana! Usuń obiekty sceny i spróbuj ponownie.");
            return;
        }

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
            cam.backgroundColor   = new Color(0.2f, 0.2f, 0.2f);
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

        // ── CAT ─────────────────────────────────────────────────────────────
        var cat   = new GameObject("Cat");
        cat.transform.position = new Vector3(-2f, -1.5f, 0f);
        var catSR = cat.AddComponent<SpriteRenderer>();
        catSR.sortingOrder = 5;
        var catRB = cat.AddComponent<Rigidbody2D>();
        catRB.gravityScale = 0f;
        catRB.constraints  = RigidbodyConstraints2D.FreezeRotation;
        var catCtrl = cat.AddComponent<CatController>();
        SetField(catCtrl, "spriteRenderer", catSR);

        // ── MOUSE ───────────────────────────────────────────────────────────
        var mouse  = new GameObject("Mouse");
        mouse.transform.position = new Vector3(0f, -1f, 0f);
        var mouseSR  = mouse.AddComponent<SpriteRenderer>();
        mouseSR.sortingOrder = 3;
        mouse.AddComponent<Animator>();
        var mouseCol = mouse.AddComponent<CircleCollider2D>();
        mouseCol.radius = 0.5f;

        var dirtyGO = new GameObject("DirtyOverlay");
        dirtyGO.transform.SetParent(mouse.transform, false);
        var dirtySR = dirtyGO.AddComponent<SpriteRenderer>();
        dirtySR.sortingOrder = 4;
        dirtySR.color        = new Color(0.45f, 0.27f, 0.07f, 0.65f);
        dirtyGO.SetActive(false);

        var mouseCtrl = mouse.AddComponent<MouseController>();
        SetField(mouseCtrl, "spriteRenderer", mouseSR);
        SetField(mouseCtrl, "animator",       mouse.GetComponent<Animator>());
        SetField(mouseCtrl, "dirtyOverlay",   dirtySR);

        // ── KITCHEN OBJECTS ──────────────────────────────────────────────────
        // Pot (with Animator for cook animation)
        var pot = new GameObject("Pot");
        pot.transform.position = new Vector3(1f, 0f, 0f);
        var potSR = pot.AddComponent<SpriteRenderer>();
        potSR.sortingOrder = 2;
        var potAnim = pot.AddComponent<Animator>();
        pot.AddComponent<BoxCollider2D>().size = new Vector2(1f, 1f);

        // 3 Ingredient Sources
        var ingPos = new Vector3[] {
            new(-2.5f,  1f, 0f),
            new( 0f,    1.5f, 0f),
            new( 2.5f,  1f, 0f)
        };
        var ingredientGOs = new GameObject[3];
        for (int i = 0; i < 3; i++)
        {
            var ing = MakeInteractable($"Ingredient_{i + 1}", ingPos[i]);
            var src = ing.AddComponent<IngredientSource>();
            SetIntField(src, "sourceIndex", i);
            ingredientGOs[i] = ing;
        }

        // Door back to Room
        var door    = MakeInteractable("Door", new Vector3(-3.5f, 0f, 0f));
        var doorObj = door.AddComponent<DoorObject>();
        SetBoolField(doorObj, "isInRoom", false);

        // ── KITCHEN SCENE MANAGER ────────────────────────────────────────────
        var kitchenMgrGO = new GameObject("KitchenManager");
        var kitchenScene = kitchenMgrGO.AddComponent<KitchenScene>();
        SetIntField(kitchenScene, "ingredientsNeeded", 3);

        // Wire all KitchenScene fields in one SerializedObject pass
        var kitchenSO   = new SerializedObject(kitchenScene);
        var sourcesProp = kitchenSO.FindProperty("ingredientSources");
        sourcesProp.arraySize = 3;
        for (int i = 0; i < 3; i++)
            sourcesProp.GetArrayElementAtIndex(i).objectReferenceValue = ingredientGOs[i];
        kitchenSO.FindProperty("potAnimator").objectReferenceValue = potAnim;
        kitchenSO.ApplyModifiedProperties();
        EditorUtility.SetDirty(kitchenScene);

        // Wire IngredientSource.kitchen references
        for (int i = 0; i < 3; i++)
        {
            var src = ingredientGOs[i].GetComponent<IngredientSource>();
            SetField(src, "kitchen", kitchenScene);
        }

        // ── UI CANVAS ────────────────────────────────────────────────────────
        var canvasGO = new GameObject("UICanvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // HUD — lewy górny róg
        var hudGO  = MakePanelPivot("HUD", canvasGO.transform,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f, -10f), new Vector2(260f, 65f));
        var hudCtrl = hudGO.AddComponent<HUDController>();
        var coinsT  = MakeTMP("CoinsText", hudGO.transform, "Monety: 10", new Vector2(130f, -18f));
        var dayT    = MakeTMP("DayText",   hudGO.transform, "Dzień 1",    new Vector2(130f, -47f));
        SetField(hudCtrl, "coinsText", coinsT);
        SetField(hudCtrl, "dayText",   dayT);

        // Inventory Panel — dół, wyśrodkowany
        var invGO = MakePanelPivot("InventoryPanel", canvasGO.transform,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 10f), new Vector2(560f, 100f));
        AddImage(invGO, new Color(0.08f, 0.08f, 0.08f, 0.75f));
        invGO.AddComponent<InventoryUI>();

        // Cook Button (shows when 3 ingredients collected)
        var cookBtnPanelGO = MakePanel("CookButtonPanel", canvasGO.transform,
            new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 80f), new Vector2(220, 60));
        AddImage(cookBtnPanelGO, new Color(0.15f, 0.35f, 0.15f, 0.95f));
        var cookBtn = cookBtnPanelGO.AddComponent<Button>();
        MakeTMP("CookLabel", cookBtnPanelGO.transform, "Gotuj!", Vector2.zero, 22);
        var cookMethod = typeof(KitchenScene).GetMethod("Cook");
        if (cookMethod != null)
        {
            var action = System.Delegate.CreateDelegate(
                typeof(UnityEngine.Events.UnityAction), kitchenScene, cookMethod)
                as UnityEngine.Events.UnityAction;
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(cookBtn.onClick, action);
        }
        // Assign reference BEFORE deactivating so Unity serializes it correctly
        SetField(kitchenScene, "cookButton", cookBtnPanelGO);
        EditorUtility.SetDirty(kitchenScene);
        cookBtnPanelGO.SetActive(false);

        // Action Menu Panel (for mouse interactions)
        var actionPanelGO = MakePanel("ActionMenuPanel", canvasGO.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(280, 80));
        AddImage(actionPanelGO, new Color(0.1f, 0.1f, 0.1f, 0.88f));
        var actionMenu = actionPanelGO.AddComponent<MouseActionMenu>();
        SetField(actionMenu, "panel", actionPanelGO);
        string[] btnLabels  = { "Przytul", "Pocałuj" };
        string[] btnMethods = { "OnHug",   "OnKiss"  };
        for (int i = 0; i < btnLabels.Length; i++)
        {
            var bGO = new GameObject(btnLabels[i]);
            bGO.transform.SetParent(actionPanelGO.transform, false);
            var bRT  = bGO.AddComponent<RectTransform>();
            bRT.sizeDelta        = new Vector2(60, 60);
            bRT.anchoredPosition = new Vector2(-31 + i * 62, 0);
            AddImage(bGO, new Color(0.25f, 0.25f, 0.35f, 1f));
            var btn   = bGO.AddComponent<Button>();
            MakeTMP(btnLabels[i] + "Label", bGO.transform, btnLabels[i], Vector2.zero, 10);
            int captured = i;
            var m = typeof(MouseActionMenu).GetMethod(btnMethods[captured]);
            if (m != null)
            {
                var action = System.Delegate.CreateDelegate(
                    typeof(UnityEngine.Events.UnityAction), actionMenu, m)
                    as UnityEngine.Events.UnityAction;
                UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick, action);
            }
        }
        actionPanelGO.SetActive(false);
        SetField(mouseCtrl, "actionMenu", actionMenu);

        // Cutscene Panel
        var cutsceneGO = MakePanel("CutscenePanel", canvasGO.transform,
            Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        AddImage(cutsceneGO, Color.black);
        var cutsceneAnim = cutsceneGO.AddComponent<Animator>();
        var cutsceneMgr  = cutsceneGO.AddComponent<CutsceneManager>();
        SetField(cutsceneMgr, "panel",    cutsceneGO);
        SetField(cutsceneMgr, "animator", cutsceneAnim);
        cutsceneGO.SetActive(false);

        // ── SAVE ─────────────────────────────────────────────────────────────
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("✓ Kitchen scene built! Otwórz scenę Kitchen i kliknij CatMouse > Setup Kitchen Scene.");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    static void SetField(Object target, string fieldName, Object value)
    {
        var so = new SerializedObject(target);
        var sp = so.FindProperty(fieldName);
        if (sp != null) { sp.objectReferenceValue = value; so.ApplyModifiedProperties(); }
        else Debug.LogWarning($"Field '{fieldName}' not found on {target.GetType().Name}");
    }

    static void SetIntField(Object target, string fieldName, int value)
    {
        var so = new SerializedObject(target);
        var sp = so.FindProperty(fieldName);
        if (sp != null) { sp.intValue = value; so.ApplyModifiedProperties(); }
        else Debug.LogWarning($"Int field '{fieldName}' not found on {target.GetType().Name}");
    }

    static void SetBoolField(Object target, string fieldName, bool value)
    {
        var so = new SerializedObject(target);
        var sp = so.FindProperty(fieldName);
        if (sp != null) { sp.boolValue = value; so.ApplyModifiedProperties(); }
        else Debug.LogWarning($"Bool field '{fieldName}' not found on {target.GetType().Name}");
    }

    static GameObject MakeInteractable(string name, Vector3 pos)
    {
        var go = new GameObject(name);
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 2;
        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(1f, 1f);
        return go;
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

    static GameObject MakePanelPivot(string name, Transform parent,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt        = go.AddComponent<RectTransform>();
        rt.anchorMin  = anchorMin;
        rt.anchorMax  = anchorMax;
        rt.pivot      = pivot;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta  = size;
        return go;
    }

    static TextMeshProUGUI MakeTMP(string name, Transform parent, string text, Vector2 pos, float fontSize = 18)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta        = new Vector2(260, 35);
        rt.anchoredPosition = pos;
        var tmp      = go.AddComponent<TextMeshProUGUI>();
        tmp.text     = text;
        tmp.fontSize = fontSize;
        tmp.color    = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        return tmp;
    }

    static void AddImage(GameObject go, Color color)
    {
        var img   = go.AddComponent<Image>();
        img.color = color;
    }
}
