using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandGripSpeedStopper : MonoBehaviour
{
    public Rigidbody Body;
    public string StoppingLayer = "Door";
    public string[] StoppingLayers;

    // Start is called before the first frame update
    void Start()
    {
        Body = this.GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        //if (checkLayersCollision(collision.gameObject.layer))
        if(collision.gameObject.layer == LayerMask.NameToLayer(StoppingLayer))
            Body.velocity *= -1f;
    }

    private void OnCollisionStay(Collision collision)
    {
        //if (checkLayersCollision(collision.gameObject.layer))
        if(collision.gameObject.layer == LayerMask.NameToLayer(StoppingLayer))
            Body.velocity *= 0f;
    }

    private bool checkLayersCollision(LayerMask collisionLayer)
    {
        foreach (string layer in StoppingLayers)
        {
            if (collisionLayer == LayerMask.NameToLayer(layer))
                return true;
        }
        return false;
    }
}
