using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class MouseTypeSOGenerator
{
    struct TypeData
    {
        public string name;
        public string displayName;
        public string galleryHint;
        public string cutsceneKey;
    }

    // Order matches CollectibleIndex in MouseStateManager (indices 0–26):
    static readonly TypeData[] Types = new TypeData[]
    {
        // 0 — Heppi
        new() { name = "Heppi",           displayName = "Heppi",             galleryHint = "Przytul lub pocałuj myszkę.",                    cutsceneKey = "Heppi"           },
        // 1 — Czonstkujaca
        new() { name = "Czonstkujaca",    displayName = "Czonstkująca",      galleryHint = "Nakarm głodną myszkę.",                          cutsceneKey = "Czonstkujaca"    },
        // 2 — Grobol
        new() { name = "Grobol",          displayName = "Grob'ol",           galleryHint = "Nakarm najedzoną myszkę (na zapas).",             cutsceneKey = "Grobol"          },
        // 3 — Obrazona
        new() { name = "Obrazona",        displayName = "Obrażona",          galleryHint = "Daj myszce okruszki lub włącz złą muzykę.",       cutsceneKey = "Obrazona"        },
        // 4 — Paczurowa
        new() { name = "Paczurowa",       displayName = "Pączurowa",         galleryHint = "Zamów myszce bilet lotniczy.",                    cutsceneKey = "Paczurowa"       },
        // 5 — Smrodliwa
        new() { name = "Smrodliwa",       displayName = "Smrodliwa",         galleryHint = "Posadź myszkę na rowerze.",                      cutsceneKey = "Smrodliwa"       },
        // 6 — Makapaka
        new() { name = "Makapaka",        displayName = "Makapaka",          galleryHint = "Zbierz 3 skarpetki w ciągu jednego dnia.",        cutsceneKey = "Makapaka"        },
        // 7 — Pirat
        new() { name = "Pirat",           displayName = "Pirat",             galleryHint = "Kliknij łóżko i poczekaj.",                      cutsceneKey = "Pirat"           },
        // 8 — Niewyspana
        new() { name = "Niewyspana",      displayName = "Niewyspana",        galleryHint = "Ustaw wczesny budzik przez telefon.",             cutsceneKey = "Niewyspana"      },
        // 9 — WesolaPoPobudce
        new() { name = "WesolaPoPobudce", displayName = "Wesoła po pobudce", galleryHint = "Ustaw normalny budzik przez telefon.",            cutsceneKey = "WesolaPoPobudce" },
        // 10 — Myszkujaca
        new() { name = "Myszkujaca",      displayName = "Myszkująca",        galleryHint = "Zabierz myszkę do kuchni.",                      cutsceneKey = "Myszkujaca"      },
        // 11 — Tanczaca
        new() { name = "Tanczaca",        displayName = "Tańcząca",          galleryHint = "Włącz muzykę na radiu.",                         cutsceneKey = "Tanczaca"        },
        // 12 — Pachnaca
        new() { name = "Pachnaca",        displayName = "Pachnąca",          galleryHint = "Umyj brudną myszkę w łazience.",                 cutsceneKey = "Pachnaca"        },
        // 13 — Czonstkowa
        new() { name = "Czonstkowa",      displayName = "Czonstkowa",        galleryHint = "Zbierz łącznie 100 okruszków.",                  cutsceneKey = "Czonstkowa"      },
        // 14 — Pumpuzka
        new() { name = "Pumpuzka",        displayName = "Pumpużka",          galleryHint = "Przytul myszkę chcącą uwagi.",                   cutsceneKey = "Pumpuzka"        },
        // 15 — Roztopiona
        new() { name = "Roztopiona",      displayName = "Roztopiona",        galleryHint = "Bumpuj rozochocona myszkę.",                     cutsceneKey = "Roztopiona"      },
        // 16 — Spankowa
        new() { name = "Spankowa",        displayName = "Spankowa",          galleryHint = "Idź spać — mysz zaśnie razem z tobą.",           cutsceneKey = "Spankowa"        },
        // 17 — Czosnkowa
        new() { name = "Czosnkowa",       displayName = "Czosnkowa",         galleryHint = "Przeciągnij czosnek na myszkę.",                 cutsceneKey = "Czosnkowa"       },
        // 18 — Zakochana
        new() { name = "Zakochana",       displayName = "Zakochana",         galleryHint = "Zamów myszce różę przez telefon.",               cutsceneKey = "Zakochana"       },
        // 19 — Rozochocona
        new() { name = "Rozochocona",     displayName = "Rozochocona",       galleryHint = "Włącz radio i postaw świeczki.",                 cutsceneKey = "Rozochocona"     },
        // 20 — Glodna
        new() { name = "Glodna",          displayName = "Głodna",            galleryHint = "Pozwól, żeby myszka się zgłodniała.",            cutsceneKey = "Glodna"          },
        // 21 — Chcaca
        new() { name = "Chcaca",          displayName = "Chcąca",            galleryHint = "Zignoruj myszkę przez chwilę.",                  cutsceneKey = "Chcaca"          },
        // 22 — Smutna
        new() { name = "Smutna",          displayName = "Smutna",            galleryHint = "Zignoruj myszkę przez minutę.",                  cutsceneKey = "Smutna"          },
        // 23 — Zrozpaczona
        new() { name = "Zrozpaczona",     displayName = "Zrozpaczona",       galleryHint = "Pozostaw smutną myszkę bez pomocy.",             cutsceneKey = "Zrozpaczona"     },
        // 24 — Zlowroga
        new() { name = "Zlowroga",        displayName = "Złowroga",          galleryHint = "Kontynuuj pogarszanie nastroju.",                cutsceneKey = "Zlowroga"        },
        // 25 — Sciekla
        new() { name = "Sciekla",         displayName = "Ściekła",           galleryHint = "Doprowadź myszkę do ostateczności.",             cutsceneKey = "Sciekla"         },
        // 26 — ScieklaII
        new() { name = "ScieklaII",       displayName = "Ściekła II",        galleryHint = "Nie ratuj myszki na czas.",                     cutsceneKey = "ScieklaII"       },
    };

    // Auto-generate .asset files whenever scripts recompile
    static MouseTypeSOGenerator() => EditorApplication.delayCall += AutoGenerate;

    static void AutoGenerate()
    {
        EnsureFolder("Assets/Data", "MouseTypes");
        bool any = false;
        foreach (var t in Types)
        {
            string path = $"Assets/Data/MouseTypes/{t.name}.asset";
            if (AssetDatabase.LoadAssetAtPath<MouseTypeSO>(path) != null) continue;
            var so = ScriptableObject.CreateInstance<MouseTypeSO>();
            so.displayName = t.displayName;
            so.galleryHint = t.galleryHint;
            so.cutsceneKey = t.cutsceneKey;
            so.format      = CutsceneFormat.Animation;
            AssetDatabase.CreateAsset(so, path);
            any = true;
        }
        if (any)
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("✓ MouseTypeSOGenerator: nowe assety wygenerowane w Assets/Data/MouseTypes/");
        }
    }

    [MenuItem("CatMouse/Generate MouseType Assets")]
    public static void Generate()
    {
        EnsureFolder("Assets/Data", "MouseTypes");

        foreach (var t in Types)
        {
            string path = $"Assets/Data/MouseTypes/{t.name}.asset";
            var existing = AssetDatabase.LoadAssetAtPath<MouseTypeSO>(path);
            if (existing != null)
            {
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
        Debug.Log($"✓ Wygenerowano/zaktualizowano {Types.Length} MouseTypeSO assetów.");

        WireGallery();
    }

    [MenuItem("CatMouse/Wire Gallery MouseTypes")]
    public static void WireGallery()
    {
        // Try active scene first, then open Gallery scene if needed
        var galleryUI = Object.FindObjectOfType<GalleryUI>();
        bool openedGallery = false;

        if (galleryUI == null)
        {
            string galleryPath = FindScenePath("Gallery");
            if (galleryPath == null)
            {
                Debug.LogWarning("MouseTypeSOGenerator: Nie znaleziono sceny Gallery w Build Settings. Dodaj ją do Build Settings i spróbuj ponownie.");
                return;
            }
            var scene = EditorSceneManager.OpenScene(galleryPath, OpenSceneMode.Additive);
            galleryUI = Object.FindObjectOfType<GalleryUI>();
            openedGallery = true;

            if (galleryUI == null)
            {
                EditorSceneManager.CloseScene(scene, true);
                Debug.LogWarning("MouseTypeSOGenerator: Scena Gallery nie zawiera obiektu z GalleryUI.");
                return;
            }
        }

        var so   = new SerializedObject(galleryUI);
        var prop = so.FindProperty("mouseTypes");
        prop.arraySize = Types.Length;

        for (int i = 0; i < Types.Length; i++)
        {
            string path  = $"Assets/Data/MouseTypes/{Types[i].name}.asset";
            var    asset = AssetDatabase.LoadAssetAtPath<MouseTypeSO>(path);
            if (asset == null)
            {
                Debug.LogWarning($"Brak assetu: {path} — uruchom najpierw CatMouse > Generate MouseType Assets.");
                continue;
            }
            prop.GetArrayElementAtIndex(i).objectReferenceValue = asset;
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(galleryUI);
        EditorSceneManager.SaveScene(galleryUI.gameObject.scene);

        if (openedGallery)
            EditorSceneManager.CloseScene(galleryUI.gameObject.scene, true);

        Debug.Log($"✓ GalleryUI.mouseTypes[] wypełnione {Types.Length} assetami.");
    }

    static string FindScenePath(string sceneName)
    {
        foreach (var scene in EditorBuildSettings.scenes)
        {
            if (System.IO.Path.GetFileNameWithoutExtension(scene.path) == sceneName)
                return scene.path;
        }
        // Fallback: search Assets folder
        var guids = AssetDatabase.FindAssets($"{sceneName} t:Scene");
        foreach (var guid in guids)
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(p) == sceneName)
                return p;
        }
        return null;
    }

    static void EnsureFolder(string parent, string child)
    {
        string full = $"{parent}/{child}";
        if (!AssetDatabase.IsValidFolder(full))
            AssetDatabase.CreateFolder(parent, child);
    }
}
