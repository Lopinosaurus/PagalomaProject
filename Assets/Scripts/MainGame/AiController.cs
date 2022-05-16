using System;
using System.Collections.Generic;
using MainGame.Menus;
using MainGame.PlayerScripts;
using MainGame.PlayerScripts.Roles;
using UnityEngine;
using UnityEngine.AI;
using static System.Single;
using Random = UnityEngine.Random;

namespace MainGame
{
    public class AiController : MonoBehaviour
    {
        public Role targetRole;
        [SerializeField] private GameObject dissimulateParticle;
        private GameObject _targetPlayer;
        private Camera _targetCam;
        private Plane[] _targetPlanes;

        private NavMeshAgent _agent;
        private CapsuleCollider _capsuleCollider;
        [SerializeField] private Collider previousCollider;
        [SerializeField] private Collider currentHidingObstacle;
        private bool _isViewed;
        private bool _isInCameraView;

        private enum AiState
        {
            Hidden,
            Moving,
            Caught,
            Attack
        }

        private enum Speed
        {
            Freeze,
            Attack,
            Normal,
            Hiding
        }

        // Insert the LayerMask corresponding the player 
        [SerializeField] private LayerMask characterMask;
        private int _characterMaskValue;

        // Gameplay stats
        [Space] [Header("Gameplay statistics")] [SerializeField]
        private AiState currentState = AiState.Moving;

        [SerializeField] private float remainingTime;
        [SerializeField] private float timeBeingCaught;
        [SerializeField] private int remainingHealth = 1;

        private bool _isAlive = true;
        private const float TimeBeforeDeath = 15f;
        [SerializeField] private int moveCount;
        private const int MaxMoveCount = 10;

        private float DistFromTarget => Vector3.Distance(transform.position, _targetPlayer.transform.position);

        private const float CycleTime = 5;
        private const float MaxBeingCaughtDelay = 3;

        private const float AttackMaxDistancePlayer = 2.5f;
        private const float SqrMinColliderPlayerDist = 27;
        private const float RemainingMinDistance = 1;
        private const float minDistFromPlayer = 7;

        // Spawn settings
        [Space] [Header("Spawn distances")] private float _minSpawnRange = 30;
        private float _maxSpawnRange = 40;

        // NavMeshAgent settings
        [Space] [Header("Nav Mesh Settings")] [Range(0.01f, 100f)] private float _normalSpeed = 20;
        private const float _hidingSpeed = 999;
        private PlayerMovement _playerMovement;
        private PlayerLook _playerLook;
        private const float Acceleration = 20;

        private void Start()
        {
            _targetPlayer = targetRole.gameObject;
            _targetPlayer.GetComponent<CharacterController>();
        
            _playerLook = _targetPlayer.GetComponent<PlayerLook>();
            _playerMovement = _targetPlayer.GetComponent<PlayerMovement>();
            _targetPlayer = targetRole.gameObject;
            _targetCam = _targetPlayer.GetComponentInChildren<Camera>();

            _agent = GetComponent<NavMeshAgent>();
            _capsuleCollider = GetComponent<CapsuleCollider>();

            remainingTime = CycleTime;

            _characterMaskValue = GetLayerMaskValue(characterMask);

            // NavMesh settings
            _agent.speed = _normalSpeed;
            _agent.acceleration = Acceleration;
            _agent.angularSpeed = 9999;
            _agent.stoppingDistance = 0;
        }

        private void Update()
        {
            _isViewed = IsInCameraView();

            // Decides if the player is being observed
            if (!_isViewed)
            {
                // Rotates towards the player only if is not looked at
                RotateTowardsPlayer();
            }
        
            if (_isViewed && remainingTime <= CycleTime && AiState.Moving == currentState)
            {
                timeBeingCaught += Time.deltaTime;
            }

            if (timeBeingCaught >= MaxBeingCaughtDelay)
            {
                SetCurrentState(AiState.Caught);
            }
        }

        private void RotateTowardsPlayer()
        {
            Transform transform1;
            (transform1 = transform).LookAt(_targetPlayer.transform.position);
            Vector3 eulerAngles = transform1.rotation.eulerAngles;
            eulerAngles.x = 0;
            transform.rotation = Quaternion.Euler(eulerAngles);
        }

