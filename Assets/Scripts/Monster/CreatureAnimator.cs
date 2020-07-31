using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Patrol))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class CreatureAnimator : MonoBehaviour {
    
    [Header("Speeds & Attacks")]
    [Tooltip("For the sake of use in a random scenario where walkspeed must be changed manually")]
    public float WalkSpeed = 1f; //may not actually be necessary, but could be for specific cases
    [Tooltip("In case of a specific scenario in which an attack speed variable is necessary")]
    public float AttackSpeed = 1f;
    [Tooltip("Used for whenever there's more than one type of attack that works for a given scenario")]
    [Range(1,50)]
    public int RandomAttackQuantity = 1;

    private bool isMoving;
    private bool isDead;
    private bool isCloseToNoiseObject;
    private bool isJumping;
    private bool isStaring;
    private bool isOffensive;
    private bool isRotatingRight;
    private bool isRotatingLeft;

    private bool movingUp = false;
	private float moveSpeed;
    private float distanceFromTarget;
    private float targetDistanceFromHead;
    private float targetFacing;
    private float lifePercentage;
    private float parryCooldown = 1f;
    private float tugCooldown = 7f;

    public bool stab { get; set; }
    public bool pain { get; set; }
    public bool parry { get; set; }
    public bool tug { get; set; }

    private bool joyous = false;
    private bool angry = false;
    private bool depressed = false;
    private bool intimidated = false;
    private bool bored = false;
    private bool stunned = false;


    private NavMeshAgent agent;
    public Animator anim;
    private TurnToLook vision;
    private CreatureMind mind;
    private CreatureStats stats;
    private CreatureCollision collision;
    private Patrol patrol;

    void Start ()
    {
        agent = this.GetComponent<NavMeshAgent>();
        anim = this.GetComponent<Animator>();
        vision = this.GetComponentInChildren<TurnToLook>();
        mind = this.GetComponent<CreatureMind>();
        stats = this.GetComponent<CreatureStats>();
        collision = this.GetComponent<CreatureCollision>();
        patrol = this.GetComponent<Patrol>();
    }
	
	void Update ()
    {
        if (parry)
        {
            parryCooldown -= Time.deltaTime;
            if (parryCooldown <= 0)
            {
                parryCooldown = 1f;
                parry = false;
            }
        }

        if (tugCooldown > 0)
            tugCooldown -= Time.deltaTime;

		CheckStates();
        UpdateAnimator();
	}
	
	private void CheckStates()
    {
        WalkSpeed = patrol.SpeedMultiplier;
		moveSpeed = agent.velocity.magnitude;
        if (patrol.VerticalSpeed > 0)
            movingUp = true;
        else if (patrol.VerticalSpeed < 0)
            movingUp = false;
        isMoving = moveSpeed > 0f;

        lifePercentage = stats.GetHealthPercentage();
        isDead = stats.GetHealthPercentage() <= 0f;

        distanceFromTarget = (vision.TargetToKill.position - this.transform.position).magnitude;
        targetDistanceFromHead = vision.DistanceFromTarget;
        targetFacing = vision.BodyFacingTarget;

        joyous = mind.state == CreatureMind.CreatureState.Joyous;
        angry = mind.state == CreatureMind.CreatureState.Angry;
        depressed = mind.state == CreatureMind.CreatureState.Depressed;
        intimidated = mind.state == CreatureMind.CreatureState.Intimidated;
        bored = mind.state == CreatureMind.CreatureState.Bored;
        stunned = mind.state == CreatureMind.CreatureState.Stunned;

        isOffensive = mind.AttackTimer >= mind.AttackThreshold;
        isCloseToNoiseObject = patrol.CloseToNoisePoint;
        isJumping = patrol.OnNavMeshLink;
        isStaring = vision.InSight;
        isRotatingRight = patrol.IsRotatingRight;
        isRotatingLeft = patrol.IsRotatingLeft;
	}

    private void UpdateAnimator()
    {
        anim.SetFloat("Walk Animation Speed", WalkSpeed);
		anim.SetFloat("Move Speed", moveSpeed);
        anim.SetBool("Rotating Right", isRotatingRight);
        anim.SetBool("Rotating Left", isRotatingLeft);

        anim.SetFloat("Distance From Target", distanceFromTarget);
        anim.SetFloat("Target Distance From Head", targetDistanceFromHead);
        anim.SetFloat("Facing Target", targetFacing);
        anim.SetFloat("Attack Speed", AttackSpeed);

        anim.SetFloat("Life", lifePercentage);
        anim.SetInteger("Attack Number", Random.Range(1, RandomAttackQuantity + 1));
        anim.SetBool("Dead", isDead);

        anim.SetBool("Staring", isStaring);
        anim.SetBool("Jumping", isJumping);
        anim.SetBool("Moving Up", movingUp);
        anim.SetFloat("Jump Length", patrol.JumpLength);
        anim.SetBool("Can Make Noise", isCloseToNoiseObject);
        anim.SetBool("Pain", pain);

        anim.SetFloat("Pain Speed", 1/collision.Stun);
        anim.SetBool("Stabbed", stab);
        anim.SetBool("Parry", parry);

        anim.SetBool("Offensive", isOffensive);

        anim.SetBool("Joyous", joyous);
        anim.SetBool("Angry", angry);
        anim.SetBool("Depressed", depressed);
        anim.SetBool("Intimidated", intimidated);
        anim.SetBool("Bored", bored);
        anim.SetBool("Stunned", stunned);

    }

    public void setTugProperties()
    {
        if (tugCooldown <= 0)
        {
            anim.SetTrigger("Tug");
            tugCooldown = 7f;
        }
    }

    public bool getDeathState()
    {
        return isDead;
    }
}
