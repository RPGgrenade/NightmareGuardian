using UnityEngine;
using System.Collections;
using System;

public class TP_Movement : MonoBehaviour
{
    public static TP_Movement Instance; //self
    public Transform GrabbedItem;
    public Transform GripPosition;
    public float GrabOffset = 0.5f;
    public float MovementAcceleration = 0.4f;
    public float MoveSpeed = 10f; //walking speed
    public float RunSpeed = 13f; //running speed
    public float PushPower = 10f;
    public float PushResistance = 0.7f;
    public float Gravity = 21f;
    public float TerminalVelocity = 20f;
    public float LedgeJumpSpeed = 4f;
    public float JumpSpeed = 6f;
    public float RotationSpeed = 15f;
    public float SlideThreshold = 0.6f; //between 0 and 1
    public float MaxControllableSlideMagnitude = 0.4f;
    public float RunLimit = 0.6f;
    public float JumpCharge = 0f;
    public float Charge = 0.01f;
    public float ChargeUpperLimit = 1f;
    public float ChargeLowerLimit = 0.6f;
    public float CollisionDamper = 0.5f;
    public float XRotation = 0f;
    public float YRotation = 0f;
    public float ImpactDecrease = 0.5f;

    private float currentAcceleration = 0f;
    public bool grabbing = false;
    public bool gravity = true;

    public Vector3 SlideDirection { get; set; }
    public Vector3 MoveVector { get; set; }
    public Vector3 AerialMoveVector { get; set; }
    public Vector3 AerialRotation { get; set; }
    public float CurrentSpeed { get; set; }
    public float VerticalVelocity { get; set; }

