using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaperItem : MonoBehaviour
{
    public enum ItemType
    {
        Hat,
        Mask,
        Neck
    }

    public ItemType Type = ItemType.Hat;
    public string AccessoryName;
    public Renderer ItemRenderer;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Player") && other.gameObject.tag == "Player")
        {
            if (Type == ItemType.Hat)
                UnlockHat();
            else if (Type == ItemType.Mask)
                UnlockMask();
            else if (Type == ItemType.Neck)
                UnlockNeckwear();
            JinglesHandler.Instance.PlayeJingle("Puzzle");
            GameObject.Destroy(this.gameObject);
        }
    }

    private void UnlockHat()
    {
        foreach (Aesthetics.Accessory acc in SkinSwapper.Instance.Hats.Accessories)
        {
            if (acc.Name == AccessoryName)
            {
                acc.Unlocked = true;
                Saves.SaveHatFound(acc.Name);
                continue;
            }
        }
    }

    private void UnlockMask()
    {
        foreach (Aesthetics.Accessory acc in SkinSwapper.Instance.Masks.Accessories)
        {
            if (acc.Name == AccessoryName)
            {
                acc.Unlocked = true;
                Saves.SaveMaskFound(acc.Name);
                continue;
            }
        }
    }

    private void UnlockNeckwear()
    {
        foreach (Aesthetics.Accessory acc in SkinSwapper.Instance.Necks.Accessories)
        {
            if (acc.Name == AccessoryName)
            {
                acc.Unlocked = true;
                Saves.SaveNeckFound(acc.Name);
                continue;
            }
        }
    }

    public void SetItem(Material materialItem)
    {
        ItemRenderer.sharedMaterial = materialItem;
    }

    public void SetName(string itemName)
    {
        AccessoryName = itemName;
    }

    public void SetType(ItemType type)
    {
        Type = type;
    }
}
