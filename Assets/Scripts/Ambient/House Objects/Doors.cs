using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doors : MonoBehaviour
{
    [Header("Opening Properties")]
    [Tooltip("For when a door needs to be unlocked with a Key")]
    public bool Locked = true;
    [Tooltip("For when a door can't be pushed, only pulled by the Knob")]
    public bool Knobbed = true;
    [Tooltip("Indicator for when the door is closed or not")]
    public bool Closed = true;
    [Tooltip("How different the angle is capable of being")]
    public float MaxAngleDifference = 80f;
    [Tooltip("How close to the original angle it has to be to snap it into place and close it")]
    public float AngleDifferenceThreshold = 1f;
    [Tooltip("How fast the door rotates around its axis")]
    public float SpinSpeed = 0.2f;
    [Tooltip("The axis reference for the door to rotate on")]
    public Transform Hinge;
    [Tooltip("The reference for the door knob")]
    public Transform DoorKnob;
    [Tooltip("The position with which it gets close to realizing when it closes")]
    public Transform ClosePosition;
    [Tooltip("The objects that can push a door when it's capable of it")]
    public LayerMask PushableLayers;
    [Tooltip("Key that unlocks this door")]
    public GameObject Key;

    [Header("Closing Properties")]
    [Tooltip("The angle the door starts at")]
    public float InitialAngle = 0f;
    [Tooltip("The angle the door is currently at")]
    public float CurrentAngle = 0f;
    [Tooltip("How long a pull/push isn't called before the door closes on its own")]
    public float ReturnTime = 1.2f;
    [Tooltip("Current timer count for when to close")]
    public float ReturnTimer = 0f;

    [Header("Sound Properties")]
    public AudioSource Audio;
    public float Volume;
    public AudioClip[] LockSounds;
    public AudioClip[] OpenSounds;
    public AudioClip[] CloseSounds;

    private OcclusionPortal portal;

    private void Start()
    {
        InitialAngle = this.transform.localRotation.eulerAngles.y;
        portal = this.GetComponent<OcclusionPortal>();
        if(Locked)
            loadIfUnlocked();
    }

    private void loadIfUnlocked()
    {
        /*TODO: Get save data to see if door has been unlocked already,
         * If it has been unlocked before, destroy unlock and destroy the key object reference
        */
    }

    private void FixedUpdate()
    {
        if (!Closed)
        {
            ReturnTimer += Time.deltaTime;
            if (ReturnTimer >= ReturnTime)
                close();
        }

        CurrentAngle = this.transform.localRotation.eulerAngles.y;
        portal.open = !Closed;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (PushableLayers == (PushableLayers | (1 << collision.gameObject.layer)))
        {
            if((!Closed || !Knobbed) && !Locked)
                Push(collision.transform.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == Key)
        {
            Locked = false;
            //Add in particle effect for key opening TODO
            GameObject.Destroy(Key);
        }
    }

    private void close()
    {
        bool spin = spinDirection(ClosePosition.position);
        if (spin)
            RotateDoor(-SpinSpeed);
        else
            RotateDoor(SpinSpeed);
        if (Mathf.Abs(CurrentAngle - InitialAngle) <= AngleDifferenceThreshold)
        {
            Closed = true;
            Audio.PlayOneShot(getRandomSound(CloseSounds), Volume);
        }
    }

    public void Push(Vector3 pusherPosition)
    {
        if (Closed)
        {
            Closed = false;
            Audio.PlayOneShot(getRandomSound(OpenSounds), Volume);
        }
        ReturnTimer = 0f;
        if (Mathf.Abs(CurrentAngle - InitialAngle) <= MaxAngleDifference)
        {
            bool spin = spinDirection(pusherPosition);
            if (spin)
                RotateDoor(SpinSpeed);
            else
                RotateDoor(-SpinSpeed);
        }
    }

    public void Pull(Vector3 pullerPosition)
    {
        if (Closed)
        {
            Closed = false;
            Audio.PlayOneShot(getRandomSound(OpenSounds), Volume);
        }
        ReturnTimer = 0f;
        if (Mathf.Abs(CurrentAngle - InitialAngle) <= MaxAngleDifference)
        {
            bool spin = spinDirection(pullerPosition);
            if(spin)
                RotateDoor(-SpinSpeed);
            else
                RotateDoor(SpinSpeed);
        }
    }

    private bool spinDirection(Vector3 actorPosition)
    {
        Vector3 doorDirection = this.transform.forward; //using right to make calculations easier
        Vector3 actorDirection = (actorPosition - this.transform.position).normalized;
        actorDirection.y = 0f;
        bool spinDir = (Vector3.Dot(actorDirection, doorDirection) > 0);
        return spinDir;
    }

    private void RotateDoor(float angle)
    {
        if (!Locked)
        {
            this.transform.RotateAround(Hinge.position, Vector3.up, angle * Time.deltaTime);
        }
    }

    private AudioClip getRandomSound(AudioClip[] sounds)
    {
        return sounds[Random.Range(0, sounds.Length)];
    }
}
