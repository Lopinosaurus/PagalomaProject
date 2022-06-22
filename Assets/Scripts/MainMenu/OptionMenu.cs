using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionMenu : MonoBehaviour
{
    private Resolution[] _screenRes;
    public AudioMixer mainAudioMixer;
    public AudioMixer musicMixer;
    public Dropdown resDropdown;

    public void Start()
    {
        _screenRes = Screen.resolutions;

        if (resDropdown != null)
        {
            resDropdown.ClearOptions();

            var options = new List<string>();

            int currentResIndex = 0;

            for (int i = 0; i < _screenRes.Length; i++)
            {
                string option = _screenRes[i].width + "x" + _screenRes[i].height;
                options.Add(option);

                if (_screenRes[i].width == Screen.currentResolution.width &&
                    _screenRes[i].height == Screen.currentResolution.height)
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
        Resolution screenRes = _screenRes[resIndex];

        Screen.SetResolution(screenRes.width, screenRes.height, Screen.fullScreen);
    }

    public void SetVolume(float volume)
    {
        mainAudioMixer.SetFloat("MainVolume", volume);
    }

    public void SetMusicVolume(float volume)
    {
        musicMixer.SetFloat("MusicVolume", volume);
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
