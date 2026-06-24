using UnityEngine;

public enum CutsceneFormat { Animation, Cutscene }

[CreateAssetMenu(fileName = "MouseType_", menuName = "CatMouse/MouseType")]
public class MouseTypeSO : ScriptableObject
{
    public string        displayName;
    public Sprite        sprite;
    public AnimationClip animationClip;
    public CutsceneFormat format;
    public string        cutsceneKey;
    [TextArea]
    public string        galleryHint;
}
