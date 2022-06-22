using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class PlayerListItem : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text text;
    private Player _player;

    public void SetUp(Player player)
    {
        this._player = player;
        text.text = this._player.NickName;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (Equals(_player, otherPlayer))
        {
            Destroy(gameObject);
        }
    }

    public override void OnLeftRoom()
    {
        Destroy(gameObject);
    }
}
    
