using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class WireMouseTypeSprites
{
    static readonly string[] Names = {
        "Heppi", "Czonstkujaca", "Grobol", "Obrazona", "Paczurowa",
        "Smrodliwa", "Makapaka", "Pirat", "Niewyspana", "WesolaPoPobudce",
        "Myszkujaca", "Tanczaca", "Pachnaca", "Czonstkowa", "Pumpuzka",
        "Roztopiona", "Spankowa", "Czosnkowa", "Zakochana", "Rozochocona",
        "Glodna", "Chcaca", "Smutna", "Zrozpaczona", "Zlowroga",
        "Sciekla", "ScieklaII", "Krowka"
    };

    // Mapowanie nazwy assetu → możliwe nazwy pliku (gdy plik ma polskie znaki)
    static readonly Dictionary<string, string[]> Aliases = new()
    {
        { "Krowka", new[] { "Krowka", "krówka", "Krówka" } }
    };

    [MenuItem("CatMouse/Wire MouseType Sprites")]
    public static void Wire()
    {
        int wired = 0, missing = 0;

        foreach (var name in Names)
        {
            string assetPath = $"Assets/Data/MouseTypes/{name}.asset";
            var so = AssetDatabase.LoadAssetAtPath<MouseTypeSO>(assetPath);
            if (so == null) { Debug.LogWarning($"Brak assetu: {assetPath}"); missing++; continue; }

            Aliases.TryGetValue(name, out var aliases);
            var candidates = new HashSet<string>(aliases ?? new[] { name });

            var guids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Art/MouseTypes" });
            Sprite found = null;
            foreach (var guid in guids)
            {
                string p    = AssetDatabase.GUIDToAssetPath(guid);
                string stem = System.IO.Path.GetFileNameWithoutExtension(p);
                if (candidates.Contains(stem))
                {
                    found = AssetDatabase.LoadAssetAtPath<Sprite>(p);
                    break;
                }
            }

            if (found == null) { Debug.LogWarning($"Brak sprite'a dla: {name}"); missing++; continue; }

            so.sprite = found;
            EditorUtility.SetDirty(so);
            wired++;
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"✓ Podpięto {wired} sprite'ów. Brakujące: {missing}.");
    }
}
