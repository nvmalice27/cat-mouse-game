using UnityEngine;
using System.IO;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    void Awake()
    {
        if (Instance != null) { Destroy(this); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Save()
    {
        var data = new SaveData();
        MouseStateManager.Instance.WriteSaveData(data);
        EconomyManager.Instance.WriteSaveData(data);
        InventoryManager.Instance.WriteSaveData(data);
        DayNightManager.Instance.WriteSaveData(data);
        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
    }

    public SaveData Load()
    {
        if (!File.Exists(SavePath)) return new SaveData();
        return JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
    }

    public bool HasSave()    => File.Exists(SavePath);
    public void DeleteSave() { if (File.Exists(SavePath)) File.Delete(SavePath); }
}
