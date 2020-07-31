using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MORPH3D;

public class EmotionalChanger : MonoBehaviour
{
    [System.Serializable]
    public class Emotion
    {
        public string Name;
        [Range(0f, 100f)]
        public float Value;
    }

    public M3DCharacterManager manager;

    [Header("Emotions")]
    [Range(0f,100f)]
    public float Happy = 37f;
    [Range(0f, 100f)]
    public float Shock = 0f;
    [Range(0f, 100f)]
    public float Snarl = 0f;
    [Range(0f, 100f)]
    public float Pain = 0f;
    [Range(0f, 100f)]
    public float Afraid = 0f;
    [Range(0f, 100f)]
    public float Disgust = 0f;


    public Emotion[] Emotions; //unfortunately doesn't work because it can't be used in animation

    void Start()
    {
        if(manager == null)
            manager = this.GetComponent<M3DCharacterManager>();
    }
    
    void Update()
    {
        updateEmotions();
        //updateAllEmotions();
    }

    private void updateEmotions()
    {
        manager.SetBlendshapeValue("eCTRLHappy", Happy);
        manager.SetBlendshapeValue("eCTRLShock", Shock);
        manager.SetBlendshapeValue("eCTRLSnarl", Snarl);
        manager.SetBlendshapeValue("eCTRLPain", Pain);
        manager.SetBlendshapeValue("eCTRLAfraid", Afraid);
        manager.SetBlendshapeValue("eCTRLDisgust", Disgust);
    }

    private void updateAllEmotions()
    {
        foreach(Emotion emo in Emotions)
            manager.SetBlendshapeValue(emo.Name,emo.Value);
    }
}
