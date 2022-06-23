using Photon.Pun;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public static PlayerInteraction Instance;
    [SerializeField] private bool nearDoor;
    [SerializeField] private bool nearSign;
    public GameObject door;
    [SerializeField] private PhotonView pv;
    private static readonly int OpeningHash = Animator.StringToHash("opening");

    public PlayerInteraction() => nearSign = false;

    private void Awake()
    {
        if (pv.IsMine) Instance = this;
    }

    public void NearDoor(GameObject message, GameObject door, bool nearDoor)
    {
        if (pv.IsMine)
        {
            this.door = door;
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
        if (pv.IsMine || null == RoomManager.Instance)
        {
            //Debug.Log("PV is mine");
            if (nearDoor)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    RPC_OpenCloseDoor(door.transform.name);
                    pv.RPC(nameof(RPC_OpenCloseDoor), RpcTarget.Others, door.transform.name);
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
