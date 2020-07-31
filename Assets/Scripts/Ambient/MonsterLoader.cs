using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterLoader : MonoBehaviour
{
    [Header("Monster Objects")]
    [Tooltip("The amount of time between monster spawns")]
    public float Delay = 20f;
    [Tooltip("Reference to all of the monsters so far")]
    public GameObject[] Monsters;

    private SettingsHandler settings;
    private float delay = 0f;
    private bool spawning = false;
    // Start is called before the first frame update
    void Start()
    {
        settings = SettingsHandler.Instance;
        delay = Delay;
        disableMonsters();
    }

    // Update is called once per frame
    void Update()
    {
        checkMonster();
    }

    private void disableMonsters()
    {
        foreach (GameObject Monster in Monsters)
        {
            if(Monster != null)
                Monster.SetActive(false);
        }
    }

    private void checkMonster()
    {
        //if (settings.TutorialDone && delay > 0f)
          //  delay -= Time.deltaTime;

        if (settings.TutorialDone && !Monsters[settings.EnemiesDefeated].activeSelf) //&& delay <= 0f)
        {
            disableMonsters();
            Invoke("spawnMonster", Delay);
            //delay = Delay;
        }
    }

    private void spawnMonster()
    {
        Monsters[settings.EnemiesDefeated].SetActive(true);
    }
}
