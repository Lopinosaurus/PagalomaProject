using MainGame.Menus;
using UnityEngine;

namespace MainGame
{
    public class SoundAmbiance : MonoBehaviour
    {
        [SerializeField] AudioSource dayAmbiance;
        [SerializeField] AudioSource nightAmbiance;
   

   
        void Update()
        {
            if (VoteMenu.Instance.isDay)
            {
                if (!dayAmbiance.isPlaying)
                {
                    nightAmbiance.Stop();
                    dayAmbiance.Play();
                }
            }
        
            if (!VoteMenu.Instance.isDay)
            {
                if (!nightAmbiance.isPlaying)
                {
                    dayAmbiance.Stop();
                    nightAmbiance.Play();
                }
            }
        }
    }
}