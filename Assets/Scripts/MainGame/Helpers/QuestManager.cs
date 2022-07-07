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
            IgniteLanterns
        }

        private Dictionary<Quest, string> _friendlyQuestHeader;

        private static readonly IEnumerable<Quest> VillagerRegularQuests = new[] {Quest.CollectHallows, Quest.FollowPlayer, Quest.IgniteLanterns};

        private void Awake()
        {
            if (Instance)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            _photonView = GetComponent<PhotonView>();

            _friendlyQuestHeader = new Dictionary<Quest, string> {
                {Quest.None, "prepare for the next night"},
                {Quest.GoVote, "get back at the village and cast your vote"},
                {Quest.KillVillagers, "kill the villagers and don't get spotted"},
                {Quest.CollectHallows, "collect the hallows scattered around"},
                {Quest.FollowPlayer, "follow the villager"},
                {Quest.IgniteLanterns, "Ignite the lanterns scattered around"}};
        }


        public void AssignQuests()
        {
            if (!PhotonNetwork.IsMasterClient) return;
            
            // Gives a new quest to each villager
            foreach (var player in RoomManager.Instance.players)
            {
                Quest newQuest = player switch {
                    {isAlive: false} => Quest.None,
                    _ => GetNewQuest(player.LastQuest, player)
                };
                
                SetQuest(player, newQuest);
            }
        }

        private static Quest GetNewQuest(Quest lastQuestToIgnore, Role player)
        {
            Quest newQuest;

            if (VoteMenu.Instance.IsNight) {
                if (player is Werewolf) {
                    newQuest = Quest.KillVillagers;
                }
                else {
                    var possibleQuests = VillagerRegularQuests.Where(q => q != lastQuestToIgnore).ToArray();
                    newQuest = possibleQuests[Random.Range(0, possibleQuests.Length)];
                }
            }
            else {
                newQuest = Quest.GoVote;
            }

            return newQuest;
        }

        private void SetQuest(Role player, Quest newQuest)
        {
            _photonView.RPC(nameof(RPC_AssignQuest), RpcTarget.AllBuffered, player.userId, newQuest);
        }

        private void UpdateQuestText(Quest newQuest) => RoomManager.Instance.questHeaderText.text = _friendlyQuestHeader[newQuest];

        [PunRPC]
        private void RPC_AssignQuest(string userID, Quest quest)
        {
            RoomManager.Instance.players.Find(p => p.userId == userID).CurrentQuest = quest;
            UpdateQuestText(quest);
        }
    }

    
}