using UnityEngine;
using System.Collections.Generic;

class GlowRibbon : MonoBehaviour
{
    public static GlowRibbon Instance;
    
    [Tooltip("Current Light Intensity")]
    public float Intensity = 0.5f;
    [Tooltip("Percentage intensity is modified")]
    public float IntensityPercentage = 0.3f;

    private Light glow;

    void Awake() 
    {
        Instance = this;
        glow = this.GetComponent<Light>();
    }
    void Update() 
    {
        glow.intensity = Intensity * IntensityPercentage;
        glow.range = Mathf.Clamp(Intensity * 2f, 0f, 1f);
    }
}

