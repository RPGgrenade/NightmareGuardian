using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OOBHandler : MonoBehaviour
{
    public Transform Player;
    public float InitialWaitTime = 2f;
    public float RepeatingWaitTime = 1f;
    public Vector3 LastPlayerPosition;
    public bool OutOfBounds = false;

    // Start is called before the first frame update
    void Start()
    {
        if (Player == null)
            Player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

        InvokeRepeating("updatePlayerPosition",InitialWaitTime, RepeatingWaitTime);
    }

    void updatePlayerPosition()
    {
        if (!OutOfBounds)
            LastPlayerPosition = Player.position;
        else
            OutOfBounds = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.transform.tag == "Player")
        {
            if (!OutOfBounds)
                OutOfBounds = true;
            Player.position = LastPlayerPosition;
        }
    }
}
