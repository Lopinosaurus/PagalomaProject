using Photon.Pun;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public static PlayerInteraction Instance;
    private PhotonView _photonView;
    [SerializeField] private bool nearDoor;
    [SerializeField] private bool nearSign;
    private GameObject _door;
    private static readonly int OpeningHash = Animator.StringToHash("opening");

    public PlayerInteraction() => nearSign = false;

    private void Awake()
    {
        _photonView = GetComponent<PhotonView>();
        if (_photonView.IsMine) Instance = this;
    }

    public void NearDoor(GameObject message, GameObject door, bool nearDoor)
    {
        if (_photonView.IsMine)
        {
            this._door = door;
            this.nearDoor = nearDoor;
            message.SetActive(this.nearDoor);
        }
    }
    
    public void NearSign(bool nearSign) => this.nearSign = nearSign;

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
        if (_photonView.IsMine || !RoomManager.Instance)
        {
            //Debug.Log("PV is mine");
            if (nearDoor)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    RPC_OpenCloseDoor(_door.transform.name);
                    _photonView.RPC(nameof(RPC_OpenCloseDoor), RpcTarget.Others, _door.transform.name);
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
