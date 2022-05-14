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
<<<<<<< Updated upstream
            if (_werewolf.isActive) _werewolf.UpdateTarget(other, true);
            if (_seer.isActive) _seer.UpdateTarget(other, true);
=======
            if (!VoteMenu.Instance.isDay && !VoteMenu.Instance.isFirstDay)
            {
                if (_werewolf.isActive && _werewolf.isAlive) _werewolf.UpdateTarget(other, true);
                if (_seer.isActive && _seer.isAlive) _seer.UpdateTarget(other, true);
            }
>>>>>>> Stashed changes
        }

        private void OnTriggerExit(Collider other)
        {
<<<<<<< Updated upstream
            if (_werewolf.isActive) _werewolf.UpdateTarget(other, false);
            if (_seer.isActive) _seer.UpdateTarget(other, false);
=======
            if (!VoteMenu.Instance.isDay && !VoteMenu.Instance.isFirstDay)
            {
                if (_werewolf.isActive && _werewolf.isAlive) _werewolf.UpdateTarget(other, false);
                if (_seer.isActive && _seer.isAlive) _seer.UpdateTarget(other, false);
            }
>>>>>>> Stashed changes
        }
    }
}