    private CharacterController controller;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        controller = TP_Controller.Instance.GetComponent<CharacterController>();
    }


    public void UpdateMotor()
    {
        SnapAlignCharacterWithCamera();
        if (controller.isGrounded)
        { //If character is grounded, simply replace previous aerial move vector with the current one
            if (Input.GetKey(KeyCode.LeftShift) || Mathf.Abs(Input.GetAxis("Horizontal")) >= RunLimit || Mathf.Abs(Input.GetAxis("Vertical")) >= RunLimit) //sets the CurrentSpeed property to be use dependant
            { CurrentSpeed = RunSpeed; }
            else
            { CurrentSpeed = MoveSpeed; }
        }
        else
        {
            //MoveVector = AerialMoveVector; //Problems when replacing movevector. Issues arise when camera change is applied. Most likely issue with camera.
            CurrentSpeed = MoveSpeed;
        }
        AerialMoveVector = MoveVector;

        if (controller.enabled)
            ProcessMotion();
        if (transform.eulerAngles.x != 0 && !TP_Grapple.Instance.IsGrappling)
            ResetRotation();

        if (GrabbedItem != null)
        {
            //grabbing = true;
            Quaternion properRotation = Quaternion.Euler(GrabbedItem.GetComponent<GrabProperties>().GrabRotation);
            GrabbedItem.rotation = Quaternion.RotateTowards(GrabbedItem.rotation, properRotation, 300f * Time.deltaTime);
            GrabbedItem.position = GripPosition.position + (Vector3.up * GrabOffset);
        }
    }

    void OnTriggerEnter(Collider coll)
    {
        if (this.tag == "Light" && coll.gameObject.tag == "Eyeball")
        {
            //Debug.Log("I'M BEING BLINDED");
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!controller.isGrounded && MoveVector.y > 0) //Might change so it affects no matter MoveVector's y value
        {
            MoveVector = new Vector3(MoveVector.x, 0, MoveVector.z); //this halts vertical motion upwards in midair
        }
        if (hit.collider.attachedRigidbody != null && hit.collider.tag != "Monster" && hit.collider.tag != "Blade") //This implies a physics object 
        {
            hit.collider.GetComponent<Rigidbody>().velocity = (hit.moveDirection * PushPower) / hit.collider.GetComponent<Rigidbody>().mass; //Pushes a physics object it comes in contact with.
        }

        if (hit.collider.tag == "Monster")
            this.transform.parent = hit.collider.transform.parent;
        else
            this.transform.parent = null;
    }
    void OnCollisionEnter(Collision collision)
    {
        ApplyKnockback(collision.collider.attachedRigidbody.velocity);
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "Monster")
        {
            Debug.Log("Touching Monster");
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Monster")
        {
            Debug.Log("Not Touching Monster");
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

    public void ApplyKnockback(Vector3 impactSpeed, bool Attack = false)
    {
        if (!Attack)
            controller.Move((impactSpeed / PushResistance) * Time.fixedDeltaTime); //gets pushed back if physics object collides with character
        else
            controller.Move(impactSpeed * Time.fixedDeltaTime);
    }
    void ProcessMotion()
    {
        MoveVector = transform.TransformDirection(MoveVector);  //Transform MoveVector to World Space
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
        //if (controller.isGrounded)
        //{
        //}
        if (!checkIfMovementlessAnimationIsPlaying())
        {
            ApplyGroundRotation(MoveVector);
            AerialRotation = transform.rotation.eulerAngles;
            //if (controller.enabled)
            //    this.transform.rotation = Quaternion.LookRotation(this.transform.f, Vector3.up);
        }
        else
        {
            transform.rotation = Quaternion.Euler(AerialRotation);
        }
        ApplySlide();//apply sliding if applicable
        MoveVector *= CurrentSpeed;
        MoveVector = new Vector3(MoveVector.x * currentAcceleration, VerticalVelocity, MoveVector.z * currentAcceleration);  //reapply vertical velocity to MoveVector.y
        if (gravity)
            ApplyGravity(); //Apply gravity
        if (!checkIfMovementlessAnimationIsPlaying())
        {
            //controller.Move(MoveVector * Time.deltaTime);//Move character in World Space
            this.GetComponent<Rigidbody>().AddForce(MoveVector * Time.deltaTime,ForceMode.Acceleration);
        }
    }
    bool checkIfMovementlessAnimationIsPlaying()
    {
        Animator animate = TP_Animator.Instance.anim;
        return animate.GetCurrentAnimatorStateInfo(0).IsName("Teddy_Crouch") ||
            animate.GetCurrentAnimatorStateInfo(0).IsName("Teddy_Crouch_Idle") ||
            animate.GetCurrentAnimatorStateInfo(0).IsName("Teddy_Stab") ||
            animate.GetCurrentAnimatorStateInfo(0).IsName("Teddy_Mode_Idle") ||
            animate.GetCurrentAnimatorStateInfo(0).IsName("Teddy_Shield") ||
            animate.GetCurrentAnimatorStateInfo(0).IsName("Teddy_Sewing") ||
            animate.GetCurrentAnimatorStateInfo(0).IsName("Teddy_Sewing_Idle") ||
            animate.GetCurrentAnimatorStateInfo(0).IsName("Teddy_Cover") ||
            animate.GetCurrentAnimatorStateInfo(0).IsName("Teddy_Ledge_Grab") ||
            animate.GetCurrentAnimatorStateInfo(0).IsName("Teddy_Ledge_Idle");
    }
    void ApplyGroundRotation(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            //direction = direction.normalized; //direction vector
            //float targetAngle = (Mathf.Atan2(direction.x, direction.z))*Mathf.Rad2Deg; //Angle of the control stick
            //float angle = Mathf.LerpAngle(transform.eulerAngles.y,targetAngle,Time.deltaTime*RotationSpeed);
            //transform.rotation = Quaternion.Euler(0f,angle,0f);

            transform.LookAt(transform.position + direction); //change so that it turns slowly
        }
    }
    void ApplyGravity()
    {
        if (MoveVector.y > -TerminalVelocity) //checks if the speed exceeds the terminal velocity
        {
            MoveVector = new Vector3(MoveVector.x, MoveVector.y - (Gravity * Time.deltaTime), MoveVector.z); //creates new movevector with gravity increase
        }
        if (controller.isGrounded && MoveVector.y < -1) //checks if the character is grounded
        {
            MoveVector = new Vector3(MoveVector.x, -1, MoveVector.z); //Sets the y velocity
        }
    }
    void ApplySlide()
    {
        if (!controller.isGrounded)
        {
            return;
        }
        SlideDirection = Vector3.zero; //Zeroes out the slide direction
        RaycastHit hitInfo;
        if (Physics.Raycast(transform.position, Vector3.down, out hitInfo)) //origin, direction,save hit information in hitInfo
        {  //Careful with raycasts, they may hit outward polygons on final model.
            if (hitInfo.normal.y < SlideThreshold) //checks if the surface to too steep
            {
                SlideDirection = new Vector3(hitInfo.normal.x, -hitInfo.normal.y, hitInfo.normal.z);
                if (SlideDirection.magnitude < MaxControllableSlideMagnitude)
                {
                    //Debug.Log("Steep");
                    MoveVector += SlideDirection; //forces movement to be slide-like
                }
                else
                {
                    //Debug.Log("Too Steep");
                    MoveVector = SlideDirection; //forces you to slide.
                }
            }
        }
    }
    public void Jump()
    {
        if (controller.isGrounded) //makes character jump if on the ground
        {
            VerticalVelocity = JumpSpeed * JumpCharge;
        }
        JumpCharge = ChargeLowerLimit; //Resets the jumpCharger
    }

    public void LedgeJump()
    {
        AerialMoveVector = (this.transform.forward * LedgeJumpSpeed) + (this.transform.up * LedgeJumpSpeed);
    }

    public void JumpCharger()
    {
        if (controller.isGrounded && JumpCharge <= ChargeUpperLimit)
        {
            JumpCharge += Charge;
        }
    }

    void SnapAlignCharacterWithCamera() //rotates character according to direction of camera
    {
        if (MoveVector.x != 0 || MoveVector.z != 0) //Currently forces character to always be looking ahead during motion (will change)
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
        transform.rotation = Quaternion.Euler(Camera.main.transform.eulerAngles.x, //x rotation of character stays the same
                                                  Camera.main.transform.eulerAngles.y, //y rotation of character changes according camera current direction
                                                  transform.eulerAngles.z); //z rotation of character stays the same
    }

    public void ResetRotation() //Resets to whatever I feel like. Might add onto this later on 
    {
        transform.rotation = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
    }

    public void GrabItem()
    {
        RaycastHit hit;
        float rayLength = 0.4f;
        Vector3 fwd = this.transform.TransformDirection(Vector3.forward);
        if (Physics.Raycast(GripPosition.position - (fwd * rayLength), fwd, out hit, rayLength * 2))
        {
            if (isColliderPhysicsObject(hit.collider))
            {
                grabbing = true;
                GrabbedItem = hit.collider.transform;
                //GrabOffset = GrabbedItem.GetComponent<GrabProperties>().DistanceFromCenter;
                hit.collider.attachedRigidbody.useGravity = false;
                FreezeRotation();
            }
        }
    }

    public void ReleaseItem()
    {
        if (GrabbedItem != null)
        {
            Rigidbody itemBody = GrabbedItem.GetComponent<Rigidbody>();
            itemBody.useGravity = true;
            itemBody.constraints = RigidbodyConstraints.None;
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
