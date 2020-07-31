using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockingAnimator : MonoBehaviour
{
    public Animator Anim;

    void OnTriggerStay(Collider other)
    {
        if (other.transform.tag == "Player" && other.gameObject.layer == LayerMask.NameToLayer("Player"))
            Anim.SetBool("Open", true);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.transform.tag == "Player" && other.gameObject.layer == LayerMask.NameToLayer("Player"))
            Anim.SetBool("Open", false);
    }
}
