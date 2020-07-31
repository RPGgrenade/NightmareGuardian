using UnityEngine;

public class Spawning : MonoBehaviour {

    [Header("Spawner Properties")]
    [Tooltip("Bug to be spawned")]
    public GameObject Bug;
    [Tooltip("How long the timer has run this cycle")]
    public float Timer = 0f;
    [Tooltip("How long each cycle lasts")]
    public float SpawnTime = 3f;
    [Tooltip("The chances of a bug spawning once cycle completes")]
    public int SpawnChance = 40;
    
	// Update is called once per frame
	void Update () {
        Timer += Time.deltaTime;
        if(Timer > SpawnTime)
        {
            Timer = 0;
            Spawn();
        }
	}

    private void Spawn()
    {
        int chance = Random.Range(0,SpawnChance);
        if (chance == 1)
            Instantiate(Bug,this.transform.position,Random.rotation);
    }
}
