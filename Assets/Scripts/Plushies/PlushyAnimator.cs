using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlushyAnimator : MonoBehaviour
{
    public enum KnightBehavior
    {
        Petting,
        Bolstering,
        Posing,
        Kneeling
    }

    [Header("Plushy Properties")]
    public string Name;
    public bool IsMonster = false;

    [Header("Target tracking properties")]
    public Transform TeddyTarget;
    public Transform NearestMonster;

    [Header("Tracking Number properties")]
    [Tooltip("Minimum distance from Teddy bear for activation under this range")]
    public float MinDistanceFromTeddy = 1f;
    [Tooltip("Minimum distance from closest monster for activation over this range")]
    public float MinDistanceFromMonster = 20f;
    [Tooltip("How fast the updating happens")]
    public float DetectionUpdateSpeed = 0.4f;
    [Tooltip("Limit of updating wait time")]
    public float DetectionUpdateThreshold = 1f;

    [Header("Animation state trackers")]
    [Tooltip("Indicator for when the plushy is near a monster")]
    public bool NearMonster = false;
    [Tooltip("Indicator for when the plushy is near Teddy")]
    public bool NearTeddy = false;
    [Tooltip("Indicator for when the plushy has been found")]
    public bool Found = false;

    [Header("Appealing Properties")]
    [Tooltip("Implies whether or not the plushy is a knightly plush")]
    public bool IsKnight = false;
    [Tooltip("Indicates if the plushy is happy with Teddy's displays")]
    public bool Happy = false;
    [Tooltip("The favorite display the plushy likes")]
    public KnightBehavior FavoriteAction;

    [Header("Cotton Throwing Properties")]
    public bool CanThrowCotton = true;
    [Tooltip("How long is left for next appeal opportunity")]
    public float CottonTimer = 0f;
    [Tooltip("How long to wait between each appeal to get cotton")]
    public float CottonCooldown = 30f;
    [Tooltip("How much the cotton is able to heal you")]
    public float CottonStrength = 20f;
    [Tooltip("How fast the plushy will throw the cotton")]
    public float CottonThrowSpeed = 100f;
    [Tooltip("Location where cotton will spawn from")]
    public Transform CottonSpawner;
    [Tooltip("Cotton to be thrown")]
    public GameObject Cotton;

    private float updateTimer = 0f;
    private Animator animator;
    private Animator teddyAnim;
    private PlushyHeadTurn head;
    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponent<Animator>();
        head = this.GetComponentInChildren<PlushyHeadTurn>();
        if (TeddyTarget == null)
            TeddyTarget = GameObject.FindWithTag("Player").transform.root;
        if (TeddyTarget != null)
            teddyAnim = TeddyTarget.GetComponent<Animator>();
        updateMonster();
        Found = Saves.GetPlushyFound(Name);
    }

    // Update is called once per frame
    void Update()
    {
        updateMonster();
        updateStates();
        updateAnimator();
    }

    private void updateAnimator()
    {
        animator.SetBool("Near Monster", NearMonster);
        animator.SetBool("Near Target", NearTeddy);
        animator.SetBool("Happy", Happy);
        animator.SetBool("Cotton",CanThrowCotton);
    }

    private void updateStates()
    {
        float distanceFromTeddy = 0f;
        float distanceFromMonster = 0f;
        if (TeddyTarget != null)
        {
            distanceFromTeddy = Vector3.Distance(this.transform.position, TeddyTarget.position);
            NearTeddy = distanceFromTeddy <= MinDistanceFromTeddy;
            if (NearTeddy && head.IsLooking)
            {
                Happy = (FavoriteAction == KnightBehavior.Petting && teddyAnim.GetCurrentAnimatorStateInfo(6).IsName("Petting"))
                    || (FavoriteAction == KnightBehavior.Bolstering && teddyAnim.GetCurrentAnimatorStateInfo(6).IsName("Bolstering"))
                    || (FavoriteAction == KnightBehavior.Posing && teddyAnim.GetCurrentAnimatorStateInfo(6).IsName("Posing"))
                    || (FavoriteAction == KnightBehavior.Kneeling && teddyAnim.GetCurrentAnimatorStateInfo(6).IsName("Kneeling"));
            }
            else
                Happy = false;
        }
        if (NearestMonster != null)
        {
            distanceFromMonster = Vector3.Distance(this.transform.position, NearestMonster.position);
            NearMonster = distanceFromMonster <= MinDistanceFromMonster;
        }

        if (!Found)
            Found = NearTeddy && !NearMonster;
        else
        {
            //Add in the finding jingle
            if (NearTeddy && !NearMonster && !Saves.GetPlushyFound(Name))
            {
                Saves.SavePlushyFound(Name);
                JinglesHandler.Instance.PlayeJingle("Friend");
            }
        }

        CanThrowCotton = CottonTimer <= 0f;
        if (!CanThrowCotton)
            CottonTimer -= Time.deltaTime;
    }

    private void updateMonster()
    {
        updateTimer += Time.deltaTime * DetectionUpdateSpeed;
        if(updateTimer >= DetectionUpdateThreshold)
        {
            updateTimer = 0f;
            GameObject monster = GameObject.FindWithTag("Monster");
            if (monster != null)
                NearestMonster = monster.transform.root;
            else
                NearestMonster = null;
        }
    }

    public void SpawnCotton()
    {
        CottonTimer = CottonCooldown;
        GameObject cottonBall = GameObject.Instantiate(Cotton, CottonSpawner.position, Quaternion.identity);
        cottonBall.GetComponent<CottonScript>().CottonFillQuantity = CottonStrength;
        cottonBall.GetComponent<CottonScript>().CottonSizeReduction = 15f;
        cottonBall.GetComponent<CottonScript>().SetCottonBallSize();
        Vector3 teddyDirection = (TeddyTarget.position - this.transform.position + this.transform.up).normalized;
        Vector3 velocity = teddyDirection * CottonThrowSpeed;
        cottonBall.GetComponent<Rigidbody>().AddForce(velocity);
    }
}
