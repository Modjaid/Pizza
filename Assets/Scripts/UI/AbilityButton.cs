using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AbilityButton : MonoBehaviour
{
    public Image main;
    public Image bar;

    public Color normalColor;
    public Color cooldownColor;
    
    private Color usualColor;

    private void Awake()
    {
        usualColor = main.color;
    }

    public void UpdateBarValue(bool cooldown, float normalizedValue)
    {
        bar.fillAmount = normalizedValue;

        if(cooldown)
        {
            main.color = cooldownColor;
        }
        else
        {
            main.color = usualColor;
        }
    }
}