using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDoor : MonoBehaviour
{
    public int DefeatedMonsters = 1;

    // Update is called once per frame
    void Update()
    {
        if (SettingsHandler.Instance.EnemiesDefeated >= DefeatedMonsters)
            this.GetComponent<Doors>().Locked = false;
    }
}
