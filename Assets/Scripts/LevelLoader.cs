using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour 
{
    public static LevelLoader Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void LoadScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);

        if(scene != null)
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        Debug.LogError("Scene not found by name.");
    }

    public void LoadGame()
    {
        PlayerPrefs.SetInt("levelBlockIsLoaded", 0);

        LoadScene("Game");
    }

    public void LoadLevelBlock(int level)
    {
        PlayerPrefs.SetInt("levelBlockIsLoaded", 1);
        PlayerPrefs.SetInt("levelBlockLoaded", level);

        LoadScene("Game");
    }   

    public void ReloadLevel()
    {
        // note:
        // if reload level at 0 timescale, player position just won't be changed at the start of level
        // (random player spawn point selection). And I have no fucking clue why (at the moment when player position
        // is set, the timescale should already be 1.)

        //Time.timeScale = 1f;
        Scene scene = SceneManager.GetActiveScene();
        LoadScene(scene.name);
    }
}