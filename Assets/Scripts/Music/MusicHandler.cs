using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Audio;

public class MusicHandler : MonoBehaviour 
{
    [Header("Tracking objects")]
    public AudioMixerGroup Mixer;
    [Tooltip("The player being tracked")]
    public Transform Target;

    [Header("Proximity Markers")]
    [Tooltip("Whether or not a monster exists right now, if it does, ambient music won't play")]
    public bool MonsterExists = false;
    [Tooltip("Whether or not the target is within the general music playing area")]
    public bool IsInArea = false;
    [Tooltip("Whether or not the target is within the monster hearing range")]
    public bool NearMonster = false;
    [Tooltip("Whether or not the target is within the monster's line of sight")]
    public bool SeenByMonster = false;
    [Tooltip("Whether or not the target is on top of the monster")]
    public bool OnMonster = false;

    [Header("Audio Manager")]
    [Tooltip("The Master Volume of music in the settings")]
    public float MasterMusicVolume = 1f;

    [Header("Ambient Timing Properties")]
    [Tooltip("The Maximum amount of minutes one will wait between ambient songs")]
    public int MaxMinutes = 8;
    [Tooltip("The Minimum amount of minutes one will wait between ambient songs")]
    public int MinMinutes = 2;
    [Space(10f)]
    [Tooltip("The Maximum amount of seconds one will wait between ambient songs")]
    public int MaxSeconds = 59;
    [Tooltip("The Minimum amount of seconds one will wait between ambient songs")]
    public int MinSeconds = 0;

    [Header("Trackable Properties")]
    [Tooltip("How many minutes left for next ambient song to play")]
    public int Minutes = 0;
    [Tooltip("How many minutes left for next ambient song to play")]
    public int Seconds = 0;
    [Tooltip("Name of the ambient song currently playing")]
    public string SongName;
    [Tooltip("Rate of increase of current song")]
    public float RateOfIncrease = 0.01f;
    [Tooltip("Rate of decrease of current song")]
    public float RateOfDecrease = 0.01f;

    [Header("Ambient Tracks")]
    [Tooltip("All the different tracks to be played")]
    public List<AudioClip> AmbientTrack;
    [Tooltip("The volumes for each individual track (make it the same number as the tracks)")]
    public List<float> AmbientVolumes;

    [Header("Fight Track Properties")]
    public AudioClip NearSong;
    public float NearVolume = 1f;
    [Space(10f)]
    public AudioClip SeenSong;
    public float SeenVolume = 1f;
    [Space(10f)]
    public AudioClip OnSong;
    public float OnVolume = 1f;
    [Space(10f)]
    public float FightBlend = 0.4f;

    private List<AudioSource> musicTrack; //Accumulates all the songs added onto an area
    private List<float> trackVolumes = new List<float>(); //all the default volumes of the tracks
    private AudioSource playing = null; //The audiosource currently chosen to be played
    private AudioSource fightSong = null; //The track specifically used for the fight song
    private float fightVolume = 0f;
    private float time = 0f; //the timer in indescrete seconds

    private AudioSource ambientSource;
    private AudioSource nearSource;
    private AudioSource seenSource;
    private AudioSource onSource;

	void Start () 
    {
        //MasterMusicVolume = SettingsHandler.Instance.MusicVolume;
        createMusicSource();
        createMusicSource(NearSong, NearVolume);
        createMusicSource(SeenSong, SeenVolume);
        createMusicSource(OnSong, OnVolume);

        CalculateTime();

        InvokeRepeating("checkForMonster", 1f, 1f);
	}

