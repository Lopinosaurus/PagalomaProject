using MainGame;
using UnityEngine;

public class DetectCollision : MonoBehaviour
{
    private PlayerController _pc;

    private void Awake()
    {
        _pc = GetComponentInParent<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (VoteMenu.Instance && other)
            if (!VoteMenu.Instance.isDay && !VoteMenu.Instance.isFirstDay)
            {
                if (_pc.role.isAlive) _pc.role.UpdateTarget(other, true);
            }
    }

    private void OnTriggerExit(Collider other)
    {
        if (VoteMenu.Instance && other)
            if (!VoteMenu.Instance.isDay && !VoteMenu.Instance.isFirstDay)
            {
                if (_pc.role.isAlive) _pc.role.UpdateTarget(other, false);
            }
    }
}