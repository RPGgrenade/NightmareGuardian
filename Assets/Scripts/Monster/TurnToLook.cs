using UnityEngine;
using System.Collections;
using System;

public class TurnToLook : MonoBehaviour 
{
    enum HeadState
    {
        animated,    //Mecanim is fully in control
        following,   //Mecanim turned off, following target
        blendToAnim  //Mecanim in control, but LateUpdate() is used to partially blend in the last head pose
    }
    //public static TurnToLook Instance;
    [Header("Enviroment Trackers")]
    [Tooltip("The music associated with this monster")]
    public GameObject AssociatedMusicTrack;
    [Tooltip("Target to track (the player)")]
    public Transform TargetToKill;
    [Tooltip("Position offset to place the head for proper orientation")]
    public Transform HeadTarget;
    [Tooltip("Layers that can block direct line of sight")]
    public LayerMask layers;
    [Tooltip("Says whether or not the monster will auto track when the target is close enough")]
    public bool MonsterTracksWhenClose = false;
    [Tooltip("The percentage below which their life bar needs to be to apply the close up turn")]
    public float LifePercentageToTrackClose = 0.65f;
    [Tooltip("Distance player has to be within for the auto tracking to activate")]
    public float MinDistanceFromTarget = 0.8f;
    [Tooltip("Current distance from the target")]
    public float DistanceFromTarget = 1f;
    [Tooltip("Time monster is willing to search for the player")]
    public float SearchTime = 10f;
    public float searchTimer = 0f;
    public float BodyFacingTarget = 0f;

    [Space(5)]
    [Header("Detection States")]
    [Tooltip("Determines if monster can turn head (can be disabled to make specific animations that require it)")]
    public bool CanTurnHead = true;
    [Tooltip("Target is within cone of vision")]
    public bool InRange = false;
    [Tooltip("Target is within direct line of sight")]
    public bool InSight = false;
    [Tooltip("Monster is interested in something nearby(usually a sound)")]
    public bool Interested = false;

    [Space(5)]
    [Header("Head Motion Properties")]
    [Tooltip("The offset for the head to focus on (in case of bad head orientation while modeling, hopefully rarely used)")]
    public Vector3 RotationOffset = Vector3.zero;
    [Tooltip("The multiplier to decrease the close offset")]
    public Vector3 CloseRotationOffset;
    [Tooltip("Speed with which head will turn towards target")]
    public float TurnSpeed = 5;
    [Tooltip("Offset of the head for better positioning of the head")]
    public float forwardOffset = 5f;
    //public float CheckingInterval = 0.4f;
    [Tooltip("The rotation limit of the head twisting x")]
    [Range(0f,180f)]
    public float HeadRotationLimitX = 0f;
    [Tooltip("The rotation limit of the head twisting y")]
    [Range(0f, 180f)]
    public float HeadRotationLimitY = 0f;
    [Tooltip("The rotation limit of the head twisting x when in very close proximity")]
    [Range(0f, 180f)]
    public float CloseHeadRotationLimitX = 0f;
    [Tooltip("The rotation limit of the head twisting y when in very close proximity")]
    [Range(0f, 180f)]
    public float CloseHeadRotationLimitY = 0f;

    //private float checkingTimer = 0f;
    private MusicHandler music;
    public Vector3 targetPosition;
    public float NeckAngleY;
    public float NeckAngleX;
    private Animator anim;
    public CreatureStats stats;
    //blending variables
    HeadState state = HeadState.animated;
    Quaternion storedRotation;
    float transferEndTime = -100;
    float mecanimToHeadTransitionTime = 0.05f;
    public float followToMecanimBlendTime = 0.5f;

    void Awake() //all of this is getting confusing.
    {
        //Instance = this;
        //targetPosition = TargetToKill.position;
        anim = this.GetComponent<Animator>();
        stats = this.GetComponentInParent<CreatureStats>();
        if(AssociatedMusicTrack != null)
            music = AssociatedMusicTrack.GetComponent<MusicHandler>();
    }

    private void Start()
    {
        if(TargetToKill == null)
            TargetToKill = GameObject.FindWithTag("Player").transform.root;
    }

    void Update () 
    {
        if(music != null)
            music.SeenByMonster = InSight;
        checkHeadAnimator();
        if(HeadTarget != null)
            updateHeadPosition();
        updateDistanceFromTarget();
        if(searchTimer > 0f)
            searchTimer -= Time.deltaTime;

        if (InRange)// || Interested) // for some reason the InSight bool is resetting.
        {
            sightUpdate();
            if (InSight || (searchTimer > 0f))
                HeadTurn();
        }
        else if (!InRange && !InSight && searchTimer > 0f)
        {
            HeadTurn();
            Interested = true;
        }
        updateStateTrackers();
        updateFacingTarget();
	}

