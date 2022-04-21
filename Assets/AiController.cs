using System;
using MainGame.PlayerScripts.Roles;
using UnityEngine;
using UnityEngine.AI;
using static System.Single;

public class AiController : MonoBehaviour
{
    [SerializeField] private Role targetRole;
    private GameObject targetPlayer;
    private Camera targetCam;
    private Plane[] targetPlanes;
    
    private NavMeshAgent _navMeshAgent;
    private CapsuleCollider _capsuleCollider;
    public bool isViewed;
    // Insert the LayerMask corresponding the player 
    [SerializeField] private LayerMask PlayerMask;
    
    private int remainingHealth;
    private float distanceFromTarget;
    private float timeBeforeSwitching;
    
    // NavMeshAgent settings
    [Range(0.01f, 100f)]
    public float regularSpeed = 2f;
    private const float acceleration = 3f;

    private void Awake()
    {
        targetPlayer = targetRole.gameObject;
        targetCam = targetPlayer.GetComponentInChildren<Camera>();
        
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _capsuleCollider = GetComponent<CapsuleCollider>();
        if (_capsuleCollider == null) throw new Exception("null collider");
        
        // NavMesh settings
        _navMeshAgent.speed = regularSpeed;
        _navMeshAgent.acceleration = acceleration;
    }

    private bool IsInCameraView()
    {
        // Refreshes the camera planes
        targetPlanes = GeometryUtility.CalculateFrustumPlanes(targetCam);
        
        if (GeometryUtility.TestPlanesAABB(targetPlanes, _capsuleCollider.bounds))
        {
            Vector3 camPosition = targetCam.transform.position;
            
            Vector3 ColliderPosition = _capsuleCollider.transform.position;
            float colliderHeight = _capsuleCollider.height;
            Vector3 colliderCenter = Vector3.up * colliderHeight / 2 - camPosition;
            
            Vector3 dirCamToCenter = colliderCenter - camPosition;

            // All possible destinations - the more the more accurate
            Vector3[] destinations =
            {
                dirCamToCenter,
                ColliderPosition,
                ColliderPosition + Vector3.up * colliderHeight
            };
            
            foreach (Vector3 destination in destinations)
            {
                // Ray from camera to the chosen destination
                Debug.DrawRay(camPosition, destination - camPosition);
                if (Physics.Raycast(camPosition, destination - camPosition, out RaycastHit hit,
                        PositiveInfinity))
                {
                    Debug.Log(hit.collider);

                    if (hit.collider == _capsuleCollider)
                    {
                        return true;
                    }
                }
                else
                {
                    Debug.Log("No collider found");
                }
            }
        }
        
        return false;
    }

    private void Update()
    {
        
        
        // Decides if the player is being observed
        if (isViewed)
        {
            Debug.Log("Object in sight");

            // Stops the GameObject
            EnableMovement(false);
        }
        else
        {
            Debug.Log("Object out of sight");
            
            // Rotates towards the player only if is not looked at
            transform.LookAt(targetPlayer.transform.position);
            
            // Makes the GameObject move
            EnableMovement(true);
        }
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

    private void FixedUpdate()
    {
        isViewed = IsInCameraView();
                    
        // Updates the destination to the player's position
        _navMeshAgent.SetDestination(targetPlayer.transform.position);
    }
}
