using UnityEngine;

public class PanelEscapeClose : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Close();
    }

    public void Close() => gameObject.SetActive(false);
}
