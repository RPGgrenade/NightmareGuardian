using UnityEngine;
using System.Collections;

public class TimeControl : MonoBehaviour {

    public float TimeScale = 1f;
    public float Duration = 0f;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
        if(Duration > 0)
            Duration -= Time.unscaledDeltaTime;
        SetTimeScale();
	}

    private void SetTimeScale()
    {
        if (Duration > 0)
            Time.timeScale = TimeScale;
        else
            Time.timeScale = 1f;
    }

    public void SetHitlag(float scale, float hitlag)
    {
        TimeScale = scale;
        Duration = hitlag;
    }
}
