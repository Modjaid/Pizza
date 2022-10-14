
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugPanel : MonoBehaviour
{
    private bool godmode = false;
    private bool panelShown;
    public PlayerAxisController player;
    public Text godmodeButtonText;
    public GameObject panel;

    public void GodmodeButton()
    {
        godmode = !godmode;

        player.SetGodmodeActive(godmode);
        godmodeButtonText.text = godmode ? "под спидами: жа" : "под спидами: не";
    }

    public void ShowButton()
    {
        panelShown = !panelShown;

        panel.SetActive(panelShown);
    }
}