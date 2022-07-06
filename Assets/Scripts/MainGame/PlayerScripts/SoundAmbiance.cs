using UnityEngine;

namespace MainGame.PlayerScripts
{
    public class SoundAmbiance : MonoBehaviour
    {
        [SerializeField] private AudioSource ambianceSource;
        [SerializeField] private AudioClip forestAmbiance;
        [SerializeField] private AudioClip horrorAmbiance;
        [SerializeField] private bool isDayPlaying;


        private void FixedUpdate()
        {
            if (!VoteMenu.Instance.IsNight)
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
            else if (VoteMenu.Instance.IsNight)
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
}