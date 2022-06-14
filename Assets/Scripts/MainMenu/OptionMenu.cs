using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionMenu : MonoBehaviour
{
    private Resolution[] screenRes;
    public AudioMixer MainAudioMixer;
    public AudioMixer MusicMixer;
    public Dropdown resDropdown;

    public void Start()
    {
        screenRes = Screen.resolutions;

        if (resDropdown != null)
        {
            resDropdown.ClearOptions();

            var options = new List<string>();

            int currentResIndex = 0;

            for (int i = 0; i < screenRes.Length; i++)
            {
                string option = screenRes[i].width + "x" + screenRes[i].height;
                options.Add(option);

                if (screenRes[i].width == Screen.currentResolution.width &&
                    screenRes[i].height == Screen.currentResolution.height)
                {
                    currentResIndex = i;
                }
            }

            resDropdown.AddOptions(options);
            resDropdown.value = currentResIndex;
            resDropdown.RefreshShownValue();
        }
    }

    public void SetScreenRes(int resIndex)
    {
        Resolution ScreenRes = screenRes[resIndex];

        Screen.SetResolution(ScreenRes.width, ScreenRes.height, Screen.fullScreen);
    }

    public void SetVolume(float volume)
    {
        MainAudioMixer.SetFloat("MainVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        MusicMixer.SetFloat("MusicVolume", volume);
    }
    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    public void EnableFullscreen(bool fullScreenEnabled)
    {
        Screen.fullScreen = fullScreenEnabled;
    }
}
