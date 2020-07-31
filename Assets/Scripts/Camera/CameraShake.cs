using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public Transform cameraTrans;
    [Header("Duration Properties")]
    public float ShakeDuration = 0f;
    public float ShakeDurationModifier = 0.4f;
    public float MaxShakeDuration = 1f;
    public float ShakeDecreaseSpeed = 1.0f;
    [Header("Amount Properties")]
    public float ShakeAmount = 0f;
    public float ShakeAmountModifier = 0.3f;
    public float MaxShakeAmount = 0.7f;

    // Start is called before the first frame update
    void Start()
    {
        if (cameraTrans == null)
            cameraTrans = this.transform;
    }
    
    public void ShakeUpdate(Vector3 currentPos)
    {
        if(ShakeDuration > 0)
        {
            cameraTrans.localPosition = currentPos + Random.insideUnitSphere * ShakeAmount;
            ShakeDuration -= Time.deltaTime * ShakeDecreaseSpeed;
        }
        else
            ShakeDuration = 0f;
    }

    public void ShakeCamera(float distance, float power, float intensity)
    {
        ShakeAmount = (intensity / Mathf.Pow(distance,2)) * ShakeAmountModifier;
        ShakeDuration = (intensity / power) * ShakeDurationModifier;

        if (ShakeAmount > MaxShakeAmount) { ShakeAmount = MaxShakeAmount; }
        if(ShakeDuration > MaxShakeDuration) { ShakeDuration = MaxShakeDuration; }
    }
}
