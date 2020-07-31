using UnityEngine;

public class CreatureAI : MonoBehaviour
{
    public static CreatureAI Instance;
    public static CharacterController MonsterController;

    [Header("Object References")]
    [Tooltip("The Object to target")]
    public GameObject Target;
    [Tooltip("The Head of the Creaker")]
    public GameObject Head;
    [Tooltip("The Body of the Creaker")]
    public GameObject Body;

    public LayerMask layers;

    [Header("AI States")]
    [Tooltip("Whether or not to go forward or back up")]
    public CreakerState State;
    public CreakerAction Action;

    [Tooltip("State to keep track for animation purposes")]
    public bool Attacking = false;

    [Header("Properties")]
    [Tooltip("Maximum Health (measured in eyesight)")]
    public float MaxVisibility = 100f;
    [Tooltip("Current Health (measured in eyesight)")]
    public float Visibility = 100f;

    [Tooltip("The time for the Monster losing interest in locating the target")]
    public int SearchValue = 0;
    [Tooltip("The value of the Monster's anger")]
    public int RageValue = 0;
    [Tooltip("The value of the Monster's fear")]
    public int Intimidation = 0;
    [Tooltip("Likelihood the Monster will attack, the smaller the number, the more likely")]
    public int RageLikelihood = 10000;
    [Tooltip("Likelihood the Monster will retreat, the smaller the number, the more likely")]
    public int IntimidationLikelihood = 1000000;
    [Header("Detection Properties")]
    [Tooltip("How close the monster will aim to be to the target")]
    public float MinDistanceFromTarget = 5f;
    [Tooltip("How far from the monster's central axis the teddy has to be for him to turn")]
    public float RotationDifferenceThreshold = 3f;
    [Tooltip("How far detector is from core")]
    public float MinDistanceFromCore = 2f;
    public float SideDistanceFromCore = 2f;
    [Tooltip("How far from the center of the monster is used to detect objects")]
    public float HeightDistanceFromCore = 5f;
    [Tooltip("How far the monster will want to be from objects in front of it")]
    public float MinDistanceFromObjects = 2f;
    [Tooltip("How far to the sides monster wants to be from objects")]
    public float MinDistanceFromSides = 2f;
    [Header("Movement Properties")]
    [Tooltip("How likely the creature will turn around every frame, the higher the less likely")]
    public int RotationChance = 100;
    [Tooltip("How likely the creature will move every frame, the higher the less likely")]
    public int MovementChance = 100;
    [Tooltip("Minimum amount of time creature spends moving")]
    public float MinMovementTime = 0.7f;
    [Tooltip("Maximum amount of time creature spends moving")]
    public float MaxMovementTime = 2.3f;

    public bool Obstructed;
    public bool ObstructedRight;
    public bool ObstructedLeft;

    private float movementTime = 0f;

    public float AngleLimit = 50f; //Not sure what this does

    private Vector3 searchLocation;
    private Rigidbody body;
    public float distanceToTarget;

    public Vector3 SearchLocation { get; set; }


    public enum CreakerState
    {
        Searching,
        Offensive,
        Peaceful,
        Intimidated,
        Climbing
    }
    public enum CreakerAction 
    {
        Attacking,
        Rotating,
        Walking,
        Searching,
        Pushing,
        Idling
    }

    void Awake()
    {
        Instance = this;
        SearchLocation = searchLocation;
        MonsterController = GetComponent("CharacterController") as CharacterController;
        body = this.GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (State == CreakerState.Peaceful) 
        {
            Head.GetComponent<Animator>().enabled = true;
            IdleMovement();
            //Debug.Log("Peaceful");
        }
        if (State == CreakerState.Searching)
        {
            Head.GetComponent<Animator>().enabled = false;
            SearchMovement();
            //Debug.Log("Currently Searching");
        }
        if (State == CreakerState.Offensive)
        {
            Head.GetComponent<Animator>().enabled = false;
            OffensiveMovement();
            //Debug.Log("On the Offensive.");
        }
        Rotating();
    }

    void LateUpdate()
    {
        VisibilityUpdate();
        //isObstructed();
        //sideChecking();
    }

    void OnCollisionStay(Collision collision) //compensate for touching any walls
    {
        //if (collision.transform.tag != "Floor")
            //Action = CreakerAction.Rotating;
            //CreatureMovement.Instance.Rotate();
    }