	void Update () 
    {
        //MasterMusicVolume = SettingsHandler.Instance.MusicVolume;
        
        //if(playing != null)
        //    playing.volume = trackVolumes[musicTrack.IndexOf(playing)] * MasterMusicVolume;

        if (IsInArea)
        {
            
            TeleportSource(); //TODO: Check out all lower logic to make sure they fade out in proper time
            if (MonsterExists)
            {
                if (ambientSource.isPlaying)
                {
                    int index = AmbientTrack.IndexOf(ambientSource.clip);
                    //Debug.Log(index);
                    if (index > -1)
                        decreaseMusicSource(ambientSource, AmbientVolumes[index], true);
                    else
                        decreaseMusicSource(ambientSource, MasterMusicVolume, true);
                }

                if (NearMonster)
                {
                    //    int index = AmbientTrack.IndexOf(ambientSource.clip);
                    //    decreaseMusicSource(ambientSource, AmbientVolumes[index]);
                    //}

                    bool noFightSongPlaying = !nearSource.isPlaying || !seenSource.isPlaying || !onSource.isPlaying;
                    if (noFightSongPlaying)
                    {
                        nearSource.volume = 0f;
                        nearSource.Play();

                        seenSource.volume = 0f;
                        seenSource.Play();

                        onSource.volume = 0f;
                        onSource.Play();
                    }

                    fightMusicVolumeHandler();
                }
            }
            else
            {
                //play ambient and choose the song etc
                //if (!MonsterExists)
                //{
                    if (!ambientSource.isPlaying)
                    {
                        if (time <= 0)
                        {
                            CalculateTime();
                            ChooseSong();
                            ambientSource.Play();
                        }
                        else
                            time -= Time.deltaTime;
                    }
                //}
                //else
                //{
                //    int index = AmbientTrack.IndexOf(ambientSource.clip);
                //    //Debug.Log(index);
                //    if (index > -1)
                //        decreaseMusicSource(ambientSource, AmbientVolumes[index], true);
                //    else
                //        decreaseMusicSource(ambientSource, MasterMusicVolume, true);
                //}

                decreaseMusicSource(nearSource, NearVolume);
                decreaseMusicSource(seenSource, SeenVolume);
                decreaseMusicSource(onSource, OnVolume);
            }
        }
        else 
        {   //slowly decrease volume of all songs until set to 0, then reset volumes and stop them
            int index = AmbientTrack.IndexOf(ambientSource.clip);
            //Debug.Log(index);
            if(index > -1)
                decreaseMusicSource(ambientSource,AmbientVolumes[index], true);
            else
                decreaseMusicSource(ambientSource, MasterMusicVolume, true);
            decreaseMusicSource(nearSource, NearVolume, true);
            decreaseMusicSource(seenSource, SeenVolume, true);
            decreaseMusicSource(onSource, OnVolume, true);
        }
        TempDecreaseForJingle();
        Timer();
        Song(); 
	}

    private void checkForMonster()
    {
        GameObject monster = GameObject.FindWithTag("Monster");
        MonsterExists = monster != null;
    }
    
    private void fightMusicVolumeHandler()
    {
        if (!JinglesHandler.Instance.JingleSource.isPlaying)
        {
            if (!SeenByMonster && !OnMonster)
            {
                //if (noFightSongPlaying)
                //    nearSource.Play();
                increaseSourceVolume(nearSource, NearVolume);
                decreaseMusicSource(seenSource, SeenVolume);
                decreaseMusicSource(onSource, OnVolume);
            }
            else if (SeenByMonster && !OnMonster)
            {
                //if (noFightSongPlaying)
                //    seenSource.Play();
                increaseSourceVolume(nearSource, NearVolume);
                increaseSourceVolume(seenSource, SeenVolume);
                decreaseMusicSource(onSource, OnVolume);
            }
            else if (SeenByMonster && OnMonster)
            {
                //if (noFightSongPlaying)
                //    onSource.Play();
                increaseSourceVolume(onSource, OnVolume);
                increaseSourceVolume(nearSource, NearVolume);
                increaseSourceVolume(seenSource, SeenVolume);
            }
        }
    }

