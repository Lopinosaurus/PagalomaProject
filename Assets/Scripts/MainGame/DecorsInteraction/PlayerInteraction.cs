using MainGame;
using MainGame.PlayerScripts.Roles;
using Photon.Pun;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public static PlayerInteraction Instance;
    [SerializeField] private bool nearDoor;
    [SerializeField] private bool nearSign;
    private GameObject _door;
    private PlayerController _playerController;
    
    private static readonly int OpeningHash = Animator.StringToHash("opening");

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
        
        if (_playerController.photonView.IsMine) Instance = this;
    }

    public void NearDoor(GameObject message, GameObject door, bool nearDoor)
    {
        if (!_playerController.photonView.IsMine) return;
        _door = door;
        this.nearDoor = nearDoor;
        message.SetActive(this.nearDoor);
    }
    
    public void NearSign(bool nearSign)
    {
        if (!_playerController.photonView.IsMine || !_playerController.role.isAlive) return;

        this.nearSign = nearSign;
    }

    public void Click()
    {
        if (nearSign)
        {
            Debug.Log("[+] Should open voting screen");
            MainGameMenuManager.Instance.OpenVoteMenu();
        }
    }

    private void Update()
    {
        if (_playerController.photonView.IsMine || !RoomManager.Instance)
        {
            //Debug.Log("PV is mine");
            if (nearDoor)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    RPC_OpenCloseDoor(_door.transform.name);
                    _playerController.photonView.RPC(nameof(RPC_OpenCloseDoor), RpcTarget.Others, _door.transform.name);
                }
            }
        }
    }

    [PunRPC]
    private void RPC_OpenCloseDoor(string doorId)
    {
        GameObject theDoor = GameObject.Find(doorId);
        Animator anim = theDoor.transform.GetComponent<Animator>();
        anim.SetBool(OpeningHash, !anim.GetBool(OpeningHash));
    }
}
