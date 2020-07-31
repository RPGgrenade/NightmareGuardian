using UnityEngine;
using System.Collections;
using System;

public class TP_Camera : MonoBehaviour 
{
    public static TP_Camera Instance;
    [Header("Initial Settings")]
    [Tooltip("World position of camera at the start of a scene")]
    public Vector3 InitialPosition;
    [Tooltip("Rotation of camera at the start of a scene")]
    public Vector3 InitialRotation;

    [Header("Game Object Tracking")]
    [Tooltip("Target for camera to be tracking in the scene")]
    public Transform TargetLookAt; //Not static due to possible changing
    public Transform TargetRagdoll; 
    [Tooltip("Indicator whether or not horizontal inputs reverse camera motion or not")]
    public bool InverseX = false;
    [Tooltip("Indicator whether or not vertical inputs reverse camera motion or not")]
    public bool InverseY = false;
    [Tooltip("Indicator as to whether the character is facing left or right in relation to the camera's vision")]
    public bool TargetIsFacingRight;
    [Tooltip("Unknown indicator")]
    public bool Point = true;
    [Tooltip("Indicates layers that the camera takes into account for collision purposes")]
    public LayerMask layersToHit;

    [Header("Distance Properties")]
    [Tooltip("Current camera distance from target")]
    public float Distance = 5f;
    [Tooltip("Minimum camera distance from target")]
    public float DistanceMin = 2f;
    [Tooltip("Maximum camera distance from target")]
    public float DistanceMax = 6f;
    [Tooltip("Smoothing speed between current camera position towards another distance")]
    public float DistanceSmooth = 0.05f;
    [Tooltip("Smoothing speed between current distance and stored distance of camera")]
    public float DistanceResumeSmooth = 0.5f;
    public float BackingOffSpeed = 5f;
    [Space(5)]

    [Header("Sensitivity Properties")]
    [Tooltip("Horizontal sensitivity of the stick")]
    public float X_StickSensitivity = 1.8f;
    [Tooltip("Vertical sensitivity of the stick")]
    public float Y_StickSensitivity = 1.8f;
    [Tooltip("Horizontal sensitivity of the mouse")]
    public float X_MouseSensitivity = 3f;
    [Tooltip("Vertical sensitivity of the mouse")]
    public float Y_MouseSensitivity = 3f;
    [Tooltip("Sensitivity of the mouse scroll wheel")]
    public float MouseWheelSensitivity = 5f;
    [Space(5)]

    [Header("Rotation Limits")]
    [Tooltip("How low the camera can go in angle terms")]
    public float Y_MinLimit = -40f;
    [Tooltip("How high the camera can go in angle terms")]
    public float Y_MaxLimit = 65f;
    [Tooltip("Speed of rotation around target")]
    public float RotationSpeed = 10f;
    [Tooltip("Speed of rotation around target when avoiding terrain")]
    public float RotationSpeedWhenOccluded = 4f;

    [Header("Auto Mode properties")]
    public bool AutoCamera = false;
    [Tooltip("Speed of the camera when slowly returning to the character")]
    public float SlowReturnSpeed = 1f;
    [Tooltip("How long it takes from not touching the camera for it to return automatically")]
    public float SlowReturnTimer = 1.5f;

    [Header("Smoothing")]
    [Tooltip("Position smoothing in the horizontal plane")]
    public float X_Smooth = 0.2f;
    [Tooltip("Position smoothing in the vertical plane")]
    public float Y_Smooth = 0.3f;

    [Header("Occlusion")]
    [Tooltip("How quickly the distance is made up for when an occlusion is detected")]
    public float OcclusionDistanceStep = 1f;
    [Tooltip("Maximum Occlusion Checks")]
    public int MaxOcclusionChecks = 10;
    [Tooltip("The raycast distance on a horizontal local level")]
    public float HorizontalRaycastCheckDistance = 4f;
    [Tooltip("Whether or not the camera will attempt to self adjust Horizontally")]
    public bool HorizontalAdjusting = true;
    [Tooltip("The raycast distance on a vertical local level")]
    public float VerticalRaycastCheckDistance = 1.5f;
    [Tooltip("Whether or not the camera will attempt to self adjust Horizontally")]
    public bool VerticalAdjusting = true;
    public bool DistanceAdjusting = true;

