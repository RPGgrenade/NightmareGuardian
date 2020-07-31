using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeedleDamage : MonoBehaviour {

    [Header("Interaction Properties")]
    [Tooltip("Specify Layers that can't be touched with needle")]
    public LayerMask LayersCantHit;
    [Tooltip("The environmental time controller for hitlag effects")]
    public TimeControl TimeControl;
    [Header("Stab Properties")]
    [Tooltip("Damage of the stabbing (always zero)")]
    public float StabDamage = 0f;
    [Tooltip("Hitlag of stabbing (pretty low)")]
    public float StabHitLag = 0.15f;
    [Tooltip("Stun from stabbing (more for animation)")]
    public float StabNeedleStun = 0.8f;
    [Tooltip("Multilplier for stun time")]
    public float StabStunMultipler = 1f;
    [Tooltip("Cooldown for stabbing (avoids multiple stabs at once)")]
    public float StabCooldown = 1f;

    [Header("Sound Properties")]
    [Tooltip("Sound to play when stabbing a monster")]
    public AudioClip StabMonsterSound;
    [Tooltip("Sound to play when parrying a bladeless attack")]
    public AudioClip ParryMonsterSound;
    [Tooltip("Sound to play when parrying a bladed attack")]
    public AudioClip ParryBladeSound;
    [Tooltip("Effect to play when parrying an attack")]
    public GameObject ParryEffect;
    [Tooltip("Effect to play when stabbing a monster")]
    public GameObject StabEffect;



    private float cooldown = 0f;

    // Use this for initialization
    void Start()
    {
        TimeControl = GameObject.FindWithTag("Time").GetComponent<TimeControl>();
    }

    // Update is called once per frame
    void Update()
    {
        if(cooldown > 0)
            cooldown -= Time.deltaTime;
    }

    public void PlayParrySound(string ParryType)
    {
        if (ParryType == "Monster")
            TP_Sound.Instance.PlaySelectedNoiseSound(ParryMonsterSound);
        else if (ParryType == "Blade")
            TP_Sound.Instance.PlaySelectedNoiseSound(ParryBladeSound);
        spawnParryEffect();
    }

    private void spawnParryEffect()
    {
        GameObject effect = GameObject.Instantiate(ParryEffect);
        effect.transform.parent = null;
        effect.transform.position = this.transform.position;
        effect.transform.localScale *= 0.5f;
    }

    private void spawnStabbingEffect()
    {
        GameObject effect = GameObject.Instantiate(StabEffect);
        effect.transform.parent = null;
        effect.transform.position = this.transform.position;
        effect.transform.localScale *= 1;
    }

    public void ResetStunMultiplier()
    {
        StabStunMultipler = 1f;
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (cooldown > 0) return;

        if ((collision.transform.tag == "Monster" || collision.transform.tag == "Sack") && 
            collision.gameObject.layer == LayerMask.NameToLayer("Monster")) //when hitting the monster or burlap sack
        {
            if (collision.GetComponent<Hitbox>() != null)
            {
                if (!collision.GetComponent<Hitbox>().AttackBox.enabled)
                {
                    cooldown = StabCooldown;
                    Debug.Log(collision.transform.tag + " " + LayerMask.LayerToName(collision.gameObject.layer));
                    TimeControl.SetHitlag(0f, StabHitLag);
                    TP_Sound.Instance.PlaySelectedNoiseSound(StabMonsterSound);
                    spawnStabbingEffect();
                    if (collision.gameObject.GetComponentInParent<CreatureCollision>() != null)
                    {
                        collision.gameObject.GetComponentInParent<CreatureCollision>().ApplyDamage(StabDamage, StabNeedleStun * StabStunMultipler);
                        ResetStunMultiplier();
                    }
                    if (collision.gameObject.GetComponentInParent<CreatureStats>() != null)
                        collision.gameObject.GetComponentInParent<CreatureStats>().ApplyPainDamage();
                }
            }
            else
            {
                cooldown = StabCooldown;
                Debug.Log(collision.transform.tag + " " + LayerMask.LayerToName(collision.gameObject.layer));
                TimeControl.SetHitlag(0f, StabHitLag);
                TP_Sound.Instance.PlaySelectedNoiseSound(StabMonsterSound);
                spawnStabbingEffect();
                if (collision.gameObject.GetComponentInParent<CreatureCollision>() != null)
                {
                    collision.gameObject.GetComponentInParent<CreatureCollision>().ApplyDamage(StabDamage, StabNeedleStun * StabStunMultipler);
                    ResetStunMultiplier();
                }
                if (collision.gameObject.GetComponentInParent<CreatureStats>() != null)
                    collision.gameObject.GetComponentInParent<CreatureStats>().ApplyPainDamage();
            }
        }
        if((collision.transform.tag == "Male" || collision.transform.tag == "Female"))
        {
            cooldown = StabCooldown;
            collision.gameObject.GetComponentInParent<ChildAnimator>().SetPainCooldown();
            TP_Stats.Instance.NoiseIncrease(TP_Stats.Instance.MaxNoise/2);
            TP_Stats.Instance.NoiseDefenseTimer /= 4; //quarters time child is relaxed if so
        }
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //}
}
