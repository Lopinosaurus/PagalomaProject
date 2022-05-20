using System;
using System.Collections;
using System.Collections.Generic;
using MainGame.PlayerScripts;
using Photon.Pun;
using UnityEngine;

public class JumpCollisionDetect : MonoBehaviour
{
    private bool _isColliding;
    [SerializeField] private LayerMask characterLayer;
    private int _characterLayerValue;
    public bool IsColliding => _isColliding;
    public List<Collider> ignoredJumpedColliders;

    private void Start()    
    {
        if (!GetComponentInParent<PhotonView>().IsMine)
        {
            Destroy(gameObject);
        }
        else
        {
            _characterLayerValue = (int) (Mathf.Log(characterLayer.value) / Mathf.Log(2));

            ignoredJumpedColliders = GetComponentInParent<PlayerMovement>().ignoredJumpedColliders;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider != null &&
            collision.collider.gameObject.layer != _characterLayerValue &&
            !ignoredJumpedColliders.Contains(collision.collider))
        {
            _isColliding = true;
            ignoredJumpedColliders.Add(collision.collider);
        }
    }

    private void OnCollisionExit(Collision other)
    {
        _isColliding = false;
        ignoredJumpedColliders.Remove(other.collider);
    }
}
