using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogHandler : MonoBehaviour
{
    public Transform Target;
    [Space(15)]
    public bool Adjusting = false;
    public float AreaFogAdjustmentSpeed = 1f;
    public Color AreaFogColor = Color.black;
    public float AreaFogDensity = 0.015f;

    // Start is called before the first frame update
    void Start()
    {
        if (Target == null)
            Target = GameObject.FindGameObjectWithTag("Player").transform.root;
    }

    private void Update()
    {
        if (Adjusting)
        {
            if (RenderSettings.fogDensity != AreaFogDensity)
                RenderSettings.fogDensity = 
                    Mathf.Lerp(RenderSettings.fogDensity, AreaFogDensity, Time.deltaTime * AreaFogAdjustmentSpeed);
            if (RenderSettings.fogColor != AreaFogColor)
                RenderSettings.fogColor =
                    Color.Lerp(RenderSettings.fogColor, AreaFogColor, Time.deltaTime * AreaFogAdjustmentSpeed);
            if ((RenderSettings.fogDensity - AreaFogDensity) < 0.001f && RenderSettings.fogColor == AreaFogColor)
                Adjusting = false;
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.root.tag == "Player" && 
            other.transform.root.gameObject.layer == LayerMask.NameToLayer("Player"))
            Adjusting = true;
    }
    
    private void OnTriggerExit(Collider other)
    {
        if(other.transform.root.tag == "Player" && 
            other.transform.root.gameObject.layer == LayerMask.NameToLayer("Player"))
            Adjusting = false;
    }
}
