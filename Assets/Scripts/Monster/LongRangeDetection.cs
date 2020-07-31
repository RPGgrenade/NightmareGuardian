using UnityEngine;
using System.Collections;

public class LongRangeDetection : MonoBehaviour {

    public float ViewDistance;
    public LayerMask LayersToHit;
    public bool Alive = true;

    public TurnToLook vision;

    private void Awake()
    {
        if(vision == null)
            vision = this.GetComponentInParent<TurnToLook>();
    }

    void Update() 
    {
        if (!Alive)
        {
            vision.InRange = false;
            vision.InSight = false;
        }
    }
    void OnTriggerStay(Collider collider) 
    {
        if(collider.tag == "Player" && Alive)// && !IsObstructed(collider))
        {
            vision.InRange = true;
            //AimChanger.Instance.SetAim(AimChanger.Aim.Monster);
        }
    }
    void OnTriggerExit(Collider collider) 
    {
        if(collider.tag == "Player" && Alive)
        {
            vision.InRange = false;
            vision.InSight = false;
        }
    }

    private bool IsObstructed(Collider coll)
    {
        Ray raycast = new Ray(this.transform.position, coll.transform.position);
        RaycastHit hit;
        Debug.DrawLine(this.transform.position,coll.transform.position, Color.white);
        bool result = Physics.Raycast(raycast, out hit, LayersToHit);
        //Debug.Log(hit.collider.name);
        return result;
    }
}
