using System;
using Photon.Pun;
using UnityEngine;

namespace MainGame.Helpers
{
    public class Item : MonoBehaviour
    {
        private SphereCollider _collider;
        private PhotonView _photonView;

        private void Awake()
        {
            _photonView = GetComponent<PhotonView>();
            _collider = GetComponent<SphereCollider>();
        }
        
        
    }
}