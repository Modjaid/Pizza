using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLight : MonoBehaviour
{
    [TextArea]
    public string note = "Green - Yellow - Red";

    public float[] stateTime;
    public GameObject[] stateObjects;

    public Renderer[] lamps;
    public Material[] inactiveMaterials;
    public Material[] activeMaterials;

    public int state;

    private void Start()
    {
        StartCoroutine(Work());
    }

    protected virtual void ShowStateChange()
    {
        for(int i = 0; i < lamps.Length; i++)
        {
            lamps[i].material = inactiveMaterials[i];
        }

        lamps[state].material = activeMaterials[state];
    }

    private IEnumerator Work()
    {
        ChangeState(state);

        while (true)
        {
            for (int i = (int)state; i < 3; i++)
            {
                yield return new WaitForSeconds(stateTime[i]);
                if (state < 2)
                {
                    state++;
                    ChangeState(state);
                }
            }

            ChangeState(0);
        }
    }

    private void ChangeState(int newState)
    {
        state = newState;

        foreach (GameObject go in stateObjects)
        {
            if(go != null)
            go.SetActive(false);
        }

        if(stateObjects.Length > newState && stateObjects[newState] != null)
        stateObjects[newState].SetActive(true);

        ShowStateChange();
    }
}