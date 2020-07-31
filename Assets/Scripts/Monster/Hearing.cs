using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Hearing : MonoBehaviour
{
    public GameObject AssociatedMusictrack;
    public float Noise = 0f;
    public float NoiseThreshold = 10f;
    public bool WithinRange = false;
    public bool Aware = false;

    public MusicHandler Music;
    //public static Hearing This;

    private TurnToLook vision;

    void Start()
    {
        if(AssociatedMusictrack != null)
            Music = AssociatedMusictrack.GetComponent<MusicHandler>();
        //This = this;
        vision = this.GetComponentInParent<TurnToLook>();
    }
    void Update() 
    {
        //if((Noise >= NoiseThreshold) && WithinRange)
        //    Aware = true;
        //if(Aware)
        //{
        //    //Do a thing.
        //}
    }
    void OnTriggerEnter(Collider col) 
    {
        if(col.GetComponent<Collider>().tag == "Player")
        {
            if(Music != null)
                Music.NearMonster = true;
            WithinRange = true;
            AimChanger.Instance.SetAim(AimChanger.Aim.Monster);
            AimChanger.Instance.GiveMonsterTarget(this.transform);
        }
        else if(col.transform.tag != "Monster" // A sound has been detected within range
            && col.gameObject.layer == LayerMask.NameToLayer("Sound"))
        {
            SonarManager son = col.gameObject.GetComponent<SonarManager>();
            Noise = son.MaxSize;
            if (Noise >= NoiseThreshold)
            {
                if (!vision.InSight)
                {
                    vision.Interested = true;
                    vision.SetTargetedPosition(col.transform.position);
                    vision.stats.anim.anim.SetTrigger("Alerted");
                }
            }
        }
    }

    void OnTriggerStay(Collider col)
    {
        if (col.GetComponent<Collider>().tag == "Player")
        {
            if (Music != null)
                Music.NearMonster = true;
            WithinRange = true;
            if(!TP_Controller.Instance.paused && TP_Animator.Instance.action != TP_Animator.Action.Lasso )
                AimChanger.Instance.SetAim(AimChanger.Aim.Monster);
            //AimChanger.Instance.GiveMonsterTarget(this.transform);
        }
    }

    void OnTriggerExit(Collider col) 
    {
        if (col.GetComponent<Collider>().tag == "Player")
        {
            if (Music != null)
                Music.NearMonster = false;
            WithinRange = false;
            AimChanger.Instance.SetAim(AimChanger.Aim.Default);
            AimChanger.Instance.RemovesMonsterTarget();
        }
    }
}
