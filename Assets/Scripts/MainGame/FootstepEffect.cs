using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepEffect : MonoBehaviour
{
    
    [SerializeField] CharacterController characterController;
    [SerializeField] AudioSource footsteps;


    
    void Update()
    {
        if (characterController == null) return; // Avoid error on CharacterController destruction
        if (characterController.isGrounded && footsteps.isPlaying == false && characterController.velocity.magnitude > 0f)

        {
            footsteps.volume = Random.Range(0.8f, 1);
            footsteps.pitch = Random.Range(0.8f, 1.1f);
            footsteps.Play();
        }
    }
}
