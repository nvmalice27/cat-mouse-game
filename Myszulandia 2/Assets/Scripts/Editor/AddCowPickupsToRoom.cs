using UnityEngine;
using UnityEditor;

public static class AddCowPickupsToRoom
{
    [MenuItem("CatMouse/Dodaj Krowie Pickupy do Room")]
    public static void Add()
    {
        var cowEarsSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Placeholders/cow_ears.png");
        var cowSprite     = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Placeholders/cow.png");

        if (cowEarsSprite == null || cowSprite == null)
        {
            Debug.LogError("Nie znaleziono sprite'ów cow_ears.png / cow.png w Assets/Art/Placeholders/. Upewnij się że pliki istnieją.");
            return;
        }

        bool changed = false;

        if (GameObject.Find("CowEarsPickup") == null)
        {
            CreatePickup("CowEarsPickup", cowEarsSprite, new Vector3(-1f, 1f, 0f), typeof(CowEarsObject));
            Debug.Log("✓ Dodano CowEarsPickup.");
            changed = true;
        }
        else
        {
            Debug.Log("CowEarsPickup już istnieje — pominięto.");
        }

        if (GameObject.Find("CowPickup") == null)
        {
            CreatePickup("CowPickup", cowSprite, new Vector3(1f, 1f, 0f), typeof(CowObject));
            Debug.Log("✓ Dodano CowPickup.");
            changed = true;
        }
        else
        {
            Debug.Log("CowPickup już istnieje — pominięto.");
        }

        if (changed)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            Debug.Log("✓ Scena zapisana.");
        }
    }

    static void CreatePickup(string name, Sprite sprite, Vector3 position, System.Type script)
    {
        var go = new GameObject(name);
        go.transform.position = position;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = sprite;
        sr.sortingOrder = 2;

        var col = go.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.32f, 0.32f);

        go.AddComponent(script);
    }
}
