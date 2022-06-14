using MainGame.PlayerScripts;
using Photon.Pun;
using UnityEngine;

public class PlayerGroundCheck : MonoBehaviour
{
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private LayerMask characterMask;
    private int characterMaskValue = 7;
    private int count;

    private void Awake()
    {
        characterMaskValue = (int)Mathf.Log(characterMask.value, 2);
        if (!GetComponentInParent<PhotonView>().IsMine) Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (null == collision || collision.gameObject.layer == characterMaskValue) return;
        count++;
        _playerMovement.isSphereGrounded = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (null == collision || collision.gameObject.layer == characterMaskValue) return;
        count--;
        if (count > 0) return;
        _playerMovement.isSphereGrounded = false;
    }
}