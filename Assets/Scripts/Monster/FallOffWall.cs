using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FallOffWall : MonoBehaviour
{
    [Header("Trackers")]
    public Patrol Patroller;
    public NavMeshAgent Agent;
    public Animator Anim;

    [Header("Falling Properties")]
    public bool IsFalling = false;
    public bool OnGround = false;
    public float FallSpeed = 1f;
    public float FallAcceleration = 1f;
    public Transform ReferencePoint;

    [Header("Ground Checking Properties")]
    public LayerMask GroundLayers;
    public float GroundCheckDistance = 1f;
    public float ImpactDelay = 0.5f;

    private float fallSpeed = 0f;
    // Start is called before the first frame update
    void Start()
    {
        if (Patroller == null)
            Patroller = this.GetComponent<Patrol>();
        if (Agent == null)
            Agent = this.GetComponent<NavMeshAgent>();
        if (Anim == null)
            Anim = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (IsFalling && !OnGround)
        {
            Patroller.enabled = false;
            Agent.autoTraverseOffMeshLink = false;
            OnGround = CheckForGround();
            if (!OnGround)
                Fall();
            else
                Invoke("delayedImpact", ImpactDelay);
        }
    }

    public void SetToFalling()
    {
        IsFalling = true;
    }

    private void delayedImpact()
    {
        Anim.SetBool("Impact", true);
    }
    
    public bool CheckForGround()
    {
        Debug.DrawLine(ReferencePoint.position,
            ReferencePoint.position - this.transform.up * GroundCheckDistance, Color.black);

        return Physics.Linecast(ReferencePoint.position,
            ReferencePoint.position - this.transform.up * GroundCheckDistance, 
            GroundLayers);
    }

    public void MoveForward(int forward)
    {
        Vector3 moveVector = this.transform.forward;
        if (forward == 0)
            moveVector *= -1;
        Agent.Move(moveVector * FallSpeed);
    }

    public void Fall()
    {
        fallSpeed = Mathf.Clamp(fallSpeed + (Time.deltaTime * FallAcceleration), 
            0f, FallSpeed);
        Vector3 fallVector = ((-this.transform.up) * fallSpeed);
        Agent.Move(fallVector);
    }

    public void SetCheckDistance(float distance)
    {
        GroundCheckDistance = distance;
    }
}
