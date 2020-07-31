using UnityEngine;
using System.Collections;

public class TP_Animator : MonoBehaviour 
{       //Class for determining what state of motion the character is in, and uses it to determine which animation to use.
    public enum MovingAction 
    {   //Issues with setting the uphill and downhill states.
        Dead,
        Stationary, Walk, Run, LeftTurn, RightTurn, LeftRunTurn, RightRunTurn, TurnAround,//Motion animations
        Jumping, Falling, Landing, LeftSpin, RightSpin, ForeFlip, BackFlip,//Aerial Motion Animations
        WalkUpHill, WalkDownHill, Sliding,//Misc motion animations
        LedgeGrab, LedgeFall, //Ledge motion animations
        action
    }
    public enum Action 
    {
        NoAction,
        TeddyMode, Charging, Grabbing, Throwing, //Misc action animations
        UseBlanket, //Blanket related Animations
        UseLight,  //Night Light Related Animations
        UseNeedle, //Add in fighting state animations as well
        Lasso, Grappling, Pulling,
        Pausing
        
    }
    public enum CrouchState
    {
        Standing, Crouching
    }
    public enum ActionDirection
    {
        None, Up, Down, Right, Left
    }

    public static TP_Animator Instance;
    public Animator anim;

    [Header("Objects for Tutorial")]
    public GameObject Bag;
    public GameObject Cape;
    public GameObject Needle;
    public GameObject Spindle;
    public GameObject NightLight;

    [Header("Trackers for Tutorial")]
    public bool HasBag = false;
    public bool HasCape = false;
    public bool HasNeedle = false;
    public bool HasSpindle = false;
    public bool HasNightLight = false;

    [Header("Idle properties")]
    [Tooltip("How long to wait for an idle animation to play")]
    public float IdleTime = 6f;
    [Tooltip("How long to wait for the sitting animation to play")]
    public float IdleSitTime = 20f;
    public float walkSpeed = 1f;
    [Tooltip("Total possible animations to play at random")]
    public int idleCount = 1;

    [Header("Teddy stuff")]
    public bool TeddyMode = false;

    [Header("Attack Charge Speeds")]
    public float StabChargeSpeed = 0.2f;
    public float BlockChargeSpeed = 0.2f;
    public float CoverChargeSpeed = 0.2f;

    [Header("Animation Limiters")]
    public bool CanUseStandardAttacks = true;
    public bool CanPickup = true;
    public bool CanSpecialAttack = true;

    public MovingAction MoveAction { get; set; }
    public Action action { get; set; }
    public CrouchState crouch { get; set; }
    public ActionDirection direction { get; set; }


    private float idleTimer;
    private float idleSitTimer;
    private int currentIdle = 1;

    //Boolean tutorial trackers for animation purposes
    private bool bagDone = false;
    private bool capeDone = false;
    private bool needleDone = false;
    private bool spindleDone = false;
    private bool lightDone = false;

	void Start () 
    {
        Instance = this;
        MoveAction = MovingAction.Stationary;
        action = Action.NoAction;
        anim = this.GetComponent<Animator>();
        idleSitTimer = IdleSitTime;
        if(SettingsHandler.Instance != null && SettingsHandler.Instance.TutorialDone)
        {
            HasBag = true;
            HasNightLight = true;
            HasCape = true;
            HasSpindle = true;
            HasNeedle = true;
        }
	}

    public void SwitchBag()
    {
        HasBag = !HasBag;
    }

    public void SwitchCape()
    {
        HasCape = !HasCape;
    }

    public void SwitchNeedle()
    {
        HasNeedle = !HasNeedle;
    }

    public void SwitchSpindle()
    {
        HasSpindle = !HasSpindle;
    }

    public void SwitchNightLight()
    {
        HasNightLight = !HasNightLight;
    }

    private void updateTutorial()
    {
        Bag.SetActive(HasBag);
        Cape.SetActive(HasCape);
        //Needle.SetActive(HasNeedle); //causes lag
        Spindle.SetActive(HasSpindle);
        NightLight.SetActive(HasNightLight);
        if (!SettingsHandler.Instance.TutorialDone)
        {
            bool tutorialDone = HasBag && HasCape && HasNeedle && HasSpindle && HasNightLight;
            SettingsHandler.Instance.TutorialDone = tutorialDone;
        }
    }

