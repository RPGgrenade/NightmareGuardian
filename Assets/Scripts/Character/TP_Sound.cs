using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TP_Sound : MonoBehaviour
{
    public static TP_Sound Instance;

    [System.Serializable]
    public class Sound
    {
        [Tooltip("The clip that can be played at random or selected")]
        public AudioClip Clip;
        [Tooltip("The clips that can be played if a subgroup is defined")]
        public AudioClip[] SubClips;
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
        [Tooltip("The amount of noise the sound does to affect the child (they can be 0)")]
        [Range(0f, 100f)]
        public float Noise;
        [Tooltip("The source to know where to place sonar object (Please assign them all!)")]
        public AudioSource Source;
        [Tooltip("The source to know where to place sonar object (Optional)")]
        public AudioSource[] SubSources;
    }

    [Header("Audio Clip Properties")]
    public List<Sound> Sounds;
    
    [Tooltip("All the clips that can be played at random")]
    private List<AudioClip> Clips = new List<AudioClip>();
    [Tooltip("The probability of each clip playing (Setting it to 0 prevents certain sounds from playing)")]
    private List<int> Probabilities = new List<int>();
    [Tooltip("The Volume of each individual sound (In case they're different levels)")]
    [Range(0, 1)]
    private List<float> Volumes;
    [Tooltip("The Spatial Blends of each individual sound (In case they're different levels)")]
    [Range(0, 1)]
    private List<float> Blends;
    [Tooltip("The Maximum Pitch of each individual sound (Which will vary randomly)")]
    [Range(0.1f, 3f)]
    private List<float> MaximumPitches;
    [Tooltip("The Minimum Pitch of each individual sound (Which will vary randomly)")]
    [Range(0.1f, 3f)]
    private List<float> MinimumPitches;
    [Tooltip("The amount of noise each sound does to affect the child (they can be 0)")]
    [Range(0f, 100f)]
    private List<float> Noises;
    [Tooltip("The list of sources to know where to place sonar object (Please assign them all!)")]
    private List<AudioSource> Sources;

    [Header("Audio Source Properties")]
    [Tooltip("The source of all sounds on this component")]
    public AudioSource Source;
    [Tooltip("Sonar game object (may not be used, but it's probably necessary)")]
    public GameObject Sonar;
    [Tooltip("The Maximum distance for each sound")]
    public float MaximumDistance = 12f;
    [Space(2)]
    [Tooltip("SFXVolume of the settings")]
    public float MasterSFXVolume = 1f;
    [Tooltip("The default Volume for sounds")]
    public float DefaultVolume = 1f;
    [Tooltip("The default Pitch for sounds")]
    public float DefaultPitch = 1f;
    [Tooltip("The default Blend for sounds")]
    public float DefaultBlend = 1f;

    [Header("Misc")]
    [Tooltip("The clip to be used when hit")]
    public AudioClip HitSound;

    private int maxProbability = 0; //The sum of all the probabilities

    void Start()
    {
        Instance = this;
        if (Source == null)
            Source = this.GetComponent<AudioSource>();
        addProbabilities();
        addClips();
        Source.maxDistance = MaximumDistance;
        //MasterSFXVolume = SettingsHandler.Instance.SFXVolume;
    }

    private void Update()
    {
        //MasterSFXVolume = SettingsHandler.Instance.SFXVolume;
    }

    private void addProbabilities()
    {
        Probabilities.Clear();
        foreach (Sound sound in Sounds)
        {
            Probabilities.Add(sound.Probability);
            maxProbability += sound.Probability;
        }
    }

    private void addClips()
    {
        Clips.Clear();
        foreach (Sound sound in Sounds)
            Clips.Add(sound.Clip);
    }

    private int getClipNumber()
    {
        int chance = Random.Range(0, maxProbability); //Getting the number that'll determine the clip
        int currentRange = 0;
        for (int i = 0; i < Probabilities.Count; i++)
        {
            int previousRange = currentRange;
            currentRange += Probabilities[i]; //add to the current range
            if (chance < currentRange && chance >= previousRange)
                return i;
        }
        return 0; //in case nothing's found...which should technically never happen
    }

    private float getPitch(int clipNumber)
    {
        return Random.Range(Sounds[clipNumber].MinimumPitch, Sounds[clipNumber].MaximumPitch);
        //return Random.Range(MinimumPitches[clipNumber], MaximumPitches[clipNumber]);
    }

    private void PlaySound(int clipNumber)
    {
        Sound sound = Sounds[clipNumber];
        AudioSource targetSource;
        if (sound.SubSources.Length == 0)
            targetSource = sound.Source;
        else
        {
            int subClipIndex = Random.Range(0, sound.SubSources.Length + 1);
            if (subClipIndex == 0)
                targetSource = sound.Source;
            else
                targetSource = sound.SubSources[subClipIndex - 1];
        }

        if (sound.SubClips.Length == 0)
            targetSource.clip = sound.Clip;
        else
        {
            int subClipIndex = Random.Range(0,sound.SubClips.Length + 1);
            if (subClipIndex == 0)
                targetSource.clip = sound.Clip;
            else
                targetSource.clip = sound.SubClips[subClipIndex - 1];
        }
        targetSource.volume = sound.Volume * MasterSFXVolume;
        targetSource.pitch = getPitch(clipNumber);
        targetSource.spatialBlend = sound.Blend;
        targetSource.Play();
    }

    public void PlayRandomSound()
    {
        int clipNumber = getClipNumber();
        PlaySound(clipNumber);
    }

    public void PlaySelectedSound(AudioClip clip) //usually used for walking or movement soft sounds
    {
        int clipNumber = Clips.IndexOf(clip);
        if (clipNumber >= 0) //if it's contained within the preset sounds
            PlaySound(clipNumber);
        else //if the sound hasn't been set yet
        {
            Source.clip = clip;
            Source.volume = DefaultVolume * MasterSFXVolume;
            Source.pitch = DefaultPitch;
            Source.spatialBlend = DefaultBlend;
            Source.Play();
        }
    }

    public void PlaySelectedNoiseSound(AudioClip clip) //usually used for sounds that can cause noise damage
    {
        int clipNumber = Clips.IndexOf(clip);
        float noise = Sounds[clipNumber].Noise;
        TP_Stats.Instance.NoiseIncrease(noise);
        GameObject bubble = Instantiate(Sonar, Sounds[clipNumber].Source.transform.position, this.transform.rotation); //forces it turn appear at the assigned transform (make sure the transform is assigned!)
        SonarManager son = bubble.GetComponent<SonarManager>();
        son.SetProperties(noise);
        PlaySelectedSound(clip);
    }

    public void PlayHitSound()
    {
        PlaySelectedSound(HitSound);
    }
}
