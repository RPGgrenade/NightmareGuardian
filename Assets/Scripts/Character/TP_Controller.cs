using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class TP_Controller : MonoBehaviour 
{
    public static CharacterController CharacterController;
    public static TP_Controller Instance;

    [Header("Tracked objects")]
    public Transform NightLight;

    [Header("Inputs")]
    public float StickHorizontal = 0f;
    public float StickVertical = 0f;
    [Space(10)]
    public bool JumpDown;
    public bool Jumping;
    public bool JumpUp;
    [Space(10)]
    public bool GrappleDown;
    public bool Grapple;
    public bool GrappleUp;
    [Space(10)]
    public bool CrouchDown;
    public bool Crouch;
    public bool CrouchUp;
    [Space(10)]
    public bool Needle;
    public bool Shield;
    public bool Blanket;
    public bool TeddyMode;
    public bool CamReset;
    public bool Pausing;

    [Header("Timers")]
    [Tooltip("Timer for undoing teddy mode")]
    public float TeddyTimer = 1f;
    [Tooltip("Timer for resetting scene upon death")]
    public float ResetTime = 3f;

    [Header("Teching Numbers")]
    [Tooltip("Time (in Frames) where you can hit the jump button to tech impact against a surface")]
    public int TechBuffer = 14;
    [Tooltip("NOT IMPLEMENTED Time (in Frames) where you can't attempt teching again in case you miss")]
    public int TechLockout = 30;

    [Header("Movement Capacities")]
    public bool MovementEnabled = true;
    public bool paused = false;
    [Tooltip("Amount of time crouch has to be pressed less than in order to sprint")]
    public float SprintBuffer = 0.1f;
    public bool LockCursor = true;
    public bool HideCursor = true;

    private float teddyTimer = 0f;
    private int buffer = 0;
    private float sprintBuffer = 0f;
    private float resetTimer = 0f;
    private bool resetting = false;
    private bool black = false;

    void Awake () //Setting the feilds
    {
        Cursor.visible = !HideCursor;
        if (!Application.isEditor && LockCursor)
            Cursor.lockState = CursorLockMode.Locked;
        CharacterController = GetComponent("CharacterController") as CharacterController;
        Instance = this;
        TP_Camera.UseExistingOrCreateNewMainCamera();
	}

    void FixedUpdate()
    {
        TP_Grapple.Instance.UpdateGrapplePhysics();
        TP_Supers.Instance.SupersUpdate();
        TP_Animator.Instance.UpdateChargeSpeeds();
    }

    void Update () 
    {
        if (buffer > 0)
            buffer -= 1;
        if (teddyTimer >= 0)
            teddyTimer -= Time.deltaTime;
        if (black)
        {
            resetTimer += Time.deltaTime;
            if (resetTimer > ResetTime)
                ResetScene();
        }

        if (Camera.main == null)
        {
            return;
        }
        if (MovementEnabled)
            GetLocomotionInput();
        if (!paused && TP_Animator.Instance.MoveAction != TP_Animator.MovingAction.Dead)
            HandleActionInput();
        if (teddyTimer < 0)
            TP_Motor.Instance.UpdateMotor();
        TP_Animator.Instance.DetermineCurrentMotionAction(); //Sets the state within the animator so that it can animate it

        TP_Grapple.Instance.UpdateGrapple();

        if (paused)
        {
            TP_Animator.Instance.action = TP_Animator.Action.Pausing;
            AimChanger.Instance.SetAim(AimChanger.Aim.Pause);
        }
        else if (!paused && AimChanger.Instance.AimType == AimChanger.Aim.Pause)
        {
            AimChanger.Instance.SetAim(AimChanger.Aim.Default);
            Pause.Instance.ToggleMenu(Pause.Instance.Menus[0]);
        }
        //TP_Supers.Instance.SupersUpdate();
        TP_Animator.Instance.UpdateAnimator();
        //Reference for gamepad input
	}

    void LateUpdate()
    {
    }

    public float GetTechBuffer()
    {
        return buffer;
    }

    void GetLocomotionInput() 
    {
        StickHorizontal = Input.GetAxisRaw("Horizontal");
        StickVertical = Input.GetAxisRaw("Vertical");

        var deadZone = 0.1f; //could set to public field if several platform access is necesary
        TP_Motor.Instance.VerticalVelocity = TP_Motor.Instance.MoveVector.y;
        TP_Motor.Instance.MoveVector = Vector3.zero; //turns it to zero to prevent additives
        if (StickVertical > deadZone || StickVertical < -deadZone)
        {
            if (!TP_Animator.Instance.TeddyMode)
            {
                TP_Motor.Instance.MoveVector += new Vector3(0, 0, StickVertical); //Sets vertical axis to Z axis
                TP_Grapple.Instance.SwingVector += new Vector3(0, 0, StickVertical);
            }
            if (TP_Animator.Instance.TeddyMode && teddyTimer < 0)
                ToggleTeddy();
        }
        if ((StickHorizontal > deadZone || StickHorizontal < -deadZone))// && CharacterController.isGrounded)
        {
            if (!TP_Animator.Instance.TeddyMode)
            {
                TP_Motor.Instance.MoveVector += new Vector3(StickHorizontal, 0, 0); //Sets horizontal axis to X axis
                TP_Grapple.Instance.SwingVector += new Vector3(StickHorizontal, 0, 0);
            }
            if (TP_Animator.Instance.TeddyMode && teddyTimer < 0)
                ToggleTeddy();
        }
    }
    void HandleActionInput() 
    {
        TP_Animator.Instance.action = TP_Animator.Action.NoAction;
        TP_Animator.Instance.direction = TP_Animator.ActionDirection.None;
        if (Input.GetAxis("Dpad Horizontal") > 0)
            TP_Animator.Instance.direction = TP_Animator.ActionDirection.Right;
        else if(Input.GetAxis("Dpad Horizontal") < 0)
            TP_Animator.Instance.direction = TP_Animator.ActionDirection.Left;
        if (Input.GetAxis("Dpad Vertical") > 0)
            TP_Animator.Instance.direction = TP_Animator.ActionDirection.Up;
        else if (Input.GetAxis("Dpad Vertical") < 0)
            TP_Animator.Instance.direction = TP_Animator.ActionDirection.Down;

        JumpDown = Input.GetButtonDown("Jump");
        Jumping = Input.GetButton("Jump");
        JumpUp = Input.GetButtonUp("Jump");

        GrappleDown = Input.GetButtonDown("Grapple");
        Grapple = Input.GetButton("Grapple");
        GrappleUp = Input.GetButtonUp("Grapple");

        CrouchDown = Input.GetButtonDown("Crouch");
        Crouch = Input.GetButton("Crouch");
        CrouchUp = Input.GetButtonUp("Crouch");

        Needle = Input.GetButton("Needle");
        Shield = Input.GetButtonDown("Light");
        Blanket = Input.GetButtonDown("Blanket");
        TeddyMode = Input.GetButtonDown("TeddyMode");
        CamReset = Input.GetButton("CamReset");
        Pausing = Input.GetButtonDown("Pause");

        if (TP_Animator.Instance.crouch != TP_Animator.CrouchState.Crouching && !TP_Animator.Instance.TeddyMode)
        {
            if (JumpDown && TP_Motor.Instance.knocked)
            {
                if (buffer <= 0)
                    buffer = TechBuffer;
            }
            if (Jumping && !TP_Motor.Instance.grabbing)
            {
                JumpCharge();
                TP_Animator.Instance.action = TP_Animator.Action.Charging;
            }
            if (JumpUp)
            {
                if(!TP_Motor.Instance.grabbing)
                    Jump();
                if (TP_Grapple.Instance.IsGrappling && !CharacterController.isGrounded)
                {
                    //GrappleSetting();
                    GrappleRemoving();
                    //TP_Animator.Instance.action = TP_Animator.Action.Lasso;
                }
                if (TP_Motor.Instance.grabbing)
                {
                    TP_Motor.Instance.grabbing = false;
                    TP_Motor.Instance.throwing = true;
                }
            }
        }
        if (CrouchDown)
        {
            if (TP_Motor.Instance.grabbing)
            {
                TP_Motor.Instance.grabbing = false;
            }
        }
        if (Crouch) 
        {
            sprintBuffer += Time.deltaTime;
            TP_Animator.Instance.crouch = TP_Animator.CrouchState.Crouching;
            if (Jumping)
            {
                TP_Animator.Instance.action = TP_Animator.Action.Grabbing;
            }
        }
        if(CrouchUp)
        {
            if (sprintBuffer <= SprintBuffer && CharacterController.isGrounded 
                && TP_Motor.Instance.SprintCooldownTimer <= 0f)
            {
                TP_Motor.Instance.SprintTimer = TP_Motor.Instance.SprintTime;
                TP_Motor.Instance.SprintCooldownTimer = TP_Motor.Instance.SprintCooldown;
            }
            sprintBuffer = 0f;
            TP_Animator.Instance.crouch = TP_Animator.CrouchState.Standing;
        }

        #region Grappling
        if (TP_Animator.Instance.HasSpindle && TP_Animator.Instance.HasNeedle && !RagdollHelper.Instance.Ragdoll) //needs the needle and the spindle
        {
            if (GrappleDown)
            {
                if (!TP_Grapple.Instance.IsGrappling && CharacterController.isGrounded)
                    TP_Camera.Instance.Reset();
            }
            if (Grapple)
            {
                if (!TP_Grapple.Instance.IsGrappling)
                {
                    AimChanger.Instance.SetAim(AimChanger.Aim.Grapple);
                    //if(CharacterController.isGrounded)
                        TP_Motor.Instance.SnapAlignCharacterWithAim();
                }
                    TP_Animator.Instance.action = TP_Animator.Action.Lasso;
            }
            if (GrappleUp)
            {
                //GrappleSetting();
                GrappleRemoving();
            }
        }
        #endregion

        #region Primary Actions
        if (Needle && TP_Animator.Instance.HasNeedle)
        {
            bool isCrouching = TP_Animator.Instance.crouch == TP_Animator.CrouchState.Crouching;
            bool hasBag = TP_Animator.Instance.HasBag;
            if(hasBag || (!hasBag && !isCrouching))
                TP_Animator.Instance.action = TP_Animator.Action.UseNeedle;
        }
        if (Shield && TP_Animator.Instance.HasNightLight)
        {
            TP_Animator.Instance.action = TP_Animator.Action.UseLight;
        }
        if (Blanket && TP_Animator.Instance.HasCape)
        {
            TP_Animator.Instance.action = TP_Animator.Action.UseBlanket;
        }
        #endregion

        #region Misc Actions
        if(TeddyMode)
        {
            if(teddyTimer < 0)
                ToggleTeddy();
        }
        if(CamReset || Input.GetKey(KeyCode.R))
        {
            ResetCamera();
        }
        if (Pausing && this.transform.parent == null 
            && CharacterController.isGrounded && !TP_Animator.Instance.TeddyMode)
        {
            TP_Camera.Instance.Reset();
            paused = !paused;
        }
        #endregion
    }
    void Jump() 
    {
        TP_Motor.Instance.Jump();
    }
    void JumpCharge() 
    {
        TP_Motor.Instance.JumpCharger();
    }
    void ResetCamera() 
    {
        if(!paused)
            TP_Camera.Instance.Reset();
    }
    
    public void GrappleRemoving()
    {
        if (TP_Grapple.Instance.IsGrappling)
        {
            TP_Grapple.Instance.DestroyGrapple();
            TP_Grapple.Instance.IsGrappling = false;
        }
        AimChanger.Instance.SetAim(AimChanger.Aim.Default);
    }

    public void GrappleSetting()
    {
        if (!TP_Grapple.Instance.IsGrappling)
        {
            if (TP_Grapple.Instance.HandGrip == null)
                TP_Grapple.Instance.CreateNewCharacterJoint();
            TP_Grapple.Instance.IsGrappling = true;
            AimChanger.Instance.SetAim(AimChanger.Aim.Default);
            TP_Camera.Instance.Invoke("Reset", 0.5f);
        }
        //else
        //{
        //    TP_Grapple.Instance.DestroyGrapple();
        //    TP_Grapple.Instance.IsGrappling = false;
        //    AimChanger.Instance.SetAim(AimChanger.Aim.Default);
        //}
    }

    public void ToggleTeddy()
    {
        if (CharacterController.isGrounded)
        {
            if (!TP_Animator.Instance.TeddyMode)
                teddyTimer = TeddyTimer;
            TP_Animator.Instance.TeddyMode = !TP_Animator.Instance.TeddyMode;
        }
    }

    public void TurnOffTeddy()
    {
        TP_Animator.Instance.TeddyMode = false;
    }

    public void ResetScene()
    {
        if (!resetting)
        {
            resetting = true;
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        }
    }

    public void FadeToBlack()
    {
        if (!black)
        {
            black = true;
            FadeManager.Instance.ChangeFading();
        }
    }

    public void Death()
    {
        TP_Animator.Instance.TeddyMode = true;
        RagdollHelper.Instance.Ragdoll = true;
        RagdollHelper.Instance.GetRagdollState();
        RagdollHelper.Instance.SwitchWeaponPhysics();
        FadeToBlack();
    }
}
