using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheatCodes : MonoBehaviour
{
    public bool CheatsActive = false;

    // Update is called once per frame
    void Update()
    {
        CheatsActive = Input.GetKey(KeyCode.RightControl);
        if (CheatsActive)
        {
            if (Input.GetKeyDown(KeyCode.T))
                SetTutorialDone();
            else if (Input.GetKeyDown(KeyCode.J))
                DeleteAllSaveData();
            else if (Input.GetKeyDown(KeyCode.Alpha1))
                SetMonster(0);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                SetMonster(1);
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                SetMonster(2);
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                SetMonster(3);
            //else if (Input.GetKeyDown(KeyCode.Alpha5))
            //  SetMonster(4);
            //else if (Input.GetKeyDown(KeyCode.Alpha6))
            //  SetMonster(5);
            else if (Input.GetKeyDown(KeyCode.H))
                UnlockHats();
            else if (Input.GetKeyDown(KeyCode.M))
                UnlockMasks();
            else if (Input.GetKeyDown(KeyCode.N))
                UnlockNeckwear();
        }
    }

    private void SetMonster(int monsterNumber)
    {
        SettingsHandler.Instance.EnemiesDefeated = monsterNumber;
        Saves.SaveMonstersDefeated(monsterNumber);
    }

    private void SetTutorialDone()
    {
        SettingsHandler.Instance.TutorialDone = true;
        TP_Animator.Instance.HasBag = true;
        TP_Animator.Instance.HasCape = true;
        TP_Animator.Instance.HasNeedle = true;
        TP_Animator.Instance.HasNightLight = true;
        TP_Animator.Instance.HasSpindle = true;
        Debug.Log("Set tutorial as done");
    }

    private void UnlockHats()
    {
        foreach (Aesthetics.Accessory acc in SkinSwapper.Instance.Hats.Accessories)
        {
            acc.Unlocked = true;
            Saves.SaveHatFound(acc.Name);
        }
        Debug.Log("Unlocked all hats");
    }

    private void UnlockMasks()
    {
        foreach (Aesthetics.Accessory acc in SkinSwapper.Instance.Masks.Accessories)
        {
            acc.Unlocked = true;
            Saves.SaveMaskFound(acc.Name);
        }
        Debug.Log("Unlocked all masks");
    }

    private void UnlockNeckwear()
    {
        foreach (Aesthetics.Accessory acc in SkinSwapper.Instance.Necks.Accessories)
        {
            acc.Unlocked = true;
            Saves.SaveNeckFound(acc.Name);
        }
        Debug.Log("Unlocked all neckwear");
    }

    private void DeleteAllSaveData()
    {
        Debug.Log("Deleting all this save file data");
        Saves.DeleteCurrentSaveFile();
    }
}
