using System.Collections.Generic;
using System.Linq;
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

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            try
            {
                AddDestination(RoomManager.Instance.localPlayer.transform);
            }
            catch
            {
                Debug.LogWarning("no roomManager !",  this);
            }
        }

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
                
                _agent.SetDestination(destination);
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
