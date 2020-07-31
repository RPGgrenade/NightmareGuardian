using UnityEngine;
using System.Collections.Generic;

class GlowNightLight : MonoBehaviour
{
    public static GlowNightLight Instance;

    public Color glowColor = Color.grey;
    public float RotationSpeed = 50f;
    public float IncreaseSpeed = 0.2f;
    public float DecreaseSpeed = 1f;
    public float MaxIntensity = 1f;
    public float Intensity = 0f;

    public bool decreaseIntensity = true;

    void Awake()
    {
        Instance = this;
    }
    void Update() 
    {
        if (decreaseIntensity)
        {
            IntensityDecrease();
        }
        else 
        {
            IntensityIncrease();
        }
        if (Intensity > 0)
        {
            RotateGlow();
        }
    }
    void OnWillRenderObject()
    {
        GetComponent<Renderer>().sharedMaterial.SetColor("_TintColor", glowColor * Intensity);
    }
    void IntensityDecrease() 
    {
        if (Intensity > 0f)
        {
            Intensity -= DecreaseSpeed * Time.deltaTime;
        }
        else 
        {
            Intensity = 0f;
        }
    }
    public void IntensityIncrease()
    {
        if(Intensity < MaxIntensity)
        {
            Intensity += IncreaseSpeed * Time.deltaTime;
        }
        else
        {
            Intensity = MaxIntensity;
        }
    }
    public void RotateGlow() 
    {
        transform.Rotate(Vector3.up,RotationSpeed*Time.deltaTime);
    }
}