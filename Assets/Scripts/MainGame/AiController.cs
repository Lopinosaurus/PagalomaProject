using System;
using System.Collections.Generic;
using MainGame.PlayerScripts;
using MainGame.PlayerScripts.Roles;
using UnityEngine;
using UnityEngine.AI;
using static System.Single;
using Random = UnityEngine.Random;

public class AiController : MonoBehaviour
{
    [SerializeField] private Role targetRole;
    [SerializeField] private GameObject dissimulateParticle;
    private GameObject targetPlayer;
    private Camera targetCam;
    private Plane[] targetPlanes;

    private NavMeshAgent _agent;
    private CapsuleCollider _capsuleCollider;
    [SerializeField] private Collider previousCollider;
    [SerializeField] private Collider currentHidingObstacle;
    public bool isViewed;
    public bool isInCameraView;

    private enum AiState
    {
        Hidden,
        Transition,
        Caught,
        Attack
    }

    private enum Speed
    {
        Freeze,
        Attack,
        Normal,
        Fast
    }

    // Insert the LayerMask corresponding the player 
    [SerializeField] private LayerMask characterMask;
    private int characterMaskValue;

    // Gameplay stats
    [Space] [Header("Gameplay statistics")] [SerializeField]
    private AiState currentState = AiState.Hidden;

    [SerializeField] private float remainingTime;
    [SerializeField] private float timeBeingCaught;
    [SerializeField] private int remainingHealth = 3;

    private bool isAlive = true;
    private const float timeBeforeDeath = 15f;
    [SerializeField] private int moveCount;
    private const int maxMoveCount = 10;

    private float distFromTarget => Vector3.Distance(transform.position, targetPlayer.transform.position);

    private const float cycleTime = 15;
    private const float maxBeingCaughtDelay = 0.5f;
    private const float maxWaitingTime = -3;

    private const float attackMaxDistancePlayer = 2.5f;
    private const float sqrMinColliderPlayerDist = 27;
    private const float remainingMinDistance = 1;

    // Spawn settings
    [Space] [Header("Spawn distances")] public float minSpawnRange = 30;
    public float maxSpawnRange = 40;

    // NavMeshAgent settings
    [Space] [Header("Nav Mesh Settings")] [Range(0.01f, 100f)]
    public float normalSpeed = 20;

    [Range(1, 100)] public float fastSpeed = 9999;
    private PlayerMovement _playerMovement;
    private PlayerLook _playerLook;
    private const float acceleration = 9999;

    private void Awake()
    {
        targetPlayer = targetRole.gameObject;
        targetPlayer.GetComponent<CharacterController>();
        _playerLook = targetPlayer.GetComponent<PlayerLook>();
        _playerMovement = targetPlayer.GetComponent<PlayerMovement>();
        targetPlayer = targetRole.gameObject;
        targetCam = targetPlayer.GetComponentInChildren<Camera>();

        _agent = GetComponent<NavMeshAgent>();
        _capsuleCollider = GetComponent<CapsuleCollider>();

        remainingTime = cycleTime;

        characterMaskValue = GetLayerMaskValue(characterMask);

        // NavMesh settings
        _agent.speed = normalSpeed;
        _agent.acceleration = acceleration;
        _agent.stoppingDistance = 0.5f;

        transform.position = PositionBehindPlayer(minSpawnRange, maxSpawnRange);
    }

    private void Update()
    {
        isViewed = IsInCameraView();

        // Decides if the player is being observed
        if (!isViewed)
        {
            // Rotates towards the player only if is not looked at
            RotateTowardsPlayer();
        }
        
        if (isViewed && AiState.Transition == currentState && remainingTime <= cycleTime)
        {
            timeBeingCaught += Time.deltaTime;
        }

        if (AiState.Transition != currentState)
        {
            timeBeingCaught = 0;
        }

        if (timeBeingCaught >= maxBeingCaughtDelay)
        {
            // Freezes the Ai if the player looks
            EnableMovementSpeed(Speed.Freeze);
            SetCurrentState(AiState.Caught);
        }
    }

