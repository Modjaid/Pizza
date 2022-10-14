using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinChanger : MonoBehaviour
{
    public List<GameObject> skins;

    public bool selectSkinRandomly = true;
    public int selectedSkinIndex;

    public void Awake()
    {
        if(selectSkinRandomly)
        {
            selectedSkinIndex = (int)Random.Range(0, skins.Count);

            SelectSkin(selectedSkinIndex);
        }
    }

    private void SelectSkin(int index)
    {
        DeactivateAllSkins();

        skins[index].SetActive(true);
    }

    private void DeactivateAllSkins()
    {
        foreach(GameObject skin in skins)
        {
            skin.SetActive(false);
        }
    }
}
