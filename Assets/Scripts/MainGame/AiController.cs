using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MainGame.PlayerScripts;
using UnityEngine;
using UnityEngine.AI;
using static System.Single;
using Random = UnityEngine.Random;

// ReSharper disable OperatorIsCanBeUsed

namespace MainGame
{
    public class AiController : MonoBehaviour
    {
        [SerializeField] private GameObject dissimulateParticles;
        public Role targetRole;
        private GameObject targetPlayer;
        private Camera targetCam;
        private Plane[] targetPlanes;

        [SerializeField] private NavMeshAgent _navMeshAgent;
        [SerializeField] private CapsuleCollider _capsuleCollider;

        [SerializeField] private Collider previousCollider;
        [SerializeField] private Collider currentHidingObstacle;

        private bool isViewed { get; set; }
        private bool isInCameraPlane { get; set; }

        private enum AiState
        {
            Hidden,
            Transition,
            Caught,
            Attack
        }

        // Insert the LayerMask corresponding the player 
        [SerializeField] private LayerMask ignoreCharacter;
        private int layerMaskValue;

        // Gameplay stats
        [Space] [Header("Gameplay statistics")] [SerializeField]
        private AiState currentState = AiState.Hidden;

        private bool isAlive = true;
        public int MoveCount;

        [SerializeField] private float TimeBeforeTransition = cycleTime;
        [SerializeField] private float timeBeingCaught;
        [SerializeField] private int remainingHealth = 3;
        private float timeSinceChase;
        private float distanceFromTarget => Vector3.Distance(transform.position, targetPlayer.transform.position);

        private const float cycleTime = 5;
        private const float maxBeingCaughtDelay = 0.8f;
        private const float maxWaitingTimeBeforeMoving = -3;

        private const float moveCountMaxBeforeAttacking = 10;
        private const float stoppingDistance = 0.5f;
        private const float attackPlayerDistance = 2.5f;
        private const float maxChaseTime = 15;
        private const float distanceFromObjectMinTreshold = 8;

        // Spawn settings
        [Space] [Header("Spawn distances")] public float minSpawnRange = 30f;
        public float maxSpawnRange = 40f;

        // NavMeshAgent settings
        [Space] [Header("Nav Mesh Settings")] [Range(0.01f, 100f)]
        public float regularSpeed = 20f;

        [Range(0.01f, 100f)] public float maxSpeed = 9999;

        private const float acceleration = 100f;

        private void Start()
        {
            targetPlayer = targetRole.gameObject;

            previousCollider = _capsuleCollider;
            currentHidingObstacle = _capsuleCollider;
            layerMaskValue = (int)(Mathf.Log(ignoreCharacter.value) / Mathf.Log(2));
            
            TimeBeforeTransition = cycleTime;

            // NavMesh settings
            _navMeshAgent.speed = regularSpeed;
            _navMeshAgent.acceleration = acceleration;
            _navMeshAgent.stoppingDistance = stoppingDistance;

            // Bake NavMesh
            // NavMeshBuilder.BuildNavMesh();

            HideFarBehindPlayer(minSpawnRange, maxSpawnRange, true, true);

            targetCam = targetPlayer.GetComponentInChildren<Camera>();
        }

        private void Update()
        {
            isViewed = IsInCameraView();

            // Rotates the Ai if the player cannot see it or if in transition
            if (AiState.Transition == currentState || isViewed)
            {
                // Rotates towards the player but only on the Y-axis
                Transform _transform = transform;
                _transform.LookAt(targetPlayer.transform.position);
                Vector3 eulerAngles = _transform.rotation.eulerAngles;
                eulerAngles.x = 0;
                transform.rotation = Quaternion.Euler(eulerAngles);
            }

            if (isViewed)
            {
                // Gives invulnerability to Ai if got caught before
                if (timeBeingCaught >= maxBeingCaughtDelay && TimeBeforeTransition <= cycleTime)
                {
                    SetCurrentState(AiState.Caught);
                }

                // Only in transition mode
                if (AiState.Transition == currentState)
                {
                    timeBeingCaught += Time.deltaTime;
                }
            }
        }

