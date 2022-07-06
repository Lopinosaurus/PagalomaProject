using MainGame.PlayerScripts.Roles;
using TMPro;
using UnityEngine;

namespace MainGame.PlayerScripts
{
    public class TabMenuItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerName;
        private Role _player;
        private readonly Color _werefolfColor = new Color(0.72f, 0.01f, 0f);
    
        public void SetUp(Role player)
        {
            playerName.text = player.username;
            if (RoomManager.Instance.localPlayer is Werewolf && player is Werewolf) 
                playerName.color = _werefolfColor;
        }

        public void UpdateItem()
        {
            if (RoomManager.Instance.localPlayer is Werewolf && _player is Werewolf) playerName.color = _werefolfColor;
        }
    }
}