    void IdleMovement()
    {
        int moveNumber = Random.Range(0,MovementChance);

        if(moveNumber == 1 && Action != CreakerAction.Rotating)
            Action = CreakerAction.Walking;
        else
            Action = CreakerAction.Idling;
        /*
        if(moveNumber == 1)
            movementTime = Random.Range(MinMovementTime,MaxMovementTime);

        if (movementTime >= 0 && !Obstructed)
        {
            Action = CreakerAction.Walking;
            movementTime -= Time.deltaTime;
            //CreatureMovement.Instance.Move();
        }
        */
    }

    private void isObstructed()
    {
        Vector3 origin = (this.transform.position - new Vector3(0,HeightDistanceFromCore,0)) 
            + (-MonsterController.transform.forward * MinDistanceFromCore);
        Vector3 direction = -MonsterController.transform.forward;
        Ray raycast = new Ray(origin, direction);
        RaycastHit hit;
        Obstructed = Physics.Raycast(raycast, out hit, MinDistanceFromObjects, layers);
        Debug.DrawRay(origin,direction * MinDistanceFromObjects,Color.blue);
    }

    private void sideChecking()
    {
        Vector3 originRight = (this.transform.position - new Vector3(0, HeightDistanceFromCore, 0))
            + (-MonsterController.transform.right * SideDistanceFromCore);
        Vector3 originLeft = (this.transform.position - new Vector3(0, HeightDistanceFromCore, 0))
            + (MonsterController.transform.right * SideDistanceFromCore);

        Vector3 directionRight = -this.transform.right;
        Vector3 directionLeft = this.transform.right;

        Ray raycastRight = new Ray(originRight, directionRight);
        RaycastHit hitRight;

        Ray raycastLeft = new Ray(originLeft, directionLeft);
        RaycastHit hitLeft;

        ObstructedRight = Physics.Raycast(raycastRight, out hitRight, MinDistanceFromSides, layers);
        ObstructedLeft = Physics.Raycast(raycastLeft, out hitLeft, MinDistanceFromSides, layers);

        Debug.DrawRay(originRight, directionRight * MinDistanceFromSides, Color.green);
        Debug.DrawRay(originLeft, directionLeft * MinDistanceFromSides, Color.green);

        if (ObstructedLeft)
        {
            CreatureMovement.Instance.RotatingRight = true;
            Action = CreakerAction.Rotating;
            //CreatureMovement.Instance.Rotate();
        }
        else if (ObstructedRight)
        {
            CreatureMovement.Instance.RotatingRight = false;
            Action = CreakerAction.Rotating;
            //CreatureMovement.Instance.Rotate();
        }
    }

    void OffensiveMovement()
    {
        if (!Obstructed && Vector3.Distance(Target.transform.position, Head.transform.position) > MinDistanceFromTarget)
                Action = CreakerAction.Walking;
        else
            Action = CreakerAction.Idling;
    }

    void SearchMovement() 
    {
        if(distanceToTarget > MinDistanceFromTarget)
        {
            Action = CreakerAction.Searching;
            CreatureMovement.Instance.Move();
        }
    }

    void Rotating()
    {
        /*Rotation target should be calculated carefully, otherwise
          there could be a crazy rotation issue with the target
          There will be rotation for following, and rotation for random
          rotations
         */
        int rotateNumber = Random.Range(0, MovementChance);

        if(State == CreakerState.Peaceful && rotateNumber == 1)
        {
            Action = CreakerAction.Rotating;
        }

        if (State == CreakerState.Offensive || State == CreakerState.Intimidated)
        {
            float positionOfTarget = TargetRelativeHorizontalPosition();
            if (Mathf.Abs(positionOfTarget) > RotationDifferenceThreshold)
            {
                Action = CreakerAction.Rotating;
                CreatureMovement.Instance.RotatingRight = positionOfTarget < 0;
            }
        }
        /*
        else
        {
            if (Obstructed)
            {
                Action = CreakerAction.Rotating;
            }
        }
        */
    }

    private float TargetRelativeHorizontalPosition()
    {
        return this.transform.InverseTransformPoint(Target.transform.position).x;
    }

    void VisibilityUpdate() 
    {
        if (State != CreakerState.Peaceful && GlowEye.Instance != null)
        {
            GlowEye.Instance.ChangeIntensity(Visibility / MaxVisibility);
        }
        else if(State != CreakerState.Peaceful && GlowEye.Instance != null)
        {
            GlowEye.Instance.ChangeIntensity(0f);
        }
    }
}

