using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetManager : MonoBehaviour {

    public Transform TrackedObject;
    public Vector3 offset = new Vector3(0f,-0.7f,0f);

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        this.transform.position = TrackedObject.transform.position + offset;
	}

    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
    }
}
