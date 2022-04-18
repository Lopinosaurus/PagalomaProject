using System;
using JetBrains.Annotations;
using MainGame.Menus;
using MainGame.PlayerScripts.Roles;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

namespace MainGame
{
    public class VoteMenu : MonoBehaviourPunCallbacks
    {
        public static VoteMenu Instance;
        public Transform voteList;
        [SerializeField] private VoteListItem voteListItem;
        [SerializeField] private Button voteButton;
        [SerializeField] private PhotonView PV;

        public void Awake()
        {
            Instance = this;
            PV = GetComponent<PhotonView>();
        }

        // Add player to vote list
        public void Add(Role player)
        {
            Instantiate(voteListItem, voteList).GetComponent<VoteListItem>().SetUp(player);
        }
        
        public void UpdateVoteItems()
        {
            foreach (Transform trans in voteList)
            {
                trans.GetComponent<VoteListItem>().UpdateItem();
            }
        }

        public void SubmitVote()
        {
            voteButton.interactable = false;
            Role votedPlayer = RoomManager.Instance.localPlayer.vote;
            RoomManager.Instance.votes.Add(votedPlayer);
            if (votedPlayer != null) PV.RPC("RPC_SubmitVote", RpcTarget.Others, votedPlayer.userId);
            else PV.RPC("RPC_SubmitVote", RpcTarget.Others, null);
        }
        
        [PunRPC]
        void RPC_SubmitVote([CanBeNull] string userId)
        {
            if (userId != null)
            {
                foreach (Role player in RoomManager.Instance.players)
                {
                    if (player.userId == userId) RoomManager.Instance.votes.Add(player);
                }
            }
            else
            {
                RoomManager.Instance.votes.Add(null);
            }
        }
    }
}