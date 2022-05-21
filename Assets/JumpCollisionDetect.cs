using System.Collections.Generic;
using MainGame.PlayerScripts;
using Photon.Pun;
using UnityEngine;

public class JumpCollisionDetect : MonoBehaviour
{
    public bool IsColliding => collisions > 0;
    public List<Collider> ignoredJumpedColliders;
    private int collisions;

    private void Start()    
    {
        if (!GetComponentInParent<PhotonView>().IsMine)
        {
            Destroy(gameObject);
        }
        else
        {
            ignoredJumpedColliders = GetComponentInParent<PlayerMovement>().ignoredJumpedColliders;
            GetComponent<Collider>();
        }
    }

    private void OnCollisionStay(Collision collision)
    {
            if (!collision.gameObject.CompareTag("Player"))
            {
                collisions++;
                if (!ignoredJumpedColliders.Contains(collision.collider))
                {
                    ignoredJumpedColliders.Add(collision.collider);
                }
            }
    }

    private void OnCollisionExit(Collision other)
    {
        if (!other.gameObject.CompareTag("Player"))
        {
            collisions -= collisions == 0 ? 0 : 1;
            ignoredJumpedColliders.Remove(other.collider);
        }
    }
}
