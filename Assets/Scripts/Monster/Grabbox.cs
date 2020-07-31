using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabbox : MonoBehaviour
{
    [Header("Grabbox Properties")]
    [Tooltip("The collider that's used for the attack")]
    public Collider AttackBox;
    public bool Monster = true;
    //public bool CanBlock = true;
    
    [Header("Timing Properties")]
    [Tooltip("Time control object")]
    public TimeControl Time;
    [Tooltip("Amount of time in seconds making Teddy untouchable")]
    public float IFrames = 0.5f;
    [Tooltip("The amount of time landing the attack freezes time for visual impact")]
    public float HitLag = 0.1f;
    [Tooltip("The slowdown of the landing of the attacks to show the impact")]
    public float Slowdown = 0.1f;
    [Tooltip("How close this hitbox has to be to the camera to affect camera shake")]
    public float minDistanceFromCamera = 0.5f;

    private CreatureMind mind;
    private CreatureAnimator anim;

    // Use this for initialization
    void Start()
    {
        if (!Time)
        {
            Time = GameObject.FindWithTag("Time").GetComponent<TimeControl>();
        }
        if (Monster)
        {
            mind = this.transform.root.GetComponent<CreatureMind>();
            anim = this.transform.root.GetComponent<CreatureAnimator>();
        }
        if (AttackBox == null)
        {
            Collider[] colliders = this.GetComponents<Collider>();
            foreach (Collider col in colliders)
            {
                if (col.isTrigger)
                {
                    AttackBox = col;
                    break;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (AttackBox.enabled && AttackBox.isTrigger)
        {
            Transform mainCam = Camera.main.transform;
            float distance = Vector3.Distance(mainCam.position, this.transform.position);
            if (distance <= minDistanceFromCamera)
                mainCam.GetComponent<CameraShake>().ShakeCamera(distance, 1f, 1f);
        }
    }
    
    void OnTriggerStay(Collider coll)
    {
        if (AttackBox.enabled)
        {
            if (coll.transform.tag == "Player")
            {
                Grabbing(coll);
            }
            //else if ((coll.transform.tag == "Light" && !coll.isTrigger)
            //    || coll.transform.tag == "Needle" || coll.transform.tag == "Cape")
            //{
            //    Debug.Log("Tag is " + coll.transform.tag);
            //    Knocking(coll, (1 - TP_Motor.Instance.KnockbackResistance), (1 - TP_Stats.Instance.DamageResistance));
            //}
        }

        //else if(coll.transform.tag == "Needle"){
        //    Deflect();
        //}
    }

    private void Grabbing(Collider coll)
    {
        if (TP_Motor.Instance.IFrames <= 0f)
        {
            //Vector3 newAngle = Quaternion.AngleAxis(SidewaysAngle, this.transform.root.up)
            //        * Quaternion.AngleAxis(UpwardsAngle, this.transform.root.right)
            //        * this.transform.root.forward;
            //Vector3 impact = newAngle * Knockback * KnockbackPercentage;
            TP_Motor.Instance.ImpactPoint = this.transform.position;
            //apply effects
            //play noise
            Time.SetHitlag(Slowdown, HitLag);
            TP_Motor.Instance.IFrames = IFrames;
            TP_Sound.Instance.PlayHitSound();
            if (Monster)
            {
                if (coll.transform.tag == "Player")
                {
                    mind.AddJoy();
                    TP_Animator.Instance.anim.SetBool("Fall", true);
                    coll.transform.root.parent = this.transform;

                    //TP_Sound.Instance.PlayHitSound();
                    //TP_Motor.Instance.ApplyKnockback(impact, true);
                }
                //else if (coll.transform.tag == "Light")
                //{
                //    coll.gameObject.GetComponent<LightDamage>().PlayBlockSound(this.transform.tag);
                //}
                //else if (coll.transform.tag == "Needle")
                //{   //Check on this thing
                //    coll.gameObject.GetComponent<NeedleDamage>().PlayParrySound(this.transform.tag);
                //    anim.parry = true;
                //    mind.AddFear();
                //    mind.AddSorrow();
                //    TP_Motor.Instance.ApplyKnockback(impact, true);
                //}
                //else if (coll.transform.tag == "Cape")
                //{
                //    mind.AddJoy();
                //    TP_Motor.Instance.ApplyKnockback(impact, true);
                //}

                //if (Bladed && coll.transform.tag != "Cape")
                //{
                //    TP_Stats.Instance.ReduceCotton(Damage * DamagePercentage);
                //}
            }
            else
            {
                //if (CanBlock)
                //{
                //    if (coll.transform.tag == "Light")
                //        coll.gameObject.GetComponent<LightDamage>().PlayBlockSound("Monster");
                //    else if (coll.transform.tag == "Needle")
                //    {   //Check on this thing
                //        coll.gameObject.GetComponent<NeedleDamage>().PlayParrySound("Monster");
                //        TP_Motor.Instance.ApplyKnockback(impact, true);
                //    }
                //    else if (coll.transform.tag == "Cape")
                //        TP_Motor.Instance.ApplyKnockback(impact, true);

                //    if (Bladed && coll.transform.tag != "Cape")
                //        TP_Stats.Instance.ReduceCotton(Damage * DamagePercentage);
                //}
                if (coll.transform.tag == "Player")
                {
                    TP_Animator.Instance.anim.SetBool("Fall", true);
                    coll.transform.parent = this.transform;
                }
            }
        }
    }
}
