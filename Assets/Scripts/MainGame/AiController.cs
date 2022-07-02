using System;
using System.Collections;
using System.Collections.Generic;
using MainGame;
using MainGame.PlayerScripts;
using MainGame.PlayerScripts.Roles;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering.PostProcessing;
using Random = UnityEngine.Random;

public class AiController : MonoBehaviour
{
    public Role targetRole;
    [SerializeField] private GameObject dissimulateParticle;
    [SerializeField] private GameObject render;
    private GameObject _targetPlayer;
    private Camera _targetCam;
    private Plane[] _targetPlanes;

    private PostProcessVolume _postProcessVolume;
    
    private NavMeshAgent _agent;
    private CapsuleCollider _capsuleCollider;
    [Space, Header("Colliders")]
    [SerializeField] private Collider previousCollider;
    [SerializeField] private Collider currentHidingObstacle;
    private bool _isViewed;
    public bool isDanger;
    private bool _isInCameraView;

    // Sound "State of Shock"
    [SerializeField] private AudioClip stateOfShock;
    [SerializeField] private AudioSource ambiantAudioSource;
    private AudioSource _movementAudioSource;
    

    private enum AiState
    {
        Hidden,
        Moving,
        Caught,
        Attack,
        Relocate
    }

    private enum Speed
    {
        Frozen,
        Attacking,
        Moving,
        Hiding,
        Relocating
    }
    
    // Insert the LayerMask corresponding the player 
    [SerializeField] private LayerMask characterMask;
    private int _characterMaskValue;

    // Gameplay stats
    [Space, Header("Gameplay statistics")] [SerializeField]
    private AiState currentState = AiState.Relocate;
    private float _timeBeingCaught;
    private const float MaxTimeBeingCaught = 2.5f;

    private float _remainingTime = 4;
    private int _remainingHealth = 1;

    private bool _isAlive = true;
    private const float ShakeDuration = 30;
    private const float SlowSpeedDuration = 30;
    private int _moveCount;
    private const int MaxMoveCount = 15;

    private float DistFromTarget => Vector3.Distance(transform.position, _targetPlayer.transform.position);

    private float CycleTime => 4 - _moveCount / MaxMoveCount;
    private const float AttackMaxDistancePlayer = 1.5f;
    private const float RemainingMinDistance = 1;
    private const float MinDangerDistFromPlayer = 15;
    private const float MinCriticalDistFromPlayer = 7;

    // Spawn settings
    [Space] [Header("Spawn distances")] private const float MinSpawnRange = 50, MaxSpawnRange = 60;

    // NavMeshAgent settings
    [Space] [Header("Nav Mesh Settings")] [Range(0.01f, 100f)]
    private const float NormalSpeed = 200;

    private const float HidingSpeed = 50;
    private PlayerMovement _playerMovement;
    private PlayerLook _playerLook;
    public Collider[] hitColliders;
    private bool _reachedLast;
    private const float Acceleration = 500;
    private const float AccelerationFast = 500;

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
        
        // Postprocessing
        _postProcessVolume = GetComponentInChildren<PostProcessVolume>();

        // CurrentHidingObstacle
        transform.position = PositionBehindPlayer(MinSpawnRange, MaxSpawnRange);
        StartCoroutine(NullToObstacle());
        
        SetTime(3);
        
        // Sound
        _movementAudioSource = GetComponent<AudioSource>();

        _characterMaskValue = GetLayerMaskValue(characterMask);