    private void RotateTowardsPlayer()
    {
        Transform _transform = transform;
        _transform.LookAt(targetPlayer.transform.position);
        Vector3 eulerAngles = _transform.rotation.eulerAngles;
        eulerAngles.x = 0;
        transform.rotation = Quaternion.Euler(eulerAngles);
    }

    private void FixedUpdate()
    {
        Debug.DrawLine(_agent.destination, transform.position, Color.green);
        try
        {
            Debug.DrawRay(currentHidingObstacle.bounds.center,
                Vector3.up * 5, Color.yellow, 0, false);
        }
        catch{}


        switch (currentState)
        {
            case AiState.Hidden when isAlive:
                // Sets the speed at fast
                EnableMovementSpeed(Speed.Fast);

                // Reduces the timer
                remainingTime -= Time.fixedDeltaTime;

                // Tries to stay hidden
                _agent.SetDestination(FindHidingSpot(true));

                if (moveCount >= maxMoveCount)
                {
                    SetCurrentState(AiState.Attack);
                }

                // When the time limit reached the maximum, moves behind
                else if (remainingTime < maxWaitingTime)
                {
                    moveCount++;
                    remainingTime = cycleTime;
                    transform.position = PositionBehindPlayer(minSpawnRange, maxSpawnRange);
                    _agent.SetDestination(FindHidingSpot(false, true));
                }
                // When the time has run out normally, tries to hide
                else if (remainingTime < 0)
                {
                    if (!isInCameraView)
                    {
                        remainingTime = cycleTime;
                        moveCount++;
                        SetCurrentState(AiState.Transition);
                        _agent.SetDestination(FindHidingSpot(false));
                    }
                }

                break;
            
            case AiState.Transition when isAlive:

                EnableMovementSpeed(Speed.Normal);
                remainingTime = cycleTime;

                if (_agent.remainingDistance < remainingMinDistance)
                {
                    SetCurrentState(AiState.Hidden);
                }
                else
                {
                    _agent.SetDestination(FindHidingSpot(true));
                }

                break;
            
            case AiState.Caught when isAlive:
                
                EnableMovementSpeed(Speed.Freeze);
                SetCurrentState(AiState.Hidden);

                remainingHealth--;
                moveCount = 0;
                timeBeingCaught = 0;
                remainingTime = cycleTime * 1.5f;

                PlayAiDamaged();

                if (remainingHealth <= 0)
                {
                    Destroy(gameObject, 0.5f);
                    isAlive = false;
                }
                else
                {
                    transform.position = PositionBehindPlayer(minSpawnRange, maxSpawnRange);
                    _agent.SetDestination(FindHidingSpot(false, true));
                }

                break;
            case AiState.Attack when isAlive:
                EnableMovementSpeed(Speed.Attack);

                if (_agent.remainingDistance <= attackMaxDistancePlayer)
                {
                    PlayAiDamaged();

                    StartCoroutine(_playerMovement.SlowSpeed(timeBeforeDeath));
                    StartCoroutine(_playerLook.Shake(timeBeforeDeath));
                    transform.position = new Vector3(0, -50, 0); //far
                    Destroy(gameObject, timeBeforeDeath);
                    
                    isAlive = false;
                }
                else
                {
                    _agent.SetDestination(targetPlayer.transform.position);
                }

                break;
        }
    }

    private void PlayAiDamaged()
    {
        var position1 = transform.position;
        var position = position1 +
                       transform.TransformDirection((targetPlayer.transform.position - position1).normalized);

        Instantiate(dissimulateParticle, position, Quaternion.Euler(-90, 0, 0));
    }

    private void SetCurrentState(AiState newState) => currentState = newState;

