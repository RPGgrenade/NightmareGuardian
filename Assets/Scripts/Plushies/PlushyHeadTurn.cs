using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlushyHeadTurn : MonoBehaviour
{
    public PlushyAnimator Anim;
    [Header("Head Motion Properties")]
    public Transform Target;
    [Tooltip("Sometimes necessary reference to the foot position of the plushy")]
    public Transform FootPosition;
    [Tooltip("Speed with which head will turn towards target")]
    public int TurnSpeed = 5;
    [Tooltip("The offset for the head to focus on (in case of bad head orientation while modeling, hopefully rarely used)")]
    public Vector3 LookRotationOffset = Vector3.zero;
    [Tooltip("The offset for the head to face when deactivated (Hopefully rarely used)")]
    public Vector3 InactiveRotationOffset = Vector3.zero;
    [Tooltip("The offset for the head to face when activated but unable to see player (Hopefully rarely used)")]
    public Vector3 ActiveRotationOffset = Vector3.zero;
    [Tooltip("The rotation limit of the head twisting x")]
    [Range(0f, 180f)]
    public float HeadRotationLimitX = 0f;
    [Tooltip("The rotation limit of the head twisting y")]
    [Range(0f, 180f)]
    public float HeadRotationLimitY = 0f;
    public bool IsLooking = false;

    public float NeckAngleX;
    public float NeckAngleY;

    // Start is called before the first frame update
    void Start()
    {
        if(Anim == null)
            Anim = this.transform.root.GetComponent<PlushyAnimator>();
        if (Target == null)
            Target = GameObject.FindWithTag("Player").transform.root;
        if (InactiveRotationOffset == Vector3.zero)
            InactiveRotationOffset = this.transform.rotation.eulerAngles;
        //if (ActiveRotationOffset == Vector3.zero)
        //    ActiveRotationOffset = this.transform.rotation.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Anim.NearMonster && Anim.NearTeddy)
            HeadTurn();
        else
            rotateHead(this.transform.root.forward, InactiveRotationOffset);
    }

    void HeadTurn()
    {
        //Getting important vectors
        Vector3 localRotationOffset = Vector3.zero;

        Vector3 headTargetVector = Target.position - transform.position;
        Vector3 bodyTargetVector = Vector3.zero;
        if (FootPosition != null)
            bodyTargetVector = Target.position - FootPosition.position;
        Vector3 bodyForward = Anim.transform.forward;

        //Creating 2D versions of the vectors
        Vector2 bodyForwardX = new Vector2(bodyForward.x, bodyForward.z);

        Vector2 targetVectorX = new Vector2(headTargetVector.x, headTargetVector.z);
        Vector2 targetVectorY = new Vector2(headTargetVector.y, headTargetVector.z);

        //Comparing the angles of twisting
        float neckAngleX = Vector2.Angle(bodyForwardX, targetVectorX);

        float neckAngleY;
        if(FootPosition == null)
            neckAngleY = Mathf.Abs(Vector3.SignedAngle(bodyForward, headTargetVector, Anim.transform.right));
        else
            neckAngleY = Mathf.Abs(Vector3.SignedAngle(bodyTargetVector, headTargetVector, Anim.transform.right));
        
        NeckAngleX = neckAngleX;
        NeckAngleY = neckAngleY;
        
        bool isWithinHeadRanges = neckAngleX < HeadRotationLimitX && neckAngleY < HeadRotationLimitY;

        IsLooking = isWithinHeadRanges;
        if (isWithinHeadRanges) 
            rotateHead(headTargetVector, LookRotationOffset);
        else
            rotateHead(bodyForward, ActiveRotationOffset);
    }

    private void rotateHead(Vector3 targetVector, Vector3 rotationOffset)
    {
        float timeStep = TurnSpeed * Time.deltaTime;
        var targetRotation = Quaternion.LookRotation(targetVector);

        Quaternion extraRotation = Quaternion.Euler(rotationOffset);

        var finalRotation = Quaternion.Euler(targetRotation.eulerAngles + extraRotation.eulerAngles);
        transform.rotation = Quaternion.Slerp(transform.rotation, finalRotation, timeStep);
    }
}