    [Header("Adjustment Speeds")]
    [Tooltip("Speed at which to escape objects that are too close horizontally")]
    public float HorizontalEscapeSpeed = 2.5f;
    [Tooltip("Max speed at which to escape objects that are too close horizontally")]
    public float MaxHorizontalEscapeSpeed = 2.5f;
    [Tooltip("Speed at which to escape objects that are too close vertically")]
    public float VerticalEscapeSpeed = 2.5f;
    [Tooltip("Max speed at which to escape objects that are too close vertically")]
    public float MaxVerticalEscapeSpeed = 2.5f;

    [Header("Occlusion Indicators")]
    public bool RightIsBlocked = false;
    public bool LefttIsBlocked = false;
    public bool TopIsBlocked = false;
    public bool BottomIsBlocked = false;
    public bool BackIsBlocked = false;

    [Header("Collision Properties")]
    [Tooltip("The collision used to check for terrain")]
    public SphereCollider Colliding;
    [Tooltip("Whether or not the camera is colliding with terrain")]
    public bool IsColliding = false;
    [Tooltip("Multiplier for the camera to test its sensitivity at distance")]
    public float CollisionSizeMultiplier = 0.5f;
    [Tooltip("Multiplier for the camera to push it forward or backwards based on distance")]
    public float CollisionForwardMultiplier = 0.5f;
    [Tooltip("Multiplier for the camera to push it up or down based on distance")]
    public float CollisionUpwardMultiplier = 0.5f;
    [Tooltip("Speed towards which it will move away from the terrain")]
    public float CollisionMoveSpeed = 5f;
    [Tooltip("Indicates layers that the camera takes into account for terrain avoidance purposes")]
    public LayerMask layersToAvoid;

    [Header("Zoom Speeds")]
    public float NegZoom = -0.1f;
    public float PosZoom = 0.1f;

    [Header("Offsets")]
    public float AimOffsetX = 0f;
    public float AimOffsetY = 0f;

    private Animator anim;
    private CameraShake shake;
    public float mouseX = 0f;
    public float mouseY = 0f;
    private float velX = 0f;
    private float velY = 0f;
    private float velZ = 0f;
    private float startDistance = 4f;
    private float desiredDistance = 0f;
    private Vector3 desiredPosition;
    private float velDistance = 0f;
    private Vector3 position = Vector3.zero;
    private float distanceSmooth = 0f;
    private float preOccludedDistance = 0f;
    private Vector3 offset = Vector3.zero;
    private float autoTimer = 0;
    

	void Awake ()
    {
        Instance = this;
	}

    void Start() 
    {
        TargetLookAt = GameObject.Find("targetLookAt").GetComponent<Transform>();
        anim = this.GetComponent<Animator>();
        Distance = Mathf.Clamp(Distance, DistanceMin, DistanceMax); //clamps the camera distance to be within the min and max values, setting actual distance to Distance.
        startDistance = Distance; //Once verified, sets the starting distance to current distance

        mouseX = InitialRotation.y; //Appears to work perfectly, will keep for now.
        mouseY = InitialRotation.x;
        Distance = startDistance; //prerecorded start distance to reset the camera
        desiredDistance = Distance;
        desiredPosition = this.transform.position;
        preOccludedDistance = Distance;
        SetOffset(Vector3.zero);

        shake = this.gameObject.GetComponent<CameraShake>();
    }

