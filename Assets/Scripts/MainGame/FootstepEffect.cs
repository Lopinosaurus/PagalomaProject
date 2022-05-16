using System.Collections;
using UnityEngine;

namespace MainGame
{
    public class FootstepEffect : MonoBehaviour
    {
    
        [SerializeField] CharacterController characterController;
        [SerializeField] AudioClip dryCrouching;
        [SerializeField] AudioClip dryWalking;
        [SerializeField] AudioClip drySprinting;
        [SerializeField] AudioClip wetCrouching;
        [SerializeField] AudioClip wetWalking;
        [SerializeField] AudioClip wetSprinting;
        private bool _trueIsPlaying = false;
        public AudioSource footsteps;
        public enum FootstepState
        {
            CROUCHING,
            WALKING,
            SPRINTING
        }
    
        private IEnumerator _PlayFootstep(AudioClip clip, FootstepState state)
        {
            _trueIsPlaying = true;
            footsteps.clip = clip;
            if (state == FootstepState.CROUCHING)
            {
                footsteps.volume = Random.Range(0.1f, 0.2f);
                footsteps.pitch = Random.Range(0.8f, 1.1f);
                footsteps.Play();
                yield return new WaitForSeconds(.8f);
            }
        
            else if (state == FootstepState.WALKING)
            {
                footsteps.volume = Random.Range(0.25f, 0.35f);
                footsteps.pitch = Random.Range(0.8f, 1.1f);
                footsteps.Play();
                yield return new WaitForSeconds(.5f);
            }
        
            else if (state == FootstepState.SPRINTING)
            {
                footsteps.volume = Random.Range(0.25f, 0.35f);
                footsteps.pitch = Random.Range(0.8f, 1.1f);
                footsteps.Play();
                yield return null;
            }

            _trueIsPlaying = false;
        }

        void Update()
        {
            // Avoid error on CharacterController destruction
            if (characterController == null) 
                return;

            #region Crouching Case
        
            if (characterController.isGrounded 
                && !footsteps.isPlaying 
                && characterController.velocity.magnitude > 0f 
                && characterController.velocity.magnitude < 1.5f)

            {
                int soundTaker = Random.Range(1, 2);

                if (1 == soundTaker)
                {
                    if (!_trueIsPlaying)
                    {
                        StartCoroutine(_PlayFootstep(dryCrouching, FootstepState.CROUCHING));
                    }
                }

                else
                {
                    StartCoroutine(_PlayFootstep(wetCrouching, FootstepState.CROUCHING));
                }
            }

            #endregion

            #region Walking Case
            else if (characterController.isGrounded 
                     && !footsteps.isPlaying 
                     && characterController.velocity.magnitude > 0f 
                     && characterController.velocity.magnitude < 3f)
            {
                int soundTaker = Random.Range(1, 2);
            
            
                if (1 == soundTaker)
                {
                    if (!_trueIsPlaying)
                    {
                        StartCoroutine(_PlayFootstep(dryWalking, FootstepState.WALKING));
                    }
                }

                else
                {
                    if (!_trueIsPlaying)
                    {
                        StartCoroutine(_PlayFootstep(wetWalking, FootstepState.WALKING));
                    }
                }
            }
            #endregion

            #region Sprinting Case
            else if (characterController.isGrounded
                     && !footsteps.isPlaying 
                     && characterController.velocity.magnitude > 0f
                     && characterController.velocity.magnitude < 6f)
            {
                int soundTaker = Random.Range(1, 2);

                if (1 == soundTaker)
                {
                    if (!_trueIsPlaying)
                    {
                        StartCoroutine(_PlayFootstep(drySprinting, FootstepState.SPRINTING));
                    }
                }

                else
                {
                    if (!_trueIsPlaying)
                    {
                        StartCoroutine(_PlayFootstep(wetSprinting, FootstepState.SPRINTING));
                    }
                }
            }
            #endregion
        }
    }
}
