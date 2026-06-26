using UnityEngine;
using UnityEditor;

public static class WireMouseTypeSprites
{
    static readonly string[] Names = {
        "Heppi", "Czonstkujaca", "Grobol", "Obrazona", "Paczurowa",
        "Smrodliwa", "Makapaka", "Pirat", "Niewyspana", "WesolaPoPobudce",
        "Myszkujaca", "Tanczaca", "Pachnaca", "Czonstkowa", "Pumpuzka",
        "Roztopiona", "Spankowa", "Czosnkowa", "Zakochana", "Rozochocona",
        "Glodna", "Chcaca", "Smutna", "Zrozpaczona", "Zlowroga",
        "Sciekla", "ScieklaII"
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

            // Search for a sprite with this exact name anywhere in Assets/
            var guids = AssetDatabase.FindAssets($"{name} t:Sprite");
            Sprite found = null;
            foreach (var guid in guids)
            {
                string p = AssetDatabase.GUIDToAssetPath(guid);
                // Match by filename without extension
                if (System.IO.Path.GetFileNameWithoutExtension(p) == name)
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