        private void OnMouseDown()
        {
            Debug.DrawLine(currentHidingObstacle.transform.position, Vector3.up * 3, Color.red, 0.1f);
        }

        private void FixedUpdate()
        {
            switch (currentState)
            {
                case AiState.Hidden:
                    // Tries to stay hidden
                    EnableMovement(3);
                    _navMeshAgent.SetDestination(FindHidingSpot(true));
                    timeBeingCaught = 0;

                    // Moves far behind if waited for too long
                    if (TimeBeforeTransition < maxWaitingTimeBeforeMoving)
                    {
                        // Moves
                        MoveCount++;
                        EnableMovement(2);
                        HideFarBehindPlayer(minSpawnRange, maxSpawnRange, true);

                        // Sets time so that if will move immediately after
                        TimeBeforeTransition = 0.01f;
                    }

                    // Looks for a new hiding place and moves normally
                    else if (TimeBeforeTransition <= 0)
                    {
                        if (!isInCameraPlane)
                        {
                            // Moves
                            MoveCount++;
                            EnableMovement(2);
                            _navMeshAgent.SetDestination(FindHidingSpot(false));

                            // Sets new state
                            SetCurrentState(AiState.Transition);
                        }
                    }

                    // Attacks !
                    if (TimeBeforeTransition <= 0 && MoveCount >= moveCountMaxBeforeAttacking)
                    {
                        // Moves
                        EnableMovement(1);
                        _navMeshAgent.SetDestination(targetPlayer.transform.position);

                        // Sets new state
                        SetCurrentState(AiState.Attack);
                    }

                    TimeBeforeTransition -= Time.fixedDeltaTime;

                    break;

                case AiState.Transition:
                    // Locks the timer while moving
                    TimeBeforeTransition = cycleTime;

                    // Detects when to stop
                    if (_navMeshAgent.remainingDistance < attackPlayerDistance)
                    {
                        SetCurrentState(AiState.Hidden);
                    }
                    else
                    {
                        _navMeshAgent.SetDestination(FindHidingSpot(true));
                    }

                    break;
                case AiState.Caught when isAlive:
                    // Freezes the Ai
                    EnableMovement(0);

                    // Removes health points
                    remainingHealth--;

                    // Resets the move count
                    MoveCount = 0;

                    // Resets the timeBeingCaught value
                    timeBeingCaught = 0;

                    // Time to die ?
                    if (remainingHealth <= 0)
                    {
                        StartCoroutine(AiCaught(true));
                    }
                    else
                    {
                        StartCoroutine(AiCaught(false));

                        // Moves the Ai away
                        HideFarBehindPlayer(minSpawnRange, maxSpawnRange);

                        // Sets new state
                        SetCurrentState(AiState.Hidden);

                        // Sets timer longer than usual
                        TimeBeforeTransition = cycleTime * 1.5f;
                    }

                    break;

                case AiState.Attack when isAlive:
                    // Updates the destination to the player's position
                    _navMeshAgent.SetDestination(targetPlayer.transform.position);

                    timeSinceChase += Time.fixedDeltaTime;
                    
                    // Applies the malus and then dies
                    if (_navMeshAgent.remainingDistance < attackPlayerDistance
                        || timeSinceChase > maxChaseTime)
                    {
                        StartCoroutine(AiCaught(true));
                        ApplyMalus();
                    }

                    break;
            }
        }

        private void ApplyMalus()
        {
            targetPlayer.GetComponent<PlayerLook>().NauseaCam(15);
            targetPlayer.GetComponent<PlayerMovement>().SlowSpeed(15);
        }

        private void SetCurrentState(AiState newState)
        {
            currentState = newState;
        }

