using UnityEngine;
using System.Collections.Generic;

class FaceCamera : MonoBehaviour
{
    public Transform cameraTransform;
    public Transform Target;
    public bool Rotate = false;
    public bool Clipping = true;
    public float Distance = 2f;

    void Start() 
    {
        if(cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        if(Target == null)
        {
            return;
        }
    }
    void Update() 
    {
        if(Rotate)
        {
            transform.LookAt(cameraTransform);
        }
        UpdatePosition();
    }
    void UpdatePosition() 
    {
        if (!Clipping)
        {
            transform.position = Target.position + (((cameraTransform.position - Target.position).normalized) * Distance);
        }
        else 
        {
            transform.position = Target.position;
        }
    }
}
