using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

public class CutsceneDetector : MonoBehaviour
{
    [System.Serializable]
    public class Cutscene
    {
        [Tooltip("Indicates if the cutscene has been activated before")]
        public bool Activated = false;
        [Tooltip("The cutscene to play")]
        public TimelineAsset Scene;
        [Tooltip("The associated monster that has to be active in order for the scene activate")]
        public GameObject Monster;
    }

    [Tooltip("The Handler for the cutscenes to reference")]
    public CutsceneHandler Handler;
    [Tooltip("The various cutscenes throughout the game to play")]
    public Cutscene CutScene;
    // Start is called before the first frame update

    private void Start()
    {
        if (Handler == null)
            Handler = this.GetComponentInParent<CutsceneHandler>();
    }

    private void OnTriggerEnter(Collider coll)
    {
        if (!CutScene.Activated && CutScene.Monster.activeSelf)
        {
            if (coll.gameObject.CompareTag("Player"))
            {
                CutScene.Activated = true;
                Handler.PlayCutscene(CutScene.Scene);
            }
        }
    }
}
