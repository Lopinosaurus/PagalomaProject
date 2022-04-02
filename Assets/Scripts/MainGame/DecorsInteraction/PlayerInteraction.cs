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
    public GameObject door;
    [SerializeField] private PhotonView PV;

    // Update is called once per frame
    void Update()
    {
        if (nearDoor && Input.GetKeyDown(KeyCode.E))
        {
            RPC_OpenCloseDoor(door.transform.name);
            PV.RPC("RPC_OpenCloseDoor", RpcTarget.Others, door.transform.name);
        }
    }

    public void NearDoor(GameObject message, GameObject theDoor)
    {
        nearDoor = true;
        this.door = theDoor;
        message.SetActive(true);
    }

    public void FarDoor(GameObject message, GameObject theDoor)
    {
        nearDoor = false;
        this.door = theDoor;
        message.SetActive(false);
    }
    
    [PunRPC]
    public void RPC_OpenCloseDoor(string doorId)
    {
        GameObject theDoor = GameObject.Find(doorId);
        Animator anim = theDoor.transform.GetComponent<Animator>();
        anim.SetBool("opening",!anim.GetBool("opening") );
    }

}
