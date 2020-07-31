using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildDoorManager : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (SettingsHandler.Instance.TutorialDone)
            this.GetComponent<Doors>().Locked = false;
    }
}
