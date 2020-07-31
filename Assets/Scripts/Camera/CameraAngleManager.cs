using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAngleManager : MonoBehaviour
{
    [System.Serializable]
    public class CameraMovement
    {
        [Tooltip("How long the cutscene lasts, can cut off movement if it fits")]
        public float MovementTime = 2f;
        [Space(15)]
        public float PositionThreshold = 1f;
        [Min(0f)]
        public float PositionChangeSpeed = 1f;
        [Tooltip("Positions Camera moves to based on the order of this list")]
        public List<Vector3> PositionsInOrder = new List<Vector3>();
        [Space(15)]
        public float RotationThreshold = 0.2f;
        [Min(0f)]
        public float RotationChangeSpeed = 1f;
        [Tooltip("Rotations Camera moves to based on the order of this list")]
        public List<Vector3> RotationsInOrder = new List<Vector3>();
    }

    [Header("Camera Object Reference")]
    public Camera MainCamera;
    public GameObject CameraObject;
    public bool Saveable = true;
    public bool Relative = false;
    public string TriggerId = ""; //will eventually save and load

    [Header("Camera Trigger Options")]
    public bool Triggered = false;
    [Tooltip("If true, will always trigger the camera transition change")]
    public bool IsAlwaysTriggered = false;
    [Min(0)]
    public int CurrentTriggerCount = 0; //will eventually save and load
    [Min(0)]
    public int MaxTriggerCount = 1;

    [Header("Position Movement Properties")]
    public CameraMovement Movement;

    private int positionCount = 0;
    private int rotationCount = 0;
    private float timer = 0f;
    private float velX;
    private float velY;
    private float velZ;

    // Start is called before the first frame update
    void Start()
    {
        //Load Trigger ID and count
        if (MainCamera == null)
            MainCamera = Camera.main;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (Triggered)
        {
            timer += Time.deltaTime;
            if (positionCount == 0)
            {
                CameraObject.transform.position = Movement.PositionsInOrder[0];
                CameraObject.transform.rotation = Quaternion.Euler(Movement.RotationsInOrder[0]);
                CameraObject.SetActive(true);
                MainCamera.enabled = false;
                positionCount++;
                rotationCount++;
            }
            else if(positionCount > 0 && positionCount <= Movement.PositionsInOrder.Count-1)
            {
                if (!Relative)
                {
                    if ((CameraObject.transform.rotation.eulerAngles - Movement.RotationsInOrder[positionCount]).magnitude
                        > Movement.RotationThreshold)
                        CameraObject.transform.rotation = Quaternion.Lerp(CameraObject.transform.rotation,
                            Quaternion.Euler(Movement.RotationsInOrder[positionCount]),
                            Time.deltaTime * Movement.RotationChangeSpeed);

                    if ((CameraObject.transform.position - Movement.PositionsInOrder[positionCount]).magnitude
                        > Movement.PositionThreshold)
                    {
                        var posX = Mathf.SmoothDamp(CameraObject.transform.position.x, Movement.PositionsInOrder[positionCount].x, 
                            ref velX, Time.deltaTime * Movement.PositionChangeSpeed);
                        var posY = Mathf.SmoothDamp(CameraObject.transform.position.y, Movement.PositionsInOrder[positionCount].y, 
                            ref velY, Time.deltaTime * Movement.PositionChangeSpeed);
                        var posZ = Mathf.SmoothDamp(CameraObject.transform.position.z, Movement.PositionsInOrder[positionCount].z, 
                            ref velZ, Time.deltaTime * Movement.PositionChangeSpeed);
                        Vector3 newPosition = new Vector3(posX, posY, posZ);

                        CameraObject.transform.position = newPosition;

                        //CameraObject.transform.position = Vector3.Slerp(CameraObject.transform.position,
                        //    Movement.PositionsInOrder[positionCount],
                        //    Time.deltaTime * Movement.PositionChangeSpeed);
                    }
                    else
                    {
                        if (positionCount < Movement.PositionsInOrder.Count - 1)
                        {
                            positionCount++;
                            rotationCount++;
                        }
                    }
                }
                else
                {
                    if ((CameraObject.transform.localRotation.eulerAngles - Movement.RotationsInOrder[positionCount]).magnitude
                        > Movement.RotationThreshold)
                        CameraObject.transform.localRotation = Quaternion.Lerp(CameraObject.transform.localRotation,
                            Quaternion.Euler(Movement.RotationsInOrder[positionCount]),
                            Time.deltaTime * Movement.RotationChangeSpeed);

                    if ((CameraObject.transform.localPosition - Movement.PositionsInOrder[positionCount]).magnitude
                        > Movement.PositionThreshold)
                    {
                        var posX = Mathf.SmoothDamp(CameraObject.transform.localPosition.x, Movement.PositionsInOrder[positionCount].x,
                            ref velX, Time.deltaTime * Movement.PositionChangeSpeed);
                        var posY = Mathf.SmoothDamp(CameraObject.transform.localPosition.y, Movement.PositionsInOrder[positionCount].y,
                            ref velY, Time.deltaTime * Movement.PositionChangeSpeed);
                        var posZ = Mathf.SmoothDamp(CameraObject.transform.localPosition.z, Movement.PositionsInOrder[positionCount].z,
                            ref velZ, Time.deltaTime * Movement.PositionChangeSpeed);
                        Vector3 newPosition = new Vector3(posX, posY, posZ);

                        CameraObject.transform.position = newPosition;

                        //CameraObject.transform.localPosition = Vector3.Slerp(CameraObject.transform.localPosition,
                        //    Movement.PositionsInOrder[positionCount],
                        //    Time.deltaTime * Movement.PositionChangeSpeed);
                    }
                    else
                    {
                        if (positionCount < Movement.PositionsInOrder.Count - 1)
                        {
                            positionCount++;
                            rotationCount++;
                        }
                    }
                }
            }
            if (timer >= Movement.MovementTime)
            {
                timer = 0f;
                positionCount = 0;
                rotationCount = 0;
                Triggered = false;
                CameraObject.SetActive(false);
                MainCamera.enabled = true;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.root.gameObject.layer == LayerMask.NameToLayer("Player") &&
            other.transform.root.tag == "Player")
        {
            if (IsAlwaysTriggered)
            {
                Triggered = true;
                //Save trigger ID and Count
            }
            else
            {
                if (CurrentTriggerCount < MaxTriggerCount)
                {
                    Triggered = true;
                    CurrentTriggerCount++;
                    //Save trigger ID and Count
                }
            }
        }
    }
}
