using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MainGame.PlayerScripts.Roles;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;

namespace MainGame.PlayerScripts
{
    public class FakePlayerController : MonoBehaviour
    {
        // Components
        private NavMeshAgent _agent;
        private FakePlayerAnimation _fakePlayerAnimation;
    
        // Ai behaviour
        [SerializeField] private Transform target;
        [SerializeField] private List<Transform> positions;
        private FakePlayerController _fakePlayerController;
        private SkinnedMeshRenderer _fakePlayerRenderer;
        private PhotonView _photonView;

        private void Awake()
        {
            _photonView = GetComponent<PhotonView>();
            _fakePlayerRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            _fakePlayerController = GetComponent<FakePlayerController>();
            _agent = GetComponent<NavMeshAgent>();

            SetColor(Color.white);

            // Set destinations
            Map.FindVillage();
            
            if (!_photonView.IsMine)
            {
                Destroy(_fakePlayerController);
                Destroy(_fakePlayerAnimation);
            }
        }

        private IEnumerator GetLocalPlayer()
        {
            WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

            while (RoomManager.Instance && !target)
            {
                try
                {
                    target = RoomManager.Instance.localPlayer.transform;
                }
                catch{ // ignore
                };
                yield return waitForFixedUpdate;
            }
        }

        private void SetColor(Color color) => _fakePlayerRenderer.materials[1].color = color;

        private void FixedUpdate()
        {
            // Gets new path
            if (false)
            {
                if (GetNextDestination(out Transform nextDestination))
                {
                    target = nextDestination;
                }
            }
            
            // Sets destination
            if (target)
            {
                Vector3 destination = target.position;
                
                if (_agent.destination != destination) _agent.SetDestination(destination);
            }
        }

        private void AddDestination(Transform position) => positions.Add(position);
        private void RemoveDestination(Transform position) => positions.Remove(position);

        private bool GetNextDestination(out Transform nextDestination)
        {
            nextDestination = positions.LastOrDefault();
            return nextDestination;
        }
    }
}
