using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

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
                    
                }
            }
        }

        [PunRPC]
        private void AssignQuest(string userID, int quest) => RoomManager.Instance.players.Find(p => p.userId == userID).CurrentQuest = (Quest)quest;
    }

    
}