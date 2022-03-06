using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionMenu : MonoBehaviour
{

    Resolution[] screenRes;
    public AudioMixer MainAudioMixer;
    public Dropdown resDropdown;

    public void Start()
    {
        screenRes = Screen.resolutions;

        resDropdown.ClearOptions();

        List<string> options = new List<string>();

        int currentResIndex = 0;

        for (int i = 0; i < screenRes.Length; i++ )
        {
            string option = screenRes[i].width + "x" + screenRes[i].height;
            options.Add(option);

            if (screenRes[i].width == Screen.currentResolution.width && screenRes[i].height == Screen.currentResolution.height)
            {
                currentResIndex = i;
            }
        }

        resDropdown.AddOptions(options);
        resDropdown.value = currentResIndex;
        resDropdown.RefreshShownValue();
    }

    public void SetVolume(float volume)
    {
        MainAudioMixer.SetFloat("MainVolume", volume);
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