    private void createMusicSource(AudioClip clip = null, float volume = 0f)
    {
        GameObject newSource = new GameObject();
        newSource.layer = LayerMask.NameToLayer("Music");
        AudioSource source = newSource.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = Mixer;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        newSource.transform.parent = this.transform;
        if (clip == null)
        {
            newSource.name = "Ambient Songs";
            ambientSource = source;
            source.loop = false;
        }
        else
        {
            newSource.name = clip.name;
            source.clip = clip;
            source.volume = 0f;
            source.loop = true;

            if (clip == NearSong && nearSource == null)
                nearSource = source;
            else if (clip == SeenSong && seenSource == null)
                seenSource = source;
            else if (clip == OnSong && onSource == null)
                onSource = source;
        }
    }

    private void decreaseMusicSource(AudioSource source, float volume, bool reset = false)
    {
        if (source.isPlaying)
        {
            decreaseSourceVolume(source, volume);
            if (source.volume <= 0 && reset)
                resetMusicSource(source, volume);
        }
    }

    private void resetMusicSource(AudioSource source, float volume)
    {
        source.Pause();
        source.volume = volume;
        source.Stop();
    }

    void decreaseSourceVolume(AudioSource source, float sourceVolume) 
    {
        if (source.volume > 0)
        {
            float decreaseSpeed = RateOfDecrease * Time.deltaTime * sourceVolume;
            source.volume -= decreaseSpeed;
        }
    }

    void increaseSourceVolume(AudioSource source, float sourceVolume)
    {
        if (source.volume < sourceVolume * MasterMusicVolume)
        {
            float increaseSpeed = RateOfIncrease * Time.deltaTime * sourceVolume;
            source.volume += increaseSpeed;
        }
    }

    public void CalculateTime() 
    {
        time = (Random.Range(MinMinutes, MaxMinutes) * 60) + (Random.Range(MinSeconds,MaxSeconds));
    }

    void Timer() 
    {
        Minutes = (int)(time / 60);
        Seconds = (int)(time % 60);
    }

    void Song() 
    {
        if (ambientSource.isPlaying)
            SongName = ambientSource.clip.name;
        else if (nearSource.isPlaying)
            SongName = nearSource.clip.name;
        else if (seenSource.isPlaying)
            SongName = seenSource.clip.name;
        else if (onSource.isPlaying)
            SongName = onSource.clip.name;
        else
            SongName = "No Song Playing";
    }

    void TempDecreaseForJingle()
    {
        if (IsInArea)
        {
            if (JinglesHandler.Instance.JingleSource.isPlaying)
            {
                if (ambientSource.isPlaying)
                {
                    int trackIndex = AmbientTrack.IndexOf(ambientSource.clip);
                    decreaseSourceVolume(ambientSource, AmbientVolumes[trackIndex] * 2);
                }
                else
                {
                    decreaseSourceVolume(nearSource, NearVolume * 2);
                    decreaseMusicSource(seenSource, SeenVolume * 2);
                    decreaseMusicSource(onSource, OnVolume * 2);
                }
            }
            else
            {
                if (ambientSource.isPlaying)
                {
                    int trackIndex = AmbientTrack.IndexOf(ambientSource.clip);
                    increaseSourceVolume(ambientSource, AmbientVolumes[trackIndex]);
                }
                else
                    fightMusicVolumeHandler();
            }
        }
    }

    void ChooseSong() 
    {
        int index = Random.Range(0, AmbientTrack.Count);
        ambientSource.clip = AmbientTrack[index];
        ambientSource.volume = AmbientVolumes[index];
    }

    void TeleportSource() 
    {
        transform.position = Target.position;
    }

    //void DecreaseFight() 
    //{
    //    fightSong.volume -= Time.deltaTime * RateOfDecrease;
    //}

    //void IncreaseFight() 
    //{
    //    if (fightSong.volume < fightVolume * MasterMusicVolume)
    //        fightSong.volume += Time.deltaTime * RateOfDecrease;
    //}
}
