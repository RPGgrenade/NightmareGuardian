using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CapeChanges : MonoBehaviour
{
    public Cloth cloth;

    [Header("Cloth Properties")]
    public bool Tethers = true;
    public bool Gravity = true;
    public bool ClothFaded = false;
    [Header("Animation Properties")]
    public string CoverAnimName = "Teddy_Cover";
    public string TurtleAnimName = "Teddy_Turtle";
    public string ChuteAnimName = "Parachute";
    public int CoverAnimNumber = 2;
    public int TurtleAnimNumber = 1;
    public int ChuteAnimNumber = 4;
    [Header("Fade Properties")]
    public float FadeSpeed = 0.2f;

    private Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        if (cloth == null)
            cloth = this.GetComponentInChildren<Cloth>();
        anim = this.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!inCapeRelatedAnimation())
        {
            cloth.SetEnabledFading(true, FadeSpeed * Time.deltaTime);
            cloth.useTethers = true;
            Tethers = true;
            ClothFaded = false;
            Gravity = true;
            cloth.useGravity = true;
        }
    }

    private bool inCapeRelatedAnimation()
    {
        return anim.GetCurrentAnimatorStateInfo(CoverAnimNumber).IsName(CoverAnimName) ||
            anim.GetCurrentAnimatorStateInfo(TurtleAnimNumber).IsName(TurtleAnimName) ||
            anim.GetCurrentAnimatorStateInfo(ChuteAnimNumber).IsName(ChuteAnimName);
    }

    public void ToggleFadeCloth()
    {
        cloth.SetEnabledFading(ClothFaded, FadeSpeed * Time.deltaTime);
        ClothFaded = !ClothFaded;
    }

    public void SetFadeCloth()
    {
        ClothFaded = true;
        cloth.SetEnabledFading(ClothFaded, FadeSpeed * Time.deltaTime);
    }

    public void FastToggleFadeCloth()
    {
        cloth.SetEnabledFading(ClothFaded);
        ClothFaded = !ClothFaded;
    }

    public void FastSetCloth(int faded)
    {
        ClothFaded = faded == 1;
        cloth.SetEnabledFading(ClothFaded);
    }

    public void ToggleGravity()
    {
        cloth.useGravity = !cloth.useGravity;
        Gravity = cloth.useGravity;
    }

    public void SetGravity()
    {
        Gravity = true;
        cloth.useGravity = Gravity;
    }

    public void ToggleTether()
    {
        cloth.useTethers = !cloth.useTethers;
        Tethers = cloth.useTethers;
    }
}
