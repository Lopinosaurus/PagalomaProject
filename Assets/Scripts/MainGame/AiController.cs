using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MainGame;
using MainGame.PlayerScripts;
using MainGame.PlayerScripts.Roles;
using UnityEngine;
using UnityEngine.AI;
using static System.Single;
using Random = UnityEngine.Random;

public class AiController : MonoBehaviour
{
    public Role targetRole;
    [SerializeField] private GameObject dissimulateParticle;
    private GameObject _targetPlayer;
    public GameObject _camHolder;
    private Camera _targetCam;
    private Plane[] _targetPlanes;

    private NavMeshAgent _agent;
    private CapsuleCollider _capsuleCollider;
    [SerializeField] private Collider previousCollider;
    [SerializeField] private Collider currentHidingObstacle;
    private bool _isViewed;
    public bool isDanger;
    private bool _isInCameraView;
    private bool canShake;
    
    // Sound "State of Shock"
    [SerializeField] private AudioSource iaSound;
    [SerializeField] private AudioClip stateOfShock;
    [SerializeField] private AudioClip iaKilled;
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
        Freeze,
        Attack,
        Moving,
        Hiding,
        Relocating
    }

    // Insert the LayerMask corresponding the player 
    [SerializeField] private LayerMask characterMask;
    private int _characterMaskValue;

    // Gameplay stats
    [Space] [Header("Gameplay statistics")] [SerializeField]
    private AiState currentState = AiState.Relocate;

    private float remainingTime = 4;
    private int remainingHealth = 3;

    private bool _isAlive = true;
    private const float shakeDuration = 30;
    private const float slowSpeedDuration = 15;
    private int moveCount;
    private const int MaxMoveCount = 15;

    private float DistFromTarget => Vector3.Distance(transform.position, _targetPlayer.transform.position);

    private const float CycleTime = 2;

    private const float AttackMaxDistancePlayer = 1.5f;
    private const float RemainingMinDistance = 1;
    private const float minCriticalDistFromPlayer = 7;
    private const float minDangerDistFromPlayer = 30;
    private const float minFlee = 20;
    

    // Spawn settings
    [Space] [Header("Spawn distances")] private float _minSpawnRange = 40;
    private float _maxSpawnRange = 50;

    // NavMeshAgent settings
    [Space] [Header("Nav Mesh Settings")] [Range(0.01f, 100f)]
    private float _normalSpeed = 50;

    private const float _hidingSpeed = 50;
    private PlayerMovement _playerMovement;
    private PlayerLook _playerLook;
    private const float Acceleration = 500;
    private const float AccelerationFast = 100;

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

        // CurrentHidingObstacle
        transform.position = PositionBehindPlayer(_minSpawnRange, _maxSpawnRange);
        StartCoroutine(NullToObstacle());
        
        SetTime(3);

        _characterMaskValue = GetLayerMaskValue(characterMask);

        // NavMesh settings
        try
        {
            _agent.speed = _normalSpeed;
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

    private IEnumerator NullToObstacle()
    {
        while (true)
        {
            if (null == currentHidingObstacle)
            {
                FindNewObstacle(true);
            }
            yield return new WaitForFixedUpdate();
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
        (transform1 = transform).LookAt(_targetPlayer.transform.position);
        Vector3 eulerAngles = transform1.rotation.eulerAngles;
        eulerAngles.x = 0;
        transform.rotation = Quaternion.Euler(eulerAngles);
    }

    private void FixedUpdate()
    {
        // Destroys if it's day
        try
        {
            if (RoomManager.Instance && !VoteMenu.Instance.isNight)
            {
                Destroy(gameObject);
                
                iaSound.clip = iaKilled;
                iaSound.Stop();
                iaSound.Play();
                return;
            }
        }
        catch
        {
            Debug.LogWarning("No RoomManager found ! (AiController)");
        }

        switch (currentState)
        {
            case AiState.Hidden when _isAlive:
                EnableMovementSpeed(Speed.Hiding);

                float distFromTarget = DistFromTarget;
                // Reduces the timer
                if (!_isInCameraView)
                {
                    remainingTime -= Time.fixedDeltaTime;

                    canShake = true;
                }
                else
                {

                    remainingTime -= Time.deltaTime * 0.5f;
                    if (canShake)
                    {
                        canShake = false;
                        Debug.Log("should shake");
                        _playerLook.StartShake(0.1f, 5);
                    }
                }

                // Decides when to attack
                if (moveCount >= MaxMoveCount)
                {
                    SetCurrentState(AiState.Attack);
                }

                // Teleports if too close or too far
                else
                {
                    bool tooFar = distFromTarget > _maxSpawnRange + 10;
                    bool tooClose = distFromTarget < minCriticalDistFromPlayer;
                    isDanger = distFromTarget < minFlee;

                    if (remainingTime <= CycleTime && (tooClose || tooFar))
                    {
                        if (tooClose)
                        {
                            SetCurrentState(AiState.Caught);
                        }
                        else
                        {
                            _agent.SetDestination(FindHidingSpot(false, true));
                        }

                        if (previousCollider != currentHidingObstacle &&
                            _agent.remainingDistance < RemainingMinDistance) moveCount++;
                    }

                    // When the time has run out normally, moves
                    else if (remainingTime < 0)
                    {
                        SetTime(CycleTime);
                        SetCurrentState(AiState.Moving);

                        _agent.SetDestination(FindHidingSpot(false, isDanger));
                        if (previousCollider != currentHidingObstacle) moveCount++;

                        Debug.DrawRay(_agent.destination, Vector3.up * 12, Color.green, 1f, false);
                    }
                    else
                    {
                        // Stays hidden
                        _agent.SetDestination(FindHidingSpot(true));
                    }
                }

                break;

            case AiState.Moving when _isAlive:
                EnableMovementSpeed(Speed.Moving);
                SetTime(CycleTime);

                _agent.SetDestination(FindHidingSpot(true));
                SetCurrentState(AiState.Moving);
                
                Debug.DrawRay(_agent.destination, Vector3.up * 12, Color.green, 1f, false);

                // When the Ai has arrived, goes back to Hidden
                if (_agent.remainingDistance < RemainingMinDistance)
                {
                    SetCurrentState(AiState.Hidden);
                }

                break;

            case AiState.Caught when _isAlive:
                EnableMovementSpeed(Speed.Freeze);
                SetCurrentState(AiState.Hidden);

                remainingHealth--;
                SetTime(CycleTime * 2);

                PlayAiDamaged();

                if (remainingHealth <= 0)
                {
                    Destroy(gameObject, 0.5f);
                    _isAlive = false;
                }
                else
                {
                    SetCurrentState(AiState.Relocate);
                    transform.position = PositionBehindPlayer(_minSpawnRange, _maxSpawnRange);
                    _agent.SetDestination(FindHidingSpot(false, true));
                }

                break;
            case AiState.Attack when _isAlive:
                EnableMovementSpeed(Speed.Attack);

                _agent.SetDestination(_targetPlayer.transform.position);

                if (DistFromTarget <= AttackMaxDistancePlayer)
                {
                    PlayAiDamaged();

                    _playerMovement.StartModifySpeed(slowSpeedDuration, 0.3f, 0.3f, 0.8f);
                    _playerLook.StartShake(shakeDuration);

                    Destroy(gameObject);

                    EnableMovementSpeed(Speed.Freeze);

                    _isAlive = false;
                }
                else
                {
                    iaSound.Stop();
                    iaSound.clip = stateOfShock;
                    iaSound.Play();
                }

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

    private void SetTime(float cycleTime) => remainingTime = cycleTime;

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
        switch (selected)
        {
            case Speed.Freeze:
                _agent.speed = 0;
                _agent.acceleration = 9999;
                break;
            case Speed.Attack:
                _agent.speed = Mathf.Clamp(20 - _agent.remainingDistance, 8, 20);
                _agent.acceleration = Acceleration;
                break;
            case Speed.Moving:
                _agent.speed = _normalSpeed;
                _agent.acceleration = Acceleration;
                break;
            case Speed.Hiding:
                _agent.speed = _hidingSpeed;
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

        var radius = _maxSpawnRange;

        // Potential colliders to go to
        Vector3 center = targetPosition;
        Debug.DrawRay(center, Vector3.up * 20, Color.blue, 1, false);
        var hitColliders = new Collider[400];
        Physics.OverlapSphereNonAlloc(center, radius, hitColliders);

        var correctCol = new List<(Collider, float)>();

        for (var index = 0; index < hitColliders.Length && hitColliders[index] != null; index++)
        {
            Collider c = hitColliders[index];
            // Skip invalids
            if (IsInvalidCollider(c) ||
                (c.transform.position - targetPosition).sqrMagnitude <
                minDangerDistFromPlayer * minDangerDistFromPlayer ||
                c == previousCollider ||
                c == currentHidingObstacle ||
                c == _capsuleCollider)
                continue;

            float sqrDist = (c.transform.position - _targetPlayer.transform.position).sqrMagnitude;

            InsertSorted(correctCol, c, sqrDist);
        }

        // Debug
        for (int i = 0; i < hitColliders.Length && hitColliders[i] != null; i++)
        {
            Debug.DrawRay(hitColliders[i].transform.position, Vector3.up * 10 + Vector3.back,
                Color.Lerp(Color.red, Color.yellow, i / (float) hitColliders.Length),
                2, false);
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

        var index = 0;
        var prob = largeSearch ? 0.7f : 0.9f;
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

    private bool IsInvalidCollider(Collider c)
    {
        var transformPosition = c.gameObject.transform.position;
        
        // Refreshes the camera planes
        _targetPlanes = GeometryUtility.CalculateFrustumPlanes(_targetCam);
        if (GeometryUtility.TestPlanesAABB(_targetPlanes, _capsuleCollider.bounds)) return true;
        
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
            StartCoroutine(NullToObstacle());
        }
        Vector3 targetPosition = _targetPlayer.transform.position;

        Vector3 direction = obstaclePosition - targetPosition;

        // Tries to find the point behind the obstacle aligned with the player
        Vector3 hidingSpot = obstaclePosition + direction.normalized * 1.5f;

        // Sticks it to the ground
        if (Physics.Raycast(hidingSpot, Vector3.down, out RaycastHit hit, _characterMaskValue))
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

        Vector3 spawnPointGlobal = _targetPlayer.transform.position +
                                   _targetPlayer.transform.TransformDirection(spawnPointLocal);

        return spawnPointGlobal;
    }
}