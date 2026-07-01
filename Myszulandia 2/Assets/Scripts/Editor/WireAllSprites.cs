using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class WireAllSprites
{
    const string PlaceholderDir = "Assets/Art/Placeholders";

    static readonly string[] AllScenes =
    {
        "Assets/_Scenes/Room.unity",
        "Assets/_Scenes/Kitchen.unity",
        "Assets/_Scenes/Bathroom.unity",
    };

    [MenuItem("CatMouse/Wire All Sprites (nowe grafiki)")]
    static void Run()
    {
        // Scena-objekty
        Sprite spSock1      = Load("sock_1");
        Sprite spSock2      = Load("sock_2");
        Sprite spIngJajko   = Load("ingredient");
        Sprite spIngSer     = Load("ingredient_ser");
        Sprite spIngChleb   = Load("ingredient_chleb");
        Sprite spDrink      = Load("drink");
        Sprite spMouseBall  = Load("mouse_ball");
        Sprite spCow        = Load("cow");
        Sprite spCowEars    = Load("cow_ears");
        Sprite spPrysznic   = Load("prysznic");

        // Ikonki ekwipunku
        Sprite icoCrumb     = Load("ico_crumb");
        Sprite icoSock      = Load("ico_sock");
        Sprite icoMealGood  = Load("ico_meal_good");
        Sprite icoMealBad   = Load("ico_meal_bad");
        Sprite icoRose      = Load("ico_rose");
        Sprite icoTicket    = Load("ico_ticket");
        Sprite icoGarlic    = Load("ico_garlic");
        Sprite icoDrink     = Load("ico_drink");
        Sprite icoMouseBall = Load("ico_mouse_ball");
        Sprite icoCow       = Load("ico_cow");
        Sprite icoCowEars   = Load("ico_cow_ears");

        foreach (string scenePath in AllScenes)
        {
            bool wasLoaded = false;
            Scene scene    = default;

            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                Scene s = EditorSceneManager.GetSceneAt(i);
                if (s.path == scenePath && s.isLoaded) { scene = s; wasLoaded = true; break; }
            }
            if (!wasLoaded)
                scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);

            bool changed = false;

            foreach (var root in scene.GetRootGameObjects())
            {
                // Skarpety (scena Room)
                AssignByName(root, "Sock_1", spSock1,   ref changed);
                AssignByName(root, "Sock_2", spSock2,   ref changed);

                // Składniki kuchni
                AssignByName(root, "Ingredient_1", spIngJajko, ref changed);
                AssignByName(root, "Ingredient_2", spIngSer,   ref changed);
                AssignByName(root, "Ingredient_3", spIngChleb, ref changed);

                // DrinkObject - cokolwiek ma ten komponent
                foreach (var comp in root.GetComponentsInChildren<DrinkObject>(true))
                    AssignSR(comp.gameObject, spDrink, ref changed);

                // MouseBallObject
                foreach (var comp in root.GetComponentsInChildren<MouseBallObject>(true))
                    AssignSR(comp.gameObject, spMouseBall, ref changed);

                // CowObject / CowEarsObject (Room)
                foreach (var comp in root.GetComponentsInChildren<CowObject>(true))
                    AssignSR(comp.gameObject, spCow, ref changed);
                foreach (var comp in root.GetComponentsInChildren<CowEarsObject>(true))
                    AssignSR(comp.gameObject, spCowEars, ref changed);

                // Prysznic (szuka obiektu o tej nazwie)
                AssignByName(root, "Prysznic",  spPrysznic, ref changed);
                AssignByName(root, "prysznic",  spPrysznic, ref changed);
                AssignByName(root, "Shower",    spPrysznic, ref changed);

                // InventoryUI - ikonki ekwipunku
                foreach (var ui in root.GetComponentsInChildren<InventoryUI>(true))
                {
                    var so = new SerializedObject(ui);
                    bool dirty = false;
                    SetSprite(so, "crumbSprite",     icoCrumb,     ref dirty);
                    SetSprite(so, "sockSprite",      icoSock,      ref dirty);
                    SetSprite(so, "mealGoodSprite",  icoMealGood,  ref dirty);
                    SetSprite(so, "mealBadSprite",   icoMealBad,   ref dirty);
                    SetSprite(so, "roseSprite",      icoRose,      ref dirty);
                    SetSprite(so, "ticketSprite",    icoTicket,    ref dirty);
                    SetSprite(so, "garlicSprite",    icoGarlic,    ref dirty);
                    SetSprite(so, "drinkSprite",     icoDrink,     ref dirty);
                    SetSprite(so, "mouseBallSprite", icoMouseBall, ref dirty);
                    SetSprite(so, "cowEarsSprite",   icoCowEars,   ref dirty);
                    SetSprite(so, "cowSprite",       icoCow,       ref dirty);
                    if (dirty) { so.ApplyModifiedProperties(); changed = true; }
                }

                // ChcacaBubble - ikonki żądań myszki
                foreach (var bubble in root.GetComponentsInChildren<ChcacaBubble>(true))
                {
                    var so = new SerializedObject(bubble);
                    bool dirty = false;
                    SetSprite(so, "drinkIcon",     icoDrink,     ref dirty);
                    SetSprite(so, "mouseBallIcon", icoMouseBall, ref dirty);
                    if (dirty) { so.ApplyModifiedProperties(); changed = true; }
                }
            }

            if (changed) EditorSceneManager.SaveScene(scene);
            if (!wasLoaded) EditorSceneManager.CloseScene(scene, false);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("✓ WireAllSprites: gotowe! Wszystkie sprite'y przypisane.");
    }

    static Sprite Load(string name)
    {
        var sp = AssetDatabase.LoadAssetAtPath<Sprite>($"{PlaceholderDir}/{name}.png");
        if (sp == null) Debug.LogWarning($"WireAllSprites: nie znaleziono: {name}.png");
        return sp;
    }

    static void AssignByName(GameObject root, string goName, Sprite sp, ref bool changed)
    {
        if (sp == null) return;
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
        {
            if (t.name != goName) continue;
            AssignSR(t.gameObject, sp, ref changed);
        }
    }

    static void AssignSR(GameObject go, Sprite sp, ref bool changed)
    {
        if (sp == null) return;
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == sp) return;
        sr.sprite = sp;
        EditorUtility.SetDirty(sr);
        changed = true;
    }

    static void SetSprite(SerializedObject so, string field, Sprite sp, ref bool changed)
    {
        if (sp == null) return;
        var prop = so.FindProperty(field);
        if (prop == null || prop.objectReferenceValue == sp) return;
        prop.objectReferenceValue = sp;
        changed = true;
    }
}