	void FixedUpdate ()
    {
        anim.SetBool("Pause", TP_Controller.Instance.paused);
       
        if (TargetLookAt == null && TargetRagdoll == null)
        {
            return;
        }

        CheckWhereTargetIsFacing();
        updateCollisionProperties();

        if (!TP_Controller.Instance.paused)
        {
            HandlePlayerInput();
            //TODO
            if ((AimChanger.Instance.AimType == AimChanger.Aim.Default || AimChanger.Instance.AimType == AimChanger.Aim.Monster))
            {
                FixPositionWhenTooCloseToWall();
                FixPositionWhenTooCloseToTarget();
            }

            if (AutoCamera)
            {
                autoTimer += Time.deltaTime;
                if (autoTimer > SlowReturnTimer)
                    Reset(true, false);
            }
        }
        var count = 0;
        do
        {
            CalculateDesiredPosition();
            count++;
        } while (CheckIfOccluded(count));

        //AimChanger.Instance.CameraTransition(TargetIsFacingRight ? AimOffsetX : -AimOffsetX);
    }

    private void OnTriggerStay(Collider other)
    {
        IsColliding = true;
        Vector3 closestPointToCamera = other.ClosestPoint(this.transform.position);
        Vector3 betweenClosestAndCamera = closestPointToCamera - this.transform.position;
        Vector3 newPosition = Vector3.Lerp(desiredPosition,  desiredPosition - betweenClosestAndCamera, 
            (Time.deltaTime * CollisionMoveSpeed)/betweenClosestAndCamera.magnitude);
        //Vector3 newPosition = desiredPosition - ((betweenClosestAndCamera.normalized * Time.deltaTime * CollisionMoveSpeed)/betweenClosestAndCamera.magnitude);
        CalculateMouse(newPosition);
        Debug.DrawLine(closestPointToCamera, this.transform.position,Color.red);
        Debug.DrawLine(desiredPosition, desiredPosition - betweenClosestAndCamera, Color.blue);
    }

    void Update()
    {
    }

    private void LateUpdate()
    {
        UpdatePosition();
        shake.ShakeUpdate(this.transform.position);

        //if(startupTimer < 3f)
        //{
        //    startupTimer += Time.deltaTime;
        //    this.transform.position = InitialPosition;
        //    this.transform.rotation = Quaternion.Euler(InitialRotation);
        //}
    }

    private void updateCollisionProperties()
    {
        Colliding.radius = Distance * CollisionSizeMultiplier;
        Colliding.center = new Vector3(0f, Distance * CollisionUpwardMultiplier, Distance * CollisionForwardMultiplier);
        IsColliding = false;
    }

    private void CheckWhereTargetIsFacing()
    {   //This part of the script isn't working as intended. It seems to work only in some situations.
        Vector3 targetDirection = TP_Controller.CharacterController.transform.forward;
        Vector3 cameraDirection = this.transform.right; //using right to make calculations easier
        TargetIsFacingRight = (Vector3.Dot(targetDirection, cameraDirection) > 0);
    }

    public void ResetToFront()
    {
        mouseX = TP_Controller.CharacterController.transform.eulerAngles.y + 180; //Appears to work perfectly, will keep for now.
        mouseY = 10;
        //Distance = startDistance; //prerecorded start distance to reset the camera
        desiredDistance = Distance;
        preOccludedDistance = Distance;
        SetOffset(Vector3.zero);
    }

    public void Reset(bool slow = false, bool resetDistance = true) 
    {
        if (!slow)
        {
            mouseX = TP_Controller.CharacterController.transform.eulerAngles.y; //Appears to work perfectly, will keep for now.
            mouseY = 10;
            if (resetDistance)
            {
                Distance = startDistance; //prerecorded start distance to reset the camera
                desiredDistance = Distance;
                preOccludedDistance = Distance;
            }
            SetOffset(Vector3.zero);
        }
        else
        {
            //if (TP_Controller.CharacterController.transform.eulerAngles.y > mouseX)
            if(TargetIsFacingRight)
                mouseX = Mathf.Lerp(mouseX, TP_Controller.CharacterController.transform.eulerAngles.y, SlowReturnSpeed * Time.deltaTime);
            else
                mouseX = Mathf.Lerp(mouseX, TP_Controller.CharacterController.transform.eulerAngles.y, SlowReturnSpeed * Time.deltaTime);
            mouseY = Mathf.Lerp(mouseY, 10, SlowReturnSpeed * Time.deltaTime);
            if (resetDistance)
            {
                Distance = Mathf.Lerp(Distance, startDistance, SlowReturnSpeed * Time.deltaTime); //prerecorded start distance to reset the camera
                desiredDistance = Distance;
                preOccludedDistance = Distance;
            }
            //SetOffset(Vector3.zero);
        }
    }

