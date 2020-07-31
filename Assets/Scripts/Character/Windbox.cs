using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Windbox : MonoBehaviour
{
    public Collider Trigger;
    [Header("Launch Properties for Toys")]
    [Tooltip("How fast the launch is")]
    public float LaunchSpeed = 10f;
    [Tooltip("Angle upwards so it can launch at least a little bit")]
    public float LaunchAngle = 0f;
    [Tooltip("Used by other scripts to modify strength")]
    public float LaunchMultiplier = 1f;
    [Tooltip("How fast the multiplier can be increased")]
    public float LaunchMultiplierStep = 0.05f;
    [Header("Launch Properties for Player")]
    [Tooltip("Indicates whether this windbox can push the player (mostly used by monsters)")]
    public bool CanPushPlayer = false;
    [Tooltip("Indicates whether or not the windbox pushes the player vertically")]
    public bool AppliesVertically = true;

    // Start is called before the first frame update
    void Start()
    {
        if (Trigger == null)
            this.GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(Trigger.isTrigger && Trigger.enabled)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Toys"))
                LaunchItem(other.gameObject);
            if (CanPushPlayer && other.gameObject.layer == LayerMask.NameToLayer("Player"))
                PushPlayer();
        }
    }

    public void IncreaseLaunchMultiplier()
    {
        LaunchMultiplier += LaunchMultiplierStep;
    }

    public void PushPlayer()
    {
        //Grabs the normalized direction between player and windbox
        Vector3 playerDirection = (TP_Motor.Instance.transform.position - this.transform.position).normalized;
        if (!AppliesVertically)
            playerDirection = new Vector3(playerDirection.x, 0f, playerDirection.z);
        Vector3 playerDirectionRight = new Vector3(playerDirection.x, 0f, 0f);
        TP_Motor.Instance.MomentumVector = Quaternion.AngleAxis(LaunchAngle, playerDirectionRight)
            * playerDirection * LaunchSpeed;
    }

    public void LaunchItem(GameObject item)
    {
        Rigidbody itemBody = item.GetComponent<Rigidbody>();
        itemBody.useGravity = true;
        itemBody.constraints = RigidbodyConstraints.None;
        Vector3 itemDirection = (item.transform.position - this.transform.position).normalized;
        Vector3 itemDirectionRight = new Vector3(itemDirection.x, 0f, 0f);

        //itemBody.AddForce(Quaternion.AngleAxis(LaunchAngle, this.transform.root.right)
        //    * this.transform.root.forward * LaunchSpeed, 
        //    ForceMode.Impulse);

        itemBody.AddForce(Quaternion.AngleAxis(LaunchAngle, itemDirectionRight)
            * itemDirection * LaunchSpeed,
            ForceMode.Impulse);

        item.GetComponent<GrabProperties>().Thrown = true;
        item.GetComponent<SoundManager>().Grabbed = false;
        ResetLaunchMultiplier();
    }

    public void ResetLaunchMultiplier()
    {
        LaunchMultiplier = 1f;
    }
}
