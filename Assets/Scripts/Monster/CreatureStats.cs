using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CreatureAnimator))]
public class CreatureStats : MonoBehaviour {

    [Header("Stats")]
    [Header("Health")]
    [Tooltip("Max amount of health monster can have")]
    public float FullHealth = 100f;
    [Tooltip("Current amount of health mosnter has")]
    public float Health = 100f;

    [Header("Eyeball Properties")]
    [Tooltip("Amount of damage taken at any moment for smooth transitions")]
    public float Damage = 0f;
    [Tooltip("Percentage damage goes down via stab or item thrown on each eye (there's an upper limit)")]
    public float StabDamage = 3f;
    [Tooltip("All eyeballs monster has")]
    public EyeStats[] Eyeballs;

    [Header("Escape properties")]
    [Tooltip("Whether or not the monster was killed outright or let go")]
    public bool Killed = false;
    [Tooltip("How much time before the escape opportunity starts")]
    public float EscapeTimer = 5f;
    [Tooltip("How far the teddy bear has to be for an escape to be valid")]
    public float EscapeDistance = 4f;
    [Tooltip("The Offset of the death particles")]
    public Vector3 EscapeOffset = Vector3.zero;

    [Header("Particles")]
    [Min(0f)]
    public float EscapeParticleScale = 1f;
    public GameObject EscapeParticles;

    [Space(15)]
    public CreatureAnimator anim;

	void Awake () {
        anim = this.gameObject.GetComponent<CreatureAnimator>();
	}

    private void Start()
    {
        SetFullHealth();
        Health = FullHealth;
    }

    // Update is called once per frame
    void Update () {
        CheckEyePain();
        SetHealth();
        if (GetHealthPercentage() <= 0 && !Killed)
            checkIfEscape();
	}

    public void CheckEyePain()
    {
        foreach (EyeStats eye in Eyeballs)
        {
            if (eye.Pain)
            {
                anim.pain = true;
                return;
            }
        }
        anim.pain = false;
    }

    public void WasKilled()
    {
        Killed = true;
    }

    void checkIfEscape()
    {
        if (EscapeTimer > 0f)
            EscapeTimer -= Time.deltaTime;
        else
        {
            Transform cameraTrans = Camera.main.transform;
            TurnToLook head = this.transform.root.GetComponentInChildren<TurnToLook>();
            Transform monsterHeadPosition = head.transform;
            Vector3 directionBetweenCameraAndMonster = (monsterHeadPosition.position - cameraTrans.position).normalized;
            float dotProdMonster = Vector3.Dot(directionBetweenCameraAndMonster, cameraTrans.forward);
            if (dotProdMonster < -0.3f && head.DistanceFromTarget > EscapeDistance)
            {
                //TODO: Spawn a death particle object
                GameObject escapeParticles = GameObject.Instantiate(EscapeParticles, 
                    this.transform.position - this.transform.InverseTransformDirection(EscapeOffset), 
                    Quaternion.Euler(90f,0f,0f));
                escapeParticles.transform.localScale = Vector3.one * EscapeParticleScale;
                DestroySelfOnDeath();
            }
        }
    }

    public void ApplyPainDamage()
    {
        foreach(EyeStats eye in Eyeballs)
        {
            if(eye.Health / eye.FullHealth >= 0.8f)
                eye.Health -= StabDamage;
        }
    }

    void SetFullHealth()
    {
        float fullHealth = 0f;
        foreach(EyeStats eye in Eyeballs)
        {
            fullHealth += eye.FullHealth;
        }
        FullHealth = fullHealth;
    }

    void SetHealth()
    {
        float health = 0f;
        foreach (EyeStats eye in Eyeballs)
        {
            health += eye.Health;
        }
        Health = health;
    }

    public float GetHealthPercentage()
    {
        return Health / FullHealth;
    }

    public void DestroySelfOnDeath()
    {
        //something with the particle system to fix its suddenly disappearance
        SettingsHandler.Instance.AddToDefeatedEnemies();
        //if(killed) //TODO
            //do a thing with kill confirms
        GameObject.Destroy(this.gameObject);
    }
}
