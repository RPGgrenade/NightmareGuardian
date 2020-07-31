using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleDestroyer : MonoBehaviour
{
    private float timeLeft;
    public void Awake()
    {
        ParticleSystem system = GetComponent<ParticleSystem>();
        timeLeft = system.main.duration;
    }
    public void Update()
    {
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0)
        {
            GameObject.Destroy(gameObject);
        }
    }
}
