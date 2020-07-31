using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinSwapper : MonoBehaviour
{
    public static SkinSwapper Instance;

    public Aesthetics Hats;
    public Aesthetics Masks;
    public Aesthetics Necks;
    [Space(10)]
    public int ActiveSkin = 0;
    public GameObject LeftEye;
    public GameObject RightEye;
    public bool[] EyesActive;
    public GameObject[] Skins;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        ActiveSkin = Saves.GetSkinSelected();
        SetSkin(ActiveSkin);
    }

    public void SetSkin(int skinNumber)
    {
        ActiveSkin = skinNumber;
        deactivateAllSkins();
        Skins[skinNumber].SetActive(true);
        LeftEye.SetActive(EyesActive[skinNumber]);
        RightEye.SetActive(EyesActive[skinNumber]);
        Saves.SaveSkinSelected(skinNumber);
    }

    void deactivateAllSkins()
    {
        foreach (GameObject skin in Skins)
        {
            if(skin.activeSelf)
                skin.SetActive(false);
        }
    }


}
