using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Diagnostics.Tracing;
using MainGame.PlayerScripts;
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
    private static readonly int _openingHash = Animator.StringToHash("opening");

    public PlayerInteraction() => nearSign = false;

    private void Awake()
    {
        if (PV.IsMine) Instance = this;
    }

    public void NearDoor(GameObject message, GameObject door, bool nearDoor)
    {
        if (PV.IsMine)
        {
            Debug.Log($"nearDoor = {nearDoor}");
            this.door = door;
            this.nearDoor = nearDoor;
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
        
        if (PV.IsMine && nearDoor && Input.GetMouseButtonDown(0))
        {
            RPC_OpenCloseDoor(door.transform.name);
            PV.RPC(nameof(RPC_OpenCloseDoor), RpcTarget.Others, door.transform.name);
        }
    }

    [PunRPC]
    private void RPC_OpenCloseDoor(string doorId)
    {
        GameObject theDoor = GameObject.Find(doorId);
        Animator anim = theDoor.transform.GetComponent<Animator>();
        anim.SetBool(_openingHash, !anim.GetBool(_openingHash));
        
        Debug.Log($"isOpen is {anim.GetBool(_openingHash)}");
    }

}
