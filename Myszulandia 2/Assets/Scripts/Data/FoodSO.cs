using UnityEngine;

[CreateAssetMenu(fileName = "Food_", menuName = "CatMouse/Food")]
public class FoodSO : ScriptableObject
{
    public string displayName;
    public Sprite icon;
    public bool   isGood;
    public int    price;
}
