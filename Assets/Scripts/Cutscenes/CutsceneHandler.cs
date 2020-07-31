using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class CutsceneHandler : MonoBehaviour
{

    [Tooltip("The Director that controls the cutscenes and plays them")]
    public PlayableDirector Director;
    // Start is called before the first frame update
    void Start()
    {
        if(Director == null)
            Director = this.GetComponent<PlayableDirector>();
    }

    private void Update()
    {
        if (Director.state != PlayState.Playing)
        {
            Director.Stop();
            Director.playableAsset = null;
        }
    }

    public void PlayCutscene(TimelineAsset scene)
    {
        Director.Play(scene, DirectorWrapMode.None);
    }
}
