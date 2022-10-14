using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitHandler : MonoBehaviour
{
    public void Exit()
    {
        Application.Quit();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }
}
