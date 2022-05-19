using System;
using System.Collections;
using System.Collections.Generic;
using MainGame.PlayerScripts;
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
        _characterLayerValue = (int) (Mathf.Log(characterLayer.value) / Mathf.Log(2));
        ignoredJumpedColliders = GetComponentInParent<PlayerMovement>().ignoredJumpedColliders;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.layer != _characterLayerValue) _isColliding = true;
        if (_isColliding && !ignoredJumpedColliders.Contains(collision.collider))
        {
            ignoredJumpedColliders.Add(collision.collider);
        }

        Debug.Log("OnCollisionStay !");
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.layer != _characterLayerValue) _isColliding = false;
        ignoredJumpedColliders.Remove(other.collider);
    }
}