        private void FixedUpdate()
        {
            try
            {
                if (RoomManager.Instance && !VoteMenu.Instance.isNight)
                {
                    Destroy(gameObject);
                    return;
                }
            }
            catch
            {
                Debug.LogWarning("No RoomManager found ! (AiController)");
            }
        
        
            if (!_isAlive)
            {
                transform.position = _targetPlayer.transform.position +
                                     _targetPlayer.transform.TransformDirection(Vector3.back * 90);
            
                return;
            }

            if (!currentHidingObstacle)
            {
                transform.position = PositionBehindPlayer(_minSpawnRange, _maxSpawnRange);
                EnableMovementSpeed(Speed.Hiding);
                _agent.SetDestination(FindHidingSpot(false, true));
            }
        
            switch (currentState)
            {
                case AiState.Hidden when _isAlive:
                    // Sets the speed at fast
                    EnableMovementSpeed(Speed.Hiding);
                
                    if (_isInCameraView)
                    {
                        if (remainingTime < 1)
                        {
                            remainingTime = 1;
                        }
                    }
                    else
                    {
                        // Reduces the timer
                        remainingTime -= Time.fixedDeltaTime;
                    }
                
                    // Decides when to attack
                    if (moveCount >= MaxMoveCount)
                    {
                        SetCurrentState(AiState.Attack);
                        EnableMovementSpeed(Speed.Attack);
                    }
                
                    // Teleports if too close
                    else if (DistFromTarget < minDistFromPlayer)
                    {
                        PlayAiDamaged();
                        transform.position = PositionBehindPlayer(_minSpawnRange, _maxSpawnRange);
                        EnableMovementSpeed(Speed.Hiding);
                        _agent.SetDestination(FindHidingSpot(false, true));
                        moveCount++;
                    }
                
                    // When the time has run out normally, moves forwards
                    else if (remainingTime < 0)
                    {
                        SetCurrentState(AiState.Moving);
                        EnableMovementSpeed(Speed.Normal);
                        _agent.SetDestination(FindHidingSpot(false));
                        if (previousCollider != currentHidingObstacle) moveCount++;
                    }
                    else
                    {
                        // Stays hidden
                        _agent.SetDestination(FindHidingSpot(true));
                    }

                    break;
            
                case AiState.Moving when _isAlive:
                    EnableMovementSpeed(Speed.Normal);
                    remainingTime = CycleTime;
                
                    // When the Ai has arrived, goes back to Hidden
                    if (_agent.remainingDistance > RemainingMinDistance)
                    {
                        _agent.SetDestination(FindHidingSpot(true));
                    }
                    else
                    {
                        EnableMovementSpeed(Speed.Hiding);
                        SetCurrentState(AiState.Hidden);
                    }

                    break;
            
                case AiState.Caught when _isAlive:
                    EnableMovementSpeed(Speed.Freeze);

                    remainingHealth--;
                    moveCount = 0;
                    timeBeingCaught = 0;
                    remainingTime = CycleTime * 2;

                    PlayAiDamaged();

                    if (remainingHealth <= 0)
                    {
                        Destroy(gameObject, 0.5f);
                        PlayAiDamaged();
                        _isAlive = false;
                    }
                    else
                    {
                        transform.position = PositionBehindPlayer(_minSpawnRange, _maxSpawnRange);
                        EnableMovementSpeed(Speed.Attack);
                        _agent.SetDestination(FindHidingSpot(false, true));
                    }

                    break;
                case AiState.Attack when _isAlive:
                    EnableMovementSpeed(Speed.Attack);

                    if (_agent.remainingDistance <= AttackMaxDistancePlayer)
                    {
                        PlayAiDamaged();

                        StartCoroutine(_playerMovement.SlowSpeed(TimeBeforeDeath));
                        StartCoroutine(_playerLook.Shake(TimeBeforeDeath));
                    
                        Destroy(gameObject, TimeBeforeDeath + 5);
                    
                        EnableMovementSpeed(Speed.Freeze);
                    
                        _isAlive = false;
                    }
                    else
                    {
                        EnableMovementSpeed(Speed.Attack);
                        _agent.SetDestination(_targetPlayer.transform.position);
                    }
                
                    break;
            }
        }

        private void PlayAiDamaged()
        {
            var position1 = transform.position;
            var position = position1 +
                           transform.TransformDirection((_targetPlayer.transform.position - position1).normalized);

            Instantiate(dissimulateParticle, position, Quaternion.Euler(-90, 0, 0));
        }

        private void SetCurrentState(AiState newState) => currentState = newState;

        private bool IsInCameraView()
        {
            // Refreshes the camera planes
            _targetPlanes = GeometryUtility.CalculateFrustumPlanes(_targetCam);

            if (GeometryUtility.TestPlanesAABB(_targetPlanes, _capsuleCollider.bounds))
            {
                _isInCameraView = true;

                Vector3 camPosition = _targetCam.transform.position;

                float colliderHeight = _capsuleCollider.height;
                Vector3 colliderPosition = _capsuleCollider.transform.position;
                Vector3 colliderCenter = colliderPosition + Vector3.up * colliderHeight / 2;

                // All possible destinations - the more the more accurate
                Vector3[] destinations =
                {
                    colliderCenter,
                    colliderPosition,
                    colliderPosition + Vector3.up * colliderHeight
                };

                foreach (Vector3 destination in destinations)
                {
                    // Ray from camera to the chosen destination
                    if (Physics.Raycast(camPosition, destination - camPosition, out RaycastHit hit,
                            PositiveInfinity))
                    {
                        if (hit.collider == _capsuleCollider) return true;
                    }
                }
            }
            else
            {
                _isInCameraView = false;
            }

            return false;
        }

