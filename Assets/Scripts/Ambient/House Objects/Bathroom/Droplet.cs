using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Droplet : MonoBehaviour
{
    public float Noise = 10f;
    public GameObject Sonar;
    public ParticleSystem Particles;
    public GameObject Monster;

    private void OnCollisionEnter(Collision collision)
    {
        TP_Stats.Instance.NoiseIncrease(Noise);
        GameObject bubble = Instantiate(Sonar, this.transform.position, this.transform.rotation); //forces it turn appear at the assigned transform (make sure the transform is assigned!)
        bubble.tag = "Monster";
        SonarManager son = bubble.GetComponent<SonarManager>();
        son.SetProperties(Noise);
        if (Monster == null)
        {
            if (GameObject.FindGameObjectWithTag("Monster") != null)
                GameObject.FindGameObjectWithTag("Monster").transform.root.GetComponent<CreatureMind>().AddJoy();
        }
        else
            Monster.GetComponent<CreatureMind>().AddJoy();
    GameObject.Instantiate(Particles, this.transform.position, Quaternion.identity);
        GameObject.Destroy(this.gameObject);
    }
}
