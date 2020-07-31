using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    public static Pause Instance;  //Pause Menu needs a lot of work.

    [Header("Menu stuff")]
    public EventSystem Events;
    public Renderer Paper;
    public Material[] Drawings;
    public GameObject[] Menus;

    [Header("Settings tracking")]
    [Header("Volume")]
    public Text MasterVolume;
    public Text SFXVolume;
    public Text MusicVolume;

    [Header("Video")]
    public Text AntiAliasing;
    public Text Windowed;
    public Text Resolution;
    public Text DepthOfField;
    public Text MotionBlur;

    [Header("Controls")]
    public Text CameraSensitivity;
    public Text InvertX;
    public Text InvertY;

    [Header("Aesthetics")]
    public GameObject Button;
    public Transform Hats;
    public Transform Masks;
    public Transform Necks;

    private GameObject firstSelected = null;
    private GameObject lastSelected = null;
    private Resolution[] resolutions;
    private Resolution currentRes;
    private int resIndex = 0;

    private float RepeatDelay = 0f;
    private float delay = 0f;

    private bool fading = false;
    private bool loading = false;

    void Awake() 
    {
        Instance = this;
        if(Events.currentInputModule == null)
            Events = this.gameObject.GetComponent<EventSystem>();

        RepeatDelay = this.transform.GetComponent<StandaloneInputModule>().repeatDelay;
        resolutions = Screen.resolutions;
        currentRes = Screen.currentResolution;
        for(int i = 0; i < resolutions.Length; i++) {
            if (resolutions[i].width == currentRes.width && resolutions[i].height == currentRes.height){
                resIndex = i;
                break;
            }
        }
        if(Drawings.Length > 0)
            SetMonsterDrawing();
        SetMenuSettingsNumbers();
        firstSelected = Events.currentSelectedGameObject;
        setAestheticsButtons();
        InvokeRepeating("setSelectedItem", 0f, 0.4f);
        //ChangeResolution();
    }

    private void OnEnable()
    {
        ToggleToFirstMenu();
        setAestheticsButtons();
    }

    //private void OnApplicationFocus(bool focus)
    //{
    //    if(focus)
    //    {
    //        Events.SetSelectedGameObject(lastSelected);
    //        //Debug.Log("Selected: " + Events.currentSelectedGameObject.name);
    //    }
    //}

    void Update()
    {
        SetMonsterDrawing();

        if (Events.currentInputModule.input.GetButtonDown("Pause"))
            Unpause();

        delay += Time.deltaTime;
        if (delay >= RepeatDelay)
        {
            float input = Events.currentInputModule.input.GetAxisRaw("Horizontal");
            if (Mathf.Abs(input) >= 1f)
            {
                delay = 0f;
                checkInputAndUpdateSettings(input);
            }
        }

        if(fading && FadeManager.Instance.Image.color.a >= 1f)
        {
            if (!loading)
            {
                loading = true;
                SceneManager.LoadSceneAsync("Main Menu");
            }
        }
    }

    private void setSelectedItem()
    {
        if (Events.currentSelectedGameObject == null)// && lastSelected.GetComponentInParent<CanvasGroup>().alpha == 1)
            if (lastSelected != null)
                Events.SetSelectedGameObject(lastSelected);
            else
                Events.SetSelectedGameObject(Events.firstSelectedGameObject);
        else
            lastSelected = Events.currentSelectedGameObject;
    }

    private float calculateSettingsNumber(float input, float variable, float increment, float min, float max)
    {
        Debug.Log("New variable size");
        float newIncrement = Mathf.Log10(increment);
        if (input > 0)
            return Mathf.Clamp(variable + increment, min, max);
        else if (input < 0)
            return Mathf.Clamp(variable - increment, min, max);
        else
            return variable;
    }

    private void checkInputAndUpdateSettings(float input = 0f)
    {
        if (input != 0f)
        {
            if (Events.currentSelectedGameObject == MasterVolume.transform.parent.gameObject)
            {
                SettingsHandler.Instance.MasterVolume = calculateSettingsNumber(input, SettingsHandler.Instance.MasterVolume, 0.05f, 0.01f, 1f);
                //SettingsHandler.Instance.MasterVolume = calculateSettingsNumber(input, SettingsHandler.Instance.MasterVolume, 3f, -60f, 0f);
                SettingsHandler.Instance.SetMasterVolume();
                SetMenuSettingsNumbers("Volume");
            }
            else if (Events.currentSelectedGameObject == SFXVolume.transform.parent.gameObject)
            {
                SettingsHandler.Instance.SFXVolume = calculateSettingsNumber(input, SettingsHandler.Instance.SFXVolume, 0.05f, 0.01f, 1f);
                //SettingsHandler.Instance.SFXVolume = calculateSettingsNumber(input, SettingsHandler.Instance.SFXVolume, 3f, -60f, 0f);
                SettingsHandler.Instance.SetEffectsVolume();
                SetMenuSettingsNumbers("Volume");
            }
            else if (Events.currentSelectedGameObject == MusicVolume.transform.parent.gameObject)
            {
                SettingsHandler.Instance.MusicVolume = calculateSettingsNumber(input, SettingsHandler.Instance.MusicVolume, 0.05f, 0.01f, 1f);
                //SettingsHandler.Instance.MusicVolume = calculateSettingsNumber(input, SettingsHandler.Instance.MusicVolume, 3f, -60f, 0f);
                SettingsHandler.Instance.SetMusicVolume();
                SetMenuSettingsNumbers("Volume");
            }
            else if (Events.currentSelectedGameObject == CameraSensitivity.transform.parent.gameObject)
            {
                SettingsHandler.Instance.CameraSensitivity = calculateSettingsNumber(input, SettingsHandler.Instance.CameraSensitivity, 0.375f, 0.5f, 8f);
                SettingsHandler.Instance.SetCameraSensitivity();
                SetMenuSettingsNumbers("Camera");
            }
            else if (Events.currentSelectedGameObject == DepthOfField.transform.parent.gameObject)
            {
                SettingsHandler.Instance.DepthOfField = calculateSettingsNumber(input, SettingsHandler.Instance.DepthOfField, 0.05f, 0f, 1f);
                SettingsHandler.Instance.SetDepthOfField();
                SetMenuSettingsNumbers("Video");
            }
            else if (Events.currentSelectedGameObject == MotionBlur.transform.parent.gameObject)
            {
                SettingsHandler.Instance.MotionBlur = calculateSettingsNumber(input, SettingsHandler.Instance.MotionBlur, 5f, 0f, 100f);
                SettingsHandler.Instance.SetMotionBlur();
                SetMenuSettingsNumbers("Video");
            }
            else if (Events.currentSelectedGameObject == Resolution.transform.parent.gameObject)
            {
                if (input > 0)
                    resIndex++;
                else if (input < 0)
                    resIndex--;
                resIndex = Mathf.Clamp(resIndex, 0, resolutions.Length - 1);
                currentRes = resolutions[resIndex];
                SetMenuSettingsNumbers("Video");
            }
        }
        //SettingsHandler.Instance.SetCameraSensitivity();
        //SettingsHandler.Instance.SetMasterVolume();
        //SettingsHandler.Instance.SetMotionBlur();
        //SettingsHandler.Instance.SetDepthOfField();
        //SetMenuSettingsNumbers();
    }

    private void setAestheticsButtons()
    {
        if (Hats != null)
        {
            foreach (Transform child in Hats)
                GameObject.Destroy(child.gameObject);
        }
        if (Masks != null)
        {
            foreach (Transform child in Masks)
                GameObject.Destroy(child.gameObject);
        }
        if (Necks != null)
        {
            foreach (Transform child in Necks)
                GameObject.Destroy(child.gameObject);
        }
        string[] hatsFound = Saves.GetAllHats().Split(',');
        string[] masksFound = Saves.GetAllMasks().Split(',');
        string[] NecksFound = Saves.GetAllNecks().Split(',');

        if (Hats != null)
        {
            foreach (string hat in hatsFound)
            {
                if (hat != "")
                {
                    GameObject hatButton = GameObject.Instantiate(Button);
                    hatButton.name = hat;
                    hatButton.GetComponent<AestheticButton>().Name.text = hat;
                    hatButton.GetComponent<AestheticButton>().Type = AestheticButton.AccessoryType.Hat;
                    hatButton.GetComponent<RectTransform>().SetParent(Hats);
                    hatButton.GetComponent<RectTransform>().localPosition =
                        new Vector3(hatButton.GetComponent<RectTransform>().localPosition.x,
                        hatButton.GetComponent<RectTransform>().localPosition.y,
                        0);
                    hatButton.GetComponent<RectTransform>().localRotation = Quaternion.identity;
                    hatButton.GetComponent<RectTransform>().localScale = Vector3.one;
                    if (Saves.GetHatSelected(hat))
                        hatButton.GetComponent<AestheticButton>().SetActive(true);
                }
            }
        }

        if (Masks != null)
        {
            foreach (string mask in masksFound)
            {
                if (mask != "")
                {
                    GameObject maskButton = GameObject.Instantiate(Button);
                    maskButton.name = mask;
                    maskButton.GetComponent<AestheticButton>().Name.text = mask;
                    maskButton.GetComponent<AestheticButton>().Type = AestheticButton.AccessoryType.Mask;
                    maskButton.GetComponent<RectTransform>().SetParent(Masks);
                    maskButton.GetComponent<RectTransform>().localPosition =
                        new Vector3(maskButton.GetComponent<RectTransform>().localPosition.x,
                        maskButton.GetComponent<RectTransform>().localPosition.y,
                        0);
                    maskButton.GetComponent<RectTransform>().localRotation = Quaternion.identity;
                    maskButton.GetComponent<RectTransform>().localScale = Vector3.one;
                    if (Saves.GetHatSelected(mask))
                        maskButton.GetComponent<AestheticButton>().SetActive(true);
                }
            }
        }

        if (Necks != null)
        {
            foreach (string Neck in NecksFound)
            {
                if (Neck != "")
                {
                    GameObject NeckButton = GameObject.Instantiate(Button);
                    NeckButton.name = Neck;
                    NeckButton.GetComponent<AestheticButton>().Name.text = Neck;
                    NeckButton.GetComponent<AestheticButton>().Type = AestheticButton.AccessoryType.Neck;
                    NeckButton.GetComponent<RectTransform>().SetParent(Necks);
                    NeckButton.GetComponent<RectTransform>().localPosition =
                        new Vector3(NeckButton.GetComponent<RectTransform>().localPosition.x,
                        NeckButton.GetComponent<RectTransform>().localPosition.y,
                        0);
                    NeckButton.GetComponent<RectTransform>().localRotation = Quaternion.identity;
                    NeckButton.GetComponent<RectTransform>().localScale = Vector3.one;
                    if (Saves.GetHatSelected(Neck))
                        NeckButton.GetComponent<AestheticButton>().SetActive(true);
                }
            }
        }
    }

    public void SetMonsterDrawing()
    {
        Material targetMaterial = Drawings[SettingsHandler.Instance.EnemiesDefeated];

        if(Paper.sharedMaterial != targetMaterial)
            Paper.material = targetMaterial;
    }

    public void SetMenuSettingsNumbers(string settingsGroup = "")
    {
        if (settingsGroup == "Volume" || settingsGroup == "")
        {
            // Volume Settings
            MasterVolume.text = new string('l', (int)(SettingsHandler.Instance.MasterVolume / 0.05f));
            SFXVolume.text = new string('l', (int)(SettingsHandler.Instance.SFXVolume / 0.05f));
            MusicVolume.text = new string('l', (int)(SettingsHandler.Instance.MusicVolume / 0.05f));
            //MasterVolume.text = new string('l', (int)((60f + SettingsHandler.Instance.MasterVolume) / 3f));
            //SFXVolume.text = new string('l', (int)((60f + SettingsHandler.Instance.SFXVolume) / 3f));
            //MusicVolume.text = new string('l', (int)((60f + SettingsHandler.Instance.MusicVolume) / 3f));
        }
        if (settingsGroup == "Video" || settingsGroup == "")
        {
            // Video Settings
            AntiAliasing.text = SettingsHandler.Instance.AntiAliasing ? "L x l" : "L   l";
            Windowed.text = SettingsHandler.Instance.Windowed ? "L x l" : "L   l";
            Resolution.text = currentRes.width + "x" + currentRes.height;
            DepthOfField.text = new string('l', (int)(SettingsHandler.Instance.DepthOfField / 0.05f));
            MotionBlur.text = new string('l', (int)(SettingsHandler.Instance.MotionBlur / 5f));
        }
        if (settingsGroup == "Camera" || settingsGroup == "")
        {
            // Camera Settings
            CameraSensitivity.text = new string('l', (int)(SettingsHandler.Instance.CameraSensitivity / 0.375f));
            InvertX.text = SettingsHandler.Instance.InverseCameraX ? "L x l" : "L   l";
            InvertY.text = SettingsHandler.Instance.InverseCameraY ? "L x l" : "L   l";
        }
    }

    public void ToggleMenu(GameObject menu)
    {
        foreach (GameObject subMenu in Menus)
        {
            CanvasGroup group = subMenu.GetComponent<CanvasGroup>();
            bool sameMenu = subMenu == menu;
            group.alpha = sameMenu ? 1f : 0f;
            group.blocksRaycasts = sameMenu;
            group.interactable = sameMenu;

            //EventSystem.current.SetSelectedGameObject(null);
            Events.SetSelectedGameObject(null);
            //subMenu.SetActive(false);
        }
        
        //menu.SetActive(true);
    }

    public void ToggleToFirstMenu()
    {
        GameObject menu = Menus[0];
        foreach (GameObject subMenu in Menus)
        {
            CanvasGroup group = subMenu.GetComponent<CanvasGroup>();
            bool sameMenu = subMenu == menu;
            group.alpha = sameMenu ? 1f : 0f;
            group.blocksRaycasts = sameMenu;
            group.interactable = sameMenu;
            EventSystem.current.SetSelectedGameObject(Events.firstSelectedGameObject);
            //Events.SetSelectedGameObject(null);
        }
    }

    public void Unpause()
    {
        AimChanger.Instance.SetAim(AimChanger.Aim.Default);
        TP_Camera.Instance.Reset(false, false);
        TP_Controller.Instance.paused = false;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void ChangeAntiAliasing()
    {
        SettingsHandler.Instance.AntiAliasing = !SettingsHandler.Instance.AntiAliasing; //switch it
        SettingsHandler.Instance.SetAntiAliasing();
        SetMenuSettingsNumbers("Video");
    }

    public void ChangeWindowed()
    {
        SettingsHandler.Instance.Windowed = !SettingsHandler.Instance.Windowed; //switch it
        SettingsHandler.Instance.SetWindowedMode();
        SetMenuSettingsNumbers("Video");
    }

    public void ChangeResolution()
    {
        SettingsHandler.Instance.CurrentResolution = currentRes;
        SettingsHandler.Instance.SetResolution(currentRes);
        SetMenuSettingsNumbers("Video");
    }

    public void ChangeInvertX()
    {
        SettingsHandler.Instance.InverseCameraX = !SettingsHandler.Instance.InverseCameraX;
        SetMenuSettingsNumbers();
        SettingsHandler.Instance.SetCameraAxis();
        SetMenuSettingsNumbers("Camera");
    }

    public void ChangeInvertY()
    {
        SettingsHandler.Instance.InverseCameraY = !SettingsHandler.Instance.InverseCameraY;
        SetMenuSettingsNumbers();
        SettingsHandler.Instance.SetCameraAxis();
        SetMenuSettingsNumbers("Camera");
    }

    public void LoadMainMenu()
    {
        if (!fading)
        {
            fading = true;
            FadeManager.Instance.ChangeFading();
        }
    }
}
