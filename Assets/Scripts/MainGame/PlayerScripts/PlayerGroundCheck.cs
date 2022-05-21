using System;
using MainGame.PlayerScripts;
using UnityEngine;

public class PlayerGroundCheck : MonoBehaviour
{
    [SerializeField] private PlayerMovement _playerMovement;
    private int characterMaskValue = 7;

    private void Awake()
    {
        characterMaskValue = _playerMovement._characterLayerValue;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (null != collision && collision.gameObject.layer != characterMaskValue)
        {
            _playerMovement.isSphereGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (null != collision && collision.gameObject.layer != characterMaskValue)
        {
            _playerMovement.isSphereGrounded = false;
        }
    }
}