        private bool IsInCameraView()
        {
            // Refreshes the camera planes
            targetPlanes = GeometryUtility.CalculateFrustumPlanes(targetCam);

            if (GeometryUtility.TestPlanesAABB(targetPlanes, _capsuleCollider.bounds))
            {
                isInCameraPlane = true;

                Vector3 camPosition = targetCam.transform.position;

                float colliderHeight = _capsuleCollider.height;
                Vector3 ColliderPosition = _capsuleCollider.transform.position;
                Vector3 colliderCenter = ColliderPosition + Vector3.up * colliderHeight / 2;

                // All possible destinations - the more the more accurate
                Vector3[] destinations =
                {
                    colliderCenter,
                    ColliderPosition,
                    ColliderPosition + Vector3.up * colliderHeight
                };

                foreach (Vector3 destination in destinations)
                    // Ray from camera to the chosen destination
                    if (Physics.Raycast(camPosition, destination - camPosition, out RaycastHit hit,
                            PositiveInfinity))
                        if (hit.collider == _capsuleCollider)
                            return true;
            }
            else
            {
                isInCameraPlane = false;
            }

            return false;
        }

        private void EnableMovement(int speed)
        {
            switch (speed)
            {
                case 0:
                    _navMeshAgent.speed = 0;
                    _navMeshAgent.acceleration = 9999;
                    break;
                case 1:
                    _navMeshAgent.speed = regularSpeed / 2;
                    _navMeshAgent.acceleration = acceleration;
                    break;
                case 2:
                    _navMeshAgent.speed = regularSpeed;
                    _navMeshAgent.acceleration = acceleration;
                    break;
                case 3:
                    _navMeshAgent.speed = maxSpeed;
                    _navMeshAgent.acceleration = maxSpeed;
                    break;
            }
        }

        private Collider FindHidingObstacle(bool largeSearch = false)
        {
            Collider hidingCollider = previousCollider; // res
            
            Vector3 position = transform.position;
            Vector3 targetPosition = targetPlayer.transform.position;
            Vector3 center = largeSearch ? position : (position + targetPosition) / 2;

            // Potential colliders to go to
            var finalRadius = largeSearch ? maxSpawnRange * 2 : distanceFromTarget / 2;
            var hitColliders = Physics.OverlapSphere(center, finalRadius, (int)layerMaskValue).ToList();

            // Removes invalid colliders
            hitColliders.Remove(previousCollider);
            hitColliders.RemoveAll(c => typeof(CharacterController) == c.GetType() ||
                                        typeof(CapsuleCollider) == c.GetType());
            hitColliders.RemoveAll(c => c.gameObject.CompareTag("mapFloor"));

            // Filters out colliders that are too far
            hitColliders.RemoveAll(c =>
                (targetPlayer.transform.position - c.transform.position).sqrMagnitude >
                distanceFromTarget * distanceFromTarget);

            if (hitColliders.Count > 0)
            {
                var despisedColliders = new List<(Collider, float)>();
                var favouredColliders = new List<(Collider, float)>();

                // Looks for valid colliders
                foreach (Collider _collider in hitColliders)
                {
                    float distColliderPlayer = Vector3.Distance(_collider.transform.position, targetPosition);

                    var result = (_collider, distColliderPlayer);

                    // If collider is far enough from the player, then valid
                    if (distColliderPlayer > distanceFromObjectMinTreshold &&
                        (_collider.CompareTag("tree") || _collider.CompareTag("stone")))
                    {
                        // Inserts in order to distance
                        InsertWithDistance(favouredColliders, result);

                        Debug.DrawRay(_collider.transform.position, Vector3.up * 5, Color.green, 3, false);
                    }
                    else
                    {
                        // Inserts in order to distance
                        InsertWithDistance(despisedColliders, (_collider, distColliderPlayer));

                        Debug.DrawRay(_collider.transform.position, Vector3.up * 5, Color.yellow, 3, false);
                    }
                }

                // Chooses a random collider between those available
                if (favouredColliders.Count > 0)
                {
                    int index = 0;
                    while (0 == Random.Range(0, favouredColliders.Count - index) &&
                           index < favouredColliders.Count - 1) index++;
                    hidingCollider = favouredColliders[index].Item1;
                }
                else if (despisedColliders.Count > 0)
                {
                    int index = 0;
                    while (0 == Random.Range(0, despisedColliders.Count - index) &&
                           index < despisedColliders.Count - 1) index++;
                    hidingCollider = despisedColliders[index].Item1;
                }
            }

            return hidingCollider;
        }

