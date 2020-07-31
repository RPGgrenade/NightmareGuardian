using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;

public class TP_Grapple : MonoBehaviour
{
    //This class will take care of things related to the grappling needle
    //Including activating the camera control during aiming
    //You have the option to not use the changing camera (will only grapple forward)
    public static TP_Grapple Instance;

    /*
     * There's going to be a ton of comments here due to how confusing this script will be.
     * Some random commentary will be here for anyone who may read this in the future if I get all this stuff to work properly
     * This script is most likely going to wind up being the most tweaked and complicated in the entire game, compensating for various different situations the testing will come up with.
     * That is the reason for the random commentary, simply because if I lose track of ANYTHING, I will get lost, and for the sake of if anyone wants to learn with my example.
    */
    [Header("Object References")]
    [Tooltip("Keeps track of the character that is to move along the motion")]
    public GameObject MainCharacter;
    [Tooltip("Teddy Needle to track for deactivating")]
    public GameObject Needle;
    [Tooltip("Teddy String to track for deactivating")]
    public GameObject HandString;
    [Tooltip("Keeps a reference to the string prefab object to allow for creation")]
    public GameObject String;
    [Tooltip("The created hand grip which applies the swinging physics")]
    public GameObject HandGrip; //The object that will be created as what will attach to the needle anchor, as a new object must be created due to how the character is created to work.
    [Tooltip("The created anchor which the string is connected to")]
    public GameObject Anchor; //The gameobject holding the character joint and the rigidbody that is necesary.

    [Header("Grapple Collision Properties")]
    [Tooltip("Character height for collision")]
    public float CharacterHeight = 1.0f;
    [Tooltip("Character width for collision")]
    public float CharacterWidth = 1.0f;
    [Tooltip("Character distance to check for floor")]
    public float FloorDetectionDistance = 1f;
    [Tooltip("Vertical offset for collision to be more accurate")]
    public float CollisionOffset = 0.3f;

    [Header("Grapple State Properties")]
    [Tooltip("Quantity of line segments on string (might not have a real effect)")]
    public int LineSegments = 0;
    [Tooltip("Shows us whether or not the grappling is occuring, this affects many properties, so it's good to keep track of it.")]
    public bool IsGrappling = false;
    [Tooltip("The frames taken to activate change in the grapple")]
    public int AnchorChangeTiming = 1;
    [Tooltip("The seconds it take to change grappling state from one state to another")]
    public float StateChangeTiming = 0.3f;
    [Tooltip("Not Implemented: amount of times one can bank around corners before grapple snaps")]
    public int BankingMaxQuantity = 4;

    [Header("Lasso Limits")]
    [Tooltip("The maximum length of the grappling after having attached to the object. As you can get closer to the point of attatchment if you wish, but not farther than the distance you grappled with")]
    public float MaxGrappleRadius = 0f;
    [Tooltip("The length of the grappling thread at each moment in time")]
    public float GrappleDistance = 0f; 
    [Tooltip("The maximum length that is possible for a grappling thread to hit")]
    public float MaxGrappleDistance = 10f; 
    [Tooltip("since the camera is as at different distance from the character's hand holding the thread, this is necesary as a reference  to how far the grapple can go before being used")]
    public float MaxDistanceFromCamera = 12f;

    [Header("Swinging Properties")]
    [Tooltip("The Layers to pay attention in order to land (prevents landing on certain things I'd rather not)")]
    public LayerMask LandingLayers;
    [Tooltip("The vector that tracks the swinging inputs")]
    public Vector3 SwingVector;
    [Tooltip("The speed of pulling that might be remove")]
    public float PullSpeed = 100f;
    [Tooltip("The speed of the swing that might be removed")]
    public float SwingSpeed = 20f;
    [Tooltip("The speed multiplier to prevent extreme speeds")]
    public float VelocityMultiplier = 0.5f;
    [Tooltip("Drag to slow down the swinging so that you don't swing up the same height no matter what, it prevents extra abusing of the grapple")]
    public float AerialDrag = 0.2f;
    [Tooltip("Mass affects the speed on the way down and in general while grappling, it helps to be able to control for testing purposes")]
    public float Mass = 20f; 
    [Tooltip("Magnitude of the motion velocity of the objects that will be in motion, mostly for physics. Is shown for testing purposes, later will be removed for use only when necesary")]
    public float Velocity = 0f; 
    [Tooltip("Length of the anchoring at the point an impact of the thread occurs that changes up the speed of the motion in that instant change")]
    public float Radius = 0f; 
    [Tooltip("The radius of the collider while singing")]
    public float ColliderRadius = 1f;
    [Tooltip("Holds the positioning of the hand that'll be holding the thread")]
    public Transform Hand; 
    [Tooltip("The Swing Twist limit for the grappling needle")]
    public float SwingAndTwistLimit = 180f;
    [Tooltip("The Speed at which the character catches up with the grip object (changes depending on distance)")]
    public float PositionUpdateSpeed = 1f;
    [Tooltip("Dampens the jump height vector added (may be speed dependant later)")]
    public float JumpDampen = 0.2f;
    [Tooltip("Not Implemented: Will cut the grapple once this velocity has been surpassed")]
    public float MaxPossibleVelocty = 200f;
    [Tooltip("The maximum vertical velocity on release")]
    public float MaxReleaseVelocity = 5f;

