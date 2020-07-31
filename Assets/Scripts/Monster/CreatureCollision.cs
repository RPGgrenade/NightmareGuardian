using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureCollision : MonoBehaviour {

    public GameObject AssociatedMusicTrack;
    public bool PlayerIsOn = false;
    public float ImpactSpeedThreshold = 10f;
    public float Stun;

    public int StunFrames = 0;

    private CreatureAnimator anim;
    private CreatureMind mind;
    private TurnToLook vision;
    private CreatureStats stats;
    private MusicHandler music;
    // Use this for initialization
    private int stunFrames = 5;

	void Awake () {
        mind = this.gameObject.GetComponent<CreatureMind>();
        vision = this.gameObject.GetComponentInChildren<TurnToLook>();
        stats = this.gameObject.GetComponent<CreatureStats>();
        anim = this.gameObject.GetComponent<CreatureAnimator>();
        if(AssociatedMusicTrack != null)
            music = AssociatedMusicTrack.GetComponent<MusicHandler>();
	}
	
	// Update is called once per frame
	void Update () {
        PlayerIsOn = vision.DistanceFromTarget <= vision.MinDistanceFromTarget;
        //PlayerIsOn = TP_Motor.Instance.transform.parent != null;
        if(music != null)
            music.OnMonster = PlayerIsOn;
        if(this.GetComponentInChildren<EyeStats>().Damage <= 0)
            reduceStun();
	}

    void reduceStun()
    {
        if (StunFrames > 0)
            StunFrames -= 1; //possibly make it into the speed of pain/parry animation TODO
        else
            anim.stab = false;
    }

    public void ApplyDamage(float damage, float stun = 0f)
    {
        mind.AddSorrow();
        mind.gotHit();
        Stun = stun;
        StunFrames = stunFrames;
        anim.stab = true;
        if (!vision.InSight)
        {
            vision.Interested = true;
            vision.SetTargetedPosition(vision.TargetToKill.position);
            vision.searchTimer = vision.SearchTime;
        }
    }

    //void OnCollisionEnter(Collision collision)
    //{
        //if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Toys"))
        //{
        //    Vector3 speed = collision.relativeVelocity;
        //    Vector3 impactDirection = speed.normalized;
        //    float velocity = speed.magnitude;
        //    if (velocity > ImpactSpeedThreshold)
        //    {
        //        ApplyDamage(0, 20f);
        //    }
        //}
    //}

    //void OnCollisionStay(Collision collision)
    //{

    //}
    
    //void OnCollisionExit(Collision collision)
    //{

    //}
}