    void LateUpdate()
    {
        if (state == HeadState.blendToAnim)
        {
            float blendAmount = 1.0f - (Time.time - transferEndTime - mecanimToHeadTransitionTime) / followToMecanimBlendTime;
            blendAmount = Mathf.Clamp01(blendAmount);
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, storedRotation, blendAmount);
            if(blendAmount == 0)
            {
                state = HeadState.animated;
                return;
            }
        }
        if (searchTimer > SearchTime)
            searchTimer = SearchTime;
    }

    private void updateFacingTarget()
    {
        if(TargetToKill != null)
        {
            Vector3 directionBetweenMonsterAndPlayer = (TargetToKill.position - this.transform.root.position).normalized;
            Vector3 directionMonsterIsFacing = this.transform.root.forward;

            BodyFacingTarget = Vector3.Dot(directionMonsterIsFacing, directionBetweenMonsterAndPlayer);
        }
    }

    private void updateStateTrackers()
    {
        if(state == HeadState.animated && !anim.enabled)
        {
            state = HeadState.following;
        }
        else if (state == HeadState.following && anim.enabled)
        {
            transferEndTime = Time.time;
            state = HeadState.blendToAnim;
            storedRotation = transform.rotation;
        }
    }

    private void updateDistanceFromTarget()
    {
        DistanceFromTarget = Vector3.Distance(TargetToKill.position, this.transform.position);
        if (MonsterTracksWhenClose)
        {
            if (DistanceFromTarget < MinDistanceFromTarget && stats.GetHealthPercentage() <= LifePercentageToTrackClose)// && (!InRange || !InSight))
            {
                anim.enabled = false;
                SetTargetedPosition(TargetToKill.position);
                HeadTurn(true);
            }
        }
    }

    void checkHeadAnimator()
    {
        //bool beforeAnim = anim.enabled;
        anim.enabled = !InSight && searchTimer <= 0f;
        //bool afterAnim = anim.enabled;
        //if (beforeAnim && !afterAnim)
          //  stats.anim.anim.SetTrigger("Alerted");
    }

    private void sightUpdate()
    {
        if (InSight)
        {
            searchTimer = SearchTime;
            Interested = false;
            SetTargetedPosition(TargetToKill.position);
            //Debug.Log("Target Position is: " + targetPosition);
            SetLineOfSight();
        }
        else
            SetLineOfSight();
    }

    private void updateHeadPosition()
    {
        Vector3 fwd = transform.TransformDirection(Vector3.forward);
        this.transform.position = HeadTarget.transform.position + (fwd*forwardOffset);
    }

    void HeadTurn(bool IsClose = false)
    {
        if (CanTurnHead)
        {
            //Getting important vectors
            Vector3 localRotationOffset = Vector3.zero;
            if (!IsClose)
                localRotationOffset = RotationOffset;//transform.root.InverseTransformVector(RotationOffset);
            else
                localRotationOffset = CloseRotationOffset;//transform.root.InverseTransformVector(CloseRotationOffset);

            Vector3 bodyForward = transform.root.forward;
            Vector3 nonOffsetTargetVector = (targetPosition - transform.position);
            Vector3 targetVector = nonOffsetTargetVector;// + localRotationOffset;

            Debug.DrawLine(transform.root.position, transform.root.position + bodyForward * 30, Color.red);
            Debug.DrawLine(transform.position, transform.position + nonOffsetTargetVector, Color.yellow);

            //Creating 2D versions of the vectors

            Vector3 bodyForwardY = new Vector3(bodyForward.x, 0f, bodyForward.z);
            Vector3 targetVectorY = new Vector3(nonOffsetTargetVector.x, 0f, nonOffsetTargetVector.z);
            float neckAngleY = Vector3.Angle(bodyForwardY, targetVectorY);

            float neckAngleX = Mathf.Abs(Vector3.SignedAngle(bodyForward, nonOffsetTargetVector, transform.root.right));
            neckAngleX -= Mathf.Pow(neckAngleY, (2f / 3f)); //May not be perfect, but it works for intents and purposes it seems

            NeckAngleY = neckAngleY;
            NeckAngleX = neckAngleX;

            bool isWithinHeadRanges = false;
            if (!IsClose)
                isWithinHeadRanges = neckAngleX < HeadRotationLimitX && neckAngleY < HeadRotationLimitY;
            else
                isWithinHeadRanges = neckAngleX < CloseHeadRotationLimitX && neckAngleY < CloseHeadRotationLimitY;

            if (isWithinHeadRanges)
                rotateHead(targetVector, localRotationOffset);
        }
    }

    //private float angleBetweenVectors2D(Vector2 u1, Vector2 u2)
    //{
    //    float sqrtU1 = Mathf.Sqrt(Mathf.Pow(u1.x, 2) + Mathf.Pow(u1.y, 2));
    //    float sqrtU2 = Mathf.Sqrt(Mathf.Pow(u2.x, 2) + Mathf.Pow(u2.y, 2));
    //    float angle = Mathf.Acos((u1.x * u1.y + u2.x * u2.y)/(sqrtU1 * sqrtU2));
    //    return angle * 180 / Mathf.PI;
    //}

    private void rotateHead(Vector3 targetVector, Vector3 rotationOffset)
    {
        //May possibly need to add in z axis retention for visual effects on some monsters in the future (in case bodies are distorted in strange angles along with the head)
        
        float timeStep = TurnSpeed * Time.deltaTime;
        var targetRotation = Quaternion.LookRotation(targetVector);
        var extraRotation = Quaternion.Euler(rotationOffset);
        var finalRotation = Quaternion.Euler(targetRotation.eulerAngles + extraRotation.eulerAngles);
        transform.rotation = Quaternion.Slerp(transform.rotation, finalRotation, timeStep);
    }

    void SetLineOfSight() 
    {
        RaycastHit hitInfo;
        if (!Physics.Linecast(transform.position, TargetToKill.position, out hitInfo, layers)) //Checks to make sure the line of sight is unobstructed
        {
            if (!TP_Animator.Instance.TeddyMode)
                InSight = true;
            //Debug.DrawLine(transform.position, targetPosition, Color.red);
        }
        else
        {
            InSight = false;
            //Debug.DrawLine(transform.position, targetPosition, Color.yellow);
        }
    }

    public Vector3 GetTargetedPosition()
    {
        return targetPosition;
    }

    public void SetTargetedPosition(Vector3 position)
    {
        targetPosition = position;
    }
}
