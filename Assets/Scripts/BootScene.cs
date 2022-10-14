using UnityEngine;
using UnityEngine.SceneManagement;

public class BootScene : MonoBehaviour 
{
    private void Start()
    {
        #if UNITY_EDITOR
        {
            string name = PlayerPrefs.GetString("FirstLaunchedSceneName");
            
            if(name != "Boot")
            {
                SceneManager.LoadScene(name);
            }
            else
            {
                SceneManager.LoadScene("Menu");
            }

            Utils.ClearEditorLog();

            return;
        }
        #endif

        SceneManager.LoadScene("Menu");
    }    
}