    [Header("Tugging properties")]
    [Tooltip("The layers of objects that can be tugged on")]
    public LayerMask TuggingLayers;
    [Tooltip("The strength with which the grappled items are tugged on")]
    public float TuggingForce = 10f;

    [Header("Audio Properties")]
    [Tooltip("The Sound Effects Mixer assigned when the sound happens")]
    public AudioMixerGroup Mixer;
    [Tooltip("The clip to play when grappling something")]
    public AudioClip GrappleClip;
    [Tooltip("Volume of grapple sound")]
    public float Volume = 1f;
    [Tooltip("Spatial blend of grapple sound")]
    [Range(0f,1f)]
    public float AudioBlend = 0.95f;

    [Header("Special effects")]
    [Tooltip("Particles to play when grappling something")]
    public GameObject Particles;

    [Header("Mesh Properties")]
    [Tooltip("Mesh to show attached to wall")]
    public Mesh NeedleMesh;
    [Tooltip("Material of mesh attached to wall")]
    public Material NeedleMaterial;
    [Tooltip("Offset of the position of the needle mesh")]
    public Vector3 PositionOffset;
    [Tooltip("Layers Needle Mesh needs to parent to directly")]
    public LayerMask RenderLayers;

    //This is just the start, I told you there was a lot more in this script... it's HARD to think of all the variables! (how did the Spiderman 2 (PS2) programmer do it? X_X)

    private float angularMomentum = 0f; //Angular momentum is the magnitude described to be used to predict a new speed magnitude if a thread collision occurs during a swing.
    private Vector3 needleAnchor; //the position where the needle anchor will be located during the swing before any collisions, if one occurs, this position will change.
    private CharacterJoint needleJoint;
    //private ConfigurableJoint needleJoint; //This is the new needle anchor component that will be created during a swing, this, as well as the handgrip will be deleted once the grapple is cancelled in one way or another

    private float cooldown;
    private Vector3 lastPosition;
    private Vector3 velocity;
    private LineRenderer Line; 
    private GameObject NewString;// The instance of the string prefab
    private GameObject NeedleRender; //Instance of the needle visuals
    private CharacterController controller;

    private int deactivationTiming = 0;
    private int reactivationTiming = 0;
    private float activationTimer = 0f;

    private bool IsTethered = false; //TODO: Might be removed
    private float Acceleration = 0f; //TODO: Might be removed
    private float RotationSpeed = 0.5f; //TODO: Might be removed

