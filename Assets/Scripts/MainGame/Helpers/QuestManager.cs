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

                    player.CurrentQuest = GetNewQuest(player.LastQuest);
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