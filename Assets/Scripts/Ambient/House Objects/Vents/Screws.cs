using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Screws : MonoBehaviour
{
    [Tooltip("How many monsters at minimum need to have been defeated for this grate to open")]
    public int MonstersNeeded = 3;
    public bool Unscrewed = false;
    public GameObject ParticleLights;

    private Animator anim;
    private int monstersKilled = 0;

    private void OnTriggerEnter(Collider other)
    {
        if(monstersKilled >= MonstersNeeded && 
            other.gameObject.layer == LayerMask.NameToLayer("Player") && 
            other.tag == "Needle")
        {
            Unscrewed = true;
            anim.SetTrigger("Unscrew");
            Invoke("offParticles", 1f);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        anim = this.GetComponent<Animator>();
        InvokeRepeating("checkParticles", 1f, 1f);
    }

    public void OffParticles()
    {
        Unscrewed = true;
        ParticleLights.SetActive(false);
    }

    void checkParticles()
    {
        if (!Unscrewed)
        {
            monstersKilled = SettingsHandler.Instance.EnemiesDefeated;
            if (monstersKilled >= MonstersNeeded)
            {
                ParticleLights.SetActive(true);
                CancelInvoke("checkParticles");
            }
        }
    }
}
