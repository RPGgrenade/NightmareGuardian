using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GrabProperties : MonoBehaviour {

    public bool Thrown = false;
    //public float DistanceFromCenter = 1f;
    public Vector3 GrabOffset;
    public Vector3 GrabRotation;

    public List<Collider> Collisions = new List<Collider>();

    void Start()
    {
        if(Collisions.Count == 0)
        {
            Collider[] colliders = this.GetComponents<Collider>();
            Collisions = colliders.ToList();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag != "Player" && collision.gameObject.layer != LayerMask.NameToLayer("Player") 
            && collision.gameObject.layer != LayerMask.NameToLayer("Monster"))
            Thrown = false;
    }

    public void ActivateCollision(bool activate)
    {
        foreach (Collider col in Collisions)
        {
            col.isTrigger = !activate;
        }
    }
}
