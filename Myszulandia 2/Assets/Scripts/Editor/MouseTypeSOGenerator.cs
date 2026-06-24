using UnityEngine;
using UnityEditor;

public static class MouseTypeSOGenerator
{
    struct TypeData
    {
        public string name;
        public string displayName;
        public string galleryHint;
        public string cutsceneKey;
    }

    // Order matches CollectibleIndex: MouseState value - 10
    static readonly TypeData[] Types = new TypeData[]
    {
        // 0 — Kochana (10)
        new() { name = "Kochana",         displayName = "Kochana",          galleryHint = "Zamów myszcze kwiaty przez telefon.",             cutsceneKey = "Kochana"         },
        // 1 — Szczesliwa (11)
        new() { name = "Szczesliwa",      displayName = "Szczęśliwa",       galleryHint = "Nakarm głodną myszkę dobrym jedzeniem.",          cutsceneKey = "Szczesliwa"      },
        // 2 — Grobol (12)
        new() { name = "Grobol",          displayName = "Grob'ol",          galleryHint = "Nakarm najedzoną myszkę (na zapas).",             cutsceneKey = "Grobol"          },
        // 3 — Obrazona (13)
        new() { name = "Obrazona",        displayName = "Obrażona",         galleryHint = "Rzuć skarpetką w myszkę.",                       cutsceneKey = "Obrazona"        },
        // 4 — Wakacyjna (14)
        new() { name = "Wakacyjna",       displayName = "Wakacyjna",        galleryHint = "Zamów wakacje przez telefon.",                    cutsceneKey = "Wakacyjna"       },
        // 5 — Zla (15)
        new() { name = "Zla",             displayName = "Zła",              galleryHint = "Rzuć okruszkiem w myszkę.",                      cutsceneKey = "Zla"             },
        // 6 — Brudna (16)
        new() { name = "Brudna",          displayName = "Brudna",           galleryHint = "Posadź myszkę na rowerze.",                      cutsceneKey = "Brudna"          },
        // 7 — Makapaka (17)
        new() { name = "Makapaka",        displayName = "Makapaka",         galleryHint = "Zbierz 3 skarpetki w ciągu jednego dnia.",        cutsceneKey = "Makapaka"        },
        // 8 — Pirat (18)
        new() { name = "Pirat",           displayName = "Pirat",            galleryHint = "Odkryj sekretną interakcję...",                  cutsceneKey = "Pirat"           },
        // 9 — Niewyspana (19)
        new() { name = "Niewyspana",      displayName = "Niewyspana",       galleryHint = "Ustaw wczesny budzik przez telefon.",             cutsceneKey = "Niewyspana"      },
        // 10 — WesolaPoPobudce (20)
        new() { name = "WesolaPoPobudce", displayName = "Wesoła po pobudce",galleryHint = "Ustaw normalny budzik przez telefon.",            cutsceneKey = "WesolaPoPobudce" },
        // 11 — Myszkujaca (21)
        new() { name = "Myszkujaca",      displayName = "Myszkująca",       galleryHint = "Zabierz myszkę do kuchni po raz pierwszy.",       cutsceneKey = "Myszkujaca"      },
        // 12 — Tanczaca (22)
        new() { name = "Tanczaca",        displayName = "Tańcząca",         galleryHint = "Włącz muzykę na radiu.",                         cutsceneKey = "Tanczaca"        },
        // 13 — Pachnaca (23)
        new() { name = "Pachnaca",        displayName = "Pachnąca",         galleryHint = "Umyj brudną myszkę w łazience.",                 cutsceneKey = "Pachnaca"        },
        // 14 — Czonstkowa (24)
        new() { name = "Czonstkowa",      displayName = "Czonstkowa",       galleryHint = "Zbierz łącznie 100 okruszków.",                  cutsceneKey = "Czonstkowa"      },
    };

    [MenuItem("CatMouse/Generate MouseType Assets")]
    public static void Generate()
    {
        EnsureFolder("Assets/Data", "MouseTypes");

        for (int i = 0; i < Types.Length; i++)
        {
            var t    = Types[i];
            string path = $"Assets/Data/MouseTypes/{t.name}.asset";

            var existing = AssetDatabase.LoadAssetAtPath<MouseTypeSO>(path);
            if (existing != null)
            {
                // Update text fields but don't overwrite sprite/clip if already assigned
                existing.displayName = t.displayName;
                existing.galleryHint = t.galleryHint;
                existing.cutsceneKey = t.cutsceneKey;
                EditorUtility.SetDirty(existing);
                continue;
            }

            var so = ScriptableObject.CreateInstance<MouseTypeSO>();
            so.displayName = t.displayName;
            so.galleryHint = t.galleryHint;
            so.cutsceneKey = t.cutsceneKey;
            so.format      = CutsceneFormat.Animation;
            AssetDatabase.CreateAsset(so, path);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✓ Wygenerowano {Types.Length} MouseTypeSO assetów w Assets/Data/MouseTypes/");
        Debug.Log("Teraz: otwórz scenę Gallery i kliknij CatMouse > Wire Gallery MouseTypes.");
    }

    [MenuItem("CatMouse/Wire Gallery MouseTypes")]
    public static void WireGallery()
    {
        var galleryUI = Object.FindObjectOfType<GalleryUI>();
        if (galleryUI == null)
        {
            Debug.LogWarning("Nie znaleziono GalleryUI na scenie. Otwórz scenę Gallery i spróbuj ponownie.");
            return;
        }

        var so = new SerializedObject(galleryUI);
        var prop = so.FindProperty("mouseTypes");
        prop.arraySize = Types.Length;

        for (int i = 0; i < Types.Length; i++)
        {
            string path = $"Assets/Data/MouseTypes/{Types[i].name}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<MouseTypeSO>(path);
            if (asset == null)
            {
                Debug.LogWarning($"Brak assetu: {path} — uruchom najpierw Generate MouseType Assets.");
                continue;
            }
            prop.GetArrayElementAtIndex(i).objectReferenceValue = asset;
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(galleryUI);

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();

        Debug.Log("✓ GalleryUI.mouseTypes[] wypełnione!");
    }

    static void EnsureFolder(string parent, string child)
    {
        string full = $"{parent}/{child}";
        if (!AssetDatabase.IsValidFolder(full))
            AssetDatabase.CreateFolder(parent, child);
    }
}