    void HandlePlayerInput() //Seperate into different functions to easily manage controls
    {
        var deadZone = 0.05f;

        if (TP_Controller.CharacterController.velocity != Vector3.zero || 
            Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0 || 
            Input.GetAxis("HorizontalCam") != 0 || Input.GetAxis("VerticalCam") != 0 || 
            Input.GetAxis("Mouse ScrollWheel") != 0 || Input.GetButton("ZoomOut") || Input.GetButton("ZoomIn"))
            autoTimer = 0f;

        if (!InverseX)
        {
            mouseX += Input.GetAxis("Mouse X") * X_MouseSensitivity;
            mouseX += Input.GetAxis("HorizontalCam") * X_StickSensitivity;
        }
        else
        {
            mouseX -= Input.GetAxis("Mouse X") * X_MouseSensitivity;
            mouseX -= Input.GetAxis("HorizontalCam") * X_StickSensitivity;
        }
        if (!InverseY)
        {
            mouseY -= Input.GetAxis("Mouse Y") * Y_MouseSensitivity;
            mouseY -= Input.GetAxis("VerticalCam") * Y_StickSensitivity;
        }
        else
        {
            mouseY += Input.GetAxis("Mouse Y") * Y_MouseSensitivity;
            mouseY += Input.GetAxis("VerticalCam") * Y_StickSensitivity;
        }

        mouseY = Helper.ClampAngle(mouseY, Y_MinLimit,Y_MaxLimit); //Clamps the Y rotation
        if (mouseX > 360f)
            mouseX -= 360f;
        else if (mouseX < -360f)
            mouseX += 360f;

        if (Input.GetAxis("Mouse ScrollWheel") < -deadZone || Input.GetAxis("Mouse ScrollWheel") > deadZone)
        {
            desiredDistance = Mathf.Clamp(Distance - Input.GetAxis("Mouse ScrollWheel") * MouseWheelSensitivity, DistanceMin, DistanceMax);
            preOccludedDistance = desiredDistance;
            distanceSmooth = DistanceSmooth;
        }
        else if(Input.GetButton("ZoomOut")) //Alternate controls
        {
            desiredDistance = Mathf.Clamp(Distance - NegZoom * MouseWheelSensitivity, DistanceMin, DistanceMax);
            preOccludedDistance = desiredDistance;
            distanceSmooth = DistanceSmooth;
        }
        else if(Input.GetButton("ZoomIn"))
        {
            desiredDistance = Mathf.Clamp(Distance - PosZoom * MouseWheelSensitivity, DistanceMin, DistanceMax);
            preOccludedDistance = desiredDistance;
            distanceSmooth = DistanceSmooth;
        }
    }

