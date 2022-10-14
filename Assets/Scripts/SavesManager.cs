using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class SavesManager: MonoBehaviour
{
    public static SavesManager Instance;

    private string saveFolder;

    private void Awake()
    {
        Instance = this;
        saveFolder = Application.dataPath + "/Saves/";

        if(!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(this);

        if(!PlayerPrefs.HasKey("level"))
        {
            PlayerPrefs.SetInt("level", 1);
        }
    }

    public void Save(string value, string fileName)
    {
        File.WriteAllText(saveFolder + fileName + ".txt", value);
    }

    public string Load(string fileName)
    {
        if(File.Exists(saveFolder + fileName + ".txt"))
        {
            return File.ReadAllText(saveFolder + fileName + ".txt");
        }
        else
        {
            return null;
        }
    }

    public bool FileExists(string fileName)
    {
        return File.Exists(saveFolder + fileName + ".txt");
    }
}