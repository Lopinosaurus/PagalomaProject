using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

public class FakePlayerController : MonoBehaviour
{
    // Components
    private NavMeshAgent _agent;
    private PhotonView _photonView;
    
    // Ai behaviour
    [SerializeField] private Transform target;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _photonView = GetComponent<PhotonView>();
    }

    private void FixedUpdate()
    {
        if (target)
        {
            _agent.SetDestination(target.position);
        }
        else
        {
            try
            {
                target = RoomManager.Instance.localPlayer.transform;
            }
            catch
            {
                // ignore
            }
        }
    }
}
