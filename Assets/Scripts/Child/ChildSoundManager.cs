using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildSoundManager : MonoBehaviour
{
    public enum SoundType { groan, scared, terrified, scream }

    [System.Serializable]
    public class Sound
    {
        [Tooltip("The clip that can be played at random or selected")]
        public AudioClip Clip;
        [Tooltip("The probability of the clip playing (Setting it to 0 prevents certain sounds from playing unless selected)")]
        public int Probability;
        [Tooltip("The Volume of the individual sound")]
        [Range(0, 3)]
        public float Volume;
        [Tooltip("The Spatial Blends of the individual sound")]
        [Range(0, 1)]
        public float Blend;
        [Tooltip("The Maximum Pitch of the individual sound (Which will vary randomly)")]
        [Range(0.1f, 3f)]
        public float MaximumPitch;
        [Tooltip("The Minimum Pitch of the individual sound (Which will vary randomly)")]
        [Range(0.1f, 3f)]
        public float MinimumPitch;
        public SoundType type;
    }
    [Tooltip("The source to know where to place the sound")]
    public AudioSource Source;
    [Tooltip("The Sounds that are to be played")]
    public List<Sound> Sounds;
    [Space(5)]
    [Tooltip("SFXVolume of the settings")]
    public float MasterSFXVolume = 1f;

    private List<int> groanProbabilities = new List<int>();
    private List<int> scaredProbabilities = new List<int>();
    private List<int> terrifiedProbabilities = new List<int>();
    private List<int> screamProbabilities = new List<int>();

    private int maxGroanProbability = 0;
    private int maxScaredProbability = 0;
    private int maxTerrifiedProbability = 0;
    private int maxScreamProbability = 0;

    // Start is called before the first frame update
    void Start()
    {
        addProbabilities();
    }

    // Update is called once per frame
    void Update()
    {
        //MasterSFXVolume = SettingsHandler.Instance.SFXVolume;
    }

    private void addProbabilities()
    {
        foreach(Sound sound in Sounds)
        {
            if (sound.type == SoundType.groan)
            {
                groanProbabilities.Add(sound.Probability);
                maxGroanProbability += sound.Probability;
            }
            else if (sound.type == SoundType.scared)
            {
                scaredProbabilities.Add(sound.Probability);
                maxScaredProbability += sound.Probability;
            }
            else if (sound.type == SoundType.terrified)
            {
                terrifiedProbabilities.Add(sound.Probability);
                maxTerrifiedProbability += sound.Probability;
            }
            else if (sound.type == SoundType.scream)
            {
                screamProbabilities.Add(sound.Probability);
                maxScreamProbability += sound.Probability;
            }
        }
    }
    
    public void PlayGroanSound()
    {
        int clipNumber = getClipNumber(SoundType.groan);
        PlaySound(clipNumber);
    }

    public void PlayScaredSound()
    {
        int clipNumber = getClipNumber(SoundType.scared);
        PlaySound(clipNumber);
    }

    public void PlayTerrifiedSound()
    {
        int clipNumber = getClipNumber(SoundType.terrified);
        PlaySound(clipNumber);
    }

    public void PlayScreamSound()
    {
        int clipNumber = getClipNumber(SoundType.scream);
        PlaySound(clipNumber);
    }

    private int getClipNumber(SoundType type)
    {
        if (type == SoundType.groan)
        {
            int chance = Random.Range(0, maxGroanProbability); //Getting the number that'll determine the clip
            int currentRange = 0;
            foreach(Sound sound in Sounds)
            {
                if(sound.type == SoundType.groan)
                {
                    int previousRange = currentRange;
                    currentRange += sound.Probability;
                    if (chance < currentRange && chance >= previousRange)
                        return Sounds.IndexOf(sound);
                }
            }
            //for (int i = 0; i < groanProbabilities.Count; i++)
            //{
            //    int previousRange = currentRange;
            //    currentRange += groanProbabilities[i]; //add to the current range
            //    if (chance < currentRange && chance >= previousRange)
            //        return i;
            //}
        }
        else if (type == SoundType.scared)
        {
            int chance = Random.Range(0, maxScaredProbability); //Getting the number that'll determine the clip
            int currentRange = 0;
            foreach (Sound sound in Sounds)
            {
                if (sound.type == SoundType.scared)
                {
                    int previousRange = currentRange;
                    currentRange += sound.Probability;
                    if (chance < currentRange && chance >= previousRange)
                        return Sounds.IndexOf(sound);
                }
            }
            //for (int i = 0; i < scaredProbabilities.Count; i++)
            //{
            //    int previousRange = currentRange;
            //    currentRange += scaredProbabilities[i]; //add to the current range
            //    if (chance < currentRange && chance >= previousRange)
            //        return i;
            //}
        }
        else if (type == SoundType.terrified)
        {
            int chance = Random.Range(0, maxTerrifiedProbability); //Getting the number that'll determine the clip
            int currentRange = 0;
            foreach (Sound sound in Sounds)
            {
                if (sound.type == SoundType.terrified)
                {
                    int previousRange = currentRange;
                    currentRange += sound.Probability;
                    if (chance < currentRange && chance >= previousRange)
                        return Sounds.IndexOf(sound);
                }
            }
            //for (int i = 0; i < terrifiedProbabilities.Count; i++)
            //{
            //    int previousRange = currentRange;
            //    currentRange += terrifiedProbabilities[i]; //add to the current range
            //    if (chance < currentRange && chance >= previousRange)
            //        return i;
            //}
        }
        else if (type == SoundType.scream)
        {
            int chance = Random.Range(0, maxScreamProbability); //Getting the number that'll determine the clip
            int currentRange = 0;
            foreach (Sound sound in Sounds)
            {
                if (sound.type == SoundType.scream)
                {
                    int previousRange = currentRange;
                    currentRange += sound.Probability;
                    if (chance < currentRange && chance >= previousRange)
                        return Sounds.IndexOf(sound);
                }
            }
            //for (int i = 0; i < screamProbabilities.Count; i++)
            //{
            //    int previousRange = currentRange;
            //    currentRange += screamProbabilities[i]; //add to the current range
            //    if (chance < currentRange && chance >= previousRange)
            //        return i;
            //}
        }
        return 0; //in case nothing's found...which should technically never happen
    }

    private float getPitch(int clipNumber)
    {
        return Random.Range(Sounds[clipNumber].MinimumPitch, Sounds[clipNumber].MaximumPitch);
        //return Random.Range(MinimumPitches[clipNumber],MaximumPitches[clipNumber]);
    }

    private void PlaySound(int clipNumber)
    {
        AudioSource targetSource = Source;
        targetSource.clip = Sounds[clipNumber].Clip;
        targetSource.volume = Sounds[clipNumber].Volume * MasterSFXVolume;
        targetSource.pitch = getPitch(clipNumber);
        targetSource.spatialBlend = Sounds[clipNumber].Blend;
        targetSource.Play();
    }
}
