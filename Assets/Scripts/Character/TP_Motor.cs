using UnityEngine;
using System.Collections;
using System;

public class TP_Motor : MonoBehaviour 
{
    public static TP_Motor Instance; //self
    [Header("Grip Properties")]
    [Tooltip("The Item that is grabbed when it applies")]
    public Transform GrabbedItem;
    [Tooltip("The game object that shows where items are to be grabbed")]
    public Transform GripPosition;
    [Tooltip("The offset of a grabbed item so it doesn't overlap with character model")]
    public Vector3 GrabOffset;
    [Tooltip("How far the bear should try to grab an object")]
    public float GrabSize = 0.4f;
    [Tooltip("Weight of the item grabbed affecting speed")]
    public float GrabWeight = 1f;
    [Tooltip("Weight multiplier so weight isn't as strong on the speed")]
    public float GrabWeightMultiplier = 0.3f;
    [Tooltip("Layers used for ignoring objects when grabbing an object")]
    public LayerMask GrabbingLayers;
    [Tooltip("Force of item throwing")]
    public float ThrowForce = 30f;
    [Tooltip("Angle of item throwing")]
    public float ThrowAngle = 45f;
    [Tooltip("Indicates")]
    public bool grabbing = false;
    [Tooltip("Indicates")]
    public bool throwing = false;

    [Header("Movement Properties")]
    [Tooltip("Amount of time left before hitstun ends")]
    public float KnockedTime = 0f;
    [Tooltip("Indicates whether the character is being knocked back or not")]
    public bool knocked = false;
    [Tooltip("Indicates whether the character is colliding with a surface or not when knocked around")]
    public bool colliding = false;
    [Tooltip("The multiplier reduction of knockback")]
    [Range(0f, 1f)]
    public float KnockbackResistance = 1f;
    [Tooltip("Invincibility Frames after getting hit")]
    public float IFrames = 2f;
    [Tooltip("Speed at which character accelerates into top speed")]
    public float MovementAcceleration = 0.4f;
    [Tooltip("The top aerial speed that can achieved when in control")]
    public float AerialSpeed = 5f;
    [Tooltip("Top walking speed of character")]
    public float MoveSpeed = 10f; //walking speed
    [Tooltip("Top running speed of character")]
    public float RunSpeed = 13f; //running speed
    [Tooltip("The speed at which it is considered running instead of walking")]
    public float RunLimit = 0.6f;
    [Tooltip("The speed at which the character rotates to realign themselves to a new direction")]
    public float RotationSpeed = 15f;
    public float DistanceFromMonster = 1.3f;
    public float SpeedOnMonster = 1f;

    [Header("Momentum Properties")]
    [Tooltip("If active will force turn off all momentum application")]
    public bool MomentumOverride = false;
    [Tooltip("If active momentum will reduce over time")]
    public bool MomentumReduces = true;
    [Tooltip("If active momentum will apply to movement")]
    public bool MomentumApplies = true;
    [Tooltip("Current Momentum")]
    public float Momentum = 0f;
    [Tooltip("Maximum Momentum to reach")]
    public float MaxMomentum = 20f;
    [Tooltip("Speed at which character decelerates extra momentum")]
    public float MomentumReductionPercentage = 0.1f;

    [Header("Sprint Properties")]
    public float SprintTimer = 0f;
    public float SprintCooldownTimer = 0f;
    [Tooltip("Time in seconds for how long one can sprint")]
    public float SprintTime = 1.5f;
    [Tooltip("Time seconds one has to wait before sprinting again")]
    public float SprintCooldown = 3f;
    [Tooltip("Top sprinting speed of character")]
    public float SprintSpeed = 13f; //sprinting speed

    [Header("Physics Object interaction")]
    [Tooltip("The power the character has when moving around physics objects")]
    public float PushPower = 10f;
    [Tooltip("The amount the character can resist being pushed back by physics objects")]
    public float PushResistance = 0.7f;

    [Header("Falling properties")]
    [Tooltip("Indicates if gravity is on or not for the character")]
    public bool gravity = true;
    [Tooltip("The acceleceration at which the character falls back down")]
    public float Gravity = 21f;
    [Tooltip("The maximum falling speed the character can reach")]
    public float TerminalVelocity = 20f;
    [Tooltip("The fall speed")]
    public float FallSpeed = 0f;
    public bool Gliding = false;

