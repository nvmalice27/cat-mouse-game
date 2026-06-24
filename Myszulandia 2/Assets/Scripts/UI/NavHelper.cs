using UnityEngine;
using UnityEngine.SceneManagement;

public class NavHelper : MonoBehaviour
{
    public void GoToGallery()  => SceneManager.LoadScene("Gallery");
    public void GoToMainMenu() => SceneManager.LoadScene("MainMenu");
    public void GoToRoom()     => SceneManager.LoadScene("Room");
}