    private bool IsInCameraView()
    {
        // Refreshes the camera planes
        targetPlanes = GeometryUtility.CalculateFrustumPlanes(targetCam);

        if (GeometryUtility.TestPlanesAABB(targetPlanes, _capsuleCollider.bounds))
        {
            isInCameraView = true;

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
        else
        {
            isInCameraView = false;
        }

        return false;
    }

    private void EnableMovementSpeed(Speed selected)
    {
        _agent.speed = selected switch
        {
            Speed.Freeze => 0,
            Speed.Attack => Mathf.Clamp(normalSpeed - _agent.remainingDistance, 8, normalSpeed),
            Speed.Normal => normalSpeed,
            Speed.Fast => fastSpeed,
            _ => _agent.speed
        };
    }

    private void FindNewObstacle()
    {
        Vector3 position = transform.position;
        Vector3 targetPosition = targetPlayer.transform.position;

        Vector3 center = (position + targetPosition) / 2;
        var radius = distFromTarget / 2;

        // Potential colliders to go to
        List<Collider> hitColliders = new List<Collider>(Physics.OverlapSphere(center, radius));

        // Filter out invalid colliders
        hitColliders.RemoveAll(IsInvalidCollider);
        hitColliders.Remove(previousCollider);
        // Filters out colliders that are too far
        hitColliders.RemoveAll(c =>
            Vector3.Distance(c.transform.position, targetPlayer.transform.position) >
            distFromTarget * distFromTarget);

        var tooCloseCol = new List<(Collider, float)>();
        var correctCol = new List<(Collider, float)>();

        foreach (var c in hitColliders)
        {
            float sqrDist = (c.transform.position - targetPlayer.transform.position).sqrMagnitude;
            bool isTooClose = sqrDist < sqrMinColliderPlayerDist;
            InsertSorted(isTooClose ? tooCloseCol : correctCol, c, sqrDist);
        }

        Collider newCollider = ChooseRandom(correctCol.Count > 0 ? correctCol : tooCloseCol);

        if (!Equals(currentHidingObstacle, newCollider))
        {
            previousCollider = currentHidingObstacle;
            currentHidingObstacle = newCollider;
        }
    }

    private Collider ChooseRandom(List<(Collider, float)> cols)
    {
        if (0 == cols.Count)
        {
            return null;
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
        return typeof(CharacterController) == type
               || typeof(CapsuleCollider) == type
               || c.CompareTag("mapFloor")
               || c.CompareTag("village")
               || c.CompareTag("sign")
               || c.CompareTag("Player")
               || c.gameObject.layer == characterMaskValue;
    }

    private int GetLayerMaskValue(LayerMask layerMask)
    {
        return (int) (Mathf.Log(layerMask.value) / Mathf.Log(2));
    }

    private Vector3 FindHidingSpot(bool useCurrentCollider, bool largeSearch = false)
    {
        if (!useCurrentCollider || !currentHidingObstacle) FindNewObstacle();

        var bounds = currentHidingObstacle.bounds;
        Vector3 obstaclePosition = bounds.center;
        Vector3 targetPosition = targetPlayer.transform.position;

        Vector3 direction = obstaclePosition - targetPosition;

        // Tries to find the point behind the obstacle aligned with the player
        Vector3 hidingSpot = obstaclePosition + direction.normalized * bounds.size.magnitude;
        Ray ray = new Ray(hidingSpot, obstaclePosition - hidingSpot);
        
        if (Physics.Raycast(ray, out RaycastHit hit1, PositiveInfinity, characterMaskValue) && hit1.collider == currentHidingObstacle)
        {
            hidingSpot = hit1.point + direction.normalized * 2;
        }

        Debug.DrawRay(hidingSpot, Vector3.up * 6, Color.red, 2, false);

        // Sticks it to the ground
        if (Physics.Raycast(hidingSpot, Vector3.down, out RaycastHit hit, characterMaskValue))
        {
            hidingSpot.y = hit.point.y;
        }
        
        return hidingSpot;
    }

    private Vector3 PositionBehindPlayer(float minDistance, float maxDistance)
    {
        // SpawnPoint
        float spawnAngle = Random.Range(Mathf.PI * 11 / 8, Mathf.PI * 13 / 8);
        Vector3 spawnPointLocal = new Vector3(Mathf.Cos(spawnAngle), 0, Mathf.Sin(spawnAngle));
        // Sets the magnitude of the spawnPoint
        float length = Random.Range(minDistance, maxDistance);
        spawnPointLocal *= length;

        Vector3 spawnPointGlobal = targetPlayer.transform.position +
                                   targetPlayer.transform.TransformDirection(spawnPointLocal);

        return spawnPointGlobal;
    }
}