        private void InsertWithDistance(List<(Collider, float)> listColliders, (Collider _collider, float distColliderPlayer) result)
        {
            float distColliderPlayer = result.distColliderPlayer;
            
            if (listColliders.Count == 0)
            {
                listColliders.Add(result);
            }
            else
            {
                int index = 0;
                foreach ((_, float distance) in listColliders)
                {
                    if (distColliderPlayer < distance) break;
                    index++;
                }

                if (index >= listColliders.Count)
                    listColliders.Add(result);
                else
                {
                    listColliders.Insert(index, result);
                }
            }
        }

        private Vector3 FindHidingSpot(bool shouldUseCurrentObstacle, bool largeSearch = false)
        {
            var usedCollider = currentHidingObstacle;
            if (!shouldUseCurrentObstacle)
            {
                currentHidingObstacle = FindHidingObstacle(largeSearch);
                usedCollider = currentHidingObstacle;
            }

            // Goes behind the player if nothing found
            if (!usedCollider || usedCollider == _capsuleCollider)
            {
                var t = targetPlayer.transform;
                return t.position + t.TransformDirection(Vector3.back * 100);
            };

            // if (currentHidingObstacle) Debug.Log("here is the currentHidingObstacle", currentHidingObstacle);

            Vector3 obstaclePosition = usedCollider.transform.position;
            Vector3 targetPosition = targetPlayer.transform.position;

            // Sets the hiding spot to be 1 meter behind the obstacle on the axis of obstacle-player
            Vector3 direction = obstaclePosition - targetPosition;
            Vector3 extendedDir = GetPosBehindCollider(direction, currentHidingObstacle);

            Vector3 hidingSpot;

            if (IsColliderValidForClosestPoint(usedCollider.GetType(), usedCollider))
            {
                hidingSpot = usedCollider.ClosestPoint(targetPosition + extendedDir) + direction.normalized;
            }
            else
            {
                hidingSpot = obstaclePosition + direction.normalized * usedCollider.bounds.extents.sqrMagnitude;
            }

            // Sets the spawnPoint on the ground if possible
            if (Physics.Raycast(hidingSpot, Vector3.down, out RaycastHit hit)) hidingSpot = hit.point;

            return hidingSpot;
        }

        private Vector3 GetPosBehindCollider(Vector3 direction, Collider obstacle)
        {
            // Sets the hiding spot to be 1 meter behind the obstacle on the axis of obstacle-player
            var bounds = obstacle.bounds;
            return direction + direction.normalized * Vector3.Distance(bounds.max, bounds.min);
        }

        private bool IsColliderValidForClosestPoint(Type type, Collider usedCollider)
        {
            return type == typeof(CapsuleCollider) ||
                   type == typeof(SphereCollider) ||
                   type == typeof(BoxCollider) ||
                   type == typeof(CharacterController) ||
                   type == typeof(MeshCollider) && ((MeshCollider) usedCollider).convex;
        }

        private void HideFarBehindPlayer(float minDistance, float maxDistance, bool largeSearch = false,
            bool spawn = false)
        {
            // Moves the Ai behind the player
            {
                // SpawnPoint
                float spawnAngle = Random.Range(Mathf.PI * 11 / 8, Mathf.PI * 13 / 8);
                var spawnPointLocal = new Vector3(Mathf.Cos(spawnAngle), 0, Mathf.Sin(spawnAngle));
                // Sets the magnitude of the spawnPoint
                float length = Random.Range(minDistance, maxDistance);
                spawnPointLocal *= length;

                transform.position = targetPlayer.transform.position +
                                     targetPlayer.transform.TransformDirection(spawnPointLocal);
            }

            if (spawn)
            {
                transform.position = FindHidingSpot(false);
            }
            else
            {
                _navMeshAgent.SetDestination(FindHidingSpot(false, largeSearch));
            }
        }

        private IEnumerator AiCaught(bool shouldDie)
        {
            var t = targetPlayer.transform;
            var pos = t.position + t.TransformDirection(Vector3.forward);
            Instantiate(dissimulateParticles, pos, Quaternion.Euler(-90, 0, 0));
            if (shouldDie) isAlive = false;

            if (shouldDie)
            {
                Destroy(gameObject, 0.1f);
            }

            yield break;
        }
    }
}