    private void FixPositionWhenTooCloseToWall()
    {
        RaycastHit hitRight;
        RaycastHit hitLeft;
        RaycastHit hitBack;
        RaycastHit hitTop;
        RaycastHit hitBottom;

        bool rightIsBlocked = Physics.Raycast(this.transform.position,this.transform.right, out hitRight, HorizontalRaycastCheckDistance, layersToHit);
        bool leftIsBlocked = Physics.Raycast(this.transform.position, -this.transform.right, out hitLeft, HorizontalRaycastCheckDistance, layersToHit);
        bool backIsBlocked = Physics.Raycast(this.transform.position, -this.transform.forward, out hitBack, HorizontalRaycastCheckDistance, layersToHit);
        bool topIsBlocked = Physics.Raycast(this.transform.position, this.transform.up, out hitTop, VerticalRaycastCheckDistance, layersToHit);
        bool bottomIsBlocked = Physics.Raycast(this.transform.position, -this.transform.up, out hitBottom, VerticalRaycastCheckDistance, layersToHit);

        RightIsBlocked = rightIsBlocked;
        LefttIsBlocked = leftIsBlocked;
        BackIsBlocked = backIsBlocked;
        TopIsBlocked = topIsBlocked;
        BottomIsBlocked = bottomIsBlocked;

        if (!CameraIsTooCloseToTerrain())
            return;

        float distanceRight = hitRight.distance;
        float distanceLeft = hitLeft.distance;
        float distanceHorizontalAv = (distanceLeft + distanceRight) / 2;

        float distanceBack = hitBack.distance;

        float distanceTop = hitTop.distance;
        float distanceBottom = hitBottom.distance;
        float distanceVerticalAv = (distanceBottom + distanceTop) / 2;

        //Debug.Log("Right Blocked: " + rightIsBlocked + ", Left Blocked: " + leftIsBlocked);

        if (HorizontalAdjusting)
        {
            if (rightIsBlocked)
                mouseX += getFloatDirection("Horizontal", distanceRight); //go left
            if (leftIsBlocked)
                mouseX -= getFloatDirection("Horizontal", distanceLeft); //go right
            if (rightIsBlocked && leftIsBlocked)
                mouseY += getFloatDirection("Vertical", distanceHorizontalAv); //go up
        }

        if (VerticalAdjusting)
        {
            if (bottomIsBlocked)
                mouseY += getFloatDirection("Vertical", distanceBottom); //go up
            if (topIsBlocked)
                mouseY -= getFloatDirection("Vertical", distanceTop); //go down
        }

        if (DistanceAdjusting)
        {
            if (bottomIsBlocked && topIsBlocked)
                Distance -= getFloatDirection("Distance", distanceVerticalAv); //go in
            if (backIsBlocked)
                Distance -= getFloatDirection("Distance", distanceBack); //go in
        }

        //if (CameraIsTooCloseToTerrain())
        //{
        //    if(TargetIsFacingRight && !leftIsBlocked)
        //        mouseX += getFloatDirection("Horizontal", Mathf.Max(distanceHorizontalAv, distanceVerticalAv, distanceBack)); //go left
        //    else if(!TargetIsFacingRight && !rightIsBlocked)
        //        mouseX -= getFloatDirection("Horizontal", Mathf.Max(distanceHorizontalAv, distanceVerticalAv, distanceBack)); //go right
        //}
        
        mouseY = Helper.ClampAngle(mouseY, Y_MinLimit, Y_MaxLimit); //Clamps the Y rotation
    }

    private float getFloatDirection(string direction, float distanceDivider)
    {
        //Method used to add a direction to the camera motion
        switch (direction)
        {
            case "Vertical":
                return Y_StickSensitivity * Time.deltaTime * Mathf.Clamp(VerticalEscapeSpeed / distanceDivider, 0f, MaxVerticalEscapeSpeed);
            case "Horizontal":
                return X_StickSensitivity * Time.deltaTime * Mathf.Clamp(HorizontalEscapeSpeed / distanceDivider, 0f, MaxHorizontalEscapeSpeed);
            case "Distance":
                return Time.deltaTime * Mathf.Clamp(HorizontalEscapeSpeed / distanceDivider, 0f, MaxHorizontalEscapeSpeed);
        }
        return 0f;
    }

    private void FixPositionWhenTooCloseToTarget()
    {
        bool tooClose = Distance < DistanceMin;
        //Debug.Log("Too Close: " + tooClose);// + ", Left Blocked: " + leftIsBlocked);
        if (tooClose)// && !CameraIsTooCloseToTerrain())
        {
            Distance += Time.deltaTime * BackingOffSpeed; //go back
        }
    }

    private bool CameraIsTooCloseToTerrain()
    {
        return RightIsBlocked || LefttIsBlocked
            || TopIsBlocked || BottomIsBlocked
            || BackIsBlocked;
    }

    void CalculateDesiredPosition() 
    {
        ResetDesiredDistance();
        Distance = Mathf.SmoothDamp(Distance, desiredDistance, ref velDistance, distanceSmooth);  //Evaluate our distance
        desiredPosition = CalculatePosition(mouseY, mouseX, Distance);  //Calculate desired position
    }

    public void SetOffset(Vector3 newOffset)
    {
        offset = this.transform.TransformDirection(newOffset);
    }

