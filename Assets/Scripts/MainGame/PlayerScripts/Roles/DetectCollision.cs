using MainGame;
using MainGame.PlayerScripts.Roles;
using UnityEngine;

public class DetectCollision : MonoBehaviour
{
    [SerializeField] private Werewolf werewolf;
    [SerializeField] private Seer seer;
    [SerializeField] private Priest priest;

    private void OnTriggerEnter(Collider other)
    {
        if (VoteMenu.Instance != null)
            if (!VoteMenu.Instance.isDay && !VoteMenu.Instance.isFirstDay)
            {
                if (werewolf.isActive && werewolf.isAlive) werewolf.UpdateTarget(other, true);
                if (seer.isActive && seer.isAlive) seer.UpdateTarget(other, true);
                if (priest.isActive && priest.isAlive) priest.UpdateTarget(other, true);
            }
    }

    private void OnTriggerExit(Collider other)
    {
        if (VoteMenu.Instance != null)
            if (!VoteMenu.Instance.isDay && !VoteMenu.Instance.isFirstDay)
            {
                if (werewolf.isActive && werewolf.isAlive) werewolf.UpdateTarget(other, false);
                if (seer.isActive && seer.isAlive) seer.UpdateTarget(other, false);
                if (priest.isActive && priest.isAlive) priest.UpdateTarget(other, false);
            }
    }
}