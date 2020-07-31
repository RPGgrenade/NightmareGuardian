using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeProximity : MonoBehaviour
{
    [Header("Tracking Objects")]
    [Tooltip("The mesh renderer to fade (will be this object's mesh renderer automatically unless assigned)")]
    public MeshRenderer RendererMesh;
    [Tooltip("The camera to track in order to choose when to fade (will be main camera unless assigned)")]
    public Camera Camera;
    [Header("Fading properties")]
    [Tooltip("How much fade step to fade the object")]
    [Min(0.001f)]
    public float FadeStep = 0.05f;
    [Tooltip("How fast to fade the object (the smaller the number, the faster)")]
    [Min(0.001f)]
    public float FadeSpeed = 0.05f;
    [Tooltip("How close before fading occurs")]
    public float FadeDistance = 1f;
    [Tooltip("How close camera has to be to worry about fading (save some minor memory and could vary for larger objects)")]
    public float MaxDistanceToConsider = 2f;
    [Tooltip("Distance tracked on camera")]
    public float DistanceFromCamera;


    // Start is called before the first frame update
    void Start()
    {
        if(RendererMesh == null)
            RendererMesh = this.GetComponent<MeshRenderer>();
        if (Camera == null)
            Camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        checkDistance();
        if(DistanceFromCamera < MaxDistanceToConsider)
            fading();
    }

    void checkDistance()
    {
        DistanceFromCamera = Vector3.Distance(this.transform.position, Camera.transform.position);
    }

    void fading()
    {
        Color color = RendererMesh.material.color;
        if (DistanceFromCamera > FadeDistance)
            StartCoroutine("FadeOut");
        else if (DistanceFromCamera <= FadeDistance)
            StartCoroutine("FadeIn");
    }

    IEnumerator FadeIn()
    {
        for (float opacity = 0; opacity <= 1; opacity += FadeStep)
        {
            Color color = RendererMesh.material.color;
            color.a = opacity;
            RendererMesh.material.color = color;
            yield return new WaitForSeconds(FadeSpeed);
        }
    }

    IEnumerator FadeOut()
    {
        for (float opacity = 1; opacity >= 0; opacity -= FadeStep)
        {
            Color color = RendererMesh.material.color;
            color.a = opacity;
            RendererMesh.material.color = color;
            yield return new WaitForSeconds(FadeSpeed);
        }
    }

}
