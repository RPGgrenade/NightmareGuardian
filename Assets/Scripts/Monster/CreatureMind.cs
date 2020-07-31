using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(CreatureCollision))]
[RequireComponent(typeof(CreatureStats))]
[RequireComponent(typeof(CreatureAnimator))]
[RequireComponent(typeof(CreatureSound))]
[RequireComponent(typeof(Patrol))]
public class CreatureMind : MonoBehaviour {

    [Header("Personality")]
    [Tooltip("Multiplier applied to joy (unique to each monster)")]
    [Range(0,100)]
    public float Optimism = 0f;
    [Tooltip("Multiplier applied to rage (unique to each monster)")]
    [Range(0, 100)]
    public float Temper = 0f;
    [Tooltip("Multiplier applied to sorrow (unique to each monster)")]
    [Range(0, 100)]
    public float Pessimism = 0f;
    [Tooltip("Multiplier applied to fear (unique to each monster)")]
    [Range(0, 100)]
    public float Cowardice = 0f;
    [Tooltip("Multiplier applied to end (unique to each monster)")]
    [Range(0, 100)]
    public float Apathy = 0f;
    [Tooltip("Multiplier applied to pain (unique to each monster)")]
    [Range(0, 100)]
    public float Sensitivity = 0f;

    [Header("Emotional Stabilizing")]
    [Tooltip("Multiplier subtracted from joy (unique to each monster)")]
    [Range(0, 10)]
    public float Deoptimism = 0f;
    [Tooltip("Multiplier subtracted from rage (unique to each monster)")]
    [Range(0, 10)]
    public float Detemper = 0f;
    [Tooltip("Multiplier subtracted from sorrow (unique to each monster)")]
    [Range(0, 10)]
    public float Depessimism = 0f;
    [Tooltip("Multiplier subtracted from fear (unique to each monster)")]
    [Range(0, 10)]
    public float Decowardice = 0f;
    [Tooltip("Multiplier subtracted from end (unique to each monster)")]
    [Range(0, 10)]
    public float Deapathy = 0f;
    [Tooltip("Multiplier subtracted from pain (unique to each monster)")]
    [Range(0, 10)]
    public float Desensitivity = 0f;


    [Header("Emotional Values")] //yay MGS3 reference!
    [Tooltip("The more the player gets hit or manages to create noise, the more this increases")]
    public float Joy = 0f;
    [Tooltip("The more damaged and interrupted the monster is, the more this increases")]
    public float Rage = 0f;
    [Tooltip("The more the monster is interrupted, parried, and dodge, the more this increases")]
    public float Sorrow = 0f;
    [Tooltip("The closer the player is and the monster gets stabbed or parried, the more this increases")]
    public float Fear = 0f;
    [Tooltip("The Longer nothing happens, the more this increases")]
    public float End = 0f; //boredom
    [Tooltip("The more blinded and stabbed they are, the more this increases")]
    public float Pain = 0f;

    [Header("Emotional Thresholds")]
    [Tooltip("The minimum value for joy to be considered active")]
    public float MinimalJoy = 100f;
    [Tooltip("The maximum value joy can reach (to avoid infinites)")]
    public float MaximalJoy = 200f;
    [Space(10f)]
    [Tooltip("The minimum value for fear to be considered active")]
    public float MinimalRage = 100f;
    [Tooltip("The maximum value fear can reach (to avoid infinites)")]
    public float MaximalRage = 200f;
    [Space(10f)]
    [Tooltip("The minimum value for sorrow to be considered active")]
    public float MinimalSorrow = 100f;
    [Tooltip("The maximum value sorrow can reach (to avoid infinites)")]
    public float MaximalSorrow = 200f;
    [Space(10f)]
    [Tooltip("The minimum value for fear to be considered active")]
    public float MinimalFear = 100f;
    [Tooltip("The maximum value fear can reach (to avoid infinites)")]
    public float MaximalFear = 200f;
    [Space(10f)]
    [Tooltip("The minimum value for end to be considered active")]
    public float MinimalEnd = 100f;
    [Tooltip("The maximum value end can reach (to avoid infinites)")]
    public float MaximalEnd = 200f;
    [Space(10f)]
    [Tooltip("The minimum value for pain to be considered active")]
    public float MinimalPain = 100f;
    [Tooltip("The maximum value pain can reach (to avoid infinites)")]
    public float MaximalPain = 200f;

    [Header("Attack Frequency Properties")]
    [Tooltip("The timer which will attack when reaching upper value")]
    public float AttackTimer = 0f;
    [Tooltip("The upper value of the attack timer")]
    public float AttackThreshold = 150f;
    [Tooltip("The highest the value can reach")]
    public float AttackLimit = 170f;
    [Tooltip("Base Speed with which the monster will always attempt to attack")]
    public float BaseAttackUrge = 0.1f;
    [Tooltip("Minimum Speed with which the monster will attempt to attack")]
    public float MinAttackUrge = 0f;
    [Tooltip("Maximum Speed with which the monster will attempt to attack")]
    public float MaxAttackUrge = 300f;
    [Tooltip("Speed with which it will reach upper limit depending on emotions (joy and rage increase, others decrease)")]
    public float AttackUrge = 0f;
    [Tooltip("How much being offensive is increased in monster when close to the item that let's it make noise")]
    public float NoiseObjectUrge = 0f;

