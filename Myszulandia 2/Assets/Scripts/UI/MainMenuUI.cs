using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] GameObject continueButton;

    void Start() => continueButton.SetActive(SaveManager.Instance.HasSave());

    public void OnNewGame()  => GameManager.Instance.StartNewGame();
    public void OnContinue() => GameManager.Instance.ContinueGame();
    public void OnGallery()  => GameManager.Instance.NavigateTo("Gallery");
    public void OnQuit()     => Application.Quit();
}
