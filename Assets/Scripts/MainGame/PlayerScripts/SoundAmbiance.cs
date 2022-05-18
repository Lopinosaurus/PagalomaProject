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
        if (VoteMenu.Instance.isDay)
        {
            if (!isDayPlaying)
            {
                ambianceSource.Stop();
                ambianceSource.clip = forestAmbiance;
                ambianceSource.Play();
                isDayPlaying = true;
            }
        }
        
        if (!VoteMenu.Instance.isDay)
        {
            if (isDayPlaying)
            {
                ambianceSource.Stop();
                ambianceSource.clip = horrorAmbiance;
                ambianceSource.Play();
                isDayPlaying = false;
            }
        }
    }
}