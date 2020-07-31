using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHeadTurn : MonoBehaviour
{
    public enum TargetType
    {
        Monster,
        Plushy,
        Toy,
        Child,
        None
    }

    [Header("Head Motion Properties")]
    public Transform Target;
    public TargetType Type = TargetType.None;
    public Transform FootPosition;
    public bool CanTurnHead = true;
    [Tooltip("Speed with which head will turn towards target")]
    public int TurnSpeed = 5;
    [Tooltip("The offset for the head to focus on (in case of bad head orientation while modeling, hopefully rarely used)")]
    public Vector3 RotationOffset = Vector3.zero;
    public Vector3 OriginalRotation = Vector3.zero;
    public Vector3 ActiveOriginalRotation = Vector3.zero;
    [Tooltip("The rotation limit of the head twisting x")]
    [Range(0f, 180f)]
    public float HeadRotationLimitX = 0f;
    [Tooltip("The rotation limit of the head twisting y")]
    [Range(0f, 180f)]
    public float HeadRotationLimitY = 0f;
    [Header("Trackables Properties")]
    public float TrackableRadius = 3f;
    public float UpdateRate = 0.7f;
    public LayerMask LayersToLookAt;
    public bool IsLooking = false;

    public float NeckAngleX;
    public float NeckAngleY;

    public Transform thisRoot;
    private Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        if(thisRoot == null)
            thisRoot = this.transform.root;
        anim = this.transform.GetComponentInParent<Animator>();
        if (OriginalRotation == Vector3.zero)
            OriginalRotation = this.transform.rotation.eulerAngles;
        InvokeRepeating("updateTarget", UpdateRate, UpdateRate);
        //if (ActiveOriginalRotation == Vector3.zero)
        //    ActiveOriginalRotation = this.transform.rotation.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        if (!TP_Animator.Instance.TeddyMode && CanTurnHead && !isPlayingAnAnimation())
        {
            if (Target != null)
                HeadTurn();
            else
                rotateHead(thisRoot.forward - ActiveOriginalRotation);
        }
    }

    void FixedUpdate()
    {
        CanTurnHead = TP_Controller.CharacterController.enabled;
    }

    private bool isPlayingAnAnimation()
    {
        return (anim.GetCurrentAnimatorStateInfo(4).IsName("Teddy_Cover"));
    }

    private void updateTarget()
    {
        Target = null;
        Type = TargetType.None;
        Collider[] thingsNearby = Physics.OverlapSphere(this.transform.position, TrackableRadius, LayersToLookAt);
        if (thingsNearby.Length > 0)
        {
            Transform closestTarget = null;
            foreach (Collider col in thingsNearby)
            {
                Transform target = null;
                if (col.transform.gameObject.layer == LayerMask.NameToLayer("Monster"))
                {
                    Type = TargetType.Monster;
                    if (col.transform.root.GetComponentInChildren<TurnToLook>() != null)
                        target = col.transform.root.GetComponentInChildren<TurnToLook>().transform;
                    else
                        target = col.transform;
                }
                else if (col.transform.tag == "Male" || col.transform.tag == "Female")
                {
                    Type = TargetType.Child;
                    target = col.transform.root.GetComponentInChildren<AudioSource>().transform;
                }

                else if (col.transform.root.tag == "Plush")
                {
                    Type = TargetType.Plushy;
                    target = col.transform.GetComponentInParent<PlushyAnimator>().transform.
                        GetComponentInChildren<PlushyHeadTurn>().transform;
                }
                else if (col.transform.gameObject.layer == LayerMask.NameToLayer("Toys") &&
                    col.transform.GetComponentInParent<GrabProperties>())
                {
                    if ((TP_Motor.Instance.GrabbedItem != null && TP_Motor.Instance.GrabbedItem != col.transform) ||
                        (TP_Motor.Instance.GrabbedItem == null))
                    {
                        Type = TargetType.Toy;
                        target = col.transform.GetComponentInParent<GrabProperties>().transform;
                    }
                }

                if (closestTarget == null)
                    closestTarget = target;
                else if(closestTarget != null && target != null)
                {
                    if (closestTarget != target)
                    {
                        float oldTargetDistance = Vector3.Distance(this.transform.position, closestTarget.position);
                        float newTargetDistance = Vector3.Distance(this.transform.position, target.position);
                        if (newTargetDistance < oldTargetDistance)
                            closestTarget = target;
                    }
                }
            }
            Target = closestTarget;
        }
    }

    void HeadTurn()
    {
        //Getting important vectors
        Vector3 localRotationOffset = Vector3.zero;

        Vector3 targetVector = Target.position - transform.position - RotationOffset;
        Vector3 bodyTargetVector = Vector3.zero;
        if (FootPosition != null)
        {
            Vector3 TargetAtFootHeight = new Vector3(Target.position.x,
                FootPosition.position.y,
                Target.position.z);
            bodyTargetVector = TargetAtFootHeight - FootPosition.position;
            //if (Type == TargetType.Monster)
            //{
            //    Vector3 MonsterAtFootHeight = new Vector3(Target.root.position.x, 
            //        FootPosition.position.y, 
            //        Target.root.position.z);
            //    bodyTargetVector = MonsterAtFootHeight - FootPosition.position;
            //}
            //else if (Type == TargetType.Plushy)
            //    bodyTargetVector = Target.GetComponentInParent<PlushyAnimator>().transform.position - FootPosition.position;
            //else
            //    bodyTargetVector = Target.position - FootPosition.position;
        }
        Vector3 bodyForward = thisRoot.forward;

        //Creating 2D versions of the vectors
        Vector2 bodyForwardX = new Vector2(bodyForward.x, bodyForward.z);

        Vector2 targetVectorX = new Vector2(targetVector.x, targetVector.z);
        Vector2 targetVectorY = new Vector2(targetVector.y, targetVector.z);

        //Comparing the angles of twisting
        float neckAngleX = Vector2.Angle(bodyForwardX, targetVectorX);
        
        float neckAngleY;
        if (FootPosition == null)
            neckAngleY = Mathf.Abs(Vector3.SignedAngle(bodyForward, targetVector, thisRoot.right));
        else
            neckAngleY = Mathf.Abs(Vector3.SignedAngle(bodyTargetVector, targetVector, thisRoot.right));

        NeckAngleX = neckAngleX;
        NeckAngleY = neckAngleY;

        bool isWithinHeadRanges = neckAngleX < HeadRotationLimitX && neckAngleY < HeadRotationLimitY;

        IsLooking = isWithinHeadRanges;
        if (isWithinHeadRanges) 
            rotateHead(targetVector);
        else
            rotateHead(bodyForward - ActiveOriginalRotation);
    }

    private void rotateHead(Vector3 targetVector)
    {
        float timeStep = TurnSpeed * Time.deltaTime;
        var targetRotation = Quaternion.LookRotation(targetVector,thisRoot.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, timeStep);
    }
}
