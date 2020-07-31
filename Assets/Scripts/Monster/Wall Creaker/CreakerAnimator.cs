using UnityEngine;
using System.Collections;
using System;

public class CreakerAnimator : MonoBehaviour {

    private bool isWalking;
    private bool isRotating;
    private bool isRotatingRight;
    private bool isPushing;
    private bool isOffensive;
    private bool isMoving;
    private int position = 1;

    private Rigidbody body;
    private Animator anim;

    void Start ()
    {
        body = this.GetComponentInParent<Rigidbody>();
        anim = this.GetComponent<Animator>();
	}
	
	void Update ()
    {
        UpdateValues();
        UpdateStates();
	}

    private void UpdateValues()
    {
        isWalking = (CreatureAI.Instance.Action == CreatureAI.CreakerAction.Walking);
        isRotating = (CreatureAI.Instance.Action == CreatureAI.CreakerAction.Rotating);
        isPushing = (CreatureAI.Instance.Action == CreatureAI.CreakerAction.Pushing);
        isRotatingRight = CreatureMovement.Instance.RotatingRight;
        isOffensive = (CreatureAI.Instance.State == CreatureAI.CreakerState.Offensive);
    }

    private void UpdateStates()
    {
        anim.SetBool("Walking",isWalking);
        anim.SetBool("Rotating", isRotating);
        anim.SetBool("Pushing", isPushing);
        anim.SetBool("Rotating Right", isRotatingRight);
        anim.SetBool("Offensive", isOffensive);
        anim.SetBool("Forward", getFacing());
        anim.SetInteger("Position", getPosition());
    }

    private bool getFacing()
    {
        return (this.transform.rotation.y < 90 && this.transform.rotation.y > -90);
    }

    private int getPosition()
    {
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Nothing 1"))
            return 1;
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Nothing 2"))
            return 2;
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Nothing 3"))
            return 3;
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Nothing 4"))
            return 4;
        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Nothing 5"))
            return 5;
        return position;
    }

    public void Move()
    {
        CreatureMovement.Instance.Move();
    }

    public void Rotate()
    {
        CreatureMovement.Instance.Rotate();
    }
}
