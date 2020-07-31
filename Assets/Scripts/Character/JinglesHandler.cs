using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JinglesHandler : MonoBehaviour
{
    [Serializable]
    public class Jingle
    {
        public int ID = 0;
        public string Name;
        public AudioClip Clip;
        [Range(0f,3f)]
        public float Volume = 1f;
    }

    public static JinglesHandler Instance;

    public AudioSource JingleSource;
    public List<Jingle> Jingles = new List<Jingle>();

    void Start()
    {
        Instance = this;
    }

    public void PlayeJingle(string jingleName)
    {
        foreach (Jingle jingle in Jingles)
        {
            if (jingle.Name == jingleName)
            {
                JingleSource.volume = jingle.Volume;
                JingleSource.PlayOneShot(jingle.Clip);
            }
        }
    }
}
