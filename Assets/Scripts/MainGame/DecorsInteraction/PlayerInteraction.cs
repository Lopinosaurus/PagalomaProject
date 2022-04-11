using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Diagnostics.Tracing;
using Photon.Pun;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{

    [SerializeField] private bool nearDoor = false;
    [SerializeField] private bool nearSign = false;
    public GameObject door;
    public GameObject sign;
    [SerializeField] private PhotonView PV;

    // Update is called once per frame
    void Update()
    {
        if (nearDoor && Input.GetKeyDown(KeyCode.E))
        {
            RPC_OpenCloseDoor(door.transform.name);
            PV.RPC("RPC_OpenCloseDoor", RpcTarget.Others, door.transform.name);
        }

        if (nearSign && Input.GetMouseButtonDown(0))
        {
            Debug.Log("[+] Should open voting screen");
        }
    }

    public void NearDoor(GameObject message, GameObject door, bool nearDoor)
    {
        this.nearDoor = nearDoor;
        this.door = door;
        message.SetActive(nearDoor);
    }
    
    public void NearSign(GameObject sign, bool nearSign)
    {
        this.nearSign = nearSign;
        this.sign = sign;
    }

    [PunRPC]
    public void RPC_OpenCloseDoor(string doorId)
    {
        GameObject theDoor = GameObject.Find(doorId);
        Animator anim = theDoor.transform.GetComponent<Animator>();
        anim.SetBool("opening",!anim.GetBool("opening") );
    }

}
