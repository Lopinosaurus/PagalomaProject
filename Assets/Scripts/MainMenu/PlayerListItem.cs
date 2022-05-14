using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

namespace MainMenu
{
    public class PlayerListItem : MonoBehaviourPunCallbacks
    {
        [SerializeField] private TMP_Text text;
        private Player player;

        public void SetUp(Player _player)
        {
            player = _player;
            text.text = player.NickName;
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            if (player == otherPlayer)
            {
                Destroy(gameObject);
            }
        }

        public override void OnLeftRoom()
        {
            Destroy(gameObject);
        }
    }
}
    
