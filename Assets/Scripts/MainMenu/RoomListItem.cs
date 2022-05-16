using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

namespace MainMenu
{
    public class RoomListItem : MonoBehaviourPunCallbacks
    {
        [SerializeField] private TMP_Text text;
        public RoomInfo info;
    
        public void SetUp(RoomInfo _info)
        {
            info = _info;
            text.text = _info.Name;
        }

        public void OnClick()
        {
            Launcher.Instance.JoinRoom(info);
        }
    }
}
