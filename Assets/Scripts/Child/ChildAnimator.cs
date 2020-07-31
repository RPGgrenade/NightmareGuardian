using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildAnimator : MonoBehaviour
{
    private float noise;
    private bool pain;
    private bool relaxed;

    public float RelaxationTime = 3f;
    public float PainCooldown = 0.5f;

    public float relaxationTimer = 0f;
    private float painCooldown = 0f;

    private Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        anim = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        noise = TP_Stats.Instance.Noise;
        relaxed = TP_Stats.Instance.NoiseDefenseTimer > 0;
        pain = painCooldown > 0;
        if (pain)
            painCooldown -= Time.deltaTime;
        if (relaxationTimer >= RelaxationTime)
            TP_Stats.Instance.SetNoiseDefense();
        updateStates();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player") && TP_Animator.Instance.TeddyMode)
            relaxationTimer += Time.deltaTime;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            relaxationTimer = 0;
    }

    public void SetPainCooldown()
    {
        painCooldown = PainCooldown;
    }

    void updateStates()
    {
        anim.SetBool("Pain", pain);
        anim.SetBool("Relaxed",relaxed);
        anim.SetFloat("Noise", noise);
    }
}