    public void UpdateAnimator () 
    {
        updateTutorial();
        if (action == Action.UseBlanket || action == Action.UseLight || action == Action.UseNeedle) // || action == Action.Grappling)
        {
            MoveAction = MovingAction.action;
        }
        walkSpeed = (TP_Controller.CharacterController.velocity.magnitude);
        if (!TeddyMode)
        {
            if (MoveAction == MovingAction.Stationary)
            {
                idleTimer -= Time.deltaTime;
                idleSitTimer -= Time.deltaTime;
                if (idleTimer <= 0)
                {
                    idleTimer = IdleTime;
                    currentIdle = (int)Random.Range(1, idleCount + 1);
                }
            }
            else
            {
                idleTimer = IdleTime;
                idleSitTimer = IdleSitTime;
            }
        }
        else 
        {
            action = Action.TeddyMode;
        }
        if(TP_Stats.Instance.CottonGauge == 0 || TP_Stats.Instance.Noise == TP_Stats.Instance.MaxNoise)
        {
            MoveAction = MovingAction.Dead;
            TP_Motor.Instance.IFrames = 1f;
        }
        anim.SetBool("Still", MoveAction == MovingAction.Stationary); //Still
        anim.SetInteger("IdleNumber",currentIdle);
        anim.SetFloat("Idle", idleTimer);
        anim.SetFloat("Sit", idleSitTimer);

        anim.SetFloat("Hitstun", TP_Motor.Instance.KnockedTime);
        anim.SetBool("Knocked", TP_Motor.Instance.knocked);
        anim.SetBool("Colliding", TP_Motor.Instance.colliding);
        anim.SetBool("Grounded", TP_Controller.CharacterController.isGrounded);
        anim.SetBool("Crouch", crouch == CrouchState.Crouching 
            || isPlayingAnimation("Teddy_Burst",3)
            || isPlayingAnimation("Teddy_Turtle", 3)
            || isPlayingAnimation("Teddy_Sewing", 3)
            || isPlayingAnimation("Teddy_Sewing_Idle", 3)
            || isPlayingAnimation("Teddy_Pickup", 5)
            || isPlayingAnimation("Teddy_Putdown", 5)); //Crouching or in a crouch Powered Animation

        anim.SetFloat("WalkSpeed",walkSpeed); //Set speed of walking animation
        anim.SetBool("Walk", MoveAction == MovingAction.Walk); //Walking
        anim.SetBool("Run", MoveAction == MovingAction.Run); //Running
        anim.SetBool("Sprint", TP_Motor.Instance.SprintTimer > 0f);

        anim.SetBool("Fall", MoveAction == MovingAction.Falling); //Falling
        anim.SetFloat("FallSpeed", Mathf.Clamp(TP_Motor.Instance.TerminalVelocity-TP_Motor.Instance.FallSpeed,3f,35f)); //Falling Speed
        anim.SetBool("Teched", TP_Controller.Instance.GetTechBuffer() > 0f); //Is in tech window
                                                                             //anim.SetBool("Ledge", MoveAction == MovingAction.LedgeGrab);
        anim.SetBool("Jump", MoveAction == MovingAction.Jumping);//Jumping
        anim.SetBool("Charge", action == Action.Charging);//Charging

        anim.SetBool("Needle",action == Action.UseNeedle); //Stabbing
        anim.SetBool("Light",action == Action.UseLight); //Shielding
        anim.SetBool("Blanket",action == Action.UseBlanket); //Covering up
        anim.SetBool("Teddy", action == Action.TeddyMode); //Teddy Mode

        anim.SetBool("Grabbing", action == Action.Grabbing); //Grabbing
        anim.SetBool("Throwing", action == Action.Throwing); //Throwing

        anim.SetBool("Lasso", action == Action.Lasso); //Lassoing
        anim.SetBool("Tethered", TP_Grapple.Instance.IsGrappling); //Tethered
        anim.SetBool("Grapple", action == Action.Grappling); //Grappling

        anim.SetBool("Pause", action == Action.Pausing); //Pausing
        //anim.SetBool("Up", direction == ActionDirection.Up); //Up
        //anim.SetBool("Down", direction == ActionDirection.Down); //Down
        //anim.SetBool("Left", direction == ActionDirection.Left); //Left
        //anim.SetBool("Right", direction == ActionDirection.Right); //Right

        anim.SetBool("Dead", MoveAction == MovingAction.Dead);//Dead
        anim.SetBool("Cotton", TP_Stats.Instance.CottonGauge > 0);
        anim.SetBool("Noise", TP_Stats.Instance.Noise == TP_Stats.Instance.MaxNoise);

        anim.SetBool("Super Jump", TP_Supers.Instance.SuperJump);
        //anim.SetFloat("Block Speed", TP_Supers.Instance.ChargeBlock ? BlockChargeSpeed : 1f);
        //anim.SetFloat("Parry Speed", TP_Supers.Instance.ChargeStab ? StabChargeSpeed : 1f);
        //anim.SetFloat("Cover Speed", TP_Supers.Instance.ChargeCover ? CoverChargeSpeed : 1f);
        anim.SetBool("Super String", TP_Supers.Instance.SuperString);

        anim.SetBool("Near Plushy", TP_Motor.Instance.NearPlushy);
        anim.SetBool("Near Knight", TP_Motor.Instance.NearKnight);
        anim.SetBool("Can Attack", CanUseStandardAttacks);
        anim.SetBool("Can Pickup", CanPickup);
        anim.SetBool("Can Special", CanSpecialAttack);// && !isLanding());
    }

