using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CostumeSetter : MonoBehaviour
{
    [Tooltip("Says whether or not the pedestal is usable")]
    public bool Active = false;
    [Tooltip("The skin ID this pedestal activates")]
    public int SkinNumber = 0;
    [Tooltip("How long before it can activate again")]
    public float CoolDown = 3f;
    [Tooltip("How many plushies need to be found before this is usable")]
    public int PlushCount = 0;

    private Animator anim;
    private float timer = 0f;
    // Start is called before the first frame update
    void Start()
    {
        anim = this.GetComponent<Animator>();
        InvokeRepeating("checkIfActive",0.1f, 3f);
    }
    
    void Update()
    {
        if (timer > 0f)
            timer -= Time.deltaTime;
    }

    private void checkIfActive()
    {
        Active = Saves.GetAllPlushies().Split(',').Length >= PlushCount;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root.tag == "Player" && timer <= 0f && Active)
        {
            timer = CoolDown;
            anim.SetTrigger("Costume");
        }
    }

    public void ChangeSkin()
    {
        SkinSwapper.Instance.SetSkin(SkinNumber);
    }
}
