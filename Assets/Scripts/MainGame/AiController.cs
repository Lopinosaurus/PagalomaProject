using System;
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
    public Transform greenSphere;
    
    [SerializeField] private Role targetRole;
    private GameObject targetPlayer;
    private Camera targetCam;
    private Plane[] targetPlanes;
    
    private NavMeshAgent _navMeshAgent;
    private CapsuleCollider _capsuleCollider;
    public bool isViewed;
    public bool isInCameraField;
    private enum AiState
    {
        Hidden,
        Transition,
        Caught
    }

    
    // Insert the LayerMask corresponding the player 
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private string obstacleTag;
    private int obstacleTagHash;

    // Gameplay stats
    [SerializeField] private AiState currentState = AiState.Hidden;
    [SerializeField] private float remainingTimeBeforeTransition;
    [SerializeField] private float timeBeingCaught;
    [SerializeField] private int remainingHealth = 3;
    private Collider currentHidingObstacle;
    private int moveCount = 0;
    
    private float distanceFromTarget => Vector3.Distance(transform.position, targetPlayer.transform.position);
    
    private const float cycleTime = 5;
    private const float maxBeingCaughtDelay = 0.8f;
    private const float maxWaitingTimeBeforeMoving = -10;
    
    private const float distanceToPlayerMaxBeforeAttacking = 10;
    private const float allowedMaxDistanceFromDestination = 0.5f;
    private const float distanceFromObjectMinTreshold = 1;
    private const float distanceFromObjectMaxTreshold = 1;
    private const float obstacleMaxHeightTreshold = 1.5f;
    
    // Spawn settings
    public  float minSpawnRange = 30f;
    public  float maxSpawnRange = 40f;
    
    // NavMeshAgent settings
    [Range(0.01f, 100f)]
    public float regularSpeed = 20f;
    private const float acceleration = 100f;

    private void Awake()
    {
        targetPlayer = targetRole.gameObject;
        targetCam = targetPlayer.GetComponentInChildren<Camera>();
        
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
        if (_capsuleCollider == null) throw new Exception("null collider");

        obstacleTagHash = obstacleTag.GetHashCode();
        remainingTimeBeforeTransition = cycleTime;
        
        // NavMesh settings
        _navMeshAgent.speed = regularSpeed;
        _navMeshAgent.acceleration = acceleration;
        _navMeshAgent.stoppingDistance = 0.5f;
        
        transform.position = SpawnBehindPlayer(minSpawnRange, maxSpawnRange);
    }

    private void Update()
    {
        isViewed = IsInCameraView();
        
        // Decides if the player is being observed
        if (!isViewed)
        {
            // Rotates towards the player only if is not looked at
            transform.LookAt(targetPlayer.transform.position);
        }
        else
        {
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
        if (Input.GetMouseButton(0)) Debug.DrawLine(_navMeshAgent.destination, transform.position, Color.red);

        // The clock decreases
        remainingTimeBeforeTransition -= Time.fixedDeltaTime;

        switch (currentState)
        {
            case AiState.Hidden:
            {
                EnableMovement(false);
                
                // The Ai only moves if it won't be seen after moving
                if (isInCameraField)
                {
                    // If the Ai will be seen by moving, but it's been too long, it moves behind the player
                    if (remainingTimeBeforeTransition < maxWaitingTimeBeforeMoving)
                    {
                        var distance = distanceFromTarget;
                        transform.position = SpawnBehindPlayer(distance, distance);

                        remainingTimeBeforeTransition = cycleTime;
                    }
                }
                else
                // The Ai can here move without being seen
                {
                    if (remainingTimeBeforeTransition <= 0)
                    {
                        // Updates the destination to the hiding place position
                        Vector3 newHidingPlace = FindHidingSpot(currentHidingObstacle);
                        _navMeshAgent.SetDestination(newHidingPlace);
                        Debug.DrawLine(transform.position, _navMeshAgent.destination);

                        // Starts moving
                        SetCurrentState(AiState.Transition);
                    }
                }
                break;
            }
            
            case AiState.Transition:
                
                EnableMovement(true);
                
                // Makes sure the Ai won't try to move somewhere else while already moving
                remainingTimeBeforeTransition = cycleTime;
                
                // Decides if the Ai is finally hidden
                if (_navMeshAgent.remainingDistance < allowedMaxDistanceFromDestination)
                {
                    SetCurrentState(AiState.Hidden);
                }
                
                break;
            
            case AiState.Caught:
                // Removes "health" points to the Ai
                remainingHealth--;
                
                // Makes the Ai go away
                transform.position = SpawnBehindPlayer(minSpawnRange, maxSpawnRange);
                SetCurrentState(AiState.Hidden);
                
                // Resets the timer
                remainingTimeBeforeTransition = cycleTime * 1.5f;
                
                Debug.Log("Caught !");
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
            isInCameraField = true;
            
            Vector3 camPosition = targetCam.transform.position;
            
            float colliderHeight = _capsuleCollider.height;
            Vector3 ColliderPosition = _capsuleCollider.transform.position;
            Vector3 colliderCenter = ColliderPosition + Vector3.up * colliderHeight / 2;
            
            Vector3 dirCamToCenter = colliderCenter - camPosition;
            
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
            isInCameraField = false;
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
            case false:
                _navMeshAgent.speed = 0;
                _navMeshAgent.acceleration = 9999;
                break;
        }
    }

    private Collider FindHidingObstacle()
    {
        Vector3 position = transform.position;
        
        Vector3 targetPosition = targetPlayer.transform.position;
        Vector3 center = (position + targetPosition) / 2;

        Collider hidingCollider = _capsuleCollider;
        
        // Potential colliders to go to
        List<Collider> hitColliders = new List<Collider>(Physics.OverlapSphere(center, distanceFromTarget));

        // Filters out colliders that are too far
        if (hitColliders.Count > 0)
        {
            List<Collider> tooCloseColliders = new List<Collider>();
            List<Collider> farAndCloseEnoughColliders = new List<Collider>();
            
            // Looks for valid colliders
            foreach (Collider _collider in hitColliders)
            {
                float distanceBetweenColliderAndPlayer = Vector3.Distance(_collider.ClosestPoint(position), targetPosition);

                if (_collider.gameObject.layer != playerMask)
                {
                    // If further than a given distance, then invalid
                    if (distanceBetweenColliderAndPlayer < Vector3.Distance(position, targetPosition))
                    {
                        // If closer than a given distance, then invalid
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
        
        
        return hidingCollider;
    }

    private Vector3 FindHidingSpot(Collider hidingObstacle = null)
    {
        currentHidingObstacle = hidingObstacle ? FindHidingObstacle() : _capsuleCollider;
        
        Vector3 obstaclePosition = hidingObstacle.transform.position;
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
