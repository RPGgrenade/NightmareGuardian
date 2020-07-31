using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    [Header("FOV properties")]
    public float CurrentFOV = 0f;
    public float VariationFOV = 20f;
    public float VariationFOVUpSpeed = 1f;
    public float VariationFOVDownSpeed = 1f;

    private bool up = true;

    // Start is called before the first frame update
    void Start()
    {
        CurrentFOV = Camera.main.fieldOfView;
    }

    // Update is called once per frame
    void Update()
    {
        if (TP_Stats.Instance.Noise > 0f)
            UpdateFOVWithNoise();
    }

    public void UpdateFOVWithNoise()
    {
        float noisePercentage = TP_Stats.Instance.Noise / TP_Stats.Instance.MaxNoise;
        float limitFOV = CurrentFOV + VariationFOV;

        if (up)
            Camera.main.fieldOfView += (Time.deltaTime * VariationFOVUpSpeed * noisePercentage);
        else
            Camera.main.fieldOfView -= (Time.deltaTime * VariationFOVDownSpeed * noisePercentage);

        if (up)
        {
            if (Camera.main.fieldOfView >= limitFOV)
                up = false;
        }
        else
        {
            if (Camera.main.fieldOfView <= CurrentFOV)
                up = true;
        }
    }
}
