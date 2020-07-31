using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.PostProcessing;
using UnityEngine;
using UnityStandardAssets.ImageEffects;
using UnityEngine.Audio;

public class SettingsHandler : MonoBehaviour
{
    public static SettingsHandler Instance;

    public GameObject MainCamera;
    //public AudioListener MainListener;
    public TP_Camera CameraController;
    public DepthOfField CameraDOF;
    public PostProcessingProfile Profile;

    [Header("Game")]
    public int SaveFileNumber = 1;
    [Range(0,15)] //numbered undetermined, will determine later
    public int EnemiesDefeated = 0;
    public bool TutorialDone = false;
    public bool DeleteData = false;

    [Header("Audio Settings")]
    public AudioMixer Mixer;
    [Range(0.01f,1f)]
    public float MasterVolume = 1f;
    [Range(0.01f,1f)]
    public float SFXVolume = 1f;
    [Range(0.01f,1f)]
    public float MusicVolume = 1f;

    [Header("Video Settings")]
    public Resolution CurrentResolution;
    public bool Windowed = true;
    [Range(0f,2f)]
    public float VideoBrightness = 1f; //learn
    public bool AntiAliasing = true;
    public float DepthOfField = 0.6f;
    public float MotionBlur = 20f;

    [Header("Control Settings")]
    [Range(0.5f, 8f)]
    public float CameraSensitivity = 2.5f;
    public bool InverseCameraY = false;
    public bool InverseCameraX = false;

    // Start is called before the first frame update
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
            CurrentResolution = Screen.currentResolution;
            Windowed = !Screen.fullScreen;
            SetAllSettings();
        }
        else
            GameObject.Destroy(this.gameObject);
    }

    void Start()
    {
    }

    private void Update()
    {
        setUpCamera();
    }

    public void SetAllSettings()
    {
        setUpCamera();
        if (Profile == null)
            Profile = MainCamera.GetComponent<PostProcessingBehaviour>().profile;
        SetAntiAliasing();
        SetMasterVolume();
        SetEnemiesDefeated();
    }

    private void setUpCamera()
    {
        if (MainCamera == null)
        {
            MainCamera = Camera.main.gameObject;

            if (CameraController == null)
                CameraController = MainCamera.GetComponent<TP_Camera>();
            if (CameraDOF == null)
                CameraDOF = MainCamera.GetComponent<DepthOfField>();

            SetCameraAxis();
            SetCameraSensitivity();
        }
    }

    public void SetMasterVolume()
    {
        //AudioListener.volume = MasterVolume;
        Mixer.SetFloat("MasterVolume", Mathf.Log(MasterVolume) * 20);
    }

    public void SetEffectsVolume()
    {
        //AudioListener.volume = MasterVolume;
        Mixer.SetFloat("EffectsVolume", Mathf.Log(SFXVolume) * 20);
    }

    public void SetMusicVolume()
    {
        //AudioListener.volume = MasterVolume;
        Mixer.SetFloat("MusicVolume", Mathf.Log(MusicVolume) * 20);
    }

    public void SetCameraSensitivity()
    {
        if (CameraController != null)
        {
            CameraController.X_MouseSensitivity = CameraSensitivity;
            CameraController.Y_MouseSensitivity = CameraSensitivity;

            CameraController.X_StickSensitivity = CameraSensitivity;
            CameraController.Y_StickSensitivity = CameraSensitivity;
        }
    }

    public void SetMotionBlur()
    {
        MotionBlurModel.Settings settings = Profile.motionBlur.settings;
        settings.shutterAngle = MotionBlur;
        Profile.motionBlur.settings = settings;
    }

    public void SetDepthOfField()
    {
        CameraDOF.aperture = DepthOfField;
    }

    public void SetCameraAxis()
    {
        if (CameraController != null)
        {
            CameraController.InverseX = InverseCameraX;
            CameraController.InverseY = InverseCameraY;
        }
    }

    public void SetAntiAliasing()
    {
        Profile.antialiasing.enabled = AntiAliasing;
    }

    public void SetWindowedMode()
    {
        Screen.fullScreen = !Windowed;
    }

    public void SetResolution(Resolution res)
    {
        Screen.SetResolution(res.width,res.height,Screen.fullScreen);
    }

    public void SetResolution(int resX, int resY)
    {
        Screen.SetResolution(resX, resY, Screen.fullScreen);
    }

    public void AddToDefeatedEnemies()
    {
        EnemiesDefeated += 1;
        Saves.SaveMonstersDefeated(EnemiesDefeated);
    }

    public void SetEnemiesDefeated()
    {
        EnemiesDefeated = Saves.GetMonstersDefeated();
    }

    private void OnApplicationQuit()
    {
        if (DeleteData)
        {
            Saves.DeleteAllSaveFiles();
        }
    }
}