    private Vector3[] BankingPositions;
    private bool canChangeControllerState = true;
    private bool canJumpOnStateChange = true;
    private float timeToChangeStateEnabled = 0f;


    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        cooldown = 0;
        controller = TP_Controller.Instance.GetComponent<CharacterController>();
        //BankingPositions = new Vector3[BankingMaxQuantity];
    }

    void LateUpdate()
    {
        if (IsGrappling)
        {
            if (HandGrip == null)
                CreateNewCharacterJoint();
        }

        if (HandGrip == null && (Anchor != null || NewString != null || NeedleRender != null))
        {
            GameObject.Destroy(Anchor.gameObject);
            GameObject.Destroy(NewString.gameObject);
            GameObject.Destroy(NeedleRender.gameObject);
            NeedleRender = null;
        }

        //StringPull();
        if (NewString != null && Line != null)
            UpdateString();

        if (HandGrip != null) 
        {
            velocity = HandGrip.GetComponent<Rigidbody>().velocity;
            Velocity = HandGrip.GetComponent<Rigidbody>().velocity.magnitude;
            Debug.DrawRay(HandGrip.transform.position, velocity, Color.green);
            if(Anchor != null)
                GrappleDistance = Vector3.Distance(HandGrip.transform.position, Anchor.transform.position);
        }
    }

    public void UpdateGrapplePhysics()
    {
        bool isFalling = TP_Animator.Instance.MoveAction == TP_Animator.MovingAction.Falling;
        bool isGrounded = controller.isGrounded;
        bool handGripExists = HandGrip != null;

        bool isGrippedAndFalling = !isGrounded && handGripExists && isFalling;
        bool isTooFast = Velocity > MaxPossibleVelocty;

        if (isGrippedAndFalling) //TODO:should only function if at distance of string length and also below the anchor
        {
            resetCreatedGrapple();
            //TP_Animator.Instance.action = TP_Animator.Action.Grappling;
            setSwingingCharacterPositionRotation();
            updateHandGripPhysics();
            if (isTooFast)
                DestroyGrapple();
        }
    }

    public void UpdateGrapple() 
    {
        //Setting visuals of needle and string it's attached to
        bool isNotGrapplingAndHasNeedle = (!IsGrappling && TP_Animator.Instance.HasNeedle);
        bool isNotGrapplingAndHasNeedleAndSpindle = (isNotGrapplingAndHasNeedle && TP_Animator.Instance.HasSpindle);
        Needle.SetActive(isNotGrapplingAndHasNeedle);
        HandString.SetActive(isNotGrapplingAndHasNeedleAndSpindle);

        if (IsGrappling)
        {

            //Basic necessary state tracking for grappling logic
            bool isFalling = TP_Animator.Instance.MoveAction == TP_Animator.MovingAction.Falling;
            bool isGrounded = controller.isGrounded;
            bool handGripExists = HandGrip != null;
            bool anchorExists = Anchor != null;

            //if (handGripExists)
            //{
            //    Debug.DrawLine(HandGrip.transform.position, HandGrip.transform.position + SwingVector * 4, Color.yellow);
            //    Debug.DrawLine(HandGrip.transform.position, HandGrip.transform.position + velocity, Color.red);
            //}

            //bool isGrapplingAndHandGrip = IsGrappling && handGripExists;
            //bool isGrippedAndNotGrounded = !isGrounded && handGripExists;

            //Logical states of different things useful for tracking what to do
            bool isGrapplingAndNoHandGrip = !handGripExists && IsGrappling;
            bool isGroundedWithHandGrip = isGrounded && handGripExists;
            bool isGrippedAndFalling = !isGrounded && handGripExists && isFalling;
            bool isGrippedAndRising = !isGrounded && handGripExists && !isFalling;
            //bool isTooFast = Velocity > MaxPossibleVelocty;

            bool isAnchorAboveCharacter = false;
            if (handGripExists && anchorExists)
                isAnchorAboveCharacter = (Anchor.transform.position.y > this.transform.position.y);
            bool isAnchorGripAboveAndFalling = isAnchorAboveCharacter && isGrippedAndFalling;

            //bool isHandGripFarEnoughFromCharacter = handGripExists && Vector3.Distance(HandGrip.transform.position, this.transform.position) >= MaxGrappleRadius;

            //if (canChangeControllerState)
            //{
            //    bool previousControllerState = controller.enabled;
                controller.enabled = CheckForGround(FloorDetectionDistance) ||
                    isGroundedWithHandGrip || !IsGrappling || isGrippedAndRising || !isGrippedAndFalling;

            //    canChangeControllerState = previousControllerState == controller.enabled;
            //    if (!canChangeControllerState)
            //        Invoke("undoControllerStateDisabler", StateChangeTiming);
            //}

            //    controller.enabled = (CheckForGround(FloorDetectionDistance) ||
            //        isGroundedWithHandGrip || !IsGrappling || isGrippedAndRising || !isGrippedAndFalling
            //        || (!isAnchorAboveCharacter && isGrippedAndFalling && !isGrippedAndRising));

            //    canChangeControllerState = previousControllerState == controller.enabled;

            //    if (controller.enabled && canJumpOnStateChange)
            //    {
            //        canJumpOnStateChange = false;
            //        applyMomentumVertically();
            //    }

            //    if (!canJumpOnStateChange)
            //        canJumpOnStateChange = !controller.enabled;

            //    if (!canChangeControllerState)
            //        Invoke("undoControllerStateDisabler", velocity.magnitude);
            //}

            if (isGrapplingAndNoHandGrip)
                //set a state in the animator, then once animation is complete, do the following
                //might have to play the animation during all these calculations if loading becomes an issue during the grapple creation.
                CreateNewCharacterJoint();

            if (controller.enabled) //isGroundedWithHandGrip || isGrippedAndRising || (!isAnchorAboveCharacter && isGrippedAndFalling && !isGrippedAndRising)) 
            {
                setLandedCharacterRotation();

                HandGrip.gameObject.SetActive(false);
                HandGrip.transform.position = this.transform.position;

                if (isGroundedWithHandGrip && anchorExists)
                    tugOnPhysicsObjects();
            }

            if (isGrippedAndFalling && !controller.enabled) //TODO:should only function if at distance of string length and also below the anchor
            {
                resetCreatedGrapple();
                TP_Animator.Instance.action = TP_Animator.Action.Grappling;
                setSwingingCharacterPositionRotation();
                updateHandGripPhysics();
                //if (isTooFast)
                //    DestroyGrapple();
            }

            if (cooldown > 0)
                cooldown -= Time.deltaTime;
        }
        else
            controller.enabled = true;

    }

    private void applyMomentum()
    {
        float angle = 1 - Vector3.Dot(-Anchor.transform.up, (HandGrip.transform.position - Anchor.transform.position).normalized);
        //Debug.Log("Vertical Vector " + (HandGrip.GetComponent<Rigidbody>().velocity.y) + ", Dampen" + angle);
        angle = Mathf.Clamp(angle, 0f, 1f);
        float dampenedVelocity = HandGrip.GetComponent<Rigidbody>().velocity.y * angle;
        //Debug.Log("Dampened Vector: " + dampenedVelocity);
        dampenedVelocity = Mathf.Clamp(dampenedVelocity, 0f, MaxReleaseVelocity);
        timeToChangeStateEnabled = dampenedVelocity * JumpDampen;
        if (dampenedVelocity * JumpDampen > 0.1f)
            TP_Motor.Instance.AirJump(dampenedVelocity * JumpDampen);

        Vector3 gripSpeed = HandGrip.GetComponent<Rigidbody>().velocity;
        if (gripSpeed.magnitude * JumpDampen > 0.1f)
            TP_Motor.Instance.MomentumVector = new Vector3(gripSpeed.x, 0f, gripSpeed.z);
        TP_Motor.Instance.MoveVector = (velocity);
    }

    private void applyMomentumVertically()
    {
        float angle = 1 - Vector3.Dot(-Anchor.transform.up, (HandGrip.transform.position - Anchor.transform.position).normalized);
        //Debug.Log("Vertical Vector " + (HandGrip.GetComponent<Rigidbody>().velocity.y) + ", Dampen" + angle);
        angle = Mathf.Clamp(angle, 0f, 1f);
        float dampenedVelocity = HandGrip.GetComponent<Rigidbody>().velocity.y * angle;
        //Debug.Log("Dampened Vector: " + dampenedVelocity);
        dampenedVelocity = Mathf.Clamp(dampenedVelocity, 0f, MaxReleaseVelocity);
        timeToChangeStateEnabled = dampenedVelocity * JumpDampen;
        if (dampenedVelocity * JumpDampen > 0.1f)
            TP_Motor.Instance.AirJump(dampenedVelocity * JumpDampen);
        TP_Motor.Instance.MoveVector = (velocity);
    }

    private void undoControllerStateDisabler()
    {
        canChangeControllerState = true;
    }

    void checkControllerEnabled()
    {
        bool isFalling = TP_Animator.Instance.MoveAction == TP_Animator.MovingAction.Falling;
        bool isGrounded = controller.isGrounded;
        bool handGripExists = HandGrip != null;

        bool isGrapplingAndNoHandGrip = !handGripExists && IsGrappling;
        bool isGroundedWithHandGrip = isGrounded && handGripExists;
        bool isGrippedAndFalling = !isGrounded && handGripExists && isFalling;

        //controller.enabled = !isGrapplingAndFalling;
        controller.enabled = isGroundedWithHandGrip || !isGrapplingAndNoHandGrip || isGrippedAndFalling;
    }

    public void SetGrappling()
    {
        IsGrappling = true;

    }

    public void Pull()
    {
        //SoftJointLimit Limit = new SoftJointLimit();
        //Limit.limit = needleJoint.linearLimit.limit - (Time.deltaTime * PullSpeed);
        //needleJoint.linearLimit = Limit;
        Vector3 pullDirection = (Anchor.transform.position - controller.transform.position).normalized;
        HandGrip.GetComponent<Rigidbody>().velocity = pullDirection * PullSpeed * Time.deltaTime;
        TP_Motor.Instance.AerialMoveVector = HandGrip.GetComponent<Rigidbody>().velocity;
        this.transform.LookAt(Anchor.transform.position);
    }

    #region Grappling Logic Methods
    void restartSetAnchor()
    {
        Transform parent = Anchor.transform.parent;
        needleAnchor = Anchor.transform.position;
        GameObject.Destroy(Anchor.gameObject);
        setNeedleAnchorProperties(parent, false);
        SwingVector = TP_Motor.Instance.MoveVector;
    }

    void resetCreatedGrapple()
    {
        deactivationTiming = 0;
        HandGrip.gameObject.SetActive(true);
        reactivationTiming += 1;
        if (reactivationTiming == AnchorChangeTiming)
        {
            MaxGrappleRadius = GrappleDistance;
            restartSetAnchor();
        }
    }

    void setSwingingCharacterPositionRotation()
    {
        float distance = Vector3.Distance(this.transform.position, HandGrip.transform.position);
        // TODO: check offset when fixing the collision
        this.transform.position = Vector3.Lerp(this.transform.position, HandGrip.transform.position + (HandGrip.transform.up * CollisionOffset),
            Time.deltaTime * distance * PositionUpdateSpeed); //may cause issues

        Vector3 gripDirection = HandGrip.transform.GetComponent<Rigidbody>().velocity.normalized;
        Quaternion stringTargetRotation = Quaternion.LookRotation(gripDirection, needleAnchor - HandGrip.transform.position);
        //Quaternion stringTargetRotation = Quaternion.LookRotation(TP_Camera.Instance.transform.forward, needleAnchor - HandGrip.transform.position);
        this.transform.rotation = stringTargetRotation;
    }

    void updateHandGripPhysics()
    {
        if (SwingVector.magnitude != 0)
        {
            //HandGrip.GetComponent<Rigidbody>().velocity += (TP_Camera.Instance.transform.localRotation * (Vector3.Normalize((SwingVector)) * Time.deltaTime * SwingSpeed));
            HandGrip.GetComponent<Rigidbody>().velocity += (TP_Camera.Instance.transform.TransformDirection(Vector3.Normalize((SwingVector)) * Time.deltaTime * SwingSpeed));
            SwingVector = new Vector3(0, 0, 0); //DON'T DELETE, IT CAUSES TOO MANY ISSUES WITHOUT THIS LINE
        }
    }

    void setLandedCharacterRotation()
    {
        reactivationTiming = 0;
        deactivationTiming += 1;
        if (deactivationTiming == AnchorChangeTiming && HandGrip != null && Anchor != null)
        {
            Vector3 gripDirection = HandGrip.transform.GetComponent<Rigidbody>().velocity.normalized;
            Quaternion stringTargetRotation = Quaternion.LookRotation(gripDirection, needleAnchor - HandGrip.transform.position);
            this.transform.rotation = Quaternion.Euler(0,stringTargetRotation.eulerAngles.y,0);
        }
    }

    void setLandedCharacterRotationOnChange()
    {
        if (HandGrip != null && Anchor != null)
        {
            Vector3 gripDirection = HandGrip.transform.GetComponent<Rigidbody>().velocity.normalized;
            Quaternion stringTargetRotation = Quaternion.LookRotation(gripDirection, needleAnchor - HandGrip.transform.position);
            this.transform.rotation = Quaternion.Euler(0f, stringTargetRotation.eulerAngles.y, 0f);
        }
    }

    void tugOnPhysicsObjects()
    {
        Transform anchorParent;
        if (Anchor.transform.parent != null)
            anchorParent = Anchor.transform.parent;
        else
            return;

        if (GrappleDistance >= MaxGrappleRadius && (TuggingLayers == (TuggingLayers | (1 << anchorParent.gameObject.layer))))
        {
            //Debug.Log("Tugging on object");
            if (anchorParent.GetComponent<Rigidbody>() != null && MaxGrappleRadius > 0f)
            {
                Vector3 direction = (Hand.transform.position - Anchor.transform.position).normalized;
                float magnitude = GrappleDistance - MaxGrappleRadius;
                anchorParent.GetComponent<Rigidbody>().WakeUp();
                anchorParent.GetComponent<Rigidbody>().AddForce(direction * magnitude * TuggingForce, ForceMode.Impulse);
            }
            else if (anchorParent.transform.root.GetComponent<CreatureAnimator>() != null && MaxGrappleRadius > 0f)
            {
                Debug.Log("Tugging on Monster");
                anchorParent.transform.root.GetComponent<CreatureAnimator>().setTugProperties();
            }
        }
        else if (GrappleDistance >= MaxGrappleRadius && anchorParent.tag == "Door")
        {
            Doors door = anchorParent.GetComponentInParent<Doors>();
            if (door != null)
            {   
                if (Vector3.Distance(Anchor.transform.position, door.DoorKnob.position) <= 1.7f)
                    door.Pull(this.transform.position);
            }
        }
    }
    #endregion

    #region Grappling Collision Methods
    public bool CheckForGround(float detectionDistance)
    {
        RaycastHit hit;
        Vector3 footPoint = this.transform.position - (this.transform.up * CharacterHeight);
        Debug.DrawRay(this.transform.position, Vector3.down * detectionDistance, Color.cyan);
        return (Physics.Raycast(this.transform.position - (Vector3.down * CharacterHeight), Vector3.down, out hit, detectionDistance, LandingLayers));
    }

    void checkGrappleCollision()
    {
        //if (Physics.OverlapCapsule(this.transform.position, CharacterWidth, -this.transform.up,out hit, CharacterHeight,Layers))
        //RaycastHit hit;
        Vector3 footPoint = this.transform.position - (this.transform.up * CharacterHeight);
        Collider[] colliders = Physics.OverlapCapsule(footPoint, this.transform.position, CharacterWidth, LandingLayers);
        //Debug.DrawRay(footPoint,this.transform.up * CharacterHeight,Color.blue);
        //Debug.DrawRay(footPoint, this.transform.right * CharacterWidth, Color.blue);
        //Debug.DrawRay(footPoint, -this.transform.right * CharacterWidth, Color.blue);
        //if (Physics.CapsuleCast(footPoint,this.transform.position,CharacterWidth,this.transform.up,out hit,-0.3f,Layers))

        if(colliders.Length >= 1 && cooldown <= 0)
        {
            //Debug.Log("Hit: " + hit.collider.tag);
            
            foreach (Collider col in colliders)
            {
                Vector3 hitNormal = col.ClosestPoint(this.transform.position);
                bounceOffWall(hitNormal);
            }
            //handGrip.GetComponent<Rigidbody>().velocity = -handGrip.GetComponent<Rigidbody>().velocity;
        }
    }

    void bounceOffWall(Vector3 hitNormal)
    {
        if (HandGrip != null)
        {
            //controller.enabled = true;
            //Vector3 hitNormal = hit.normal;
            float dot = (Vector3.Dot(hitNormal, (-transform.forward))) * 2;
            Vector3 reflection = hitNormal * dot;
            reflection += transform.forward;
            HandGrip.GetComponent<Rigidbody>().velocity = HandGrip.transform.TransformDirection(reflection.normalized * Velocity/2);
        }
    }
    #endregion

    #region Original Character Joint Method
    public void CreateNewCharacterJoint() 
    {
        CreateHandGripObject();
        if (HandGrip != null)
        {
            CreateNeedleAnchor();
            AddColliderToHandGrip();
            CreateStringObject();
        }
        //TP_Motor.Instance.ResetRotation();
    }
    void CreateNeedleAnchor() 
    {
        //Uses the Linecast function in order to find the proper position of the new character joint. then sets all the previously established properties to create a personalized joint mechanism.
        var camera = Camera.main.transform;
        RaycastHit hit;

        if (Physics.Raycast(camera.position, camera.forward, out hit, MaxDistanceFromCamera, LandingLayers))
        {         
            needleAnchor = hit.point; //sets the anchoring position to the place where the raycast hit and object collider.
            setNeedleAnchorProperties(hit.transform);
            MaxGrappleRadius = Vector3.Distance(HandGrip.transform.position, Anchor.transform.position);
        }
        else 
        {
            IsGrappling = false;
            GameObject.Destroy(HandGrip.gameObject);
        }

    }

    Vector3 invertScale(Vector3 scale)
    {
        return new Vector3(1/scale.x, 1/scale.y, 1/scale.z);
    }

    void setNeedleAnchorProperties(Transform parent = null, bool firstGrapple = true)
    {
        Rigidbody Body; //Placeholder Rigidbody for me we create one.
        Anchor = new GameObject("Needle Anchor"); //creates the new object that is to be edited

        Anchor.AddComponent<Rigidbody>();
        //Anchor.AddComponent<ConfigurableJoint>();
        Anchor.AddComponent<CharacterJoint>(); //Creates a Configurable joint component and adds it to the newly created anchor.
        Anchor.transform.position = needleAnchor; // Sets its position instantly to the new location

        //Attaches Anchor to object it attaches to
        Anchor.transform.parent = parent;
        Anchor.transform.localRotation = Quaternion.identity;

        //needleJoint = Anchor.GetComponent("ConfigurableJoint") as ConfigurableJoint; //Grabs the recently created Configurable joint so we can edit it really quickly
        needleJoint = Anchor.GetComponent("CharacterJoint") as CharacterJoint;
        Body = Anchor.GetComponent("Rigidbody") as Rigidbody; //Grabs the recently created Rigidbody component so we can edit it as well.

        //I wish there were constructors allowing you to create components on the spot, but there doesn't appear to be a way to do that (That's inefficient, Unity!)

        needleJoint.connectedBody = HandGrip.GetComponent<Rigidbody>();  //Connects the anchor to the Handgrip object that was created beforehand.
        needleJoint.autoConfigureConnectedAnchor = true;  //Sets it so it can auto configure its own connected anchor depending on its position, etc.
        needleJoint.enableCollision = true;
        needleJoint.axis = new Vector3(1f, 1f, 1f);  //Sets the swinging axis to be all three motion axis, for free swinging in just about every direction imaginable.

        if (NeedleRender == null)
        {
            //Adding a visible Mesh (may use for attaching string)
            NeedleRender = new GameObject("Needle Visual");
            MeshFilter filter = NeedleRender.AddComponent<MeshFilter>();
            filter.mesh = NeedleMesh;
            MeshRenderer renderer = NeedleRender.AddComponent<MeshRenderer>();
            renderer.material = NeedleMaterial;

            NeedleRender.transform.position = Anchor.transform.position;
            //NeedleRender.transform.localPosition = Vector3.zero;
            //NeedleRender.transform.localScale = Vector3.one * 0.1f;
            NeedleRender.transform.localScale = NeedleRender.transform.lossyScale * 0.1f;
            NeedleRender.transform.LookAt(this.transform);
            NeedleRender.transform.position += NeedleRender.transform.TransformDirection(PositionOffset);
            if (checkIfAnchorIsOnDynamicObject())
            {
                Debug.Log("Needle anchor parent " + Anchor.transform.parent.name + " isn't static. Seting to Anchor");
                NeedleRender.transform.parent = Anchor.transform;
            }
            else
            {
                Debug.Log("Needle anchor parent " + Anchor.transform.parent.name + " is static. Seting to Anchor");
                NeedleRender.transform.parent = Anchor.transform.root;
            }
        }
        else
        {
            if (checkIfAnchorIsOnDynamicObject())
                NeedleRender.transform.parent = Anchor.transform;
            else
                NeedleRender.transform.parent = Anchor.transform.root;
        }

        if (firstGrapple)
        {

            //Adding Sound effect for it
            AudioSource audio = Anchor.AddComponent<AudioSource>();
            audio.spatialBlend = AudioBlend; //Can always be heard but it could be near silent
            audio.volume = Volume;
            audio.playOnAwake = true;
            audio.outputAudioMixerGroup = Mixer;
            audio.clip = GrappleClip;
            audio.Play();

            //Adding Particles (Likely single burst particles)
            GameObject particles = GameObject.Instantiate(Particles,
                Anchor.transform.position, Quaternion.identity, Anchor.transform);
        }

        SetNeedleSwingLimits(); //this method creates the four other bits of the Configurable joint that aren't easy to set.

        Body.useGravity = false; //Sets the gravity of the main needle anchor to be unexistant during the sequence of swinging
        Body.isKinematic = true; //Sets it so it can't be moved. This will be changed for future parts when fighting monsters.
    }

    bool checkIfAnchorIsOnDynamicObject()
    {
        LayerMask parentLayer = Anchor.transform.parent.gameObject.layer;
        bool isObjectInDynamicLayers = RenderLayers == (RenderLayers | (1 << parentLayer));
        bool isObjectDoor = Anchor.transform.parent.tag == "Door" && parentLayer == LayerMask.NameToLayer("Door");

        return isObjectInDynamicLayers || isObjectDoor;
    }

    void SetNeedleSwingLimits() 
    {   //creates the four soft joint limits
        //(Configurable joint stuff)
        //needleJoint.xMotion = ConfigurableJointMotion.Limited;
        //needleJoint.yMotion = ConfigurableJointMotion.Limited;
        //needleJoint.zMotion = ConfigurableJointMotion.Limited;
        //SoftJointLimit LinLimit = new SoftJointLimit();
        //LinLimit.limit = Vector3.Distance(handGrip.transform.position, needleJoint.transform.position);
        //needleJoint.linearLimit = LinLimit;
        
        //(Character joint stuff)
        //First Soft Joint limit
        SoftJointLimit lowTwist = new SoftJointLimit();
        lowTwist.limit = -SwingAndTwistLimit;
        needleJoint.lowTwistLimit = lowTwist; //set
        //Second Soft Joint Limit
        SoftJointLimit highTwist = new SoftJointLimit();
        highTwist.limit = SwingAndTwistLimit;
        needleJoint.highTwistLimit = highTwist; //set
        //Third Soft Joint Limit
        SoftJointLimit swing1 = new SoftJointLimit();
        swing1.limit = SwingAndTwistLimit;
        needleJoint.swing1Limit = swing1; //set
        //Fourth Soft Joint Limit
        SoftJointLimit swing2 = new SoftJointLimit();
        swing2.limit = SwingAndTwistLimit;
        needleJoint.swing2Limit = swing2; //set
        
    }
    void CreateHandGripObject() 
    {   //Creates the attached object that will be doing the motion of the grapple.
        Rigidbody Body; //Creates its necesary rigidbody placeholder
        
        HandGrip = new GameObject("Hand Grip"); //Creates a new object called Hand Grip
        HandGrip.layer = 12;
        HandGrip.tag = "Player";
        HandGrip.AddComponent<Rigidbody>(); //Attaches a rigidbody to the hand grip
        HandGrip.transform.position = Hand.position; //Sets the position of the new object to the be located at the Configurable's hand
        HandGrip.transform.rotation = Quaternion.Euler(0, this.transform.rotation.y, 0);

        Body = HandGrip.GetComponent("Rigidbody") as Rigidbody; //Grabs the rigidbody for the sake of editing.
        Body.interpolation = RigidbodyInterpolation.Interpolate;
        //Body.constraints = RigidbodyConstraints.FreezeRotationZ;
        Body.mass = Mass; //Sets the mass
        Body.drag = AerialDrag;  //sets the drag

        HandGrip.AddComponent<HandGripSpeedStopper>();

        //Note: Attempting to make this object's parent the Configurable's right hand itself causes an insane amount of issues regarding location and teleports it off into space.
    }
    void AddColliderToHandGrip() 
    {
        if (HandGrip != null && Anchor != null)
        {
            Vector3 stringVector = (HandGrip.transform.position - Anchor.transform.position);
            HandGrip.transform.rotation = Quaternion.Euler(0, stringVector.y, 0);
        }
        CapsuleCollider Col;
        HandGrip.AddComponent<CapsuleCollider>();
        Col = HandGrip.GetComponent("CapsuleCollider") as CapsuleCollider;
        Col.radius = CharacterWidth;
        Col.height = CharacterHeight;
        Col.direction = 0;
    }
    void CreateStringObject() 
    {
        if (HandGrip != null && Anchor != null)
        {
            NewString = GameObject.Instantiate(String); //creates a prefab string object
            Line = NewString.GetComponent<LineRenderer>(); //we grab the line renderer segment of the prefab to feely manipulate it
            Line.positionCount = LineSegments; //setting the amount of segments it will have.
        }
    }
    void UpdateString() 
    {
        String.transform.position = Hand.transform.position;
        if (LineSegments > 2)
        {
            int i = 0;
            while (i < LineSegments)
            {
                Vector3 position = new Vector3(i * 0.5f, Mathf.Sin(i + Time.time), 0); //Come back to this to know how to manipulate the string
                Line.SetPosition(i, position);  //the mathematics is fairly complex, so this will have to come later.
                i++;
            }
        }
        else 
        {   //more of a tester thing to make sure the positioning works properly
            Vector3 pos1 = Hand.transform.position;
            Vector3 pos2 = NeedleRender.transform.position;
            Line.SetPosition(0, pos1);
            Line.SetPosition(1, pos2);
        }
    }
    public void DestroyGrapple() 
    {   //Simply destroys both the Anchor and the attached object for when you reset.
        TP_Controller.CharacterController.enabled = true;
        //TP_Motor.Instance.AerialMoveVector = this.transform.InverseTransformVector(handGrip.GetComponent<Rigidbody>().velocity) * VelocityMultiplier;
        //float angle = 1 - Vector3.Dot(-Anchor.transform.up,(HandGrip.transform.position - Anchor.transform.position).normalized);
        //Debug.Log("Vertical Vector " + (HandGrip.GetComponent<Rigidbody>().velocity.y) + ", Dampen" + angle);
        //angle = Mathf.Clamp(angle, 0f, 1f);
        //float dampenedVelocity = HandGrip.GetComponent<Rigidbody>().velocity.y * angle;
        //Debug.Log("Dampened Vector: " + dampenedVelocity);
        //dampenedVelocity = Mathf.Clamp(dampenedVelocity,0f, MaxReleaseVelocity);
        //TP_Motor.Instance.AirJump(dampenedVelocity * JumpDampen);
        //TP_Motor.Instance.MoveVector = (velocity);

        //applyMomentumVertically();
        applyMomentum();

        GameObject.Destroy(HandGrip.gameObject);
        //handGrip.SetActive(false);
        GameObject.Destroy(Anchor.gameObject);
        GameObject.Destroy(NewString.gameObject);
        GameObject.Destroy(NeedleRender.gameObject);
        NeedleRender = null;
        Line = null;
        MaxGrappleRadius = 0f;
        
        //TP_Motor.Instance.SnapAlignCharacterInAir(velocity * VelocityMultiplier);

        //this.transform.rotation = Quaternion.LookRotation(TP_Camera.Instance.transform.forward, Vector3.up);
    }
    #endregion

    #region new Spiderman 2 Rope Method
    public void SetTether() 
    {
        var camera = Camera.main.transform;
        RaycastHit hit;
        if (Physics.Raycast(camera.position, camera.forward, out hit, MaxDistanceFromCamera))
        {
            IsGrappling = true;
            IsTethered = true;
            Anchor = new GameObject();
            needleAnchor = hit.point;
            Anchor.transform.position = needleAnchor;
            Anchor.name = "Needle";
            MaxGrappleRadius = (MainCharacter.transform.position - Anchor.transform.position).magnitude;
        }
        else 
        {
            IsGrappling = false;
            IsTethered = true;
        }
    }
    void StringPull() 
    {
        RaycastHit hit;
        if (Anchor != null)
        {
            if (Physics.Raycast(Anchor.transform.position, (MainCharacter.transform.position - Anchor.transform.position).normalized, out hit, MaxGrappleRadius))
            {
                if (hit.collider.tag != "Player") //if it hit something other than the player, than 
                {
                    //Reset the Max Radius to compensate for circling around corners. 
                    //in the future try to work a way to keep track of if it comes back around
                }
            }
            else
            {

            }
        }
    }
    #endregion
}