using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public static class BathroomSceneSetup
{
    [MenuItem("CatMouse/Setup Bathroom Scene")]
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
        var mouse   = new GameObject("Mouse");
        mouse.transform.position = new Vector3(0f, 0f, 0f);
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

        // ── BATHROOM OBJECTS ──────────────────────────────────────────────────

        // Dirty spots (5 scattered stains to scrub away)
        var spotPositions = new Vector3[]
        {
            new(-1.5f,  0.8f, 0f),
            new( 0.8f,  1.2f, 0f),
            new(-0.5f, -0.5f, 0f),
            new( 1.5f, -0.3f, 0f),
            new( 0.2f,  0.3f, 0f),
        };
        var dirtySpotGOs = new GameObject[spotPositions.Length];
        for (int i = 0; i < spotPositions.Length; i++)
        {
            var spot = new GameObject($"DirtySpot_{i + 1}");
            spot.transform.position = spotPositions[i];
            var sr = spot.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 2;
            sr.color = new Color(0.4f, 0.25f, 0.05f, 0.8f);
            dirtySpotGOs[i] = spot;
        }

        // Sponge cursor (follows mouse during scrubbing)
        var sponge = new GameObject("SpongeCursor");
        sponge.transform.position = Vector3.zero;
        var spongeSR = sponge.AddComponent<SpriteRenderer>();
        spongeSR.sortingOrder = 10;
        spongeSR.color = new Color(1f, 0.9f, 0.3f, 0.85f);

        // Door back to Room
        var door    = MakeInteractable("Door", new Vector3(-3.5f, 0f, 0f));
        var doorObj = door.AddComponent<DoorObject>();
        SetBoolField(doorObj, "isInRoom", false);

        // ── BATHROOM SCENE MANAGER ────────────────────────────────────────────
        var bathroomMgrGO = new GameObject("BathroomManager");
        var bathroomScene = bathroomMgrGO.AddComponent<BathroomScene>();

        // Wire dirtySpots list
        var bathroomSO   = new SerializedObject(bathroomScene);
        var spotsProp    = bathroomSO.FindProperty("dirtySpots");
        spotsProp.arraySize = dirtySpotGOs.Length;
        for (int i = 0; i < dirtySpotGOs.Length; i++)
            spotsProp.GetArrayElementAtIndex(i).objectReferenceValue = dirtySpotGOs[i];
        bathroomSO.ApplyModifiedProperties();

        SetField(bathroomScene, "spongeCursor", sponge);

        // ── UI CANVAS ────────────────────────────────────────────────────────
        var canvasGO = new GameObject("UICanvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // HUD
        var hudGO  = MakePanel("HUD", canvasGO.transform, Vector2.up, Vector2.up, Vector2.zero, new Vector2(250, 60));
        var hudCtrl = hudGO.AddComponent<HUDController>();
        var coinsT  = MakeTMP("CoinsText", hudGO.transform, "Monety: 10", new Vector2(0, -15));
        var dayT    = MakeTMP("DayText",   hudGO.transform, "Dzień 1",    new Vector2(0, -40));
        SetField(hudCtrl, "coinsText", coinsT);
        SetField(hudCtrl, "dayText",   dayT);

        // Inventory Panel
        var invGO = MakePanel("InventoryPanel", canvasGO.transform,
            Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(400, 100));
        AddImage(invGO, new Color(0.08f, 0.08f, 0.08f, 0.75f));
        invGO.AddComponent<InventoryUI>();

        // Scrub hint label (visible during washing)
        var hintGO = MakePanel("ScrubHint", canvasGO.transform,
            new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(480, 44));
        AddImage(hintGO, new Color(0f, 0f, 0f, 0.55f));
        MakeTMP("HintText", hintGO.transform, "Trzymaj LPM i szoruj brud!", Vector2.zero, 20);

        // Action Menu Panel (mouse interactions still possible here)
        var actionPanelGO = MakePanel("ActionMenuPanel", canvasGO.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(280, 80));
        AddImage(actionPanelGO, new Color(0.1f, 0.1f, 0.1f, 0.88f));
        var actionMenu = actionPanelGO.AddComponent<MouseActionMenu>();
        SetField(actionMenu, "panel", actionPanelGO);
        string[] btnLabels  = { "Przytul", "Pocałuj", "Pogłaszcz", "Pobaw się" };
        string[] btnMethods = { "OnHug",   "OnKiss",  "OnPet",     "OnPlay"    };
        for (int i = 0; i < 4; i++)
        {
            var bGO = new GameObject(btnLabels[i]);
            bGO.transform.SetParent(actionPanelGO.transform, false);
            var bRT  = bGO.AddComponent<RectTransform>();
            bRT.sizeDelta        = new Vector2(60, 60);
            bRT.anchoredPosition = new Vector2(-90 + i * 62, 0);
            AddImage(bGO, new Color(0.25f, 0.25f, 0.35f, 1f));
            bGO.AddComponent<Button>();
            MakeTMP(btnLabels[i] + "Label", bGO.transform, btnLabels[i], Vector2.zero, 10);
            int captured = i;
            var m = typeof(MouseActionMenu).GetMethod(btnMethods[captured]);
            if (m != null)
            {
                var btn    = bGO.GetComponent<Button>();
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

        Debug.Log("✓ Bathroom scene built! Otwórz scenę Bathroom i kliknij CatMouse > Setup Bathroom Scene.");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    static void SetField(Object target, string fieldName, Object value)
    {
        var so = new SerializedObject(target);
        var sp = so.FindProperty(fieldName);
        if (sp != null) { sp.objectReferenceValue = value; so.ApplyModifiedProperties(); }
        else Debug.LogWarning($"Field '{fieldName}' not found on {target.GetType().Name}");
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
        go.AddComponent<SpriteRenderer>().sortingOrder = 2;
        go.AddComponent<BoxCollider2D>().size = new Vector2(1f, 1f);
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

    static TextMeshProUGUI MakeTMP(string name, Transform parent, string text, Vector2 pos, float fontSize = 18)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta        = new Vector2(320, 35);
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
