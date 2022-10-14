#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoadAttribute]
public static class DefaultSceneLoader
{
    static DefaultSceneLoader()
    {
        EditorApplication.playModeStateChanged += LoadDefaultScene;
    }

    static void LoadDefaultScene(PlayModeStateChange state)
    {
        PlayerPrefs.SetString("FirstLaunchedSceneName", EditorSceneManager.GetActiveScene().name);
    
        if(EditorSceneManager.GetActiveScene() != EditorSceneManager.GetSceneByName("Boot"))
        {
            if (state == PlayModeStateChange.ExitingEditMode) 
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo ();
            }

            if (state == PlayModeStateChange.EnteredPlayMode) 
            {
                EditorSceneManager.LoadScene ("Boot");
            }
        }
    }
}
#endif