using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaucetActivator : MonoBehaviour
{
    public GameObject Dripper;
    public Faucet faucet;
    public bool Activated = false;

    // Update is called once per frame
    void Update()
    {
        if (!Activated)
        {
            if (Dripper.activeSelf)
            {
                Activated = true;
                faucet.Open = true;
            }
        }
    }
}
