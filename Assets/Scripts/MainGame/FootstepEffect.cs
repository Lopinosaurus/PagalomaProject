using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepEffect : MonoBehaviour
{
    
    [SerializeField] CharacterController characterController;
    public AudioSource footsteps;
    [SerializeField] AudioClip dryCrouching;
    [SerializeField] AudioClip dryWalking;
    [SerializeField] AudioClip drySprinting;
    [SerializeField] AudioClip wetCrouching;
    [SerializeField] AudioClip wetWalking;
    [SerializeField] AudioClip wetSprinting;

    

    void Update()
    {
        // Avoid error on CharacterController destruction
        if (characterController == null) 
            return;

        #region Crouching Case
        
        if (characterController.isGrounded 
            && (!footsteps.isPlaying) && characterController.velocity.magnitude > 0f 
            && characterController.velocity.magnitude < 1.5f)

        {
            int soundTaker = Random.Range(1, 2);

            if (1 == soundTaker)
            {
                footsteps.clip = dryCrouching;
                footsteps.volume = Random.Range(0.8f, 1);
                footsteps.pitch = Random.Range(0.8f, 1.1f);
                footsteps.Play();
            }

            else
            {
                footsteps.clip = wetCrouching;
                footsteps.volume = Random.Range(0.8f, 1);
                footsteps.pitch = Random.Range(0.8f, 1.1f);
                footsteps.Play();
            }
        }

        #endregion

        #region Walking Case
        else if (characterController.isGrounded 
            && (!footsteps.isPlaying) && characterController.velocity.magnitude > 0f 
            && characterController.velocity.magnitude < 3f)
        {
            int soundTaker = Random.Range(1, 2);

            if (1 == soundTaker)
            {
                footsteps.clip = dryWalking;
                footsteps.volume = Random.Range(0.8f, 1);
                footsteps.pitch = Random.Range(0.8f, 1.1f);
                footsteps.Play();
            }

            else
            {
                footsteps.clip = wetWalking;
                footsteps.volume = Random.Range(0.8f, 1);
                footsteps.pitch = Random.Range(0.8f, 1.1f);
                footsteps.Play();
            }
        }
        #endregion

        #region Sprinting Case
        else if (characterController.isGrounded
            && (!footsteps.isPlaying) && characterController.velocity.magnitude > 0f
            && characterController.velocity.magnitude < 6f)
        {
            int soundTaker = Random.Range(1, 2);

            if (1 == soundTaker)
            {
                footsteps.clip = drySprinting;
                footsteps.volume = Random.Range(0.8f, 1);
                footsteps.pitch = Random.Range(0.8f, 1.1f);
                footsteps.Play();
            }

            else
            {
                footsteps.clip = wetSprinting;
                footsteps.volume = Random.Range(0.8f, 1);
                footsteps.pitch = Random.Range(0.8f, 1.1f);
                footsteps.Play();
            }
        }
        #endregion
    }
}
