using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GallerySlot : MonoBehaviour
{
    [SerializeField] Image    mouseImage;
    [SerializeField] Image    lockedOverlay;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text hintText;
    [SerializeField] Sprite   lockedSprite;

    public void SetState(bool unlocked, MouseTypeSO data)
    {
        mouseImage.sprite = unlocked ? data.sprite : lockedSprite;
        lockedOverlay.gameObject.SetActive(!unlocked);
        nameText.text     = data.displayName;
        hintText.text     = unlocked ? data.galleryHint : "";
    }
}
