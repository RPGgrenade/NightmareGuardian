using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Headband : MonoBehaviour
{
    public Cloth Flap;
    
    // Start is called before the first frame update
    void Start()
    {
        CapsuleCollider neckCollider;
        CapsuleCollider chestCollider;

        neckCollider = this.transform.root.Find("Neck").GetComponent<CapsuleCollider>();
        chestCollider = this.transform.root.Find("Chest").GetComponent<CapsuleCollider>();

        Flap.capsuleColliders = new CapsuleCollider[2] {neckCollider, chestCollider};
    }
}
