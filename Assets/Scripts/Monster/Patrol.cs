using UnityEngine;
using UnityEngine.AI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CreatureMind))]
public class Patrol : MonoBehaviour {

    [Header("Patrolling Logic")]
    [Tooltip("Whether or not the monster is able to move")]
    public bool CanMove = true;
    [Tooltip("Whether or not the monster is able to rotate")]
    public bool CanRotate = true;
    [Tooltip("If True: Creature never leaves designated points of interest")]
    public bool OnRails = false;
    [Tooltip("A manual variable to check if there's an issue with the navmesh stuff")]
    public bool IsHavingNavMeshIssues = false;
    [Tooltip("The chance that the next patrol location will be randomized")]
    [Range(0f, 1f)]
    public float PatrolRandomizingChance = 0f;

    [Header("Noise Point logic")]
    [Tooltip("How likely in % that the next point will be a noise point")]
    [Range(0f,1f)]
    public float NoisePointWeight = 0f;
    [Tooltip("If True: Creature will run towards the closest point near a noise point if scared")]
    public bool RetreatsToNoisePoint = false;
    [Tooltip("If True: Creature is close to an object that it can cause noise with")]
    public bool CloseToNoisePoint = false;

    [Header("Jumping Logic (Some won't use this)")]
    [Tooltip("If True: Creatre is off nav mesh, almost definitely jumping")]
    public bool OnNavMeshLink = false;
    [Tooltip("Used to determine whether creature is jumping up or down")]
    public float VerticalSpeed = 0f;
    [Tooltip("Tracks how long the current Off Mesh Link is for jumping varieties")]
    public float JumpLength = 0f;
    [Tooltip("Indicator as to whether or not they've jumped recently")]
    public bool HasJumpedRecently = false;
    [Tooltip("How long the cooldown is between jumping on nav mesh links")]
    public float JumpTimeCooldown = 0f;

    [Header("Speed Changing Logic (Some won't use this)")]
    [Tooltip("If True: Creatre is using speed multipliers because animations take care of general speed already")]
    public bool ManualSpeedOverride = false;
    [Tooltip("Used as a multiplier for the current movement speed (whatever it may be)")]
    public float SpeedMultiplier = 1f;
    [Tooltip("Used as a multiplier for the current angular speed (whatever it may be)")]
    public float AngleMultiplier = 1f;

    [Header("Distance Handling Properties")]
    [Tooltip("The distance the monster will want to be away from the player if scared")]
    public float DistanceFromPlayer = 3f;
    [Tooltip("The distance the monster will need to be within distance to reach navmesh")]
    public float DistanceFromNavmesh = 3f;
    [Tooltip("The distance between the monster and its goal before choosing to change position")]
	public float DistanceToTravel = 0.5f;
    [Tooltip("Distance Monster is able to try and reach at maximum")]
    public float DistanceFromGoal = 10f;
    [Tooltip("Minimum Distance Monster needs from object of noise in order to make noise")]
    public float DistanceFromNoisePoint = 1f;

    [Header("Noise Point Rotation Handling Properties")]
    [Tooltip("Shows whether or not the monster is rotating towards the noise point")]
    public bool IsRotatingTowardsNoisePoint = false;
    [Tooltip("Minimum Rotation offset Monster needs from object of noise in order to make noise (can't be facing away)")]
    public float RotationFromNoisePoint = 15f;
    [Tooltip("Speed Monster rotates at towards a noise making object")]
    public float RotationToNoisePointSpeed = 15f;

    [Header("Patrolling Locations")]
    public Transform CurrentTarget;
    [Tooltip("Points on the map the monster will want to go to when on rails or not following player (followed in sequence)")]
	public Transform[] points;
    [Tooltip("Point on the map the monster will make noise if it's near enough")]
    public Transform[] NoisePoints;

    [Header("Rotation Properties")]
    [Tooltip("Speed of Rotation before applying rotating logic")]
    public float RotationThreshold = 0.1f;
    [Tooltip("Whether or not the monster will change speed up on rotation")]
    public bool SlowsWhileRotating = false;
    [Tooltip("The speed monster will move at when rotating if it does so")]
    public float SpeedWhileRotating = 0f;

    public bool IsRotatingLeft = false;
    public bool IsRotatingRight = false;

    public bool Debugging = false;

    private Vector3 ActualTarget = Vector3.zero;
	private int destPoint = 0;
	private NavMeshAgent agent;
    private TurnToLook vision;
    private Hearing hearing;
    private CreatureMind mind;
    private float currentAngularVelocity;
    private Vector3 lastFacing;
    private float jumpCooldown = 0f;