    [Header("Jump Properties")]
    [Tooltip("The base speed at which the character leaves the ground when jumping")]
    public float JumpSpeed = 6f;
    [Tooltip("The multiplier charged up to apply to the jump speed")]
    public float JumpCharge = 0f;
    [Tooltip("The speed at which the jump charges up")]
    public float Charge = 0.01f;
    [Tooltip("The maximum a jump charge can reach")]
    public float ChargeUpperLimit = 1f;
    [Tooltip("The minimum a jump charge can be at")]
    public float ChargeLowerLimit = 0.6f;

    [Header("Sliding Properties")]
    [Tooltip("The Layers that are to be considered when taking slopes into account for sliding and speed reduction")]
    public LayerMask SlopeableObjects;
    [Tooltip("The range downwards to check for slopes")]
    public float SlopeDetectionRange = 1f;
    [Tooltip("The amount of threshold for a slide to start")]
    public float SlideThreshold = 0.6f; //between 0 and 1
    [Tooltip("Max amount you can control the slide")]
    public float MaxControllableSlideMagnitude = 0.4f;

    [Header("Plushy detection properties")]
    [Tooltip("Indicator for when Teddy is close to another plushy")]
    public bool NearPlushy = false;
    [Tooltip("Indicator for when Teddy is close to another knight")]
    public bool NearKnight = true;
    [Tooltip("How far ahead of teddy a plushy can be found")]
    public float PlushyDetectionLength = 1.5f;
    [Tooltip("How wide in front of teddy a plushy can be found")]
    public float PlushyDetectionWidth = 0.5f;
    [Tooltip("Layer on which to find the plushies")]
    public LayerMask PlushyLayer;


    public Vector3 ImpactPoint { get; set; }
    public Vector3 SlideDirection { get; set; }
    public Vector3 MomentumVector { get; set; }
    public Vector3 MoveVector { get; set; }
    public Vector3 ImpactVector { get; set; }
    public Vector3 AerialMoveVector { get; set; }
    public Vector3 AerialRotation { get; set; }
    public float CurrentSpeed { get; set; }
    public float VerticalVelocity { get; set; }

    private float currentAcceleration = 0f;
    private Vector3 initialScale = Vector3.one;
    private float timeKnockedTotal = 0f;
    private float knockedTimeCameraThreshold = 0.4f;
    //Unused variables--
    private float LedgeJumpSpeed = 4f;
    private float ImpactDecrease = 0.5f;
    private float CollisionDamper = 0.5f;
    private float XRotation = 0f;
    private float YRotation = 0f;
    //------------------

    public CharacterController Controller;

	void Awake () 
    {
        Instance = this;
	}

    void Start()
    {
        Controller = TP_Controller.CharacterController;
        initialScale = this.transform.localScale;
    }

    private void FixedUpdate()
    {
        ProcessMomentum();

        if (IFrames > 0)
            IFrames -= Time.deltaTime;
        if (KnockedTime > 0)
            KnockedTime -= Time.deltaTime;

        if (SprintTimer > 0)
            SprintTimer -= Time.deltaTime;
        if (SprintCooldownTimer > 0)
            SprintCooldownTimer -= Time.deltaTime;

        if (!knocked)
            colliding = false;
        else if (knocked && TP_Grapple.Instance.HandGrip !=null)
        {
            TP_Grapple.Instance.IsGrappling = false;
            TP_Grapple.Instance.DestroyGrapple();
        }

        if (knocked || !TP_Controller.CharacterController.isGrounded)
            TP_Controller.Instance.paused = false;

        if(knocked && !TP_Controller.CharacterController.isGrounded && timeKnockedTotal > knockedTimeCameraThreshold)
            TP_Camera.Instance.ResetToFront();

        if (this.transform.parent == null && this.transform.localScale != initialScale)
            this.transform.localScale = initialScale;

        //else if (this.transform.parent != null)
        //    this.transform.localScale = Vector3.one * 0.01f;

        checkIfCloseToPlushy();
    }

    private void LateUpdate()
    {
        if (this.transform.parent != null)
        {
            //if (!TP_Grapple.Instance.CheckForGround(DistanceFromMonster))
            //Debug.Log(Controller.velocity.magnitude);
            bool isMovingFastOnMonster = this.Controller.velocity.magnitude > SpeedOnMonster;
            bool isNotGrounded = !this.Controller.isGrounded;
            bool isMonsterDead = this.transform.root.GetComponent<CreatureAnimator>().getDeathState();

            if ((isMovingFastOnMonster && isNotGrounded) || isMonsterDead)
            {
                Quaternion rot = this.transform.rotation;
                this.transform.parent = null;
                this.transform.rotation = rot;
            }
        }

        if (MomentumOverride)
        {
            MomentumReduces = false;
            MomentumApplies = false;
        }
    }

