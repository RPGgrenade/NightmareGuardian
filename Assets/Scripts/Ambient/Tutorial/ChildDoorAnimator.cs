using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildDoorAnimator : MonoBehaviour
{
    public bool Open = false;
    public bool Cape = false;
    public bool Needle = false;
    public bool Light = false;
    public bool Bag = false;
    public bool Spindle = false;

    private Animator anim;

    private void Start()
    {
        anim = this.GetComponent<Animator>();
    }

    void Update()
    {
        Cape = TP_Animator.Instance.HasCape;
        Needle = TP_Animator.Instance.HasNeedle;
        Light = TP_Animator.Instance.HasNightLight;
        Bag = TP_Animator.Instance.HasBag;
        Spindle = TP_Animator.Instance.HasSpindle;

        Open = Cape && Needle && Light && Bag && Spindle;
        if (!SettingsHandler.Instance.TutorialDone && Open)
            SettingsHandler.Instance.TutorialDone = Open;
        anim.SetBool("Open", Open);
    }
}
