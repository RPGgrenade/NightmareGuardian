using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeballRecovery : MonoBehaviour
{
    public EyeStats Stats;
    public float RecoverySpeed = 5f;

    // Start is called before the first frame update
    void Start()
    {
        if (Stats == null)
            Stats = this.GetComponent<EyeStats>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Stats != null && Stats.Health < Stats.FullHealth)
            Stats.Health += Time.deltaTime * RecoverySpeed;
    }
}