    public void UpdateChargeSpeeds()
    {
        anim.SetFloat("Block Speed", TP_Supers.Instance.ChargeBlock ? BlockChargeSpeed : 1f);
        anim.SetFloat("Parry Speed", TP_Supers.Instance.ChargeStab ? StabChargeSpeed : 1f);
        anim.SetFloat("Cover Speed", TP_Supers.Instance.ChargeCover ? CoverChargeSpeed : 1f);
    }

    private bool isPlayingAnimation(string name, int layer)
    {
        return anim.GetCurrentAnimatorStateInfo(layer).IsName(name);
    }

    public void DetermineCurrentMotionAction() 
    {
        var grab = false;
        var throwing = false;
        var forward = false;
        var grounded = false;
        var up = false;
        var down = false;
        var weightless = false;
        var upHill = false;
        var downHill = false;
        if (TP_Motor.Instance.grabbing)
            grab = true;
        if (TP_Motor.Instance.throwing)
            throwing = true;
        if(TP_Motor.Instance.MoveVector.z != 0 || TP_Motor.Instance.MoveVector.x != 0)
            forward = true;
        if (TP_Controller.CharacterController.isGrounded)
            grounded = true;
        if (TP_Motor.Instance.VerticalVelocity > 0)
            up = true;
        if (TP_Motor.Instance.VerticalVelocity < 0)
            down = true;
        if (TP_Motor.Instance.VerticalVelocity == -1 && !TP_Controller.CharacterController.isGrounded)
            weightless = true;
        //if()
        //    upHill = true;
        //if()
        //    downHill = true;

        if (grounded) //All actions set for the ground
        {
            if (grab)
                action = Action.Grabbing;
            if (throwing)
                action = Action.Throwing;
            if (downHill)
                MoveAction = MovingAction.WalkDownHill;
            else if (upHill)
                MoveAction = MovingAction.WalkUpHill;
            else if (forward && TP_Motor.Instance.CurrentSpeed > 0) //Add in LeftTurn and RightTurn
                //Changes for future
                if (TP_Motor.Instance.CurrentSpeed < TP_Motor.Instance.RunSpeed)
                    MoveAction = MovingAction.Walk;
                else
                    MoveAction = MovingAction.Run;
            //end of Changes for future
            else if (MoveAction == MovingAction.Falling)
                MoveAction = MovingAction.Landing;
            else
                MoveAction = MovingAction.Stationary;
        }
        else //All actions set for in the Air
        {
            if (up)
                MoveAction = MovingAction.Jumping;
            else if (down && !TP_Controller.Instance.paused)
                MoveAction = MovingAction.Falling;
            else if(weightless && !TP_Controller.Instance.paused)
                MoveAction = MovingAction.LedgeFall;
        }
    }
}
