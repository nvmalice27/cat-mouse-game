using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public static class RoomSceneSetup
{
    [MenuItem("CatMouse/Setup Room Scene")]
    public static void Setup()
    {
        // EventSystem
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
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
        cat.transform.position = new Vector3(-2f, -1f, 0f);
        var catSR = cat.AddComponent<SpriteRenderer>();
        catSR.sortingOrder = 5;
        var catRB = cat.AddComponent<Rigidbody2D>();
        catRB.gravityScale = 0f;
        catRB.constraints  = RigidbodyConstraints2D.FreezeRotation;
        var catCtrl = cat.AddComponent<CatController>();
        SetField(catCtrl, "spriteRenderer", catSR);

        // ── MOUSE ───────────────────────────────────────────────────────────
        var mouse   = new GameObject("Mouse");
        mouse.transform.position = Vector3.zero;
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

        // ── INTERACTIVE OBJECTS ─────────────────────────────────────────────
        var bed    = MakeInteractable("Bed",    new Vector3( 2.0f,  1.0f, 0));
        var bedObj = bed.AddComponent<BedObject>();
        var bedSR  = bed.GetComponent<SpriteRenderer>();
        SetField(bedObj, "sr", bedSR);

        var pillow    = MakeInteractable("Pillow", new Vector3(2.5f, 0.8f, 0));
        pillow.AddComponent<PillowObject>();

        var bike = MakeInteractable("Bike", new Vector3(-3f, 1f, 0));
        bike.AddComponent<BikeObject>();

        var radio    = MakeInteractable("Radio", new Vector3(-3f, 0f, 0));
        var radioAS  = radio.AddComponent<AudioSource>();
        radioAS.playOnAwake = false;
        var radioObj = radio.AddComponent<RadioObject>();
        SetField(radioObj, "audioSource", radioAS);

        var phone    = MakeInteractable("Phone", new Vector3(-1f, -2f, 0));
        phone.AddComponent<PhoneObject>();

        var door    = MakeInteractable("Door", new Vector3(3.5f, 0f, 0));
        door.AddComponent<DoorObject>();

        for (int i = 0; i < 3; i++)
        {
            var c = MakeInteractable($"Crumb_{i + 1}", new Vector3(-1f + i * 0.8f, -1.5f, 0));
            c.AddComponent<CrumbObject>();
        }
        for (int i = 0; i < 3; i++)
        {
            var s = MakeInteractable($"Sock_{i + 1}", new Vector3(0.5f + i * 1f, 1.5f, 0));
            s.AddComponent<SockObject>();
        }

        // ── UI CANVAS ───────────────────────────────────────────────────────
        var canvasGO  = new GameObject("UICanvas");
        var canvas    = canvasGO.AddComponent<Canvas>();
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
        var invGO  = MakePanel("InventoryPanel", canvasGO.transform, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(400, 100));
        AddImage(invGO, new Color(0.08f, 0.08f, 0.08f, 0.75f));
        invGO.AddComponent<InventoryUI>();

        // Action Menu Panel (hidden, appears above mouse)
        var actionPanelGO = MakePanel("ActionMenuPanel", canvasGO.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(280, 80));
        AddImage(actionPanelGO, new Color(0.1f, 0.1f, 0.1f, 0.88f));
        var actionMenu = actionPanelGO.AddComponent<MouseActionMenu>();
        SetField(actionMenu, "panel", actionPanelGO);
        // 4 buttons: Przytul, Pocałuj, Pogłaszcz, Pobaw się
        string[] btnLabels  = { "Przytul", "Pocałuj", "Pogłaskcz", "Pobaw się" };
        string[] btnMethods = { "OnHug",   "OnKiss",  "OnPet",     "OnPlay"    };
        for (int i = 0; i < 4; i++)
        {
            var bGO = new GameObject(btnLabels[i]);
            bGO.transform.SetParent(actionPanelGO.transform, false);
            var bRT  = bGO.AddComponent<RectTransform>();
            bRT.sizeDelta        = new Vector2(60, 60);
            bRT.anchoredPosition = new Vector2(-90 + i * 62, 0);
            AddImage(bGO, new Color(0.25f, 0.25f, 0.35f, 1f));
            var btn     = bGO.AddComponent<Button>();
            var lblGO   = MakeTMP(btnLabels[i] + "Label", bGO.transform, btnLabels[i], Vector2.zero, 10);
            int captured = i;
            // OnClick wired via persistent call
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(
                btn.onClick,
                System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), actionMenu,
                    typeof(MouseActionMenu).GetMethod(btnMethods[captured])) as UnityEngine.Events.UnityAction);
        }
        actionPanelGO.SetActive(false);
        SetField(mouseCtrl, "actionMenu", actionMenu);

        // Confirm Panel (sleep)
        var confirmGO = MakePanel("ConfirmPanel", canvasGO.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(260, 130));
        AddImage(confirmGO, new Color(0.1f, 0.1f, 0.12f, 0.95f));
        MakeTMP("Question", confirmGO.transform, "Iść spać?", new Vector2(0, 30));
        var pillowComp = pillow.GetComponent<PillowObject>();
        MakeButton("Tak",  confirmGO.transform, new Vector2(-55, -25), pillowComp, "ConfirmSleep");
        MakeButton("Nie",  confirmGO.transform, new Vector2( 55, -25), pillowComp, "CancelSleep");
        confirmGO.SetActive(false);
        SetField(pillowComp, "confirmPanel", confirmGO);

        // Door Menu Panel
        var doorMenuGO = MakePanel("DoorMenuPanel", canvasGO.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(220, 140));
        AddImage(doorMenuGO, new Color(0.1f, 0.1f, 0.12f, 0.95f));
        MakeTMP("Title", doorMenuGO.transform, "Dokąd idziesz?", new Vector2(0, 45));
        var doorComp = door.GetComponent<DoorObject>();
        MakeButton("Kuchnia",  doorMenuGO.transform, new Vector2(0, 5),   doorComp, "GoToKitchen");
        MakeButton("Łazienka", doorMenuGO.transform, new Vector2(0, -40), doorComp, "GoToBathroom");
        doorMenuGO.SetActive(false);
        SetField(doorComp, "roomDoorMenu", doorMenuGO);

        // Phone Panel (hidden)
        var phonePanelGO = MakePanel("PhonePanel", canvasGO.transform,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300, 420));
        AddImage(phonePanelGO, new Color(0.12f, 0.12f, 0.18f, 0.97f));
        var phoneUI    = phonePanelGO.AddComponent<PhoneUI>();
        var phoneTitle = MakeTMP("Title",        phonePanelGO.transform, "Telefon",    new Vector2(0,  185));
        var phoneCoins = MakeTMP("CoinsPreview", phonePanelGO.transform, "Monety: 10", new Vector2(0,  155));

        // Phone main buttons
        var phoneComp = phone.GetComponent<PhoneObject>();
        MakeButton("Zamów jedzenie", phonePanelGO.transform, new Vector2(0,  90), phoneUI, "ShowFoodMenu");
        MakeButton("Zamów kwiaty",   phonePanelGO.transform, new Vector2(0,  40), phoneUI, "OrderRose");
        MakeButton("Wakacje",        phonePanelGO.transform, new Vector2(0, -10), phoneUI, "OrderVacation");
        MakeButton("Budzik",         phonePanelGO.transform, new Vector2(0, -60), phoneUI, "ShowAlarmMenu");
        MakeButton("Zamknij",        phonePanelGO.transform, new Vector2(0,-110), phoneUI, "Close");

        // Food sub-panel
        var foodSubGO = MakePanel("FoodSubPanel", phonePanelGO.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        AddImage(foodSubGO, new Color(0.1f, 0.12f, 0.1f, 0.97f));
        MakeTMP("Title", foodSubGO.transform, "Co zamawiamy?", new Vector2(0, 60));
        MakeButton("Dobre jedzenie (50)",  foodSubGO.transform, new Vector2(0,  10), phoneUI, "OrderGoodFood");
        MakeButton("Złe jedzenie (50)",    foodSubGO.transform, new Vector2(0, -40), phoneUI, "OrderBadFood");
        foodSubGO.SetActive(false);

        // Alarm sub-panel
        var alarmSubGO = MakePanel("AlarmSubPanel", phonePanelGO.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        AddImage(alarmSubGO, new Color(0.12f, 0.1f, 0.1f, 0.97f));
        MakeTMP("Title", alarmSubGO.transform, "Ustaw budzik", new Vector2(0, 60));
        MakeButton("Wcześnie",  alarmSubGO.transform, new Vector2(0,  10), phoneUI, "SetAlarmEarly");
        MakeButton("Normalnie", alarmSubGO.transform, new Vector2(0, -35), phoneUI, "SetAlarmNormal");
        MakeButton("Wyłącz",    alarmSubGO.transform, new Vector2(0, -80), phoneUI, "SetAlarmNone");
        alarmSubGO.SetActive(false);

        SetField(phoneUI, "mainPanel",    phonePanelGO);
        SetField(phoneUI, "foodSubPanel",  foodSubGO);
        SetField(phoneUI, "alarmSubPanel", alarmSubGO);
        SetField(phoneUI, "coinsPreview",  phoneCoins);
        SetField(phoneComp, "phoneUI",     phoneUI);
        phonePanelGO.SetActive(false);

        // Cutscene Panel (hidden, covers whole screen)
        var cutsceneGO = MakePanel("CutscenePanel", canvasGO.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        AddImage(cutsceneGO, Color.black);
        var cutsceneAnim = cutsceneGO.AddComponent<Animator>();
        var cutsceneMgr  = cutsceneGO.AddComponent<CutsceneManager>();
        SetField(cutsceneMgr, "panel",    cutsceneGO);
        SetField(cutsceneMgr, "animator", cutsceneAnim);
        cutsceneGO.SetActive(false);

        // Save
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("✓ Room scene built! Pozostało: przypisz sprite'y do SpriteRenderer i skonfiguruj InventoryUI (slot prefab + ikony).");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    static void SetField(Object target, string fieldName, Object value)
    {
        var so = new SerializedObject(target);
        var sp = so.FindProperty(fieldName);
        if (sp != null) { sp.objectReferenceValue = value; so.ApplyModifiedProperties(); }
        else Debug.LogWarning($"Field '{fieldName}' not found on {target.GetType().Name}");
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

    static void MakeButton(string label, Transform parent, Vector2 pos, Object target, string methodName)
    {
        var go = new GameObject(label);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta        = new Vector2(220, 36);
        rt.anchoredPosition = pos;
        AddImage(go, new Color(0.22f, 0.22f, 0.32f, 1f));
        var btn = go.AddComponent<Button>();
        MakeTMP(label + "Label", go.transform, label, Vector2.zero, 16);

        var method = target.GetType().GetMethod(methodName);
        if (method != null)
        {
            var action = System.Delegate.CreateDelegate(
                typeof(UnityEngine.Events.UnityAction), target, method)
                as UnityEngine.Events.UnityAction;
            UnityEditor.Events.UnityEventTools.AddVoidPersistentListener(btn.onClick, action);
        }
        else Debug.LogWarning($"Method '{methodName}' not found on {target.GetType().Name}");
    }
}
