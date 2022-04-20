using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Diagnostics.Tracing;
using Photon.Pun;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public static PlayerInteraction Instance;
    [SerializeField] private bool nearDoor = false;
    [SerializeField] private bool nearSign = false;
    public GameObject door;
    [SerializeField] private PhotonView PV;

    private void Awake()
    {
        if (PV.IsMine) Instance = this;
    }

    void Update()
    {
        if (nearDoor && Input.GetKeyDown(KeyCode.E)) // Should be moved to Click()
        {
            RPC_OpenCloseDoor(door.transform.name);
            PV.RPC("RPC_OpenCloseDoor", RpcTarget.Others, door.transform.name);
        }
    }

    public void NearDoor(GameObject message, GameObject door, bool nearDoor)
    {
        this.nearDoor = nearDoor;
        this.door = door;
        message.SetActive(nearDoor);
    }
    
    public void NearSign(bool nearSign)
    {
        this.nearSign = nearSign;
    }

    public void Click()
    {
        if (nearSign)
        {
            Debug.Log("[+] Should open voting screen");
            IGMenuManager.Instance.OpenVoteMenu();
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