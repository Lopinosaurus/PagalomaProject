using UnityEngine;
using Photon.Realtime;
using TMPro;
using Photon.Pun;

public class RoomListItem : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text text;
    public RoomInfo Info;
    
    public void SetUp(RoomInfo info)
    {
        Info = info;
        text.text = info.Name;
    }

    public void OnClick()
    {
        Launcher.Instance.JoinRoom(Info);
    }
}
