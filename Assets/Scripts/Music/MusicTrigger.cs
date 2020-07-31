using UnityEngine;
using System.Collections;

public class MusicTrigger : MonoBehaviour 
{
    public GameObject AssociatedMusictrack;

    private MusicHandler Music;
    void Start () 
    {
        Music = AssociatedMusictrack.GetComponent<MusicHandler>();
	}

	void LateUpdate () 
    {
        Music.IsInArea = false;
	}

    void OnTriggerEnter(Collider col) 
    {
        if (col.GetComponent<Collider>().tag == "Player")
            Music.CalculateTime();
    }
    void OnTriggerStay(Collider col) 
    {
        if(col.GetComponent<Collider>().tag == "Player")
            Music.IsInArea = true;
    }

    //void OnTriggerExit(Collider col)
    //{
    //    if (col.GetComponent<Collider>().tag == "Player")
    //        Music.IsInArea = false;
    //}
}