    Vector3 CalculatePosition(float rotationX, float rotationY, float distance) 
    {
        Vector3 direction = new Vector3(0 , 0 , -distance);
        Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0);
        if (RagdollHelper.Instance.Ragdoll)
            return TargetRagdoll.position + (rotation * direction);
        return TargetLookAt.position + (rotation * direction);
    }

    void CalculateMouse(Vector3 position)
    {
        Vector3 vAngle = Vector3.Cross(desiredPosition - TargetLookAt.position, position - TargetLookAt.position);
        Quaternion rotation = Quaternion.Euler(vAngle);
        //Debug.Log("(X,Y): (" + rotation.eulerAngles.y + "," + rotation.eulerAngles.x + ")");

        mouseX += rotation.eulerAngles.y;
        mouseY += rotation.eulerAngles.x > 180f? (360f - rotation.eulerAngles.x) : rotation.eulerAngles.x;

        if (mouseX > 360f)
            mouseX -= 360f;
        else if (mouseX < -360f)
            mouseX += 360f;
    }

    bool CheckIfOccluded(int count) 
    {
        var isOccluded = false;
        var nearestDistance = 0f;
        if(RagdollHelper.Instance.Ragdoll)
            nearestDistance = CheckCameraPoints(TargetRagdoll.position, desiredPosition);
        else
            nearestDistance = CheckCameraPoints(TargetLookAt.position, desiredPosition);
        if (nearestDistance != -1)
        {
            if (count < MaxOcclusionChecks)
            {
                isOccluded = true;
                Distance -= OcclusionDistanceStep;
                if (Distance < 0.5f)//magic number to test
                {
                    Distance = 0.5f;
                }
            }
            else 
            {
                Distance = nearestDistance - Camera.main.nearClipPlane;
            }
            desiredDistance = Distance;
            distanceSmooth = DistanceResumeSmooth;
        }
        //Debug.Log("Occluded: " + isOccluded);
        return isOccluded;
    }

    float CheckCameraPoints(Vector3 from, Vector3 to) 
    {
        //Which is the nearest distance of all those point if any of them is hit, and we need to know if something is hit or not.
        var nearDistance = -1f;
        //if (!CameraIsTooCloseToTerrain())
        //{
            RaycastHit hitInfo;
            Helper.ClipPlanePoints clipPlanePoint = Helper.ClipPlaneAtNear(to);
            //Drawing lines in the editor to make it easier to visualize
            //Debug.DrawLine(from, to + transform.forward * -GetComponent<Camera>().nearClipPlane, Color.red);
            //Debug.DrawLine(from, clipPlanePoint.UpperLeft);
            //Debug.DrawLine(from, clipPlanePoint.LowerLeft);
            //Debug.DrawLine(from, clipPlanePoint.UpperRight);
            //Debug.DrawLine(from, clipPlanePoint.LowerRight);
            //Debug.DrawLine(clipPlanePoint.UpperLeft, clipPlanePoint.UpperRight);
            //Debug.DrawLine(clipPlanePoint.UpperRight, clipPlanePoint.LowerRight);
            //Debug.DrawLine(clipPlanePoint.LowerRight, clipPlanePoint.LowerLeft);
            //Debug.DrawLine(clipPlanePoint.LowerLeft, clipPlanePoint.UpperLeft);
            if (Physics.Linecast(from, clipPlanePoint.UpperLeft, out hitInfo, layersToHit)) //check all four points for a collision
            {
                if (hitInfo.distance < nearDistance || nearDistance == -1)
                    nearDistance = hitInfo.distance;
                //Debug.Log("Occluded by " + hitInfo.collider.gameObject.name);
            }
            if (Physics.Linecast(from, clipPlanePoint.UpperRight, out hitInfo, layersToHit))
            {
                if (hitInfo.distance < nearDistance || nearDistance == -1)
                    nearDistance = hitInfo.distance;
                //Debug.Log("Occluded by " + hitInfo.collider.gameObject.name);
            }
            if (Physics.Linecast(from, clipPlanePoint.LowerLeft, out hitInfo, layersToHit))
            {
                if (hitInfo.distance < nearDistance || nearDistance == -1)
                    nearDistance = hitInfo.distance;
                //Debug.Log("Occluded by " + hitInfo.collider.gameObject.name);
            }
            if (Physics.Linecast(from, clipPlanePoint.LowerRight, out hitInfo, layersToHit))
            {
                if (hitInfo.distance < nearDistance || nearDistance == -1)
                    nearDistance = hitInfo.distance;
                //Debug.Log("Occluded by " + hitInfo.collider.gameObject.name);
            }
            if (Physics.Linecast(from, to + transform.forward * -GetComponent<Camera>().nearClipPlane, out hitInfo, layersToHit)) //a rare scenario that could happen.
            {
                if (hitInfo.distance < nearDistance || nearDistance == -1)
                    nearDistance = hitInfo.distance;
                //Debug.Log("Occluded by " + hitInfo.collider.gameObject.name);
            }
        //}
        return nearDistance;
    }

    void ResetDesiredDistance() 
    {
        if(desiredDistance < preOccludedDistance)
        {
            var pos = CalculatePosition(mouseY,mouseX, preOccludedDistance);

            var nearestDistance = 0f;
            if (RagdollHelper.Instance.Ragdoll)
                nearestDistance = CheckCameraPoints(TargetRagdoll.position, pos);
            else
                nearestDistance = CheckCameraPoints(TargetLookAt.position, pos);
            //var nearestDistance = CheckCameraPoints(TargetLookAt.position, pos);
            if(nearestDistance == -1 || nearestDistance > preOccludedDistance)
            {
                desiredDistance = preOccludedDistance;
            }
        }
    }

    void UpdatePosition() 
    {
        var posX = Mathf.SmoothDamp(position.x, desiredPosition.x, ref velX, X_Smooth);
        var posY = Mathf.SmoothDamp(position.y, desiredPosition.y, ref velY, Y_Smooth);
        var posZ = Mathf.SmoothDamp(position.z, desiredPosition.z, ref velZ, X_Smooth);
        position = new Vector3(posX, posY, posZ);
        
        transform.position = position;
        Vector3 targetVector = Vector3.zero;
        if(RagdollHelper.Instance.Ragdoll)
            targetVector = TargetRagdoll.position - offset - this.transform.position;
        else
            targetVector = TargetLookAt.position - offset - this.transform.position;
        var targetRotation = Quaternion.LookRotation(targetVector);

        if (!CameraIsTooCloseToTerrain())
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
        else
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeedWhenOccluded * Time.deltaTime);
        //transform.LookAt(TargetLookAt.position + offset);
    }

    public static void UseExistingOrCreateNewMainCamera() //Static to prevent another instance to run this
    {
        GameObject tempCamera; //place holders
        GameObject targetLookAt;
        TP_Camera myCamera;
        if (Camera.main != null) //Set current camera to the tempcamera
        {
            tempCamera = Camera.main.gameObject;
        }
        else //Create a new main camera to set to
        {
            tempCamera = new GameObject("Main Camera"); //Creates a new object with said name
            tempCamera.AddComponent<Camera>(); //Turns new object into a camera
            tempCamera.tag = "MainCamera"; //Turns camera into main camera
        }
        if(tempCamera.GetComponent("TP_Camera") == null)
        {
            tempCamera.AddComponent<TP_Camera>(); //Adds the script
        }
        myCamera = tempCamera.GetComponent("TP_Camera") as TP_Camera; //Gets the script from the newly created camera
        targetLookAt = GameObject.Find("targetLookAt") as GameObject; //Check for target
        if(targetLookAt == null) //if non-existant, create one
        {
            targetLookAt = new GameObject("targetLookAt");
            targetLookAt.transform.position = Vector3.zero; //sets at world origin
        }
        myCamera.TargetLookAt = targetLookAt.transform; //sets camera target to be the position of the current target
    }

    public void SetCameraAimDistance(float aimDistance) 
    {
        Distance = aimDistance; //prerecorded start distance to reset the camera
        desiredDistance = Distance;
        preOccludedDistance = Distance;
    }
}
