using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEyeStats : MonoBehaviour {

    public GameObject VisualCone;
    [Header("Stats")]
    public bool Alive = true;
    public float FullHealth = 100f;
    public float Health = 100f;

    [Header("Damage")]
    public float VisualMax = 500f;
    public int DamageGlowMultiplier = 5;
    public float Damage = 0f;
    public Material eyeMat;
    // Use this for initialization
    void Start()
    {
        if (VisualCone == null)
            VisualCone = this.transform.Find("Visual Range").gameObject;
        eyeMat = this.GetComponent<MeshRenderer>().material;
        Health = FullHealth;
    }

    // Update is called once per frame
    void Update()
    {
        reduceDamage();
        updateEye();
    }

    public void SetDamage(float damage)
    {
        Damage = damage;
    }

    public void updateEye()
    {
        if (Health > 0f)
        {
            float healthDifference = FullHealth - Health;
            float healthDecreasePercentage = healthDifference / FullHealth;
            eyeMat.SetFloat("_RimPower", Mathf.Pow(healthDecreasePercentage, DamageGlowMultiplier) * VisualMax);
        }
        else
        { //deactivates the visual cone and maintains visuals for proper calculation
            Health = 0;
            Alive = false;
            VisualCone.SetActive(false);
            eyeMat.SetColor("_RimColor", Color.black);
        }
    }

    void reduceDamage()
    {
        if (Damage > 0)
        {
            Damage -= 2;
            Health -= 2;
        }
    }

    public void ApplyDamage(float damage)
    {
        SetDamage(damage);
    }
}
