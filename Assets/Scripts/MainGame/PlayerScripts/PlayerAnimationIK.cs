using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace MainGame.PlayerScripts
{
    public partial class PlayerAnimation
    {
        // Render shift
        private Vector3 RenderShift => transform.position - PC.renders.transform.position;
        
        // Foot IK
        [Space, Header("IK Setup")] public bool enableFootPositionIK;
        public bool enableFootRotationIK, enableRightHandIK, debug;
        [SerializeField, Range(0, 2)] private float rayLength, rayHeightStartOffset;
        [SerializeField, Range(-1, 1)] private float footHeight;
        [SerializeField, Range(10, 50)] private float lerpFoot;
        [SerializeField, Range(0, 0)] private float motionExtrapolationFactor; // not working yet
        private Vector3 _leftFootPosIK, _rightFootPosIK;
        private Vector3 _leftFootPos, _rightFootPos;
        private Quaternion _leftFootRotIK, _rightFootRotIK;
        private const int CharacterLayerValue = 7;

        // Hand IK
        [Space, Header("Right Hand")]
        [SerializeField] private TwistChainConstraint chestIKConstraint;
        [SerializeField] private Transform rightHand;
        [SerializeField, Range(0, 30)] private float handLerp;
        
        [Space, SerializeField, Range(-180, 180)] private float handRotationOffsetX;
        [SerializeField, Range(-180, 180)] private float handRotationOffsetY;
        [SerializeField, Range(-180, 180)] private float handRotationOffsetZ;
        
        [Space, SerializeField, Range(-1, 1)] private float handPositionOffsetX;
        [SerializeField, Range(-1, 1)] private float handPositionOffsetY;
        [SerializeField, Range(-1, 1)] private float handPositionOffsetZ;
        
        [Space, SerializeField] private Transform targetedObject;
        [SerializeField, Range(0, 10)]  private float distanceRequiredToTriggerHand = 0.8f;
        private float _ikRightHandPosWeight;


        private void OnAnimatorIK(int layerIndex)
        {
            HandleFeetIKManagement();
            HandleHandsIKManagement();
        }

        private void FixedUpdate()
        {
            HandleHandToggleIK();
        }

        private void HandleHandToggleIK()
        {
            if (!targetedObject || !enableRightHandIK) return;
            
            Vector3 chestPosition = _currentAnimator.GetBoneTransform(HumanBodyBones.Chest).position;
            Vector3 targetedObjectPosition = targetedObject.position;

            bool isDistanceCloseEnough = IsDistanceCloseEnough(chestPosition, targetedObjectPosition, distanceRequiredToTriggerHand);
            Debug.DrawLine(chestPosition, targetedObjectPosition, isDistanceCloseEnough ? Color.green : Color.red);
            
            _ikRightHandPosWeight = Mathf.Lerp(_ikRightHandPosWeight, isDistanceCloseEnough ? 1 : 0, Time.fixedDeltaTime * handLerp);
            
            // Set the weight of the torso constraint based on the distance between target and right hand
            chestIKConstraint.weight = Mathf.Clamp01(1 - (targetedObjectPosition - chestPosition).magnitude);
        }

        private void HandleHandsIKManagement()
        {
            if (!targetedObject) return;
            
            // Get the data
            Vector3 rightHandPosition = _currentAnimator.GetIKPosition(AvatarIKGoal.RightHand);
            Vector3 targetClosestPoint;
            try
            {
                targetClosestPoint = targetedObject.GetComponent<SphereCollider>().ClosestPoint(rightHandPosition);
            }
            catch
            {
                targetClosestPoint = targetedObject.position;
            }
            

            // Adjust the hand rotation
            Vector3 rightHandForward = rightHand.forward, rightHandUp = rightHand.up, rightHandRight = rightHand.right;
            Quaternion rightHandRotationIk = Quaternion.LookRotation(rightHandForward, (rightHandPosition - targetClosestPoint));
            rightHandRotationIk *= Quaternion.Euler(handRotationOffsetX, handRotationOffsetY, handRotationOffsetZ);
            
            // Adjust the hand position
            Vector3 rightHandPositionIk = targetClosestPoint + rightHandForward * handPositionOffsetX + rightHandUp * handPositionOffsetY + rightHandRight * handPositionOffsetZ;
            
            // Render shift
            rightHandPositionIk += RenderShift;
            
            _currentAnimator.SetIKPosition(AvatarIKGoal.RightHand, rightHandPositionIk);
            // _currentAnimator.SetIKHintPosition(AvatarIKHint.RightElbow, 0.5f * (rightHandPositionIk 
            //     + _currentAnimator.GetBoneTransform(HumanBodyBones.RightShoulder).position));
            _currentAnimator.SetIKRotation(AvatarIKGoal.RightHand, rightHandRotationIk);
            
            _currentAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, _ikRightHandPosWeight);
            _currentAnimator.SetIKHintPositionWeight(AvatarIKHint.RightElbow, _ikRightHandPosWeight);
            _currentAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, _ikRightHandPosWeight);
        }

        private static bool IsDistanceCloseEnough(Vector3 position1, Vector3 position2, float distance)
        {
            return (position1 - position2).sqrMagnitude <= distance * distance;
        }

        private void HandleFeetIKManagement()
        {
            // Ik setup
            _currentAnimator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, enableFootPositionIK ? 1 : 0);
            _currentAnimator.SetIKPositionWeight(AvatarIKGoal.RightFoot, enableFootPositionIK ? 1 : 0);
            _currentAnimator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, enableFootRotationIK ? 0.5f : 0);
            _currentAnimator.SetIKRotationWeight(AvatarIKGoal.RightFoot, enableFootRotationIK ? 0.5f : 0);

            if (enableFootPositionIK)
            {
                // Stores current foot positions
                Vector3 renderShift = RenderShift;
                _leftFootPosIK = _currentAnimator.GetIKPosition(AvatarIKGoal.LeftFoot) - renderShift;
                _rightFootPosIK = _currentAnimator.GetIKPosition(AvatarIKGoal.RightFoot) - renderShift;
                
                // Set new foot placements
                AdjustFootPositionAndRotation(ref _leftFootPosIK, out _leftFootRotIK);
                AdjustFootPositionAndRotation(ref _rightFootPosIK, out _rightFootRotIK);

                _leftFootPos.x = _leftFootPosIK.x; _leftFootPos.z = _leftFootPosIK.z;
                _rightFootPos.x = _rightFootPosIK.x; _rightFootPos.z = _rightFootPosIK.z;
                
                _leftFootPos.y = Mathf.Lerp(_leftFootPos.y, _leftFootPosIK.y, Time.deltaTime * lerpFoot);
                _rightFootPos.y = Mathf.Lerp(_rightFootPos.y, _rightFootPosIK.y, Time.deltaTime * lerpFoot);
                
                _leftFootPosIK = _leftFootPos;
                _rightFootPosIK = _rightFootPos;

                
                if (debug)
                {
                    foreach (Vector3 foot in new[] {_leftFootPosIK, _rightFootPosIK})
                        for (int i = -1; i <= 1; i++)
                        for (int j = -1; j <= 1; j++)
                        for (int k = -1; k <= 1; k++)
                            if (i * j * k == 0) Debug.DrawRay(foot, new Vector3(i, j, k) * 0.08f, Color.red);
                }

                _leftFootPosIK += renderShift;
                _rightFootPosIK += renderShift;
                
                // Set current foot IK positions
                _currentAnimator.SetIKPosition(AvatarIKGoal.LeftFoot, _leftFootPosIK);
                _currentAnimator.SetIKPosition(AvatarIKGoal.RightFoot, _rightFootPosIK);
            }

            if (enableFootRotationIK)
            {
                _currentAnimator.SetIKRotation(AvatarIKGoal.LeftFoot, _leftFootRotIK);
                _currentAnimator.SetIKRotation(AvatarIKGoal.RightFoot, _rightFootRotIK);
            }
        }

        private void AdjustFootPositionAndRotation(ref Vector3 footPosIK, out Quaternion footRotIK)
        {
            // Position
            Vector3 motionLocal = new Vector3(VelocityX, 0, VelocityZ);
            Vector3 motionWorld = transform.TransformDirection(motionLocal);
            
            Vector3 origin = footPosIK + Vector3.up * rayHeightStartOffset + motionWorld * motionExtrapolationFactor;
            // if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayLength, CharacterLayerValue))
            if (Physics.SphereCast(origin, 0.1f, Vector3.down, out RaycastHit hit, rayLength, CharacterLayerValue))
            {
                // Correction
                if (footPosIK.y - footHeight < hit.point.y) footPosIK.y = hit.point.y + footHeight;
            }
            
            if (debug) Debug.DrawRay(hit.point, Vector3.up, Color.red);
            
            // Rotation
            footRotIK = Quaternion.LookRotation(transform.forward, hit.normal);
        }
    }
}