    public void UpdateMotor () 
    {
        //SnapAlignCharacterWithCamera();
        //AerialMoveVector = Vector3.Lerp(MoveVector,Vector3.zero,Time.deltaTime); 
        if (TP_Controller.CharacterController.isGrounded)
        {   //If character is grounded, simply replace previous aerial move vector with the current one
            if (Mathf.Abs(MoveVector.x) >= RunLimit || Mathf.Abs(MoveVector.z) >= RunLimit) //sets the CurrentSpeed property to be use dependant
            {
                if (SprintTimer <= 0f)
                    CurrentSpeed = RunSpeed / GrabWeight;
                else
                    CurrentSpeed = SprintSpeed;
            }
            else
            { CurrentSpeed = MoveSpeed/GrabWeight; }
        }
        else 
        {
            grabbing = false;
            //MoveVector = AerialMoveVector; //Problems when replacing movevector. Issues arise when camera change is applied. Most likely issue with camera.
            if (SprintTimer <= 0f)
                CurrentSpeed = AerialSpeed;
            else
                CurrentSpeed = SprintSpeed;
        }
        if (TP_Controller.CharacterController.isGrounded && TP_Controller.Instance.paused)
            grabbing = false;

        if (TP_Controller.CharacterController.enabled && !knocked)
            ProcessMotion();
        else
            ProcessImpact(ImpactVector);

        if (transform.eulerAngles.x != 0 && !TP_Grapple.Instance.IsGrappling && this.transform.parent == null) 
            ResetRotation();

        if (!TP_Controller.CharacterController.isGrounded)
        {
            if (!Gliding)
                FallSpeed = -MoveVector.y;
            else
                FallSpeed = -30f;
        }

        if (GrabbedItem != null)
        {
            //grabbing = true;
            Vector3 intendedRot = this.transform.rotation.eulerAngles + GrabbedItem.GetComponent<GrabProperties>().GrabRotation;
            Quaternion properRotation = Quaternion.Euler(intendedRot);
            GrabbedItem.rotation = Quaternion.RotateTowards(GrabbedItem.rotation, properRotation, 600f * Time.deltaTime);
            GrabbedItem.position = GripPosition.position + this.transform.TransformVector(GrabOffset);
        }
	}

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        bool isNotGroundedAndMovingUpwards = (!TP_Controller.CharacterController.isGrounded && MoveVector.y > 0);
        //Debug.Log("Hit Normal: " + hit.normal);
        bool isCollisionOnTopPortionOfCollider = hit.normal.y < -0.9;
        bool isNonMonsterPhysicsObject = (hit.collider.attachedRigidbody != null && hit.collider.tag != "Monster" && hit.collider.tag != "Blade");
        bool isKnockedAndIsNotMonster = (hit.collider.tag != "Monster" && hit.collider.tag != "Blade" && knocked);
        bool isMonsterAndCloseEnough = (hit.collider.tag == "Monster" && TP_Grapple.Instance.CheckForGround(DistanceFromMonster));

        if (isNotGroundedAndMovingUpwards && isCollisionOnTopPortionOfCollider) //Might change so it affects no matter MoveVector's y value
            MoveVector = new Vector3(MoveVector.x, 0, MoveVector.z); //this halts vertical motion upwards in midair

        if (isNonMonsterPhysicsObject)
        {//This implies a physics object 
            try
            {
                hit.collider.GetComponent<Rigidbody>().velocity = (hit.moveDirection * PushPower) / hit.collider.GetComponent<Rigidbody>().mass; //Pushes a physics object it comes in contact with.
            }
            catch (Exception e) { }
        }
        else if (isKnockedAndIsNotMonster)
        {
            colliding = true;
            //TP_Camera.Instance.Reset();
        }


