using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TP_Stats : MonoBehaviour
{
    public static TP_Stats Instance;

    [Header("Cotton Properties")]
    [Tooltip("The current amount of cotton life the character has")]
    [Range(0,200)]
    public float CottonGauge = 300f; //goes from 0 to 100. (Life "Bar")
    [Tooltip("The max of cotton life the character can have")]
    public float MaxCottonGauge = 300f; //goes from 0 to 100. (Life "Bar")
    [Tooltip("The Cotton the character is holding")]
    [Range(0, 100)]
    public float Cotton = 10f; //Maximum of 60 cotton. (Cotton In Bag)
    [Tooltip("The maximum amount of cotton the character can hold")]
    public float MaxCotton = 60f;
    [Space(1)]
    [Tooltip("Current size of the bag")]
    [Range(20, 80)]
    public float CottonBagSize = 0f; //Goes from 20 to 80
    [Tooltip("Initial size of the cotton bag")]
    public float CottonBagStartSize = 20f;
    [Tooltip("Divider for the cotton bag size for specific considerations")]
    public float CottonBagSizeDivider = 200f;
    [Tooltip("The puffiness rate of the bag upong collecting cotton")]
    public float BagPuffRate = 10f;
    [Space(8)]
    [Tooltip("The speed at which the character heals")]
    public float SewingRate = 0.2f; // from 0 to 1. (Healing Speed)
    [Tooltip("The reduction of the cotton balls on the body as health indicators")]
    public float CottonReduction = 600f;
    [Tooltip("The Cotton balls to be changed on the body in size")]
    public GameObject[] Cottons;
    public AudioClip RecoverySound;
    public AudioClip NoiseCancelSound;

    [Header("Noise Properties")]
    [Tooltip("The current amount of magic life the character has")]
    [Range(0,100)]
    public float Noise = 0f; //goes from 0 to 100. (Secondary Life "Bar")
    [Tooltip("The maximum amount of magic life the character has")]
    public float MaxNoise = 100f;
    [Tooltip("The amount the magic life recovers due to the child's calming down")]
    public float NoiseCalm = 5f;
    [Tooltip("Multiplier for the sake of quicker recovery during crouched cape animation")]
    public float CalmMultiplier = 1f;
    [Tooltip("The multiplier amount that increase noise recovery")]
    public float NoiseDefense = 2f;
    [Tooltip("Amount of time defense lasts")]
    public float NoiseDefenseTime = 600f;
    [Tooltip("Timer for abvove defense")]
    public float NoiseDefenseTimer = 0f;
    public bool NoiseBlocked = false;

    [Header("Defense properties")]
    [Tooltip("The Character's current Damage resistance")]
    [Range(0f,1f)]
    public float DamageResistance = 0f;
    public GameObject CottonBall;
    public float CottonSpeed = 50f;

    [Header("Menu Text Properties")]
    public Text DefeatedText;
    public string DefeatedString = "    Beaten ";
    public Text CottonText;
    public string CottonString = "  Cotton ";
    public Text BagText;
    public string BagString = " Bag ";


    //public enum DefensiveState 
    //{
    //    Defenseless,
    //    NightLightDefense,
    //    BlanketDefense,
    //    ParryDefense
    //}

    private float noiseGauge = 0f;
    //private DefensiveState defenseState;
    private float defense;

    public GameObject Bag;

    public float NoiseGauge { get; set; }

    void Awake() 
    {
        Instance = this;
        NoiseGauge = noiseGauge;
        CottonBagSize = CottonBagStartSize + Cotton;
        //Bag = this.transform.Find("Body").Find("Torso").Find("Cotton Bag").gameObject;
        UpdateCottonBag();
    }

    void Update() 
    {  
        if(TP_Controller.CharacterController == null)
        {
            return;
        }
        NoiseDecrease();
        if(NoiseDefenseTimer > 0)
            NoiseDefenseTimer -= Time.deltaTime;
        //UpdateDefense();
        UpdateUI();
        UpdateCottons();
        Noise = NoiseGauge;
    }

    private void UpdateUI()
    {
        DefeatedText.text = DefeatedString + SettingsHandler.Instance.EnemiesDefeated; //this will need to track the monsters defeated
        CottonText.text = CottonString + ((int)CottonGauge).ToString();
        BagText.text = BagString + ((int)Cotton).ToString();
    }

    public void UpdateCottons()
    {   //TODO: Change this so that it goes with the Cotton's initial size
        float size = (MaxCottonGauge - CottonGauge) / (CottonReduction);
        foreach(GameObject cotton in Cottons)
        {
            cotton.transform.localScale = Vector3.one * size;
        }
    }

    public void ReduceCotton(float Damage) //Damage will be determined in the future by the monster and the appropriate collision
    {
        CottonGauge -= (Damage);
        //UpdateCottonBag();
        //Apply Cotton Explosion
        int num = (int)Damage/10;//number of cotton balls to create
        for(int i = 0; i < num; i++)
        {
            GameObject cottonBall = GameObject.Instantiate(CottonBall,this.transform.position,Quaternion.identity);
            cottonBall.GetComponent<CottonScript>().CottonFillQuantity = Random.Range(1f, Damage /10f);
            cottonBall.GetComponent<CottonScript>().SetCottonBallSize();
            cottonBall.GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-CottonSpeed, CottonSpeed),
                Random.Range(-CottonSpeed, CottonSpeed),
                Random.Range(-CottonSpeed, CottonSpeed)));
        }

        if(CottonGauge <= 0f)
        {
            CottonGauge = 0f;
        }
    }

    public void SetNoiseDefense()
    {
        NoiseDefenseTimer = NoiseDefenseTime;
    }

    public void NoiseIncrease(float noise) 
    {
        if (!NoiseBlocked)
        {
            NoiseGauge += noise;
            if (NoiseGauge >= MaxNoise)
            {
                NoiseGauge = MaxNoise;
                //Activate Death Function
            }
        }
        else
        {
            this.GetComponent<ParticleHandler>().FindSpawnParticle("Noise Cancel");
            TP_Sound.Instance.PlaySelectedSound(NoiseCancelSound);
        }
        UpdateRibbon();
    }

    private void NoiseDecrease() //Decreases the noise due to child's tendency to calm down
    {
        if (NoiseGauge > 0f && NoiseGauge < MaxNoise)
        {
            if (NoiseDefenseTimer > 0)
                NoiseGauge -= NoiseCalm * CalmMultiplier * NoiseDefense * Time.deltaTime;
            else
                NoiseGauge -= NoiseCalm * CalmMultiplier * Time.deltaTime;
        }
        else if(NoiseGauge <= 0f)
        {
            NoiseGauge = 0f;
        }
        UpdateRibbon();
    }

    private void UpdateRibbon() 
    {
        //Set the brightness of the ribbon's Glow
        //GlowRibbon.Instance.Intensity = (MaxNoise - Noise) / (MaxNoise);
        GlowRibbon.Instance.Intensity = (Noise) / (MaxNoise);
        //if(Hearing.This.WithinRange)
        //{
        //    Hearing.This.Noise = Noise;
        //}
    }

    public void CottonIncrease() //The healing section using the cotton from the bag
    {
        float CottonUsed;
        if(CottonGauge < MaxCottonGauge && Cotton >= SewingRate)
        {
            CottonUsed = SewingRate;
            CottonGauge += CottonUsed;
            Cotton -= CottonUsed;
            CottonGauge = Mathf.Clamp(CottonGauge, 0f, MaxCottonGauge);
            Cotton = Mathf.Clamp(Cotton, 0f, MaxCotton);
            //Activate sewing animation
            this.GetComponent<ParticleHandler>().FindSpawnParticle("Recovery");
            TP_Sound.Instance.PlaySelectedSound(RecoverySound);
            UpdateCottonBag();
        }
    }

    public void UpdateCottonBag() //Sets the size of the bag holding the Cotton
    {
        if(Cotton <= 0f)
        {
            Cotton = 0f;
            CottonBagSize = CottonBagStartSize;
        }
        else 
        {
            CottonBagSize = CottonBagStartSize + Cotton;   
        }
        if(Bag != null)
        {
            Bag.transform.localScale = new Vector3(Bag.transform.localScale.x,Bag.transform.localScale.y,CottonBagSize / CottonBagSizeDivider);
        }
        //Set size of the bag using CottonBagSize
    }

    public void AddToCottonBag(float cotton) 
    {
        if (Cotton < MaxCotton)
        {
            Cotton += cotton;
            if(Cotton >= MaxCotton)
            {
                Cotton = MaxCotton;
            }
            UpdateCottonBag();
        }
    }
}
