using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class BackToMain : MonoBehaviour
{
    public void BackAndDisconnect()
    {
        PhotonNetwork.Disconnect();
    }
}
