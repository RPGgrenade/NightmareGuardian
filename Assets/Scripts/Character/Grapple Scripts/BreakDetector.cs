using UnityEngine;
using System.Collections.Generic;

public class BreakDetector : MonoBehaviour
{
    void OnJointBreak(float breakForce) 
    {
        //Debug.Log("Anchor has been broken, Force: " + breakForce);
        //TP_Grapple.Instance.DestroyGrapple();
    }
}