        if (isMonsterAndCloseEnough && !TP_Animator.Instance.TeddyMode)// && TP_Controller.CharacterController.isGrounded)
        {
            if (hit.collider.GetComponentInParent<CreatureAnimator>() != null)
            {
                bool isMonsterAlive = (!hit.collider.GetComponentInParent<CreatureAnimator>().getDeathState());
                if (isMonsterAlive)
                    this.transform.parent = hit.collider.transform.parent;
            }
        }
        else
            this.transform.parent = null;
    }
    void OnCollisionEnter(Collision collision) 
    {
        //ApplyKnockback(collision.collider.attachedRigidbody.velocity);
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Monster")
        {
            //Debug.Log("Touching Monster");
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Monster")
        {
            //Debug.Log("Not Touching Monster");
            this.transform.parent = null;
        }
    }

    private bool isColliderPhysicsObject(Collider collider)
    {
        bool isCeramic = collider.tag == "Ceramic";
        bool isWood = collider.tag == "Wood";
        bool isPlastic = collider.tag == "Plastic";
        bool isMetal = collider.tag == "Metal";
        bool isGlass = collider.tag == "Glass";
        bool isFabric = collider.tag == "Fabric";
        return isCeramic || isWood || isPlastic || isMetal || isGlass || isFabric;
    }

    public void ApplyKnockback(Vector3 impactSpeed ,bool Attack = false, float time = 0f) 
    {
        if (!Attack)
            TP_Controller.CharacterController.Move((impactSpeed / PushResistance) * Time.fixedDeltaTime); //gets pushed back if physics object collides with character
        else
        {
            knocked = true;
            TP_Controller.Instance.MovementEnabled = false;
            TP_Controller.Instance.TurnOffTeddy();
            ProcessImpact(impactSpeed,true);
            if (time > 0f) //if time is positive, there's a time variable attached
                KnockedTime = time;
        }
    }

    public void StopKnockback()
    {
        knocked = false;
        colliding = false;
    }

    public void ProcessImpact(Vector3 impact, bool firstImpact = false)
    {
        if (firstImpact) { 
            ImpactVector = impact;
        }
        timeKnockedTotal += Time.deltaTime;
        ImpactVector = Vector3.Normalize(ImpactVector);
        currentAcceleration = impact.magnitude;
        VerticalVelocity = impact.y;

        this.transform.rotation = Quaternion.Euler(0f, 
            (Mathf.Atan2(-ImpactVector.normalized.x, -ImpactVector.normalized.z)) * Mathf.Rad2Deg,
            0f);
        //ApplyGroundRotation(-ImpactVector);
        AerialRotation = transform.rotation.eulerAngles;

        if (!colliding)
            ImpactVector = new Vector3(ImpactVector.x * currentAcceleration, VerticalVelocity, ImpactVector.z * currentAcceleration);  //reapply vertical velocity to MoveVector.y
        else
            ImpactVector = new Vector3(ImpactVector.normalized.x, VerticalVelocity, ImpactVector.normalized.z);

        if (gravity)
            ImpactVector = new Vector3(ImpactVector.x, VerticalVelocity - (Gravity * Time.deltaTime),ImpactVector.z);
        else
            ImpactVector = new Vector3(ImpactVector.x, 0f, ImpactVector.z);

        TP_Controller.CharacterController.Move(ImpactVector * Time.deltaTime);  //Move character in World Space
        if (TP_Controller.CharacterController.isGrounded)
        {
            knocked = false;
            if(!TP_Controller.Instance.MovementEnabled)
                TP_Controller.Instance.MovementEnabled = true;
            timeKnockedTotal = 0f;
        }
    }

    public void ResetOnHitFloor()
    {
        //if (timeKnockedTotal > knockedTimeCameraThreshold)
            TP_Camera.Instance.Reset(false, false);
    }

    public void AddForwardMomentum(float momentum)
    {
        MomentumVector += this.transform.forward * momentum;
    }

    private void ProcessMomentum()
    {
        //Momentum can't exceed maximum quantity
        if (MomentumVector.magnitude > MaxMomentum)
            MomentumVector = MomentumVector.normalized * MaxMomentum;

        Momentum = MomentumVector.magnitude;
        if (MomentumReduces)
        {
            //Bothers processing the momentum if it's great than .1 because at that size it no longer has an effect and takes too long to reset
            if (MomentumVector.magnitude > 0.1f)
            {
                //Debug.DrawRay(this.transform.position, MomentumVector, Color.magenta);
                float momentumMag = 0f;
                if(!TP_Controller.CharacterController.isGrounded)
                    momentumMag = MomentumVector.magnitude * (MomentumReductionPercentage);// * Time.deltaTime);
                else
                    momentumMag = MomentumVector.magnitude * (MomentumReductionPercentage * MomentumReductionPercentage);
                MomentumVector = MomentumVector.normalized * momentumMag;
            }
            else
                MomentumVector = Vector3.zero;
        }
    }

    void ProcessMotion() 
    {
        MoveVector = Camera.main.transform.TransformDirection(MoveVector);  //Transform MoveVector to World Space
        if (MoveVector.magnitude != 0) 
        {
            currentAcceleration += MovementAcceleration * Time.deltaTime;
            if (currentAcceleration > 1) 
                currentAcceleration = 1;
        }
        else { currentAcceleration = 0; }
        if (MoveVector.magnitude > 1)  //Normalize MoveVector if Magnitude > 1
        {
            MoveVector = Vector3.Normalize(MoveVector);
        }

        ApplyGroundRotation(MoveVector);
        AerialRotation = transform.rotation.eulerAngles;

        MoveVector *= CurrentSpeed;
        MoveVector = new Vector3(MoveVector.x * currentAcceleration, VerticalVelocity, MoveVector.z * currentAcceleration);  //reapply vertical velocity to MoveVector.y
        if(MomentumApplies)
            MoveVector += MomentumVector;

        ApplySlope();//apply sliding if applicable

        if(gravity)
            ApplyGravity(); //Apply gravity

        TP_Controller.CharacterController.Move(MoveVector * Time.deltaTime);  //Move character in World Space
        
    }

    private void checkIfCloseToPlushy()
    {
        RaycastHit hit;
        //if (Physics.Raycast(this.transform.position, this.transform.forward, out hit, PlushyDetectionLength, PlushyLayer))
        if (Physics.SphereCast(this.transform.position, PlushyDetectionWidth, this.transform.forward, out hit, PlushyDetectionLength, PlushyLayer))
        {
            NearPlushy = (hit.collider.transform.root.tag == "Plush");
            NearKnight = (hit.collider.GetComponentInParent<PlushyAnimator>().IsKnight);
        }
        else
        {
            NearPlushy = false;
            NearKnight = false;
        }
    }

    bool checkIfMovementlessAnimationIsPlaying() 
    {
        Animator animate = TP_Animator.Instance.anim;
        return animate.GetCurrentAnimatorStateInfo(0).IsName("Teddy_Crouch") || 
            animate.GetCurrentAnimatorStateInfo(0).IsName("Teddy_Crouch_Idle") ||
            animate.GetCurrentAnimatorStateInfo(0).IsName("Teddy_Land") ||
            animate.GetCurrentAnimatorStateInfo(0).IsName("Teddy_Mode") ||
            animate.GetCurrentAnimatorStateInfo(0).IsName("Teddy_Mode_Idle") ||
            animate.GetCurrentAnimatorStateInfo(0).IsName("Teddy_Unpausing") ||
            animate.GetCurrentAnimatorStateInfo(0).IsName("Teddy_Pausing") ||
            animate.GetCurrentAnimatorStateInfo(0).IsName("Teddy_Paused") ||
            animate.GetCurrentAnimatorStateInfo(1).IsName("Teddy_Sewing") ||
            animate.GetCurrentAnimatorStateInfo(1).IsName("Teddy_Sewing_Idle") ||
            animate.GetCurrentAnimatorStateInfo(1).IsName("Teddy_Burst") ||
            animate.GetCurrentAnimatorStateInfo(2).IsName("Teddy_Pickup") ||
            animate.GetCurrentAnimatorStateInfo(2).IsName("Teddy_Putdown");
    }

    void ApplyGroundRotation(Vector3 direction, bool impact = false) 
    {
        if(direction != Vector3.zero)
        {
            direction = direction.normalized; //direction vector
            float targetAngle = (Mathf.Atan2(direction.x, direction.z))*Mathf.Rad2Deg; //Angle of the control stick
            float angle = Mathf.LerpAngle(this.transform.eulerAngles.y,targetAngle,Time.deltaTime*RotationSpeed);
            float targetAngle2 = (Mathf.Atan2(direction.x, direction.y)) * Mathf.Rad2Deg; //Angle of the vertical angle
            float angle2 = Mathf.LerpAngle(this.transform.eulerAngles.z, targetAngle2, Time.deltaTime * RotationSpeed);
            //Debug.Log(targetAngle2 + "," + angle2);
            if(impact)
                this.transform.rotation = Quaternion.Euler(angle2, angle, 0f);
            else
                this.transform.rotation = Quaternion.Euler(0f,angle,0f);

            
            //transform.LookAt(direction - transform.position); //change so that it turns slowly
        }
    }
    void ApplyGravity() 
    {
        if(MoveVector.y > -TerminalVelocity) //checks if the speed exceeds the terminal velocity
            MoveVector = new Vector3(MoveVector.x, MoveVector.y - (Gravity * Time.deltaTime), MoveVector.z ); //creates new movevector with gravity increase
        else
            MoveVector = new Vector3(MoveVector.x, -TerminalVelocity, MoveVector.z);
        if (TP_Controller.CharacterController.isGrounded && MoveVector.y < -1) //checks if the character is grounded
        {
            MoveVector = new Vector3(MoveVector.x, -1, MoveVector.z); //Sets the y velocity
        }
    }
    void ApplySlope() 
    {
        if(!TP_Controller.CharacterController.isGrounded)
        {
            return;
        }
        SlideDirection = Vector3.zero; //Zeroes out the slide direction
        RaycastHit hitInfo;
        if(Physics.Raycast(transform.position, Vector3.down * SlopeDetectionRange , out hitInfo, SlopeableObjects)) //origin, direction,save hit information in hitInfo
        {  //Careful with raycasts, they may hit outward polygons on final model.
            //Debug.Log("Slope: " + hitInfo.normal.y);
            if(hitInfo.normal.y < 1f)
            {
                //if (hitInfo.normal.y >= SlideThreshold)
                //{
                    MoveVector = new Vector3(MoveVector.x * hitInfo.normal.y, MoveVector.y, MoveVector.z * hitInfo.normal.y);
                    //MoveVector *= hitInfo.normal.y;
                //}
                //else  //checks if the surface to too steep
                //{
                //    SlideDirection = new Vector3(hitInfo.normal.x, -hitInfo.normal.y, hitInfo.normal.z);
                //    if (SlideDirection.magnitude < MaxControllableSlideMagnitude)
                //    {
                //        //Debug.Log("Steep");
                //        MoveVector += SlideDirection; //forces movement to be slide-like
                //    }
                //    else
                //    {
                //        //Debug.Log("Too Steep");
                //        MoveVector = SlideDirection; //forces you to slide.
                //    }
                //}
            }
            
        }
    }
    public void Jump() 
    {
        if (TP_Controller.CharacterController.isGrounded) //makes character jump if on the ground
        {
            VerticalVelocity = JumpSpeed * JumpCharge;
        }
        JumpCharge = ChargeLowerLimit; //Resets the jumpCharger
    }

    public void AirJump(float speed = 1f)
    {
        if (!TP_Controller.CharacterController.isGrounded) //makes character jump if in the air
        {
            if(speed > 0)
                VerticalVelocity = JumpSpeed * speed * JumpCharge;
        }
    }

    public void PullJump(float speed) //TODO: Get rid of this and apply in Grapple script, with controller script managing how it works
    {
        if (!TP_Controller.CharacterController.isGrounded)
        {
            Vector3 moveVector = (this.transform.forward.normalized + Vector3.up) * speed;
            TP_Controller.CharacterController.Move(moveVector * Time.deltaTime);  //Move character in World Space
        }
    }

    public void CarryOverMovement(float speed)
    {
        if (!TP_Controller.CharacterController.isGrounded)
        {
            Vector3 moveVector = (this.transform.forward.normalized) * speed;
            //MoveVector = moveVector;
            TP_Controller.CharacterController.Move(moveVector * Time.deltaTime);  //Move character in World Space
        }
    }

    public void LedgeJump()
    {
        AerialMoveVector = (this.transform.forward * LedgeJumpSpeed) + (this.transform.up * LedgeJumpSpeed);
    }

    public void JumpCharger() 
    {
        if(JumpCharge <= ChargeUpperLimit)
        {
            JumpCharge += Charge * Time.deltaTime;
            if (JumpCharge > ChargeUpperLimit)
                JumpCharge = ChargeUpperLimit;
        }
    }

    void SnapAlignCharacterWithCamera() //rotates character according to direction of camera
    {
        if(MoveVector.x != 0 || MoveVector.z != 0) //Currently forces character to always be looking ahead during motion (will change)
        {
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, //x rotation of character stays the same
                                                  Camera.main.transform.eulerAngles.y, //y rotation of character changes according camera current direction
                                                  transform.eulerAngles.z); //z rotation of character stays the same
        }
    }

    /*public void SnapAlignCharacterInAir(Vector3 velocity)
    {
        TP_Motor.Instance.AerialMoveVector = velocity;
        this.transform.rotation = Quaternion.Euler
            (0,
            0,
            0);
    }*/

    public void SnapAlignCharacterWithAim() 
    {
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, //x rotation of character stays the same
                                                  Camera.main.transform.eulerAngles.y, //y rotation of character changes according camera current direction
                                                  transform.eulerAngles.z); //z rotation of character stays the same
    }

    public void ResetRotation() //Resets to whatever I feel like. Might add onto this later on 
    {
        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
    }

    public void GrabItem()
    {
        RaycastHit hit;
        Vector3 fwd = this.transform.TransformDirection(Vector3.forward);
        Collider[] grabbableList = Physics.OverlapSphere(GripPosition.position, GrabSize, GrabbingLayers);
        if (grabbableList.Length > 0)
        //if (Physics.Raycast(GripPosition.position - (fwd * GrabSize), fwd, out hit, GrabSize*2))
        {
            
            //if (isColliderPhysicsObject(hit.collider))
            if (isColliderPhysicsObject(grabbableList[0]))
            {
                grabbing = true;
                //GrabbedItem = hit.collider.transform;
                GrabbedItem = grabbableList[0].transform;
                GrabOffset = GrabbedItem.GetComponent<GrabProperties>().GrabOffset;
                GrabbedItem.GetComponent<GrabProperties>().ActivateCollision(false);
                GrabWeight = Mathf.Clamp(GrabbedItem.GetComponent<SoundManager>().Massiveness * GrabWeightMultiplier,1f,Mathf.Infinity);
                GrabbedItem.GetComponent<SoundManager>().Grabbed = true;
                grabbableList[0].attachedRigidbody.useGravity = false;
                //hit.collider.attachedRigidbody.useGravity = false;
                FreezeRotation();
            }
        }
    }

    public void SetGrabbedItemToCollideable()
    {
        if (GrabbedItem != null)
            GrabbedItem.GetComponent<GrabProperties>().ActivateCollision(true);
    }

    public void SetGrabbedItemToNonCollideable()
    {
        if (GrabbedItem != null)
            GrabbedItem.GetComponent<GrabProperties>().ActivateCollision(false);
    }

    public void ReleaseItem()
    {
        if(GrabbedItem != null)
        {
            GrabOffset = Vector3.zero;
            GrabWeight = 1f;
            Rigidbody itemBody = GrabbedItem.GetComponent<Rigidbody>();
            itemBody.useGravity = true;
            itemBody.constraints = RigidbodyConstraints.None;
            GrabbedItem.GetComponent<GrabProperties>().ActivateCollision(true);
            GrabbedItem.GetComponent<SoundManager>().Grabbed = false;
            GrabbedItem = null;
        }
    }

    public void ThrowItem()
    {
        if (GrabbedItem != null)
        {
            GrabOffset = Vector3.zero;
            GrabWeight = 1f;
            Rigidbody itemBody = GrabbedItem.GetComponent<Rigidbody>();
            itemBody.useGravity = true;
            itemBody.constraints = RigidbodyConstraints.None;
            GrabbedItem.GetComponent<GrabProperties>().ActivateCollision(true);
            itemBody.AddForce(Quaternion.AngleAxis(ThrowAngle,this.transform.right) * this.transform.forward * ThrowForce,ForceMode.Impulse);
            GrabbedItem.GetComponent<GrabProperties>().Thrown = true;
            GrabbedItem.GetComponent<SoundManager>().Grabbed = false;
            GrabbedItem = null;
        }
    }

    public void FreezeRotation()
    {
        if (GrabbedItem != null)
        {
            Rigidbody itemBody = GrabbedItem.GetComponent<Rigidbody>();
            if (itemBody.constraints == RigidbodyConstraints.None)
            {
                itemBody.constraints = RigidbodyConstraints.FreezeRotation;
            }
            else if (itemBody.constraints == RigidbodyConstraints.FreezeRotation)
            {
                itemBody.constraints = RigidbodyConstraints.None;
            }
        }
    }
}
