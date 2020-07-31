using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Saves
{
    private static int SaveFileNumber
    {
        get
        {
            if (SettingsHandler.Instance != null)
                return SettingsHandler.Instance.SaveFileNumber;
            else
                return 0;
        }
    }
    #region File Saving Methods
    private static void Save()
    {
        PlayerPrefs.Save();
    }

    public static void DeleteCurrentSaveFile()
    {
        //include all possible player pref things
        DeleteChild();
        DeleteSettings();
        DeleteAllGrateOpenings();
        DeleteAllPlushiesFound();

        DeleteAllHatsFound();
        DeleteAllMasksFound();
        DeleteAllNecksFound();
        DeleteSkinSelected();
    }

    public static void DeleteAllSaveFiles()
    {
        //This one will never be used in-game, only for testing purposes
        PlayerPrefs.DeleteAll();
        Save();
    }
    #endregion

    #region Grate Saving Methods
    public static void DeleteAllGrateOpenings()
    {
        for (int i = 1; i <= 30; i++)
            DeleteGrateOpening(i);
    }

    public static void DeleteGrateOpening(int grateNumber)
    {
        Debug.Log("Deleting " + SaveFileNumber + "Grate" + grateNumber);
        PlayerPrefs.DeleteKey(SaveFileNumber + "Grate" + grateNumber);
        Save();
    }

    public static void SaveGrateOpening(int grateNumber)
    {
        PlayerPrefs.SetInt(SaveFileNumber + "Grate" + grateNumber, 1);
        Save();
    }

    public static int GetGrateOpening(int grateNumber)
    {
        return PlayerPrefs.GetInt(SaveFileNumber + "Grate" + grateNumber, 0);
    }
    #endregion

    #region Monster Saving Methods
    public static void SaveMonstersDefeated(int monstersDefeated)
    {
        PlayerPrefs.SetInt(SaveFileNumber + "Defeated", monstersDefeated);
        Save();
    }

    public static int GetMonstersDefeated()
    {
        return PlayerPrefs.GetInt(SaveFileNumber + "Defeated");
    }

    public static void DeteletMonstersDefeated()
    {
        PlayerPrefs.DeleteKey(SaveFileNumber + "Defeated");
        Save();
    }
    #endregion

    #region Plushy Saving Methods
    public static void AddToAllPlushiesTracker(string plushyName)
    {
        string key = SaveFileNumber + "AllPlushiesFound";
        if (PlayerPrefs.HasKey(key))
            PlayerPrefs.SetString(key, PlayerPrefs.GetString(key) + "," + plushyName);
        else
            PlayerPrefs.SetString(key, plushyName);
    }

    public static string GetAllPlushies()
    {
        return PlayerPrefs.GetString(SaveFileNumber + "AllPlushiesFound");
    }

    public static void SavePlushyFound(string plushyFound)
    {
        if(!GetPlushyFound(plushyFound))
            AddToAllPlushiesTracker(plushyFound);
        PlayerPrefs.SetInt(SaveFileNumber + "Found" + plushyFound, 1); //set to 1 as "boolean"
        Save();
    }

    public static bool GetPlushyFound(string plushyName)
    {
        return PlayerPrefs.GetInt(SaveFileNumber + "Found" + plushyName) == 1;
    }

    public static void DeleteAllPlushiesFound()
    {
        List<string> allPlushies = GetAllPlushies().Split(',').ToList();
        foreach(string plushy in allPlushies)
        {
            DeletePlushyFound(plushy);
        }
        PlayerPrefs.DeleteKey(SaveFileNumber + "AllPlushiesFound");
        Save();
    }

    public static void DeletePlushyFound(string plushyName)
    {
        PlayerPrefs.DeleteKey(SaveFileNumber + "Found" + plushyName);
        Save();
    }
    #endregion

    #region Custom Child Methods
    public static void SaveChild()
    {
        //get all the properties that are necessary for the child to be modified and save them by float number
        Save();
    }

    public static void GetChild()
    {
        //get all the properties that are necessary for the child to be modified and return them by float number
    }

    public static void DeleteChild()
    {

    }
    #endregion

    #region Custom Settings
    public static void DeleteSettings()
    {
        DeleteAudioSettings();
        DeleteVideoSettings();
        DeleteControllerSettings();
    }

    #region Custom Audio Settings Methods
    public static void SaveAudioSettings(float master, float sfx, float music)
    {
        int saveFile = SaveFileNumber;
        PlayerPrefs.SetFloat(saveFile + "MasterVolume", master);
        PlayerPrefs.SetFloat(saveFile + "SFXVolume", sfx);
        PlayerPrefs.SetFloat(saveFile + "MusicVolume", music);
        Save();
    }

    public static float GetMasterVolumeSettings()
    {
        return PlayerPrefs.GetFloat(SaveFileNumber + "MasterVolume");
    }

    public static float GetSFXVolumeSettings()
    {
        return PlayerPrefs.GetFloat(SaveFileNumber + "SFXVolume");
    }

    public static float GetMusicVolumeSettings()
    {
        return PlayerPrefs.GetFloat(SaveFileNumber + "MusicVolume");
    }

    public static void DeleteAudioSettings()
    {
        int savefile = SaveFileNumber;
        PlayerPrefs.DeleteKey(savefile + "MasterVolume");
        PlayerPrefs.DeleteKey(savefile + "SFXVolume");
        PlayerPrefs.DeleteKey(savefile + "MusicVolume");
        Save();
    }
    #endregion

    #region Custom Video Settings Methods
    public static void SaveVideoSettings(int resolutionX, int resolutionY, bool windowed, bool alias, float dof, float blur)
    {
        int saveFile = SaveFileNumber;
        PlayerPrefs.SetInt(saveFile + "ResolutionX",resolutionX);
        PlayerPrefs.SetInt(saveFile + "ResolutionY",resolutionY);
        PlayerPrefs.SetInt(saveFile + "Windowed", windowed ? 1 : 0);
        PlayerPrefs.SetInt(saveFile + "Antialiasing", alias ? 1 : 0);
        PlayerPrefs.SetFloat(saveFile + "DepthOfField", dof);
        PlayerPrefs.SetFloat(saveFile + "MotionBlur", blur);
        Save();
    }

    public static Vector2 GetResolution()
    {
        int saveFile = SaveFileNumber;
        return new Vector2(PlayerPrefs.GetInt(saveFile + "ResolutionX"), PlayerPrefs.GetInt(saveFile + "ResolutionY"));
    }

    public static bool GetWindowed()
    {
        return PlayerPrefs.GetInt(SaveFileNumber + "Windowed") == 1;
    }

    public static bool GetAntiAliasing()
    {
        return PlayerPrefs.GetInt(SaveFileNumber + "Antialiasing") == 1;
    }

    public static float GetDepthOfField()
    {
        return PlayerPrefs.GetInt(SaveFileNumber + "DepthOfField");
    }

    public static float GetMotionBlur()
    {
        return PlayerPrefs.GetInt(SaveFileNumber + "MotionBlur");
    }

    public static void DeleteVideoSettings()
    {
        int saveFile = SaveFileNumber;
        PlayerPrefs.DeleteKey(saveFile + "ResolutionX");
        PlayerPrefs.DeleteKey(saveFile + "ResolutionY");
        PlayerPrefs.DeleteKey(saveFile + "Windowed");
        PlayerPrefs.DeleteKey(saveFile + "Antialiasing");
        PlayerPrefs.DeleteKey(saveFile + "DepthOfField");
        PlayerPrefs.DeleteKey(saveFile + "MotionBlur");
        Save();
    }
    #endregion

    #region Custom Controller Settings Methods
    public static void SaveControllerSettings(float sensitivity, bool camInvertX, bool camInvertY)
    {
        int saveFile = SaveFileNumber;
        PlayerPrefs.SetFloat(saveFile + "CameraSensitivity", sensitivity);
        PlayerPrefs.SetInt(saveFile + "CameraInvertedX", camInvertX ? 1 : 0);
        PlayerPrefs.SetInt(saveFile + "CameraInvertedY", camInvertY ? 1 : 0);
        Save();
    }

    public static float GetCameraSensitivity()
    {
        return PlayerPrefs.GetFloat(SaveFileNumber + "CameraSensitivity");
    }

    public static bool GetCameraInvertedX()
    {
        return PlayerPrefs.GetFloat(SaveFileNumber + "CameraInvertedX") == 1;
    }

    public static bool GetCameraInvertedY()
    {
        return PlayerPrefs.GetFloat(SaveFileNumber + "CameraInvertedY") == 1;
    }

    public static void DeleteControllerSettings()
    {
        int saveFile = SaveFileNumber;
        PlayerPrefs.DeleteKey(saveFile + "CameraSensitivity");
        PlayerPrefs.DeleteKey(saveFile + "CameraInvertedX");
        PlayerPrefs.DeleteKey(saveFile + "CameraInvertedY");
        Save();
    }
    #endregion
    #endregion

    #region Aesthetics Methods
    #region Hats Methods
    public static void AddToAllHatsTracker(string hatName)
    {
        string key = SaveFileNumber + "AllHatsFound";
        if (PlayerPrefs.HasKey(key))
            PlayerPrefs.SetString(key, PlayerPrefs.GetString(key) + "," + hatName);
        else
            PlayerPrefs.SetString(key, hatName);
    }

    public static string GetAllHats()
    {
        return PlayerPrefs.GetString(SaveFileNumber + "AllHatsFound");
    }

    public static void SaveHatFound(string HatFound)
    {
        if(!GetHatFound(HatFound))
            AddToAllHatsTracker(HatFound);
        PlayerPrefs.SetInt(SaveFileNumber + "Found" + HatFound, 1); //set to 1 as "boolean"
        Save();
    }

    public static bool GetHatSelected(string HatName)
    {
        return PlayerPrefs.GetInt(SaveFileNumber + "Worn" + HatName, 0) == 1; //set to selected as "boolean"
    }

    public static void SaveHatSelected(string HatName, int selected)
    {
        PlayerPrefs.SetInt(SaveFileNumber + "Worn" + HatName, selected); //set to selected as "boolean"
        Save();
    }

    public static bool GetHatFound(string HatName)
    {
        return PlayerPrefs.GetInt(SaveFileNumber + "Found" + HatName, 0) == 1;
    }

    public static void DeleteAllHatsFound()
    {
        List<string> allHats = GetAllHats().Split(',').ToList();
        foreach (string Hat in allHats)
        {
            DeleteHatFound(Hat);
        }
        PlayerPrefs.DeleteKey(SaveFileNumber + "AllHatsFound");
        Save();
    }

    public static void DeleteHatFound(string HatName)
    {
        PlayerPrefs.DeleteKey(SaveFileNumber + "Found" + HatName);
        Save();
    }
    #endregion
    #region Masks Methods
    public static void AddToAllMasksTracker(string MaskName)
    {
        string key = SaveFileNumber + "AllMasksFound";
        if (PlayerPrefs.HasKey(key))
            PlayerPrefs.SetString(key, PlayerPrefs.GetString(key) + "," + MaskName);
        else
            PlayerPrefs.SetString(key, MaskName);
    }

    public static string GetAllMasks()
    {
        return PlayerPrefs.GetString(SaveFileNumber + "AllMasksFound");
    }

    public static void SaveMaskFound(string MaskFound)
    {
        if (!GetMaskFound(MaskFound))
            AddToAllMasksTracker(MaskFound);
        PlayerPrefs.SetInt(SaveFileNumber + "Found" + MaskFound, 1); //set to 1 as "boolean"
        //AddToAllMasksTracker(MaskFound);
        Save();
    }

    public static bool GetMaskSelected(string MaskName)
    {
        return PlayerPrefs.GetInt(SaveFileNumber + "Worn" + MaskName, 0) == 1; //set to selected as "boolean"
    }

    public static void SaveMaskSelected(string MaskName, int selected)
    {
        PlayerPrefs.SetInt(SaveFileNumber + "Worn" + MaskName, selected); //set to selected as "boolean"
        Save();
    }

    public static bool GetMaskFound(string MaskName)
    {
        return PlayerPrefs.GetInt(SaveFileNumber + "Found" + MaskName, 0) == 1;
    }

    public static void DeleteAllMasksFound()
    {
        List<string> allMasks = GetAllMasks().Split(',').ToList();
        foreach (string Mask in allMasks)
        {
            DeleteMaskFound(Mask);
        }
        PlayerPrefs.DeleteKey(SaveFileNumber + "AllMasksFound");
        Save();
    }

    public static void DeleteMaskFound(string MaskName)
    {
        PlayerPrefs.DeleteKey(SaveFileNumber + "Found" + MaskName);
        Save();
    }
    #endregion
    #region Neck Methods
    public static void AddToAllNecksTracker(string NeckName)
    {
        string key = SaveFileNumber + "AllNecksFound";
        if (PlayerPrefs.HasKey(key))
            PlayerPrefs.SetString(key, PlayerPrefs.GetString(key) + "," + NeckName);
        else
            PlayerPrefs.SetString(key, NeckName);
    }

    public static string GetAllNecks()
    {
        return PlayerPrefs.GetString(SaveFileNumber + "AllNecksFound");
    }

    public static void SaveNeckFound(string NeckFound)
    {
        if (!GetNeckFound(NeckFound))
            AddToAllNecksTracker(NeckFound);
        PlayerPrefs.SetInt(SaveFileNumber + "Found" + NeckFound, 1); //set to 1 as "boolean"
        //AddToAllNecksTracker(NeckFound);
        Save();
    }

    public static bool GetNeckSelected(string NeckName)
    {
        return PlayerPrefs.GetInt(SaveFileNumber + "Worn" + NeckName, 0) == 1; //set to selected as "boolean"
    }

    public static void SaveNeckSelected(string NeckName, int selected)
    {
        PlayerPrefs.SetInt(SaveFileNumber + "Worn" + NeckName, selected); //set to selected as "boolean"
        Save();
    }

    public static bool GetNeckFound(string NeckName)
    {
        return PlayerPrefs.GetInt(SaveFileNumber + "Found" + NeckName, 0) == 1;
    }

    public static void DeleteAllNecksFound()
    {
        List<string> allNecks = GetAllNecks().Split(',').ToList();
        foreach (string Neck in allNecks)
        {
            DeleteNeckFound(Neck);
        }
        PlayerPrefs.DeleteKey(SaveFileNumber + "AllNecksFound");
        Save();
    }

    public static void DeleteNeckFound(string NeckName)
    {
        PlayerPrefs.DeleteKey(SaveFileNumber + "Found" + NeckName);
        Save();
    }
    #endregion
    #region Skin Methods
    //Doesn't need all the saving since each skin is unlocked by plushy quantity
    public static int GetSkinSelected()
    {
        return PlayerPrefs.GetInt(SaveFileNumber + "CurrentSkin", 0);
    }

    public static void SaveSkinSelected(int selectedSkin)
    {
        PlayerPrefs.SetInt(SaveFileNumber + "CurrentSkin", selectedSkin);
        Save();
    }

    public static void DeleteSkinSelected()
    {
        PlayerPrefs.DeleteKey(SaveFileNumber + "CurrentSkin");
        Save();
    }
    #endregion
    #endregion
}
