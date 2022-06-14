``using System;
using System.Collections.Generic;
using System.Linq;
using MainGame.PlayerScripts.Roles;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using UnityEngine.U2D;
using static System.Single;
using Random = UnityEngine.Random;

public class AiController : MonoBehaviour
{
    public Role targetRole;
    [SerializeField] private GameObject dissimulateParticle;
    private GameObject _targetPlayer;
    private Camera _targetCam;
    private Plane[] _targetPlanes;

    private NavMeshAgent _agent;
    private CapsuleCollider _capsuleCollider;
    private Collider previousCollider;
    public bool isViewed;

    public enum AiState
    {
        Hidden,
        Transition,
        Caught
    }
    
    // Insert the LayerMask corresponding the player 
    [SerializeField] private string characterMaskName;

    // Gameplay stats
    [Space] [Header("Gameplay statistics")] [SerializeField]
    private AiState currentState = AiState.Moving;
    public AiState CurrentState => currentState;

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
    [Space]
    [Header("Spawn distances")]
    public  float minSpawnRange = 30f;
    public  float maxSpawnRange = 40f;
    
    // NavMeshAgent settings
    [Space] [Header("Nav Mesh Settings")] [Range(0.01f, 100f)] private float _normalSpeed = 20;
    private const float _hidingSpeed = 999;
    private PlayerMovement _playerMovement;
    private PlayerLook _playerLook;
    private const float Acceleration = 20;

    private void Awake()
    {
        targetPlayer = targetRole.gameObject;
        targetCam = targetPlayer.GetComponentInChildren<Camera>();
        
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _capsuleCollider = GetComponent<CapsuleCollider>();

        remainingTime = CycleTime;

        _characterMaskValue = GetLayerMaskValue(characterMask);

        remainingTimeBeforeTransition = cycleTime;
        
        // NavMesh settings
        _agent.speed = _normalSpeed;
        _agent.acceleration = Acceleration;
        _agent.angularSpeed = 9999;
        _agent.stoppingDistance = 0;
    }

    private void Update()
    {
        isViewed = IsInCameraView();
        
        // Decides if the player is being observed
        if (!isViewed || AiState.Transition == currentState)
        {
            // Rotates towards the player only if is not looked at
            Transform _transform = transform;
            _transform.LookAt(targetPlayer.transform.position);
            Vector3 eulerAngles = _transform.rotation.eulerAngles;
            eulerAngles.x = 0;
            transform.rotation = Quaternion.Euler(eulerAngles);
        }
        
        if (_isViewed && remainingTime <= CycleTime && AiState.Moving == currentState)
        {
            // Freezes the Ai if the player looks
            EnableMovement(false);
            
            if (currentState == AiState.Transition)
            {
                if (timeBeingCaught >= maxBeingCaughtDelay)
                {
                    SetCurrentState(AiState.Caught);
                }

                timeBeingCaught += Time.deltaTime;
            }
        }
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
                    _agent.SetDestination(_targetPlayer.transform.position);
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
                // The Ai can move without being seen
                {
                    if (remainingTimeBeforeTransition <= 0)
                    {
                        // Updates the destination to the hiding place position
                        Vector3 newHidingPlace = FindHidingSpot();
                        Debug.DrawRay(newHidingPlace, Vector3.up * 10, Color.magenta, 3, false);
                        _navMeshAgent.SetDestination(newHidingPlace);
                        Debug.DrawLine(transform.position, _navMeshAgent.destination, Color.red, 5);

                        // Starts moving
                        SetCurrentState(AiState.Transition);
                    }
                    
                    // The clock decreases
                    remainingTimeBeforeTransition -= Time.fixedDeltaTime;
                }
                
                break;
            }
            
            case AiState.Transition:
                
                EnableMovement(true);

                _navMeshAgent.SetDestination(FindHidingSpot(currentHidingObstacle));
                
                // Decides if the Ai is finally hidden
                if (_navMeshAgent.remainingDistance < allowedMaxDistanceFromDestination)
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

    private void SetCurrentState(AiState newState) => currentState = newState;

    private bool IsInCameraView()
    {
        // Refreshes the camera planes
        targetPlanes = GeometryUtility.CalculateFrustumPlanes(targetCam);
        
        if (GeometryUtility.TestPlanesAABB(targetPlanes, _capsuleCollider.bounds))
        {
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
            {
                // Ray from camera to the chosen destination
                if (Physics.Raycast(camPosition, destination - camPosition, out RaycastHit hit,
                        PositiveInfinity))
                {
                    if (hit.collider == _capsuleCollider) return true;
                }
            }
        }

        return false;
    }

