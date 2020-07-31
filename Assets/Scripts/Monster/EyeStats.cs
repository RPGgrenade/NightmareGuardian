using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeStats : MonoBehaviour {

    [Header("Sight Properties")]
    public TrailRenderer Trail;
    public float MaxTrailLength = 3f;
    public GameObject VisualCone;
    [Header("Stats")]
    public bool Alive = true;
    public bool Pain = false;
    public float FullHealth = 100f;
    public float Health = 100f;

    [Header("Damage")]
    public float VisualMax = 500f;
    public int DamageGlowMultiplier = 5;
    public float Damage = 0f;
    public Material eyeMat;
    public CreatureAnimator anim;
    public CreatureMind mind;
    // Use this for initialization
    void Awake()
    {
        if (VisualCone == null)
            VisualCone = this.transform.Find("Visual Range").gameObject;
        if (Trail == null)
            Trail = this.transform.GetComponentInChildren<TrailRenderer>();
        eyeMat = this.GetComponent<MeshRenderer>().material;
        if(anim == null)
            anim = this.GetComponentInParent<CreatureAnimator>();
        if(mind == null)
            mind = this.GetComponentInParent<CreatureMind>();

        Health = FullHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if(this.GetComponentInParent<CreatureCollision>().StunFrames <= 0)
            reduceDamage();
        updateEye();
    }

    public void SetDamage(float damage)
    {
        Damage = damage;
        if(anim != null)
            //anim.pain = true;
            Pain = true;
    }

    public void updateEye()
    {
        if (Health > 0f)
        {
            float healthDifference = FullHealth - Health;
            float healthDecreasePercentage = healthDifference / FullHealth;
            eyeMat.SetFloat("_RimPower", Mathf.Pow(healthDecreasePercentage, DamageGlowMultiplier) * VisualMax);
            Trail.time = MaxTrailLength * ((FullHealth - Health)/FullHealth);
        }
        else
        { //deactivates the visual cone and maintains visuals for proper calculation
            Health = 0;
            Alive = false;
            VisualCone.SetActive(false);
            VisualCone.GetComponent<LongRangeDetection>().Alive = false;
            eyeMat.SetColor("_RimColor", Color.black);
            Trail.time = 0f;
        }
    }

    void reduceDamage()
    {
        if (Damage > 0)
        {
            Damage -= 2;
            Health -= 2;
        }
        else
            //anim.pain = false;
            Pain = false;
    }

    public void ApplyDamage(float damage)
    {
        SetDamage(damage);
        mind.AddRage(); //special for when recieving damage only
        mind.AddSorrow();
        mind.AddFear();
        mind.RemoveEnd();
    }
}
