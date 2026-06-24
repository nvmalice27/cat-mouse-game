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

    // Order matches CollectibleIndex (see MouseStateManager.CollectibleIndex):
    //   0–13: kolekcjonerskie (MouseState 10–24, bez Zla)
    //   14–16: złe stany (Smutna/Zlowroga/Sciekla)
    //   17–20: stany rozgrywkowe (Hungry/Rozochocona/Chcaca/Zrozpaczona)
    static readonly TypeData[] Types = new TypeData[]
    {
        // 0 — Kochana
        new() { name = "Kochana",         displayName = "Kochana",          galleryHint = "Zamów myszce kwiaty przez telefon.",              cutsceneKey = "Kochana"         },
        // 1 — Szczesliwa
        new() { name = "Szczesliwa",      displayName = "Szczęśliwa",       galleryHint = "Nakarm głodną myszkę dobrym jedzeniem.",          cutsceneKey = "Szczesliwa"      },
        // 2 — Grobol
        new() { name = "Grobol",          displayName = "Grob'ol",          galleryHint = "Nakarm najedzoną myszkę (na zapas).",             cutsceneKey = "Grobol"          },
        // 3 — Obrazona
        new() { name = "Obrazona",        displayName = "Obrażona",         galleryHint = "Daj myszce okruszki lub włącz złą muzykę.",       cutsceneKey = "Obrazona"        },
        // 4 — Wakacyjna
        new() { name = "Wakacyjna",       displayName = "Wakacyjna",        galleryHint = "Zamów wakacje przez telefon.",                    cutsceneKey = "Wakacyjna"       },
        // 5 — Brudna (Zla usunięta)
        new() { name = "Brudna",          displayName = "Brudna",           galleryHint = "Posadź myszkę na rowerze.",                      cutsceneKey = "Brudna"          },
        // 6 — Makapaka
        new() { name = "Makapaka",        displayName = "Makapaka",         galleryHint = "Zbierz 3 skarpetki w ciągu jednego dnia.",        cutsceneKey = "Makapaka"        },
        // 7 — Pirat
        new() { name = "Pirat",           displayName = "Pirat",            galleryHint = "Odkryj sekretną interakcję...",                  cutsceneKey = "Pirat"           },
        // 8 — Niewyspana
        new() { name = "Niewyspana",      displayName = "Niewyspana",       galleryHint = "Ustaw wczesny budzik przez telefon.",             cutsceneKey = "Niewyspana"      },
        // 9 — WesolaPoPobudce
        new() { name = "WesolaPoPobudce", displayName = "Wesoła po pobudce",galleryHint = "Ustaw normalny budzik przez telefon.",            cutsceneKey = "WesolaPoPobudce" },
        // 10 — Myszkujaca
        new() { name = "Myszkujaca",      displayName = "Myszkująca",       galleryHint = "Zabierz myszkę do kuchni po raz pierwszy.",       cutsceneKey = "Myszkujaca"      },
        // 11 — Tanczaca
        new() { name = "Tanczaca",        displayName = "Tańcząca",         galleryHint = "Włącz muzykę na radiu.",                         cutsceneKey = "Tanczaca"        },
        // 12 — Pachnaca
        new() { name = "Pachnaca",        displayName = "Pachnąca",         galleryHint = "Umyj brudną myszkę w łazience.",                 cutsceneKey = "Pachnaca"        },
        // 13 — Czonstkowa
        new() { name = "Czonstkowa",      displayName = "Czonstkowa",       galleryHint = "Zbierz łącznie 100 okruszków.",                  cutsceneKey = "Czonstkowa"      },
        // 14 — Smutna (zły stan)
        new() { name = "Smutna",          displayName = "Smutna",           galleryHint = "Daj okruszki obrażonej myszce.",                 cutsceneKey = "Smutna"          },
        // 15 — Zlowroga (zły stan)
        new() { name = "Zlowroga",        displayName = "Złowroga",         galleryHint = "Kontynuuj pogarszanie nastroju smutnej myszki.", cutsceneKey = "Zlowroga"        },
        // 16 — Sciekla (zły stan)
        new() { name = "Sciekla",         displayName = "Ściekła",          galleryHint = "Doprowadź myszkę do ostateczności.",             cutsceneKey = "Sciekla"         },
        // 17 — Hungry (stan rozgrywkowy)
        new() { name = "Glodna",          displayName = "Głodna",           galleryHint = "Pozwól, żeby myszka się porządnie zgłodniała.",  cutsceneKey = "Glodna"          },
        // 18 — Rozochocona (stan losowy)
        new() { name = "Rozochocona",     displayName = "Rozochocona",      galleryHint = "Poczekaj — mysz sama wpada w ten nastrój.",      cutsceneKey = "Rozochocona"     },
        // 19 — Chcaca (stan losowy)
        new() { name = "Chcaca",          displayName = "Chcąca",           galleryHint = "Poczekaj — mysz sama wpada w ten nastrój.",      cutsceneKey = "Chcaca"          },
        // 20 — Zrozpaczona (stan losowy)
        new() { name = "Zrozpaczona",     displayName = "Zrozpaczona",      galleryHint = "Poczekaj — mysz sama wpada w ten nastrój.",      cutsceneKey = "Zrozpaczona"     },
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