        // NavMesh settings
        try
        {
            _agent.speed = NormalSpeed;
            _agent.acceleration = Acceleration;
            _agent.angularSpeed = 9999;
            _agent.stoppingDistance = 0;
        }
        catch
        {
            Debug.Log("Ai badly placed");
            Destroy(gameObject);
        }
    }

    private void ApplyMalusPostProcessAndSound()
    {
        Destroy(ambiantAudioSource);
        _targetPlayer.GetComponent<PlayerLook>().LocalPostProcessingAndSound(_postProcessVolume, ShakeDuration, stateOfShock);
    }

    private IEnumerator NullToObstacle()
    {
        int count = 0;
        
        while (count < 5)
        {
            if (null == currentHidingObstacle)
            {
                FindNewObstacle(true);
            }

            count++;
            yield return new WaitForSeconds(3);
        }
    }

    private void Update()
    {
        _isViewed = IsInCameraView();

        RotateTowardsPlayer();
    }

    private void RotateTowardsPlayer()
    {
        Transform transform1;
        (transform1 = render.transform).LookAt(_targetPlayer.transform.position);
        Vector3 eulerAngles = transform1.rotation.eulerAngles;
        eulerAngles.x = 0;
        render.transform.rotation = Quaternion.Euler(eulerAngles);
    }

    private void FixedUpdate()
    {
        // Destroys if it's day
        try
        {
            if (RoomManager.Instance && !VoteMenu.Instance.IsNight)
            {
                Destroy(gameObject);
                return;
            }
        }
        catch
        {
            Debug.LogWarning("No RoomManager found ! (AiController)");
        }
        
        float distFromTarget = DistFromTarget;
        
        switch (currentState)
        {
            case AiState.Hidden when _isAlive:
                EnableMovementSpeed(Speed.Hiding);

                _remainingTime -= Time.fixedDeltaTime;
                
                // Decides when to attack
                if (_moveCount == MaxMoveCount - 1 && !_reachedLast)
                {
                    _remainingTime = Random.Range(5f, 10f);
                    _reachedLast = true;
                }

                if (_moveCount >= MaxMoveCount)
                {
                    _reachedLast = false;
                    _moveCount = 0;
                    _timeBeingCaught = 0;
                    _agent.SetDestination(_targetPlayer.transform.position);
                    SetCurrentState(AiState.Attack);
                }
                
                // Teleports if too close or too far
                else
                {
                    bool tooClose = distFromTarget < MinCriticalDistFromPlayer;
                    bool tooFar = distFromTarget > MaxSpawnRange + 10;
                    isDanger = distFromTarget < MinDangerDistFromPlayer;

                    // When the time has run out normally, moves
                    if (_remainingTime < 0 || tooFar || tooClose)
                    {
                        SetTime(CycleTime);
                        
                        SetCurrentState(AiState.Moving);
                        _agent.SetDestination(FindHidingSpot(false, isDanger));
                        
                        if (previousCollider != currentHidingObstacle && !tooFar)
                        {
                            _moveCount++;
                            // Play sound
                            _movementAudioSource.Play();
                        }
                    }
                    else
                    {
                        PostProcessWithDistance(distFromTarget);
                        
                        // Stays hidden
                        _agent.SetDestination(FindHidingSpot(true));
                    }
                }

                break;

            case AiState.Moving when _isAlive:
                EnableMovementSpeed(Speed.Moving);
                SetTime(CycleTime);

                _agent.SetDestination(FindHidingSpot(true));
                
                // When the Ai has arrived, goes back to Hidden
                if ((transform.position - _agent.destination).sqrMagnitude < RemainingMinDistance) SetCurrentState(AiState.Hidden);

                break;

            case AiState.Caught when _isAlive:
                EnableMovementSpeed(Speed.Frozen);
                SetCurrentState(AiState.Hidden);

                _remainingHealth--;
                SetTime(CycleTime * 2);

                PlayAiDamaged();

                if (_remainingHealth <= 0)
                {
                    Destroy(gameObject, 0.5f);
                    _isAlive = false;
                }
                else
                {
                    SetCurrentState(AiState.Relocate);
                    _remainingTime = 3;
                    _agent.SetDestination(FindHidingSpot(false, true));
                }

                break;
            case AiState.Attack when _isAlive:
                EnableMovementSpeed(Speed.Attacking);

                _agent.SetDestination(_targetPlayer.transform.position);

                if (distFromTarget <= AttackMaxDistancePlayer)
                {
                    PlayAiDamaged();

                    _playerMovement.StartModifySpeed(SlowSpeedDuration, PlayerMovement.AiStunMult, 0, 0.8f);
                    _playerLook.StartShake(ShakeDuration, 5);
                    ApplyMalusPostProcessAndSound();

                    // Dead
                    Destroy(gameObject);
                    _isAlive = false;
                }
                else if (_isInCameraView && _timeBeingCaught < MaxTimeBeingCaught)
                {
                    _timeBeingCaught += Time.fixedDeltaTime;

                    LookAway();
                }
                
                if (_timeBeingCaught >= MaxTimeBeingCaught) SetCurrentState(AiState.Caught);

                break;
            case AiState.Relocate:
                EnableMovementSpeed(Speed.Relocating);
                _agent.SetDestination(FindHidingSpot(true));

                if (_agent.remainingDistance < RemainingMinDistance)
                {
                    SetCurrentState(AiState.Hidden);
                }

                break;
        }
    }

    private void PostProcessWithDistance(float distFromTarget)
    {
        float dangerDistFromPlayer =
            1 - (distFromTarget - MinCriticalDistFromPlayer) / (MinDangerDistFromPlayer - MinCriticalDistFromPlayer);

        dangerDistFromPlayer = Mathf.Clamp01(dangerDistFromPlayer);

        // Disable or enable postprocessing if moving
        _postProcessVolume.weight = _postProcessVolume.weight < dangerDistFromPlayer
            ? dangerDistFromPlayer
            : Mathf.Lerp(_postProcessVolume.weight, dangerDistFromPlayer, Time.deltaTime);
                        
        // Shakes intensely
        if (_isInCameraView)
        {
            if (distFromTarget < MinDangerDistFromPlayer)
            {
                _playerLook.StartShake(0.1f, dangerDistFromPlayer);
            }
        }
    }

    private void LookAway()
    {
        // Look away
        Vector3 dirAiPlayer = _targetPlayer.transform.position - transform.position;
        Vector3 forwardPlayer = _targetPlayer.transform.forward;
        float angle = Vector3.SignedAngle(dirAiPlayer, forwardPlayer, Vector3.up);

        float deltaAngle = angle * Time.deltaTime * 0.5f;
        deltaAngle *= Math.Sign(_playerLook.transform.localRotation.eulerAngles.y) == Math.Sign(deltaAngle) ? 0 : 1;

        _targetPlayer.transform.Rotate(Vector3.up, -deltaAngle);
    }

    private void SetTime(float cycleTime) => _remainingTime = cycleTime;

    private void PlayAiDamaged()
    {
        var position = transform.position + _targetPlayer.transform.TransformDirection(Vector3.back);

        Instantiate(dissimulateParticle, position, Quaternion.Euler(-90, 0, 0));
    }

    private void SetCurrentState(AiState newState) => currentState = newState;

    private bool IsInCameraView()
    {
        // Refreshes the camera planes
        _targetPlanes = GeometryUtility.CalculateFrustumPlanes(_targetCam);

        /*Vector3 camPosition = _targetCam.transform.position;

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
}*/

        _isInCameraView = GeometryUtility.TestPlanesAABB(_targetPlanes, _capsuleCollider.bounds);

        return false;
    }

    private void EnableMovementSpeed(Speed selected)
    {
        switch (selected)
        {
            case Speed.Frozen:
                _agent.speed = 0;
                _agent.acceleration = 9999;
                break;
            case Speed.Attacking:
                _agent.speed = Mathf.Clamp(20 - _agent.remainingDistance, 10, 20);
                _agent.acceleration = 10;
                break;
            case Speed.Moving:
                _agent.speed = NormalSpeed * Mathf.Clamp01(_agent.remainingDistance * 0.05f);
                _agent.acceleration = Acceleration;
                break;
            case Speed.Hiding:
                _agent.speed = HidingSpeed;
                _agent.acceleration = AccelerationFast;
                break;
            case Speed.Relocating:
                _agent.speed = 200;
                _agent.acceleration = 9999;
                break;
        }
    }

    private void FindNewObstacle(bool largeSearch = false)
    {
        Vector3 targetPosition = _targetPlayer.transform.position;

        var radius = MaxSpawnRange;

        // Potential colliders to go to
        Vector3 center = (targetPosition + transform.position) * 0.5f;
        hitColliders = new Collider[300];
        Physics.OverlapSphereNonAlloc(center, radius, hitColliders, _characterMaskValue);

        var correctCol = new List<(Collider, float)>();

        foreach (Collider c in hitColliders)
        {
            // Skip invalids
            if (null == c) continue;
            
            float sqrMagnitude = (c.transform.position - targetPosition).sqrMagnitude;
            if (!IsValidCollider(c) ||
                c == previousCollider ||
                c == currentHidingObstacle ||
                c == _capsuleCollider ||
                sqrMagnitude < MinDangerDistFromPlayer * MinDangerDistFromPlayer)
            {
                continue;
            }

            float sqrDist = sqrMagnitude;

            InsertSorted(correctCol, c, sqrDist);
        }

        Collider newCollider = ChooseRandom(correctCol, largeSearch);
        
        previousCollider = currentHidingObstacle;
        currentHidingObstacle = newCollider;
        }

    private Collider ChooseRandom(IReadOnlyList<(Collider, float)> cols, bool largeSearch)
    {
        if (0 == cols.Count)
        {
            return currentHidingObstacle;
        }

        if (largeSearch) return cols[cols.Count - 1].Item1;
        
        var index = 0;
        var prob = 0.9f;
        while (index < cols.Count - 1 && Random.Range(0f, 1f) > prob) index++;

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

    private bool IsValidCollider(Collider c)
    {
        // Refreshes the camera planes
        _targetPlanes = GeometryUtility.CalculateFrustumPlanes(_targetCam);
        if (GeometryUtility.TestPlanesAABB(_targetPlanes, c.bounds)) return false;
        
        Type type = c.GetType();

        if (!c.CompareTag("tree")
            && !c.CompareTag("stone")
            || typeof(CharacterController) == type
            || typeof(CapsuleCollider) == type
            || c.gameObject.layer == _characterMaskValue)
            return false;

        return true;
    }

    private int GetLayerMaskValue(LayerMask layerMask)
    {
        return (int) (Mathf.Log(layerMask.value) / Mathf.Log(2));
    }

    private Vector3 FindHidingSpot(bool useCurrentCollider, bool largeSearch = false)
    {
        if (!useCurrentCollider)
        {
            FindNewObstacle(largeSearch);
        }

        Vector3 obstaclePosition = _targetPlayer.transform.TransformDirection(Vector3.back * 10);
        
        try
        {
            obstaclePosition = currentHidingObstacle.transform.position;
        }
        catch
        {
            Debug.Log("Obstacle was null, looking for a new one");
            StartCoroutine(NullToObstacle());
        }
        
        Vector3 targetPosition = _targetPlayer.transform.position;
        Vector3 direction = obstaclePosition - targetPosition;

        // Tries to find the point behind the obstacle aligned with the player
        Vector3 hidingSpot = obstaclePosition + direction.normalized * 1.5f;

        // // Sticks it to the ground
        // if (Physics.Raycast(hidingSpot, Vector3.down, out RaycastHit hit, _characterMaskValue))
        // {
        //     hidingSpot.y = hit.point.y;
        // }

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

        Vector3 spawnPointGlobal = _targetPlayer.transform.position +
                                   _targetPlayer.transform.TransformDirection(spawnPointLocal);

        return spawnPointGlobal;
    }
}