using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(GrabProperties))]
[RequireComponent(typeof(ConstantForce))]
[RequireComponent(typeof(Rigidbody))]
public class SoundManager : MonoBehaviour
{
    [Header("Sounds")]
    public AudioSource PrimarySound;
    public AudioSource SecondarySound;
    public GameObject Sonar;
    [Tooltip("The minimum amount of time needed to go by before another sonar object can spawn or noise be applied")]
    public float Cooldown = 0.5f; //might make private so it affects all objects the same
    [Header("Object Physical Properties")]
    [Tooltip("Multiplies volume quantity on play")]
    public float VolumeMultiplier = 1f;
    [Tooltip("The active noise after calculations are done")]
    public float Noise = 0f;
    [Tooltip("The larger the mass, the larger the sound")]
    public float Massiveness = 0f; //the larger it is in mass, the larger the sound
    [Tooltip("The hollower it is, the great echo, increasing sound")]
    public float Hollowness = 0f; //The Hollower it is, the greater Echo, increasing the sound
    [Tooltip("Limits highest pitch of object")]
    public float MaxPitch = 3f;
    [Tooltip("Limits lowest pitch of object")]
    public float MinPitch = 0f;

    [Header("Material Values")]
    [Tooltip("Percentage the noise volume if Plastic")]
    public float Plastic = 0.4f;
    [Tooltip("Percentage the noise volume if Wood")]
    public float Wood = 0.6f;
    [Tooltip("Percentage the noise volume if Fabric")]
    public float Fabric = 0.2f;
    [Tooltip("Percentage the noise volume if Metal")]
    public float Metal = 0.7f;
    [Tooltip("Percentage the noise volume if Glass")]
    public float Glass = 0.9f;
    [Tooltip("Percentage the noise volume if Ceramic")]
    public float Ceramic = 1f;

    public float PlasticSurface = 3f;
    public float WoodSurface = 2f;
    public float FabricSurface = 4.5f;
    public float MetalSurface = 1.5f;
    public float GlassSurface = 0.5f;
    public float CeramicSurface = 0.5f;
    public bool Grabbed = false;
    public bool debug = true;

    public enum Material
    {
        Plastic,
        Wood,
        Fabric,
        Metal,
        Glass,
        Ceramic
    };

    public enum Surface
    {
        Plastic,
        Wood,
        Fabric,
        Metal,
        Glass,
        Ceramic
    };

    public Material material;
    public Surface surface;
    private float mat;
    private float impactArea = 0f; //The Larger the Impact Area, the quieter the sounds is.
    private float cooldown = 0f;
    private float velocity { get; set; } //The higher the speed, the larger the sound
    private float SurfaceDampening { get; set; }

    //private GameObject PrimarySound; //Main sound object makes when impact occurs
    //private GameObject SecondarySound; // Sound that surface impacted makes in same area of impact

    private Rigidbody body;
    
    void Awake() 
    {
        SetMaterial();
        body = this.GetComponent<Rigidbody>();
        //PrimarySound = this.transform.Find("Primary Sound").gameObject;
        //SecondarySound = this.transform.Find("Secondary Sound").gameObject;
        Debug.Log("Sleep Threshold is: " + body.sleepThreshold);
    }

    private void FixedUpdate()
    {
        if (body.velocity.magnitude <= body.sleepThreshold
            && !Grabbed && this.transform.Find("Needle Anchor") == null 
            && !this.GetComponent<GrabProperties>().Thrown)
            body.Sleep();
    }

    void Update()
    {
        //VolumeMultiplier = SettingsHandler.Instance.SFXVolume;
        if(cooldown < Cooldown)
            cooldown += Time.deltaTime;
    }

    void AddSecondarySound() 
    {
        SecondarySound.clip = Resources.Load("Sounds/Primary Impact Sounds/Wooden Impact 1") as AudioClip;
        SecondarySound.Play();
    }

    void SetMaterial()
    {
        //Set the material checking the tag or whatever that determines it
        GameObject noisyObject;
        noisyObject = this.gameObject;
        if(noisyObject.tag == Material.Plastic.ToString())
        {
            material = Material.Plastic;
            mat = Plastic;
        }
        else if (noisyObject.tag == Material.Ceramic.ToString())
        {
            material = Material.Ceramic;
            mat = Ceramic;
        }
        else if (noisyObject.tag == Material.Metal.ToString())
        {
            material = Material.Metal;
            mat = Metal;
        }
        else if (noisyObject.tag == Material.Glass.ToString())
        {
            material = Material.Glass;
            mat = Glass;
        }
        else if (noisyObject.tag == Material.Wood.ToString())
        {
            material = Material.Wood;
            mat = Wood;
        }
        else if (noisyObject.tag == Material.Fabric.ToString())
        {
            material = Material.Fabric;
            mat = Fabric;
        }
    }

