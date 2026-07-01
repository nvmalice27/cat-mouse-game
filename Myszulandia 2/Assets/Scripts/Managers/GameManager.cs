using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureManagers();
    }

    void EnsureManagers()
    {
        if (SaveManager.Instance == null)       gameObject.AddComponent<SaveManager>();
        if (EconomyManager.Instance == null)    gameObject.AddComponent<EconomyManager>();
        if (InventoryManager.Instance == null)  gameObject.AddComponent<InventoryManager>();
        if (MouseStateManager.Instance == null) gameObject.AddComponent<MouseStateManager>();
        if (DayNightManager.Instance == null)   gameObject.AddComponent<DayNightManager>();
    }

    void OnDestroy() { }

    public void StartNewGame()
    {
        SaveManager.Instance.DeleteSave();
        LoadSaveIntoManagers();
        SceneManager.LoadScene("Room");
    }

    public void ContinueGame()
    {
        LoadSaveIntoManagers();
        SceneManager.LoadScene("Room");
    }

    void LoadSaveIntoManagers()
    {
        var data = SaveManager.Instance.Load();
        MouseStateManager.Instance.ApplySaveData(data);
        EconomyManager.Instance.ApplySaveData(data);
        InventoryManager.Instance.ApplySaveData(data);
        DayNightManager.Instance.ApplySaveData(data);
    }

    public void NavigateTo(string sceneName) => SceneManager.LoadScene(sceneName);

    public void NavigateToGallery()  => SceneManager.LoadScene("Gallery");
    public void NavigateToMainMenu() => SceneManager.LoadScene("MainMenu");
}
