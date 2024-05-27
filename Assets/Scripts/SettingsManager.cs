using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{   
    public AudioMixer audioMixer;
    public GameObject FullscreenToggle;
    public GameObject MuteToggle;
    public TMP_Dropdown ResolutionDropdown;
    Resolution[] Resolutions;
    int CurrentResolutionIndex = 0;
    List<Resolution> UniqueResolutionsList = new List<Resolution>();
    // Start is called before the first frame update
    void Start()
    {
        if(PlayerPrefs.GetInt("IsFullScreen",1)==1)
        {
            FullscreenToggle.GetComponent<Toggle>().isOn=true;
            Screen.fullScreen=true;
        }
        else
        {
            FullscreenToggle.GetComponent<Toggle>().isOn=false;
            Screen.fullScreen=false;
        }
        
        if(PlayerPrefs.GetInt("MuteAudio",0)==0)
        {
            MuteToggle.GetComponent<Toggle>().isOn=false;
            audioMixer.SetFloat("Volume",0f);
        }
        else
        {
            MuteToggle.GetComponent<Toggle>().isOn=true;
            audioMixer.SetFloat("Volume",-80f);
        }

        Resolutions = Screen.resolutions;
        ResolutionDropdown.ClearOptions();

        HashSet<string> UniqueResolutions = new HashSet<string>();
        List<string> ScreenSizes = new List<string>();

        for (int i = 0; i < Resolutions.Length; i++)
        {
            string Option = Resolutions[i].width + " x " + Resolutions[i].height;

            if (UniqueResolutions.Add(Option))
            {
                ScreenSizes.Add(Option);
                UniqueResolutionsList.Add(Resolutions[i]);
            }

            if (Resolutions[i].width == Screen.currentResolution.width &&
                Resolutions[i].height == Screen.currentResolution.height)
            {
                CurrentResolutionIndex = PlayerPrefs.GetInt("ResolutionIndex",UniqueResolutionsList.Count - 1);
            }
        }

        ResolutionDropdown.AddOptions(ScreenSizes);
        ResolutionDropdown.value = CurrentResolutionIndex;
        ResolutionDropdown.RefreshShownValue();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetResolution(int ResolutionIndex)
    {
        PlayerPrefs.SetInt("ResolutionIndex",ResolutionIndex);
        Resolution resolution = UniqueResolutionsList[ResolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetFullscreen(bool b)
    {
        if(b)
        {
            PlayerPrefs.SetInt("IsFullScreen",1);    
        }
        else
        {
            PlayerPrefs.SetInt("IsFullScreen",0);
        }
        Screen.fullScreen=b;
    }

    public void Mute(bool b)
    {
        if(b)
        {
            PlayerPrefs.SetInt("MuteAudio",1);
            audioMixer.SetFloat("Volume",-80f);
        }
        else
        {
            PlayerPrefs.SetInt("MuteAudio",0);
            audioMixer.SetFloat("Volume",0);
        }
    }
}
