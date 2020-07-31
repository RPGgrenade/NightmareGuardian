using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hitbox : MonoBehaviour {

    [Header("Hitbox Properties")]
    [Tooltip("The collider that's used for the attack")]
    public Collider AttackBox;
    public bool Monster = true;
    public bool CanBlock = true;
    public Transform RootTransform;

    [Header("Angle properties")]
    [Tooltip("The power of the knockback of the attack")]
    public int Knockback = 100;
    [Tooltip("The angle at which the hitbox sends the Teddy vertically")]
    public float UpwardsAngle = 0f;
    [Tooltip("The angle at which the hitbox sends the Teddy horizontally")]
    public float SidewaysAngle = 0f;

    [Header("Timing Properties")]
    [Tooltip("Time control object")]
    public TimeControl Time;
    [Tooltip("Amount of time in seconds making Teddy untouchable (must be positive)")]
    public float IFrames = 2f;
    [Tooltip("Amount of time in seconds Teddy is forced to take knockback (must be above 0)")]
    public float Hitstun = 2f;
    [Tooltip("The amount of time landing the attack freezes time for visual impact (must be positive)")]
    public float HitLag = 0.2f;
    [Tooltip("The slowdown of the landing of the attacks to show the impact (must be positive)")]
    public float Slowdown = 0.4f;

    [Header("Damage Properties")]
    [Tooltip("Whether the hitbox applies damage or not due to being bladed")]
    public bool Bladed = false;
    [Tooltip("The damage applied to the Teddy if bladed")]
    public float Damage = 0f;
    [Tooltip("How close this hitbox has to be to the camera to affect camera shake")]
    public float minDistanceFromCamera = 3f;


    private CreatureMind mind;
    private CreatureAnimator anim;
    private CreatureCollision collision;

	// Use this for initialization
	void Start () {
        if (!Time)
        {
            Time = GameObject.FindWithTag("Time").GetComponent<TimeControl>();
        }
        if (Monster)
        {
            mind = this.transform.root.GetComponent<CreatureMind>();
            anim = this.transform.root.GetComponent<CreatureAnimator>();
            collision = this.transform.root.GetComponent<CreatureCollision>();
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
	void Update () {
        if (AttackBox.enabled && AttackBox.isTrigger)
        {
            Transform mainCam = Camera.main.transform;
            float distance = Vector3.Distance(mainCam.position, this.transform.position);
            if (distance <= minDistanceFromCamera)
            {
                if (Bladed)
                    mainCam.GetComponent<CameraShake>().ShakeCamera(distance, Knockback, Damage);
                else
                    mainCam.GetComponent<CameraShake>().ShakeCamera(distance, 1f, Knockback);
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!Monster)
        {
            if (RootTransform != null)
            {
                Vector3 newAngle = Quaternion.AngleAxis(SidewaysAngle, RootTransform.up)
                           * Quaternion.AngleAxis(UpwardsAngle, RootTransform.right)
                           * RootTransform.forward;
                DrawArrow.ForGizmo(RootTransform.position, newAngle.normalized * Knockback);
            }
            else
            {
                Vector3 newAngle = Quaternion.AngleAxis(SidewaysAngle, this.transform.up)
                        * Quaternion.AngleAxis(UpwardsAngle, this.transform.right)
                        * this.transform.forward;
                DrawArrow.ForGizmo(this.transform.position, newAngle.normalized * Knockback);
            }
        }
    }

    void OnTriggerEnter(Collider coll)
    {
        if (AttackBox.enabled)
        {
            if (coll.transform.tag == "Player")
            {
                Knocking(coll);
            }
            else if ((coll.transform.tag == "Light" && !coll.isTrigger)
                || coll.transform.tag == "Needle" || coll.transform.tag == "Cape")
            {
                Debug.Log("Tag is " + coll.transform.tag);
                Knocking(coll, (1 - TP_Motor.Instance.KnockbackResistance), (1 - TP_Stats.Instance.DamageResistance));
            }
        }
        
        //else if(coll.transform.tag == "Needle"){
        //    Deflect();
        //}
    }

    private void Knocking(Collider coll, float KnockbackPercentage = 1f, float DamagePercentage = 1f)
    {
        if (TP_Motor.Instance.IFrames <= 0f)
        {
            Vector3 newAngle;
            if (Monster)
            {
                newAngle = Quaternion.AngleAxis(SidewaysAngle, this.transform.root.up)
                        * Quaternion.AngleAxis(UpwardsAngle, this.transform.root.right)
                        * this.transform.root.forward;
            }
            else
            {
                if(RootTransform != null)
                    newAngle = Quaternion.AngleAxis(SidewaysAngle, RootTransform.up)
                        * Quaternion.AngleAxis(UpwardsAngle, RootTransform.right)
                        * RootTransform.forward;
                else
                    newAngle = Quaternion.AngleAxis(SidewaysAngle, this.transform.up)
                        * Quaternion.AngleAxis(UpwardsAngle, this.transform.right)
                        * this.transform.forward;
            }
            Vector3 impact = newAngle * Knockback * KnockbackPercentage;
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

                    //TP_Sound.Instance.PlayHitSound();
                    TP_Motor.Instance.ApplyKnockback(impact, true, Hitstun);
                }
                else if (coll.transform.tag == "Light")
                {
                    coll.gameObject.GetComponent<LightDamage>().PlayBlockSound(this.transform.tag);
                    if (TP_Supers.Instance.ChargeBlock)
                        TP_Animator.Instance.anim.SetTrigger("Super Block");
                }
                else if (coll.transform.tag == "Needle")
                {   //Check on this thing
                    NeedleDamage needle = coll.gameObject.GetComponent<NeedleDamage>();
                    needle.PlayParrySound(this.transform.tag);
                    collision.Stun = needle.StabNeedleStun * needle.StabStunMultipler;
                    needle.ResetStunMultiplier();
                    anim.parry = true;
                    mind.AddFear();
                    mind.AddSorrow();
                    TP_Motor.Instance.ApplyKnockback(impact, true, Hitstun * 0.8f);
                }
                else if (coll.transform.tag == "Cape")
                {
                    mind.AddJoy();
                    TP_Motor.Instance.ApplyKnockback(impact, true, Hitstun/2);
                }

                if (Bladed && coll.transform.tag != "Cape")
                {
                    TP_Stats.Instance.ReduceCotton(Damage * DamagePercentage);
                }
            }
            else
            {
                if (CanBlock)
                {
                    if (coll.transform.tag == "Light")
                        coll.gameObject.GetComponent<LightDamage>().PlayBlockSound("Monster");
                    else if (coll.transform.tag == "Needle")
                    {   //Check on this thing
                        coll.gameObject.GetComponent<NeedleDamage>().PlayParrySound("Monster");
                        TP_Motor.Instance.ApplyKnockback(impact, true, Hitstun * 0.8f);
                    }
                    else if (coll.transform.tag == "Cape")
                        TP_Motor.Instance.ApplyKnockback(impact, true, Hitstun/2);

                    if (Bladed && coll.transform.tag != "Cape")
                        TP_Stats.Instance.ReduceCotton(Damage * DamagePercentage);
                }
                if (coll.transform.tag == "Player")
                    TP_Motor.Instance.ApplyKnockback(impact, true, Hitstun);
            }
        }
    }
}
