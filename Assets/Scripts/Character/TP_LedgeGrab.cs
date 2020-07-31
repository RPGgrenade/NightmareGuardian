using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TP_LedgeGrab : MonoBehaviour {

    public LayerMask Layer;
    public float LedgeSnapHeight;
    public float LedgeSnapDistance;
    
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

	}

    private void OnTriggerStay(Collider coll)
    {
        bool isInAir = !TP_Controller.CharacterController.isGrounded;
        bool isLedge = ((1<<coll.gameObject.layer) & Layer) != 0;
        if (isInAir && isLedge)
        {
            snapToLedge(coll.transform);
        }
    }

    private void snapToLedge(Transform ledge)
    {
        transform.rotation = Quaternion.LookRotation(ledge.forward); //rotate towards ledge
        float ledgeHeight = ledge.position.y +  LedgeSnapHeight;
        transform.position = new Vector3(transform.position.x, ledgeHeight, transform.position.z);
        TP_Motor.Instance.gravity = false;
        TP_Motor.Instance.AerialMoveVector = Vector3.zero;
        TP_Animator.Instance.MoveAction = TP_Animator.MovingAction.LedgeGrab;
    }
}
