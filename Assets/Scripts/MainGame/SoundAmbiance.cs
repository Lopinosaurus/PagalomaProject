using MainGame;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundAmbiance : MonoBehaviour
{
    [SerializeField] AudioSource dayAmbiance;
    [SerializeField] AudioSource nightAmbiance;
   

   
    void Update()
    {
        if (VoteMenu.Instance.isDay)
        {
            if (!(dayAmbiance.isPlaying))
            {
                nightAmbiance.Stop();
                dayAmbiance.Play();
            }
        }
        
        if (!(VoteMenu.Instance.isDay))
        {
            if (!(nightAmbiance.isPlaying))
            {
                dayAmbiance.Stop();
                nightAmbiance.Play();
            }
        }
    }
}