    void Awake () {
		agent = GetComponent<NavMeshAgent>();

        vision = GetComponentInChildren<TurnToLook>();

        hearing = GetComponentInChildren<Hearing>();

        mind = GetComponent<CreatureMind>();

		// Disabling auto-braking allows for continuous movement
		// between points (ie, the agent doesn't slow down as it
		// approaches a destination point).
	}

    private void Start()
    {
		GotoNextPoint();
    }

    public void GotoPoint(Vector3 goal, bool retreating = false)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(goal, out hit, DistanceFromGoal, NavMesh.AllAreas))
        {
            Vector3 closestPosition = hit.position;
            ActualTarget = closestPosition;
            if (!RetreatsToNoisePoint)
                setAgentDestination(closestPosition);
            else
            {
                if (retreating)
                    agent.destination = getClosestPointToPosition(
                        getClosestPointToPosition(closestPosition, true));
                else
                    setAgentDestination(closestPosition);
            }
        }
    }

    private void setAgentDestination(Vector3 closestPosition)
    {
        if (!OnRails)
            agent.destination = closestPosition;
        else
            agent.destination = getClosestPointToPosition(closestPosition);
    }

    private Vector3 getClosestPointToPosition(Vector3 positionToFind, bool noisePoints = false)
    {
        Transform bestPoint = null;
        float closestDistanceSqr = Mathf.Infinity;
        Transform[] pointsToSearch;
        if (!noisePoints)
            pointsToSearch = points;
        else
            pointsToSearch = NoisePoints;

        foreach (Transform point in points)
        {
            Vector3 directionToTarget = point.position - positionToFind;
            float squaredDistanceToTarget = directionToTarget.sqrMagnitude;
            if (squaredDistanceToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = squaredDistanceToTarget;
                bestPoint = point;
                CurrentTarget = bestPoint;
            }
        }
        return bestPoint.position;
    }

    void GotoNextPoint() {
		// Returns if no points have been set up
		if (points.Length == 0)
			return;

        // Set the agent to go to the currently selected destination.
        if (Random.Range(0f, 1f) < PatrolRandomizingChance)
            destPoint = Random.Range(0, points.Length);

		agent.destination = points[destPoint].position;
        CurrentTarget = points[destPoint];

        //Random chance between 0 and 1
        if (NoisePoints.Length >= 0)
        {
            float chance = Random.Range(0f, 1f);
            if (chance <= NoisePointWeight)
            {
                Vector3 closestNoisePoint = getClosestPointToPosition(
                    getClosestPointToPosition(points[destPoint].position, true));
                agent.destination = closestNoisePoint;
            }
        }

        // Choose the next point in the array as the destination,
        // cycling to the start if necessary.
        destPoint = (destPoint + 1) % points.Length;
	}

    bool IsAgentOnNavMesh()
    {
        Vector3 agentPosition = this.transform.position;
        NavMeshHit hit;

        // Check for nearest point on navmesh to agent, within onMeshThreshold
        if (NavMesh.SamplePosition(agentPosition, out hit, DistanceFromNavmesh, NavMesh.AllAreas))
        {
            // Check if the positions are vertically aligned
            if (Mathf.Approximately(agentPosition.x, hit.position.x)
                && Mathf.Approximately(agentPosition.z, hit.position.z))
            {
                // Lastly, check if object is below navmesh
                return agentPosition.y >= hit.position.y;
            }
        }

        return false;
    }

    private void checkNearPoint()
    {
        if(NoisePoints.Length > 0)
        {
            foreach(Transform noiser in NoisePoints)
            {
                float currentDistanceFromPoint = Vector3.Distance(this.transform.position, noiser.position);
                //Debug.Log("Distance from Point: " + currentDistanceFromPoint);
                //Checks that the monster is both close enough to the point of noise making, and facing it within a certain degree
                if (currentDistanceFromPoint <= DistanceFromNoisePoint)
                {
                    if (checkIsFacing(noiser))
                    {
                        CloseToNoisePoint = true;
                        break;
                    }
                }
                else
                    CloseToNoisePoint = false;
            }
        }
    }

    private void rotateTowardsNearestNoisePoint()
    {
        if (NoisePoints.Length > 0 && IsRotatingTowardsNoisePoint && CanRotate)
        {
            foreach (Transform noiser in NoisePoints)
            {
                float currentDistanceFromPoint = Vector3.Distance(this.transform.position, noiser.position);
                if (currentDistanceFromPoint <= DistanceFromNoisePoint)
                {
                    Vector3 noiserDirection = (noiser.position - transform.position);
                    Quaternion lookRotationNoiser = Quaternion.LookRotation(new Vector3(noiserDirection.x,0,noiserDirection.z), Vector3.up);
                    this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, lookRotationNoiser, 
                        Time.deltaTime * RotationToNoisePointSpeed);
                }
            }
        }
    }

    private bool checkIsFacing(Transform noiseTarget)
    {
        Vector3 currentFacing = this.transform.forward;

        Vector3 flattenedPosition = new Vector3( this.transform.position.x, 0f, this.transform.position.z);
        Vector3 flattenedNoisePosition = new Vector3( noiseTarget.position.x, 0f, noiseTarget.position.z);

        Vector3 targetDirection = flattenedNoisePosition - flattenedPosition;
        float currentAngleDifference = Vector3.Angle(currentFacing, targetDirection);
        Debug.Log("Angle Difference of Noise: " + currentAngleDifference);
        //Vector3 cross = Vector3.Cross(currentFacing, targetDirection);

        //if (cross.y < 0)
        if(currentAngleDifference < 0)
            currentAngleDifference = -currentAngleDifference;
        //returns whether the monster is facing towards the 
        return currentAngleDifference <= RotationFromNoisePoint;
    }

    private void checkIsRotating()
    {
        Vector3 currentFacing = this.transform.forward;
        currentAngularVelocity = Vector3.Angle(currentFacing, lastFacing) / Time.deltaTime;
        Vector3 cross = Vector3.Cross(currentFacing, lastFacing);

        if (cross.y < 0)
            currentAngularVelocity = -currentAngularVelocity;

        lastFacing = currentFacing;

        IsRotatingLeft = currentAngularVelocity > RotationThreshold;
        IsRotatingRight = currentAngularVelocity < -RotationThreshold;
        if ((IsRotatingLeft || IsRotatingRight) && SlowsWhileRotating)
            agent.velocity = Vector3.one * SpeedWhileRotating;
    }

    public void StopMovement()
    {
        if(IsAgentOnNavMesh())
            agent.isStopped = true;
    }

    public void ResumeMovement()
    {
        if(IsAgentOnNavMesh())
            agent.isStopped = false;
    }

    private void checkCanMove()
    {
        if (!CanMove) //stops movement if set to 0
            agent.velocity = Vector3.zero;
    }

    private void checkCanRotate()
    {
        agent.updateRotation = CanRotate;
    }

    private void checkJumping()
    {
        OnNavMeshLink = agent.isOnOffMeshLink;

        //IsNavMeshLinkJump = OnNavMeshLink ? agent.currentOffMeshLinkData : ;

        VerticalSpeed = agent.velocity.y;

        if (OnNavMeshLink)
        {
            HasJumpedRecently = true;
            jumpCooldown = JumpTimeCooldown;
            JumpLength = Vector3.Distance(agent.currentOffMeshLinkData.startPos, 
                agent.currentOffMeshLinkData.endPos);
        }
        else
        {
            JumpLength = 0f;
            if (HasJumpedRecently && !OnNavMeshLink)
            {
                jumpCooldown -= Time.deltaTime;
                agent.autoTraverseOffMeshLink = false;
                if(jumpCooldown <= 0f)
                {
                    jumpCooldown = 0f;
                    HasJumpedRecently = false;
                    agent.autoTraverseOffMeshLink = true;
                }
            }
        }
    }

    private void updateSpeed()
    {
        if(agent != null)
        {
            agent.speed = agent.speed * SpeedMultiplier;
            agent.angularSpeed = agent.angularSpeed * AngleMultiplier;
            agent.acceleration = agent.acceleration * SpeedMultiplier;
        }
    }

    private void OnDrawGizmos()
    {
        if (Debugging)
        {
            Gizmos.DrawCube(ActualTarget, Vector3.one * 0.7f);
            Gizmos.DrawSphere(CurrentTarget.position, 0.7f);
        }
    }

    void Update () {
        if (IsAgentOnNavMesh() || !IsHavingNavMeshIssues)
        {
            if (agent.remainingDistance < DistanceToTravel)
                vision.Interested = false;

            if (vision.InRange || vision.Interested)
            {
                if (mind.state == CreatureMind.CreatureState.Intimidated)
                {
                    Vector3 playerDirection = (vision.targetPosition - this.transform.position).normalized;
                    GotoPoint(this.transform.position - (playerDirection * DistanceFromPlayer), true);
                }
                else
                    GotoPoint(vision.GetTargetedPosition());
            }
            // Choose the next destination point when the agent gets
            // close to the current one.
            else if (!agent.pathPending && agent.remainingDistance < DistanceToTravel)
                GotoNextPoint();
        }
        checkJumping();
        rotateTowardsNearestNoisePoint();
        checkNearPoint();
        checkCanRotate();
        checkIsRotating();
        checkCanMove();
	}

    void LateUpdate()
    {
        if (ManualSpeedOverride && !OnNavMeshLink)
            updateSpeed();
    }
}