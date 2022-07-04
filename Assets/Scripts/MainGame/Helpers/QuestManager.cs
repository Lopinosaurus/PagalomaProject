using System;
using System.Linq;
using MainGame.PlayerScripts.Roles;
using Photon.Pun;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MainGame.Helpers
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance;

        private PhotonView _photonView;
        
        public enum Quest
        {
            None,
            CollectHallows,
            FollowPlayer,
            LightLanterns
        }

        private void Awake()
        {
            if (Instance)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            _photonView = GetComponent<PhotonView>();
        }

        
        public void AssignQuests()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                foreach (var player in RoomManager.Instance.players)
                {
                    // Gives a new quest to each villager
                    if (!player.isAlive) continue;
                    if (player is Werewolf) continue;

                    Quest newQuest = GetNewQuest(player.LastQuest);
                    _photonView.RPC(nameof(RPC_AssignQuest), RpcTarget.AllBuffered, player.userId, newQuest);
                }
            }
        }

        private static Quest GetNewQuest(Quest lastQuestToIgnore)
        {
            var possibleQuests = Enum.GetValues(typeof(Quest)).Cast<Quest>().Where(q => q != Quest.None && q != lastQuestToIgnore).ToArray();
            return possibleQuests[Random.Range(0, possibleQuests.Length)];
        }

        [PunRPC]
        private void RPC_AssignQuest(string userID, int quest) => RoomManager.Instance.players.Find(p => p.userId == userID).CurrentQuest = (Quest)quest;
    }

    
}