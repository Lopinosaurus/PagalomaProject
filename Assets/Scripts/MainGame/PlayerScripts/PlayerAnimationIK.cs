using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace MainGame.PlayerScripts
{
    public partial class PlayerAnimation
    {
        // Foot IK
        [Space, Header("IK Setup")] public bool enableFootPositionIK;
        public bool enableFootRotationIK, debug;
        [SerializeField, Range(0, 2)] private float rayLength, rayHeightStartOffset;
        [SerializeField, Range(-1, 1)] private float footHeight;
        [SerializeField, Range(10, 50)] private float lerpFoot;
        [SerializeField, Range(0, 0)] private float motionExtrapolationFactor; // not working yet
        private Vector3 _leftFootPosIK, _rightFootPosIK;
        private Vector3 _leftFootPos, _rightFootPos;
        private Quaternion _leftFootRotIK, _rightFootRotIK;
        private const int CharacterLayerValue = 7;

        // Hand IK
        [Space, Header("Right Hand")] [SerializeField]
        private TwoBoneIKConstraint ikConstraint;
        [SerializeField] private Transform rightHand;
        [SerializeField, Range(0, 30)] private float handLerp;
        
        [Space, SerializeField, Range(0, 180)] private float handRotationOffsetX;
        [SerializeField, Range(0, 180)] private float handRotationOffsetY;
        [SerializeField, Range(0, 180)] private float handRotationOffsetZ;
        
        [Space, SerializeField, Range(-1, 1)] private float handPositionOffsetX;
        [SerializeField, Range(-1, 1)] private float handPositionOffsetY;
        [SerializeField, Range(-1, 1)] private float handPositionOffsetZ;
        
        [Space, SerializeField] private Transform targetedObject;
        [SerializeField, Range(0, 1)]  private float DistanceRequiredToTriggerHand = 0.8f;
        private float _ikRightHandPosWeight;


        private void OnAnimatorIK(int layerIndex)
        {
            HandleFeetIKManagement();
            ikConstraint.weight = 0;
            HandleHandsIKManagement();
        }

        private void FixedUpdate()
        {
            Vector3 chestPosition = currentAnimator.GetBoneTransform(HumanBodyBones.Chest).position;
            Vector3 targetedObjectPosition = targetedObject.position;

            bool isDistanceCloseEnough = IsDistanceCloseEnough(chestPosition, targetedObjectPosition, DistanceRequiredToTriggerHand);
            Debug.DrawLine(chestPosition, targetedObjectPosition, isDistanceCloseEnough ? Color.green : Color.red);
            
            _ikRightHandPosWeight = Mathf.Lerp(_ikRightHandPosWeight, isDistanceCloseEnough ? 1 : 0, Time.fixedDeltaTime * handLerp);
        }

        private void HandleHandsIKManagement()
        {
            // ikConstraint.data.target.position = position;
            // ikConstraint.weight = Mathf.Lerp(ikConstraint.weight, desiredWeight, Time.deltaTime * targetLerp);

            // Get the data
            Vector3 targetedObjectPos = targetedObject.position;

            // Adjust the hand rotation
            Vector3 rightHandForward = rightHand.forward;
            Vector3 rightHandPosition = rightHand.position;
            Quaternion rightHandRotationIk = Quaternion.LookRotation(rightHandForward, (rightHandPosition - targetedObjectPos));
            rightHandRotationIk *= Quaternion.Euler(handRotationOffsetX, handRotationOffsetY, handRotationOffsetZ);
            
            // Adjust the hand position
            Vector3 rightHandUp = rightHand.up, rightHandRight = rightHand.right;
            Vector3 rightHandPositionIk = rightHandPosition + rightHandForward * handPositionOffsetX + rightHandUp * handPositionOffsetY + rightHandRight * handPositionOffsetZ;

            currentAnimator.SetIKPosition(AvatarIKGoal.RightHand, rightHandPositionIk);
            currentAnimator.SetIKRotation(AvatarIKGoal.RightHand, rightHandRotationIk);
            
            currentAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, _ikRightHandPosWeight);
            currentAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, _ikRightHandPosWeight);
        }

        private static bool IsDistanceCloseEnough(Vector3 position1, Vector3 position2, float distance)
        {
            return (position1 - position2).sqrMagnitude <= distance * distance;
        }

        private void HandleFeetIKManagement()
        {
            // Ik setup
            currentAnimator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, enableFootPositionIK ? 1 : 0);
            currentAnimator.SetIKPositionWeight(AvatarIKGoal.RightFoot, enableFootPositionIK ? 1 : 0);
            currentAnimator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, enableFootRotationIK ? 0.5f : 0);
            currentAnimator.SetIKRotationWeight(AvatarIKGoal.RightFoot, enableFootRotationIK ? 0.5f : 0);

            if (enableFootPositionIK)
            {
                // Stores current foot positions
                _leftFootPosIK = currentAnimator.GetIKPosition(AvatarIKGoal.LeftFoot);
                _rightFootPosIK = currentAnimator.GetIKPosition(AvatarIKGoal.RightFoot);
                
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
                
                // Set current foot IK positions
                currentAnimator.SetIKPosition(AvatarIKGoal.LeftFoot, _leftFootPosIK);
                currentAnimator.SetIKPosition(AvatarIKGoal.RightFoot, _rightFootPosIK);
            }

            if (enableFootRotationIK)
            {
                currentAnimator.SetIKRotation(AvatarIKGoal.LeftFoot, _leftFootRotIK);
                currentAnimator.SetIKRotation(AvatarIKGoal.RightFoot, _rightFootRotIK);
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