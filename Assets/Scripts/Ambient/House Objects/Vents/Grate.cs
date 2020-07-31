using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grate : MonoBehaviour
{
    [Header("Grate Properties")]
    [Tooltip("The number in order to track the grate's open state")]
    public int GrateNumber = 1;
    public bool IsOpened = false;
    public bool Saved = false;
    [Header("Screw Trackers")]
    public Screws Screw1;
    public Screws Screw2;
    public Screws Screw3;
    public Screws Screw4;

    private Animator anim;
    private Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        anim = this.GetComponent<Animator>();
        rb = this.GetComponent<Rigidbody>();

        if (Saves.GetGrateOpening(GrateNumber) == 1)
            Invoke("opened",0.4f);
    }

    //private void FixedUpdate()
    //{
    //    if (rb.velocity.magnitude <= rb.sleepThreshold)
    //        rb.Sleep();
    //}

    // Update is called once per frame
    void Update()
    {
        IsOpened = (Screw1.Unscrewed && Screw2.Unscrewed && Screw3.Unscrewed && Screw4.Unscrewed);
        if (IsOpened && !IsInvoking("open") && !Saved)
            Invoke("open", 1f);
    }

    private void open()
    {
        Saves.SaveGrateOpening(GrateNumber);
        Saved = true;
        anim.SetTrigger("Open");
        Screw1.OffParticles();
        Screw2.OffParticles();
        Screw3.OffParticles();
        Screw4.OffParticles();
    }

    private void opened()
    {
        anim.SetTrigger("Opened");
        Saved = true;
        Screw1.OffParticles();
        Screw2.OffParticles();
        Screw3.OffParticles();
        Screw4.OffParticles();
    }
}