    void OnCollisionEnter(Collision collider) 
    {
        if (cooldown >= Cooldown)
        {
            if (collider.transform.tag != "Player")
            {
                cooldown = 0f;
                CalculateImpactArea(collider);
                SetVelocity(collider);
                GetSurfaceDampening(collider);
                CalculateNoise();
                if (collider.gameObject.layer == LayerMask.NameToLayer("Monster"))
                {
                    float velocity = collider.relativeVelocity.magnitude;
                    CreatureCollision collision = collider.transform.GetComponentInParent<CreatureCollision>();
                    GrabProperties grab = this.transform.GetComponent<GrabProperties>();
                    if (collision != null && velocity > collision.ImpactSpeedThreshold && grab.Thrown)
                        collision.ApplyDamage(0f,2f);
                }
                return;
            }
            //else if(collider.transform.tag == "Cape")
            //{
            //    cooldown = 0f;
            //    //CalculateImpactArea(collider);
            //    SetVelocity(collider);
            //    GetSurfaceDampening(collider);
            //    //CalculateNoise();
            //}
        }
    }

    //void OnCollisionStay(Collision collider) //possibly use to create sliding sounds and such 
    //{
    //    CalculateImpactArea(collider);
    //    SetVelocity(collider);
    //    GetSurfaceDampening(collider);
    //    //CalculateNoise();
    //}

    void CalculateImpactArea(Collision col) 
    {
        //Calculate the area of impact, it needs to be somewhat flexible otherwise it will always be a single point
        float points= 0f;
        points = (float)col.contacts.Length;
        impactArea = points;
    }

    void SetVelocity(Collision col, float multiplier = 1f) 
    {
        //Set the speed the object is going at each fream, don't set if an impact has occurred
        velocity = col.relativeVelocity.magnitude * multiplier;
    }

    void GetSurfaceDampening(Collision col)
    {
        //On impact, observe the tag of the surface to find the dampening factor
        string name = col.collider.tag;
        if(name == Material.Ceramic.ToString())
        {
            surface = Surface.Ceramic;
            SurfaceDampening = CeramicSurface;
        }
        else if (name == Material.Fabric.ToString()) 
        {
            surface = Surface.Fabric;
            SurfaceDampening = FabricSurface;
        }
        else if(name == Material.Glass.ToString())
        {
            surface = Surface.Glass;
            SurfaceDampening = GlassSurface;
        }
        else if(name == Material.Metal.ToString())
        {
            surface = Surface.Metal;
            SurfaceDampening = MetalSurface;
        }
        else if(name == Material.Plastic.ToString())
        {
            surface = Surface.Plastic;
            SurfaceDampening = PlasticSurface;
        }
        else if (name == Material.Wood.ToString())
        {
            surface = Surface.Wood;
            SurfaceDampening = WoodSurface;
        }
        else 
        {
            SurfaceDampening = 10f;
        }
    }

    void CalculateNoise() 
    {
        Noise = (Massiveness * Hollowness * velocity * mat) / (impactArea * SurfaceDampening); //Basic calculation, might change it.
        
        ApplyNoise(Noise); //Apply created noise to the noise gauge
        
        Noise = 0f; //Reset noise so it can be applied again later.
    }

    void PlaySound() //Sets Volumes of each sound then plays them
    {
        //AddSecondarySound();
        SecondarySound.volume = (Noise/SurfaceDampening) * VolumeMultiplier;
        PrimarySound.volume = ((Noise * Massiveness)/SurfaceDampening) * VolumeMultiplier; //Find better equations for the volumes of these sounds
        SetPitch();
        PrimarySound.Play();
        SecondarySound.Play();
        //PrimaryEcho.audio.Play();
    }

    void SetPitch() 
    {
        PrimarySound.pitch = velocity/impactArea;
        
        if(PrimarySound.pitch <= MinPitch)
        {
            PrimarySound.pitch = MinPitch;
        }
        else if(PrimarySound.GetComponent<AudioSource>().pitch >= MaxPitch)
        {
            PrimarySound.pitch = MaxPitch;
        }
        SecondarySound.pitch = PrimarySound.pitch;
        //PrimaryEcho.audio.pitch = velocity / impactArea;
    }

    void ApplyNoise(float noise) 
    {
        TP_Stats.Instance.NoiseIncrease(noise);
        GameObject bubble = Instantiate(Sonar, this.transform.position, this.transform.rotation);
        SonarManager son = bubble.GetComponent<SonarManager>();
        son.SetProperties(Noise);
        PlaySound();
        if (debug == true)
        {
            //Debug.Log("Noise was " + noise. + ".");
            //Debug.Log("Impact Area was " + impactArea + ".");
            //Debug.Log("Speed of Impact was " + velocity + ".");
        }
    }
}