        private void EnableMovementSpeed(Speed selected)
        {
            _agent.acceleration = Acceleration;
        
            switch (selected)
            {
                case Speed.Freeze:
                    _agent.speed = 0;
                    _agent.acceleration = 9999;
                    break;
                case Speed.Attack:
                    _agent.speed = Mathf.Clamp(_normalSpeed - _agent.remainingDistance, 8, _normalSpeed);
                    _agent.acceleration = Acceleration;
                    break;
                case Speed.Normal:
                    _agent.speed = _normalSpeed;
                    _agent.acceleration = Acceleration;
                    break;
                case Speed.Hiding:
                    _agent.speed = _hidingSpeed;
                    _agent.acceleration = 9999;
                    break;
            }
        }

        private void FindNewObstacle(bool largeSearch = false)
        {
            Vector3 position = transform.position;
            Vector3 targetPosition = _targetPlayer.transform.position;

            Vector3 center = (position + targetPosition) / 2;
            var radius = DistFromTarget / 2;

            // Potential colliders to go to
            List<Collider> hitColliders = new List<Collider>(Physics.OverlapSphere(center, radius));

            // Filter out invalid colliders
            hitColliders.RemoveAll(IsInvalidCollider);
            hitColliders.Remove(previousCollider);
            // Filters out colliders that are too far
            hitColliders.RemoveAll(c =>
                Vector3.Distance(c.transform.position, _targetPlayer.transform.position) >
                DistFromTarget * DistFromTarget);

            var tooCloseCol = new List<(Collider, float)>();
            var correctCol = new List<(Collider, float)>();

            foreach (var c in hitColliders)
            {
                float sqrDist = (c.transform.position - _targetPlayer.transform.position).sqrMagnitude;
                bool isTooClose = sqrDist < SqrMinColliderPlayerDist;
                InsertSorted(isTooClose ? tooCloseCol : correctCol, c, sqrDist);
            }

            Collider newCollider = ChooseRandom(correctCol.Count > 0 ? correctCol : tooCloseCol, largeSearch);

            previousCollider = currentHidingObstacle;
            currentHidingObstacle = newCollider;
        }

        private Collider ChooseRandom(List<(Collider, float)> cols, bool largeSearch = false)
        {
            if (0 == cols.Count)
            {
                return currentHidingObstacle;
            }

            if (largeSearch)
            {
                return cols[cols.Count - 1].Item1;
            }

            var index = 0;
            while (index < cols.Count - 1 && Random.Range(0f, 1f) > 0.7f) index++;

            return cols[index].Item1;
        }

        private void InsertSorted(List<(Collider, float)> listCol, Collider col, float sqrDist)
        {
            if (0 == listCol.Count) listCol.Add((col, sqrDist));
            else
            {
                var i = 0;
                while (i < listCol.Count && listCol[i].Item2 < sqrDist) i++;
                listCol.Insert(i, (col, sqrDist));
            }
        }

        private bool IsInvalidCollider(Collider c)
        {
            Type type = c.GetType();
            return !c.CompareTag("tree")
                   && !c.CompareTag("stone")
                   || typeof(CharacterController) == type
                   || typeof(CapsuleCollider) == type
                   || c.gameObject.layer == _characterMaskValue;
        }

        private int GetLayerMaskValue(LayerMask layerMask)
        {
            return (int) (Mathf.Log(layerMask.value) / Mathf.Log(2));
        }

        private Vector3 FindHidingSpot(bool useCurrentCollider, bool largeSearch = false)
        {
            if (!useCurrentCollider || !currentHidingObstacle) FindNewObstacle(largeSearch);

            var bounds = currentHidingObstacle.bounds;
            Vector3 obstaclePosition = bounds.center;
            Vector3 targetPosition = _targetPlayer.transform.position;

            Vector3 direction = obstaclePosition - targetPosition;

            // Tries to find the point behind the obstacle aligned with the player
            Vector3 hidingSpot = obstaclePosition + direction.normalized * bounds.size.magnitude;
            Ray ray = new Ray(hidingSpot, obstaclePosition - hidingSpot);
        
            if (Physics.Raycast(ray, out RaycastHit hit1, PositiveInfinity, _characterMaskValue) && hit1.collider == currentHidingObstacle)
            {
                hidingSpot = hit1.point + direction.normalized * 0.5f;
            }

            // Sticks it to the ground
            if (Physics.Raycast(hidingSpot, Vector3.down, out RaycastHit hit, _characterMaskValue))
            {
                hidingSpot.y = hit.point.y;
            }
        
            Debug.DrawRay(hidingSpot, Vector3.up * 6, Color.red, 2, false);

        
            return hidingSpot;
        }

        public Vector3 PositionBehindPlayer(float minDistance, float maxDistance)
        {
            // SpawnPoint
            float spawnAngle = Random.Range(Mathf.PI * 11 / 8, Mathf.PI * 13 / 8);
            Vector3 spawnPointLocal = new Vector3(Mathf.Cos(spawnAngle), 0, Mathf.Sin(spawnAngle));
            // Sets the magnitude of the spawnPoint
            float length = Random.Range(minDistance, maxDistance);
            spawnPointLocal *= length;

            Vector3 spawnPointGlobal = _targetPlayer.transform.position +
                                       _targetPlayer.transform.TransformDirection(spawnPointLocal);

            return spawnPointGlobal;
        }
    }
}