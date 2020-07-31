using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightDamage : MonoBehaviour {

    [Header("Attack Properties")]
    [Tooltip("The environmental time controller for hitlag effects")]
    public TimeControl TimeControl;
    [Tooltip("How much damage the light causes")]
    public float lightDamage = 20f;
    [Tooltip("The hitlag caused by landing a hit with the light")]
    public float HitLag = 0.15f;

    [Header("Sound Properties")]
    [Tooltip("Sound to play when blinding a monster")]
    public AudioClip HurtMonsterSound;
    [Tooltip("Sound to play when blocking a bladeless attack")]
    public AudioClip BlockMonsterSound;
    [Tooltip("Sound to play when blocking a bladed attack")]
    public AudioClip BlockBladeSound;
    [Tooltip("Effect to play when blocking an attack")]
    public GameObject BlockEffect;
    

    // Use this for initialization
    void Start ()
    {
        TimeControl = GameObject.FindWithTag("Time").GetComponent<TimeControl>();
    }
	
	// Update is called once per frame
	void Update () {

	}

    public void PlayBlockSound(string blockType)
    {
        if (blockType == "Monster")
            TP_Sound.Instance.PlaySelectedNoiseSound(BlockMonsterSound);
        else if (blockType == "Blade")
            TP_Sound.Instance.PlaySelectedNoiseSound(BlockBladeSound);
        spawnBlockEffect();
    }

    private void spawnBlockEffect()
    {
        GameObject effect = GameObject.Instantiate(BlockEffect);
        effect.transform.parent = null;
        effect.transform.position = this.transform.position;
        effect.transform.localScale *= 2;
    }

    private void OnTriggerEnter(Collider other) 
    {
        if(other.transform.tag == "Eyeball") //when hitting the eyeball with sphere trigger
        {
            //Debug.Log("Hit the eye!");
            TimeControl.SetHitlag(0f, HitLag);
            TP_Sound.Instance.PlaySelectedSound(HurtMonsterSound);
            spawnBlockEffect();
            if(other.gameObject.GetComponent<EyeStats>() != null)
                other.gameObject.GetComponent<EyeStats>().ApplyDamage(lightDamage);
        }
        if ((other.transform.tag == "Male" || other.transform.tag == "Female"))
        {
            other.gameObject.GetComponentInParent<ChildAnimator>().SetPainCooldown();
            TP_Stats.Instance.NoiseIncrease(TP_Stats.Instance.MaxNoise / 5);
            TP_Stats.Instance.NoiseDefenseTimer /= 2; //quarters time child is relaxed if so
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
    }
}
