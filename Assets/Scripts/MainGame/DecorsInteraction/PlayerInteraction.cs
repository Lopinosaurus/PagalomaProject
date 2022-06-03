using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Diagnostics.Tracing;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public static PlayerInteraction Instance;
    [SerializeField] private bool nearDoor;
    [SerializeField] private bool nearSign;
    public GameObject door;
    [SerializeField] private PhotonView PV;

    public PlayerInteraction() => nearSign = false;

    private void Awake()
    {
        if (PV.IsMine) Instance = this;
    }

    public void NearDoor(GameObject message, GameObject door, bool nearDoor)
    {
        if (PV.IsMine)
        {
            this.nearDoor = nearDoor;
            this.door = door;
            message.SetActive(nearDoor);
        }
    }
    
    public void NearSign(bool nearSign) => this.nearSign = nearSign;

    public void Click()
    {
        if (nearSign)
        {
            Debug.Log("[+] Should open voting screen");
            IGMenuManager.Instance.OpenVoteMenu();
        }

        if (nearDoor)
        {
            RPC_OpenCloseDoor(door.transform.name);
            PV.RPC("RPC_OpenCloseDoor", RpcTarget.All, door.transform.name);
        }
    }

    [PunRPC]
    public void RPC_OpenCloseDoor(string doorId)
    {
        GameObject theDoor = GameObject.Find(doorId);
        Animator anim = theDoor.transform.GetComponent<Animator>();
        anim.SetBool("opening",!anim.GetBool("opening") );
    }

}
