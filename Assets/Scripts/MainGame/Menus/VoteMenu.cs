using System;
using MainGame.Menus;
using MainGame.PlayerScripts.Roles;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace MainGame
{
    public class VoteMenu : MonoBehaviourPunCallbacks
    {
        public static VoteMenu Instance;
        public Transform voteList;
        [SerializeField] private VoteListItem voteListItem;

        public void Awake()
        {
            Instance = this;
        }

        public void SetUp()
        {
            Clear();
            // Instantiate a voteListItem for each player
            foreach (Role player in RoomManager.Instance.players)
            {
                Instantiate(voteListItem, voteList).GetComponent<VoteListItem>().SetUp(player);
            }
        }
        
        public void UpdateVoteItems()
        {
            // Update VoteListItems
            foreach (Transform trans in voteList)
            {
                trans.GetComponent<VoteListItem>().UpdateItem();
            }
        }

        // Clear VoteList
        public void Clear()
        {
            foreach (Transform trans in voteList)
            {
                Destroy(trans);
            }
        }
    }
}