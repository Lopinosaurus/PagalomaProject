﻿using MainGame.Menus;
using MainGame.PlayerScripts.Roles;
using Photon.Pun;
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
        [SerializeField] private PhotonView pv;
        public bool isDay;
        public bool isFirstDay;
        public bool IsNight => !isDay && !isFirstDay;

        public void Awake()
        {
            Instance = this;
            isFirstDay = true;
            isDay = false;
            voteButton.interactable = isDay;
        }

        // Add player to vote list
        public void Add(Role player)
        {
            Instantiate(voteListItem, voteList).GetComponent<VoteListItem>().SetUp(player);
            UpdateVoteItems();
        }
        
        public void UpdateVoteItems()
        {
            if (RoomManager.Instance.localPlayer.vote != null && !RoomManager.Instance.localPlayer.vote.isAlive)
                RoomManager.Instance.localPlayer.vote = null;
            voteButton.interactable = isDay && !RoomManager.Instance.localPlayer.hasVoted;
            foreach (Transform trans in voteList)
            {
                trans.GetComponent<VoteListItem>().UpdateItem();
            }
        }

        public void SubmitVote()
        {
            voteButton.interactable = false;
            RoomManager.Instance.localPlayer.hasVoted = true;
            Role votedPlayer = RoomManager.Instance.localPlayer.vote;
            RoomManager.Instance.votes.Add(votedPlayer);
            // if (votedPlayer != null) PV.RPC(nameof(RPC_SubmitVote), RpcTarget.Others, votedPlayer.userId);
            // else PV.RPC(nameof(RPC_SubmitVote), RpcTarget.Others, "");
            pv.RPC(nameof(RPC_SubmitVote), RpcTarget.Others,
                votedPlayer != null
                    ? votedPlayer.userId
                    : "");
        }

        public void KillVotedPlayer(string userId) // Only MasterClient has access to this method
        {
            pv.RPC(nameof(RPC_KillVotedPlayer), RpcTarget.All, userId);
        }
        private static void KillVotedPlayer(Role player)
        {
            string message;
            if (player == null) message = "Nobody was eliminated today";
            else
            {
                player.Die();
                message = $"A {player.username} ({player.roleName}) has been eliminated, RIP";
            }
            Debug.Log(message);
            RoomManager.Instance.UpdateInfoText(message);
        }
        
        [PunRPC]
        private void RPC_SubmitVote(string userId)
        {
            if (userId != "")
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

        [PunRPC]
        void RPC_KillVotedPlayer(string userId)
        {
            Role votedPlayer = null;
            foreach (Role player in RoomManager.Instance.players)
            {
                if (player.userId == userId) votedPlayer = player;
            }
            KillVotedPlayer(votedPlayer);
        }
    }
}