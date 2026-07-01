using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] GameObject panel;

    void OnEnable()  => GameEvents.OnGameOver += Show;
    void OnDisable() => GameEvents.OnGameOver -= Show;

    void Show() => panel.SetActive(true);

    public void OnMainMenu()
    {
        SaveManager.Instance.DeleteSave();
        GameManager.Instance.NavigateTo("MainMenu");
    }
}
