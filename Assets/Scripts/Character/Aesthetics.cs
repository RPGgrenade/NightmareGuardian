using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aesthetics : MonoBehaviour
{
    [System.Serializable]
    public class Accessory
    {
        public GameObject Item;
        public string Name;
        public bool Active = false;
        public bool Unlocked = false;
    }
    public enum AccessoryType
    {
        Hat,
        Mask,
        Neck
    }

    public AccessoryType Type = AccessoryType.Hat;
    public int AccessoryNumber = -1;

    public Accessory[] Accessories;
    public Vector3[] SkinOffsets;
    public float[] SkinSizes;

    private GameObject currentItem;
    // Start is called before the first frame update
    void Start()
    {
        setNames();
        Invoke("loadUnlocked",0.1f);
        //loadUnlocked();
    }

    private void loadUnlocked()
    {
        //check the enum for the type and load the accessory unlocks as is needed
        for (int i = 0; i < Accessories.Length; i++)
        {
            Accessory accessory = Accessories[i];
            if (Type == AccessoryType.Hat)
            {
                accessory.Unlocked = Saves.GetHatFound(accessory.Name);
                if (Saves.GetHatSelected(accessory.Name))
                    SetAesthetic(i);
            }
            else if (Type == AccessoryType.Mask)
            {
                accessory.Unlocked = Saves.GetMaskFound(accessory.Name);
                if (Saves.GetMaskSelected(accessory.Name))
                    SetAesthetic(i);
            }
            else if (Type == AccessoryType.Neck)
            {
                accessory.Unlocked = Saves.GetNeckFound(accessory.Name);
                if (Saves.GetNeckSelected(accessory.Name))
                    SetAesthetic(i);
            }
        }

        //and when implementing which one was selected, load the one selected
    }

    private void setNames()
    {
        foreach (Accessory acc in Accessories)
        {
            if (acc.Name == null || acc.Name == "")
                acc.Name = acc.Item.name;
        }
    }

    public void SetAesthetic(string name)
    {
        for (int i = 0; i < Accessories.Length; i++)
        {
            Accessory accessory = Accessories[i];
            if (accessory.Unlocked && accessory.Name == name)
            {
                SetAesthetic(i);
                return;
            }
        }
        SetAesthetic(-1);
    }

    public void SetAesthetic(int id)
    {
        AccessoryNumber = id;
    }

    public void ActivateAccessory()
    {
        if (Accessories[AccessoryNumber].Unlocked && !Accessories[AccessoryNumber].Active)
        {
            deactivateAllAccessories();
            Accessories[AccessoryNumber].Active = true;
            currentItem = GameObject.Instantiate(Accessories[AccessoryNumber].Item);
            currentItem.transform.parent = this.transform;
            currentItem.transform.localPosition = Vector3.zero;// + SkinOffsets[SkinSwapper.Instance.ActiveSkin];
            currentItem.transform.localRotation = Quaternion.identity;
            if (Type == AccessoryType.Hat)
            {
                Saves.SaveHatFound(Accessories[AccessoryNumber].Name); //Saves finding of accessor
                Saves.SaveHatSelected(Accessories[AccessoryNumber].Name, 1); //Saves removal of accessory
            }
            else if (Type == AccessoryType.Mask)
            {
                Saves.SaveMaskFound(Accessories[AccessoryNumber].Name); //Saves finding of accessor
                Saves.SaveMaskSelected(Accessories[AccessoryNumber].Name, 1); //Saves removal of accessory
            }
            else if (Type == AccessoryType.Neck)
            {
                Saves.SaveNeckFound(Accessories[AccessoryNumber].Name); //Saves finding of accessor
                Saves.SaveNeckSelected(Accessories[AccessoryNumber].Name, 1); //Saves removal of accessory
            }
        }
        else if(Accessories[AccessoryNumber].Unlocked && Accessories[AccessoryNumber].Active)
        {
            currentItem.transform.localPosition = Vector3.zero + SkinOffsets[SkinSwapper.Instance.ActiveSkin];
            currentItem.transform.localScale = SkinSizes[SkinSwapper.Instance.ActiveSkin] * Vector3.one * 0.01f;
        }
    }

    private void deactivateAllAccessories()
    {
        foreach(Accessory acc in Accessories)
        {
            if (acc.Active)
            {
                acc.Active = false;
                currentItem.transform.parent = null;
                GameObject.Destroy(currentItem);
                if (Type == AccessoryType.Hat)
                    Saves.SaveHatSelected(acc.Name, 0); //Saves removal of accessory
                if (Type == AccessoryType.Mask)
                    Saves.SaveMaskSelected(acc.Name, 0); //Saves removal of accessory
                if (Type == AccessoryType.Neck)
                    Saves.SaveNeckSelected(acc.Name, 0); //Saves removal of accessory
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(AccessoryNumber >= 0)
        {
            ActivateAccessory();
        }
        else
        {
            deactivateAllAccessories();
        }
    }
    
    //private void OnApplicationQuit()
    //{
    //    foreach (Accessory accessory in Accessories)
    //    {
    //        if (Type == AccessoryType.Hat)
    //        {
    //            if (accessory.Unlocked)
    //                Saves.SaveHatFound(accessory.Name);
    //            Saves.SaveHatSelected(accessory.Name, accessory.Active ? 1 : 0);
    //        }
    //        else if (Type == AccessoryType.Mask)
    //        {
    //            if (accessory.Unlocked)
    //                Saves.SaveMaskFound(accessory.Name);
    //            Saves.SaveMaskSelected(accessory.Name, accessory.Active ? 1 : 0);
    //        }
    //        else if (Type == AccessoryType.Neck)
    //        {
    //            if (accessory.Unlocked)
    //                Saves.SaveNeckFound(accessory.Name);
    //            Saves.SaveNeckSelected(accessory.Name, accessory.Active ? 1 : 0);
    //        }
    //    }
    //}
}
