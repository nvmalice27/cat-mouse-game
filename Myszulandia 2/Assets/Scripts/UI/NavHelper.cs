using UnityEngine;
using UnityEngine.SceneManagement;

public class NavHelper : MonoBehaviour
{
    public void GoToGallery()  => SceneManager.LoadScene("Gallery");

    public void GoToMainMenu()
    {
        // Zapisz postęp żeby "Kontynuuj" było aktywne w menu
        if (SaveManager.Instance != null) SaveManager.Instance.Save();
        SceneManager.LoadScene("MainMenu");
    }

    public void GoToRoom() => SceneManager.LoadScene("Room");
}
