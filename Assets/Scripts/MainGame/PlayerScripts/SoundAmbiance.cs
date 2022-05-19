using MainGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SoundAmbiance : MonoBehaviour
{
    [SerializeField] private AudioSource ambianceSource;
    [SerializeField] private AudioClip forestAmbiance;
    [SerializeField] private AudioClip horrorAmbiance;
    [SerializeField] private bool isDayPlaying;


    void Update()
    {
        if (!VoteMenu.Instance.isNight)
        {
            if (!isDayPlaying)
            {
                ambianceSource.Stop();
                ambianceSource.clip = forestAmbiance;
                ambianceSource.volume = .23f;
                ambianceSource.Play();
                isDayPlaying = true;
            }
        }
        
        if (VoteMenu.Instance.isNight)
        {
            if (isDayPlaying)
            {
                ambianceSource.Stop();
                ambianceSource.clip = horrorAmbiance;
                ambianceSource.volume = .8f;
                ambianceSource.Play();
                isDayPlaying = false;
            }
        }
    }
}