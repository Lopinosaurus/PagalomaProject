using MainGame.Menus;
using UnityEngine;

namespace MainGame.PlayerScripts.Roles
{
    public class DetectCollision : MonoBehaviour
    {
        [SerializeField] private Werewolf _werewolf;
        [SerializeField] private Seer _seer;
    
        private void OnTriggerEnter(Collider other)
        {
            if (!VoteMenu.Instance.isDay && !VoteMenu.Instance.isFirstDay)
            {
                if (_werewolf.isActive && _werewolf.isAlive) _werewolf.UpdateTarget(other, true);
                if (_seer.isActive && _seer.isAlive) _seer.UpdateTarget(other, true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!VoteMenu.Instance.isDay && !VoteMenu.Instance.isFirstDay)
            {
                if (_werewolf.isActive && _werewolf.isAlive) _werewolf.UpdateTarget(other, false);
                if (_seer.isActive && _seer.isAlive) _seer.UpdateTarget(other, false);
            }
        }
    }
}
