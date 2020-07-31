using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BellNoise : MonoBehaviour
{
    [Header("Sound References")]
    [Tooltip("Main Sound to be played")]
    public AudioSource PrimarySound;
    [Tooltip("Reference to the sound bubble that appears")]
    public GameObject Sonar;
    [Tooltip("The minimum amount of time needed to go by before another sonar object can spawn or noise be applied")]
    public float Cooldown = 0.5f;

    [Header("Object Physical Properties")]
    [Tooltip("Multiplies volume quantity on play")]
    public float VolumeMultiplier = 1f;
    [Tooltip("The active noise after calculations are done")]
    public float Noise = 0f;
    [Tooltip("Limits highest pitch of object")]
    public float MaxPitch = 3f;
    [Tooltip("Limits lowest pitch of object")]
    public float MinPitch = 0f;

    void PlaySound() //Sets Volumes of each sound then plays them
    {
        //AddSecondarySound();
        PrimarySound.volume = Noise * VolumeMultiplier; //Find better equations for the volumes of these sounds
        SetPitch();
        PrimarySound.Play();
        //PrimaryEcho.audio.Play();
    }

    void SetPitch()
    {
        PrimarySound.pitch = Random.Range(MinPitch, MaxPitch);
    }

    void ApplyNoise()
    {
        TP_Stats.Instance.NoiseIncrease(Noise);
        GameObject bubble = Instantiate(Sonar, this.transform.position, this.transform.rotation);
        SonarManager son = bubble.GetComponent<SonarManager>();
        son.SetProperties(Noise);
        PlaySound();
    }
}
