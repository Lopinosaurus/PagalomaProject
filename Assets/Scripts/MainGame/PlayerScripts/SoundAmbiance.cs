using MainGame;
using UnityEngine;

public class SoundAmbiance : MonoBehaviour
{
    [SerializeField] private AudioSource ambianceSource;
    [SerializeField] private AudioClip forestAmbiance;
    [SerializeField] private AudioClip horrorAmbiance;
    [SerializeField] private bool isDayPlaying;


    private void Update()
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
        else if (VoteMenu.Instance.isNight)
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