    public enum CreatureState
    {
        Joyous, //certain animations are mildly sped up
        Angry, //Obligates an Attack or noise when in-range
        Depressed, //Slows down certain animations tremendously
        Peaceful, //Default state
        Intimidated, //Forces a retreating action of some kind
        Bored, //Decreases chances of aggressive actions and movement
        Stunned //Unable to move temporarily and needs a moment to recuperate
    }

    [Header("Creature States and Actions")]
    public CreatureState state = CreatureState.Peaceful;

    private TurnToLook vision;
    private Patrol patrol;
    // Use this for initialization
    void Awake ()
    {
        vision = this.GetComponentInChildren<TurnToLook>();
        patrol = this.GetComponent<Patrol>();
    }
	
	// Update is called once per frame
	void Update () {
        checkEmotionalStates();
        checkForTarget();
        emotionalStabilityIncrease();
        setAttackProperties();
	}

    private void setAttackProperties()
    {
        float noise = 0f;
        if (patrol.CloseToNoisePoint)
            noise = NoiseObjectUrge;
        AttackUrge = Mathf.Clamp(BaseAttackUrge + Joy + Rage - Sorrow - Fear - End + noise, 
            MinAttackUrge, MaxAttackUrge);
        AttackTimer += Time.deltaTime * AttackUrge;
        if(AttackTimer > AttackLimit || (AttackTimer >= AttackThreshold && AttackUrge == 0))
        {
            AttackTimer = 0f;
        }
    }

    void OnCollisionEnter(Collision coll)
    {
        //Debug.Log("I got hit");
        //if(coll.gameObject.tag == "Needle")
        //{
        //    //if a parry add to Fear and Sorrow
        //    //else
        //    Pain += Sensitivity;
        //    Fear += Cowardice;
        //    End -= Apathy;
        //}
    }

    void checkForTarget()
    {
        if (vision.InSight)
        {
            RemoveEnd();
        }
    }

    public void gotHit()
    {
        //Pain += Sensitivity;
        //Fear += Cowardice;
        //End -= Apathy;
        AddPain();
        AddFear();
        RemoveEnd();
    }

    public void AddJoy()
    {
        Joy += Optimism;
    }

    public void AddRage()
    {
        Rage += Temper;
    }

    public void AddSorrow()
    {
        Sorrow += Pessimism;
    }

    public void AddFear()
    {
        Fear += Cowardice;
    }

    public void AddPain()
    {
        Pain += Sensitivity;
    }

    public void RemoveEnd()
    {
        End -= Apathy;
    }

    void checkEmotionalStates()
    {   // The main threshold is just 1, and any emotion that reaches 
        // that threshold and is the biggest number will activate the state
        bool isJoyous = Joy >= MinimalJoy;
        bool isAngry = Rage >= MinimalRage;
        bool isSad = Sorrow >= MinimalSorrow;
        bool isScared = Fear >= MinimalFear;
        bool isBored = End >= MinimalEnd;
        bool isStunned = Pain >= MinimalPain;

        if (isJoyous && isBiggestEmotion(0))
            state = CreatureState.Joyous;
        else if (isAngry && isBiggestEmotion(1))
            state = CreatureState.Angry;
        else if (isSad && isBiggestEmotion(2))
            state = CreatureState.Depressed;
        else if (isScared && isBiggestEmotion(3))
            state = CreatureState.Intimidated;
        else if (isBored && isBiggestEmotion(4))
            state = CreatureState.Bored;
        else if (isStunned && isBiggestEmotion(5))
            state = CreatureState.Stunned;
        else
            state = CreatureState.Peaceful;
    }

    bool isBiggestEmotion(int emotion)
    {   //Checks which emotion is the largest one currently available
        List<float> emotionTable = new List<float>() { Joy/MaximalJoy,
            Rage/MaximalRage,
            Sorrow/MaximalSorrow,
            Fear/MaximalFear,
            End/MaximalEnd,
            Pain/MaximalPain };
        float largestEmotion = emotionTable.Max<float>();
        int largestIndex = emotionTable.IndexOf(largestEmotion);
        return emotion == largestIndex;
    }

    void emotionalStabilityIncrease()
    {
        Joy -= Time.deltaTime * Deoptimism;
        Rage -= Time.deltaTime * Detemper;
        Sorrow -= Time.deltaTime * Depessimism;
        Fear -= Time.deltaTime * Decowardice;
        Pain -= Time.deltaTime * Desensitivity;
        End += Time.deltaTime * Deapathy;
        keepEmotionsPositive();
        keepEmotionsNearThreshold();
    }

    void keepEmotionsPositive()
    {
        Joy = (Joy < 0) ? 0 : Joy;
        Rage = (Rage < 0) ? 0 : Rage;
        Sorrow = (Sorrow < 0) ? 0 : Sorrow;
        Fear = (Fear < 0) ? 0 : Fear;
        Pain = (Pain < 0) ? 0 : Pain;
        End = (End < 0) ? 0 : End;
    }
    void keepEmotionsNearThreshold()
    {
        if (Joy > MaximalJoy)
            Joy = MaximalJoy;
        if (Rage > MaximalRage)
            Rage = MaximalRage;
        if (Sorrow > MaximalSorrow)
            Sorrow = MaximalSorrow;
        if (Fear > MaximalFear)
            Fear = MaximalFear;
        if (Pain > MaximalPain)
            Pain = MaximalPain;
        if (End > MaximalEnd)
            End = MaximalEnd;
    }
}
