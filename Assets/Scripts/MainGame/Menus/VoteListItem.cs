using MainGame.PlayerScripts.Roles;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MainGame.Menus
{
    public class VoteListItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private Image colorSquare;
        [SerializeField] private Image background;
        private Role _player;

        private readonly Color _notClicked = new Color32(58, 58, 58, 255);
        private readonly Color _clicked = new Color32(99,128,178,255);
        private readonly Color _werewolfColor = new Color(0.72f, 0.01f, 0f);
    
        public void SetUp(Role player)
        {
            _player = player;
            text.text = player.username;
            if (RoomManager.Instance.localPlayer is Werewolf && _player is Werewolf) text.color = _werewolfColor;
            colorSquare.color = player.color;
            background.color = _notClicked;
        }

        public void OnClick()
        {
            if (_player.isAlive && VoteMenu.Instance.isDay)
            {
                if (RoomManager.Instance.localPlayer.vote == _player)
                {
                    RoomManager.Instance.localPlayer.vote = null;
                    background.color = _notClicked;
                } else
                {
                    RoomManager.Instance.localPlayer.vote = _player;
                    background.color = _clicked;
                }
                VoteMenu.Instance.UpdateVoteItems();
            }
        }

        public void UpdateItem()
        {
            if (RoomManager.Instance.localPlayer is Werewolf && _player is Werewolf) text.color = _werewolfColor;
            // Update clicked state (background color)
            if (_player.isAlive)
            {
                if (RoomManager.Instance.localPlayer.vote == _player && VoteMenu.Instance.isDay)
                {
                    background.color = _clicked;
                }
                else
                {
                    background.color = _notClicked;
                }
            }
            else // Item player is dead
            {
                background.color = _notClicked;
                text.fontStyle = FontStyles.Strikethrough;
            }
        }
    }
}
