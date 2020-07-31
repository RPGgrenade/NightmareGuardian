using UnityEngine;
using System.Collections.Generic;
using System.Linq;
    
public class GlowEye : MonoBehaviour
{
    public static GlowEye Instance;

    public Color GlowColor = Color.grey;
    public float Intensity = 0.5f;
    public float ReductionTime = 1f;

    private float targetIntensity;

    void Awake()
    {
        Instance = this;
    }
    void Update() 
    {
        if(Intensity != targetIntensity)
        {
            Intensity = Mathf.Lerp(Intensity,targetIntensity,ReductionTime);
        }
    }
    void OnWillRenderObject()
    {
        GetComponent<Renderer>().sharedMaterial.SetColor("_TintColor", GlowColor * Intensity);
    }
    public void ChangeIntensity(float glow) 
    {
        targetIntensity = glow;
    }
}
