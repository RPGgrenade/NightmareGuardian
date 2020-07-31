using UnityEngine;
using System.Collections.Generic;

public class CottonScript : MonoBehaviour
{
    [Header("Cotton Source Properties")]
    [Tooltip("Indicates whether this cotton is a source or not")]
    public bool IsSource = false;
    [Tooltip("Indicates how quickly this source will fill the pouch")]
    public float CottonSourceFillSpeed = 5f;
    [Header("Cotton Properties (non source)")]
    [Tooltip("How long after appearing before cotton can be grabbed")]
    public float SpawnIntangibility = 0.5f;
    [Tooltip("The amount of cotton this will fill a pouch")]
    [Range(1f,10f)]
    public float CottonFillQuantity = 0f;
    [Tooltip("The smallest amount a cotton ball can fill")]
    public float CottonMinFillQuantity = 10f;
    [Tooltip("The largest amount a cotton ball can fill")]
    public float CottonMaxFillQuantity = 40f;
    [Tooltip("A scaling number to create a believable cotton size")]
    public float CottonSizeReduction = 50f;
    [Tooltip("")]
    public float CottonReductionSpeed = 0.1f;

    void Awake() 
    {
        if (!IsSource)
        { 
            if(CottonFillQuantity == 0)
                CottonFillQuantity = Random.Range(CottonMinFillQuantity, CottonMaxFillQuantity); //Sets a random quantity to how much of your pouch the single cotton ball fills
            SetCottonBallSize(); //Sets the size of the ball based on the fill quantity
        }
        else
        {
            Rigidbody body = this.GetComponent<Rigidbody>();
            body.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;
            body.useGravity = false;
        }
    }

    void Update()
    {
        SpawnIntangibility -= Time.deltaTime;

        if (!IsSource)
            reduceCottonSize();
    }

    void OnTriggerEnter(Collider other) 
    {
        if (SpawnIntangibility <= 0)
        {
            if (other.tag.ToString() == "Player" && (TP_Stats.Instance.Cotton < TP_Stats.Instance.MaxCotton) && !IsSource)
            {   //If the player touches the cotton ball while their pouch isn't full and the ball isn't a source area, it'll disappear and fill the bag by the quantity it has.
                TP_Stats.Instance.AddToCottonBag(CottonFillQuantity);
                GameObject.Destroy(this.gameObject);
            }
        }
    }

    void OnTriggerStay(Collider other) 
    { 
        if(IsSource && other.tag.ToString() == "Player" && (TP_Stats.Instance.Cotton < TP_Stats.Instance.MaxCotton))
        {   //Only occurs with a cotton source. Slowly fills the cotton bag over time.
            TP_Stats.Instance.AddToCottonBag(CottonSourceFillSpeed * Time.deltaTime);
        }
    }

    public void SetCottonBallSize()
    {
        float scale = CottonFillQuantity / CottonSizeReduction;
        transform.localScale = new Vector3(scale, scale, scale);
    }

    void reduceCottonSize()
    {
        float speed = CottonReductionSpeed * Time.deltaTime;
        this.transform.localScale -= new Vector3(speed, speed, speed);
        if(this.transform.localScale.x <= 0)
            GameObject.Destroy(this.gameObject);
    }
}