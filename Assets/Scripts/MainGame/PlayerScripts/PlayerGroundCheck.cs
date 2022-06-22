using MainGame.PlayerScripts;
using Photon.Pun;
using UnityEngine;

public class PlayerGroundCheck : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private LayerMask characterMask;
    private int _characterMaskValue = 7;
    private int _count;

    private void Awake()
    {
        _characterMaskValue = (int) Mathf.Log(characterMask.value, 2);
        if (!GetComponentInParent<PhotonView>().IsMine) Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (null == collision || collision.gameObject.layer == _characterMaskValue) return;
        _count++;
        playerMovement.isSphereGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (null == collision || collision.gameObject.layer == _characterMaskValue) return;
        _count--;
        if (_count > 0) return;
        playerMovement.isSphereGrounded = false;
    }
}