    private void EnableMovement(bool shouldMove)
    {
        switch (shouldMove)
        {
            case true:
                _navMeshAgent.speed = regularSpeed;
                _navMeshAgent.acceleration = acceleration;
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
        
        Vector3 targetPosition = targetPlayer.transform.position;
        Vector3 center = (position + targetPosition) / 2;

        Collider hidingCollider = _capsuleCollider;
        
        // Potential colliders to go to
        List<Collider> hitColliders = new List<Collider>(
            Physics.OverlapSphere(center, distanceFromTarget / 2));
        
        // Removes the previous collider
        hitColliders.Remove(previousCollider);

        // Filters out colliders that are too far
        if (hitColliders.Count > 0)
        {
            List<Collider> tooCloseColliders = new List<Collider>();
            List<Collider> farAndCloseEnoughColliders = new List<Collider>();
            
            // Looks for valid colliders
            foreach (Collider _collider in hitColliders)
            {
                float distanceBetweenColliderAndPlayer = Vector3.Distance(_collider.ClosestPoint(position), targetPosition);
                
                if (_collider.gameObject.layer != LayerMask.NameToLayer(characterMaskName))
                {
                    // If closer than the distance between Ai and Player, then valid
                    if (distanceBetweenColliderAndPlayer < Vector3.Distance(position, targetPosition))
                    {
                        // If collider is farther enough from the player, then valid
                        if (distanceBetweenColliderAndPlayer > distanceFromObjectMinTreshold)
                        {
                            farAndCloseEnoughColliders.Add(_collider);
                            Debug.DrawRay(_collider.transform.position, Vector3.up * 5, Color.green, 3, false);
                        }
                        else
                        {
                            tooCloseColliders.Add(_collider);
                            Debug.DrawRay(_collider.transform.position, Vector3.up * 5, Color.yellow, 3, false);
                        }
                    }
                }
            }

            // Chooses a random collider between those available
            if (farAndCloseEnoughColliders.Count > 0)
            {
                hidingCollider = farAndCloseEnoughColliders[Random.Range(0, farAndCloseEnoughColliders.Count)];
            }
            else if (tooCloseColliders.Count > 0)
            {
                hidingCollider = tooCloseColliders[Random.Range(0, tooCloseColliders.Count)];
            }
            
        }

        Collider newCollider = ChooseRandom(correctCol.Count > 0 ? correctCol : tooCloseCol, largeSearch);

        previousCollider = currentHidingObstacle;
        currentHidingObstacle = newCollider;
    }

    private Collider ChooseRandom(List<(Collider, float)> cols, bool largeSearch = false)
    {
        if (0 == cols.Count)
        {
            return null;
        }

        var index = 0;
        while (index < cols.Count - 1 && Random.Range(0f, 1f) > (largeSearch ? 0.1f : 0.7f)) index++;

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
        
        return hidingCollider;
    }

    private Vector3 FindHidingSpot(Collider overrideCollider = null)
    {
        if (!overrideCollider)
        {
            currentHidingObstacle = FindHidingObstacle();
        }
        
        Vector3 obstaclePosition = currentHidingObstacle.transform.position;
        Vector3 targetPosition = targetPlayer.transform.position;

        Vector3 hidingSpot; // result
        
        {
            Vector3 direction = obstaclePosition - targetPosition;
            direction += direction.normalized * 1.5f;

            hidingSpot = targetPosition + direction;

            // Sets the spawnPoint on the ground if possible
            if (Physics.Raycast(hidingSpot, Vector3.down, out RaycastHit hit))
            {
                hidingSpot = hit.point;
            }
        }

        return hidingSpot;
    }
    
    private Vector3 SpawnBehindPlayer(float minDistance, float maxDistance)
    {
        // SpawnPoint
        float spawnAngle = Random.Range(Mathf.PI * 11 / 8, Mathf.PI * 13 / 8);
        Vector3 spawnPointLocal = new Vector3(Mathf.Cos(spawnAngle), 0, Mathf.Sin(spawnAngle));
        // Sets the magnitude of the spawnPoint
        float length = Random.Range(minDistance, maxDistance);
        spawnPointLocal *= length;
        
        Vector3 spawnPointGlobal =  targetPlayer.transform.position + targetPlayer.transform.TransformDirection(spawnPointLocal);

        return spawnPointGlobal;
    }
}
