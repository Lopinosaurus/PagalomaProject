using System.Collections.Generic;
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
            GoVote,
            KillVillagers,
            CollectHallows,
            FollowPlayer,
            LightLanterns
        }

        private static readonly IEnumerable<Quest> VillagerRegularQuests = new[] {Quest.CollectHallows, Quest.FollowPlayer, Quest.LightLanterns};

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
            if (!PhotonNetwork.IsMasterClient) return;
            
            // Gives a new quest to each villager
            foreach (var player in RoomManager.Instance.players)
            {
                Quest newQuest = player switch {
                    {isAlive: false} => Quest.None,
                    Werewolf => Quest.KillVillagers,
                    _ => GetNewQuest(player.LastQuest)
                };
                
                SetQuest(newQuest, player);
            }
        }

        private static Quest GetNewQuest(Quest lastQuestToIgnore)
        {
            Quest newQuest;

            if (VoteMenu.Instance.IsNight)
            {
                var possibleQuests = VillagerRegularQuests.Where(q => q != lastQuestToIgnore).ToArray();
                newQuest = possibleQuests[Random.Range(0, possibleQuests.Length)];
            }
            else
            {
                newQuest = Quest.GoVote;
            }

            return newQuest;
        }

        private void SetQuest(Quest quest, Role player) => _photonView.RPC(nameof(RPC_AssignQuest), RpcTarget.AllBuffered, player.userId, quest);

        [PunRPC]
        private void RPC_AssignQuest(string userID, int quest) => RoomManager.Instance.players.Find(p => p.userId == userID).CurrentQuest = (Quest)quest;
    }

    
}