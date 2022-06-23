using MainGame;
using MainGame.PlayerScripts.Roles;
using UnityEngine;

public class DetectCollision : MonoBehaviour
{
    private PlayerController _playerController;

    private void Awake()
    {
        _playerController = GetComponentInParent<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (VoteMenu.Instance && other)
            if (!VoteMenu.Instance.isDay && !VoteMenu.Instance.isFirstDay)
            {
                if (_playerController.role.isAlive) _playerController.role.UpdateTarget(other, true);
            }
    }

    private void OnTriggerExit(Collider other)
    {
        if (VoteMenu.Instance && other)
            if (!VoteMenu.Instance.isDay && !VoteMenu.Instance.isFirstDay)
            {
                if (_playerController.role.isAlive) _playerController.role.UpdateTarget(other, false);
            }
    }
}