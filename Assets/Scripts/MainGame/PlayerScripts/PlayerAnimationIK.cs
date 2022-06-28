using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace MainGame.PlayerScripts
{
    public partial class PlayerAnimation
    {
        // Foot IK
        [Space, Header("IK Setup")] public bool enableFootPositionIK;
        public bool enableFootRotationIK, debug;
        [SerializeField] private Transform leftFoot, rightFoot;
        [SerializeField, Range(0, 2)] private float rayLength, rayHeightStartOffset;
        [SerializeField, Range(-1f, 1f)] private float footHeight;
        [SerializeField, Range(0, 1)] private float lerpFoot;
        private Vector3 _leftFootPosIK, _rightFootPosIK;
        private const int CharacterLayerValue = 7;

        [Space, Header("Right Hand")] [SerializeField]
        private TwoBoneIKConstraint ikConstraint;

        [SerializeField] private Transform rightHand;
        [SerializeField, Range(0, 30)] private float handLerp;
        [SerializeField] private Transform targetedObject;
        private float _ikRightHandPosWeight;
        [SerializeField, Range(0, 1)]  private float minDistanceFromHandTarget = 0.8f;

        private void OnAnimatorIK(int layerIndex)
        {
            HandleFeetIKManagement();
            ikConstraint.weight = 0;
            HandleHandsIKManagement();
        }

        private void FixedUpdate()
        {
            _leftFootPosIK = currentAnimator.GetBoneTransform(HumanBodyBones.LeftFoot).position;
            _rightFootPosIK = currentAnimator.GetBoneTransform(HumanBodyBones.RightFoot).position;
            
            // Set new foot placements
            GetNewFootPosition(ref _leftFootPosIK);
            GetNewFootPosition(ref _rightFootPosIK);
            
            Debug.Log($"{_leftFootPosIK} {_rightFootPosIK}");
            
            //////////////////////////
            
            Vector3 chestPosition = currentAnimator.GetBoneTransform(HumanBodyBones.Chest).position;
            Vector3 targetedObjectPosition = targetedObject.position;

            bool isDistanceCloseEnough = IsDistanceCloseEnough(chestPosition, targetedObjectPosition, minDistanceFromHandTarget);

            Debug.DrawLine(chestPosition, targetedObjectPosition, isDistanceCloseEnough ? Color.green : Color.red);
            
            _ikRightHandPosWeight = Mathf.Lerp(_ikRightHandPosWeight, isDistanceCloseEnough ? 1 : 0, Time.fixedDeltaTime * handLerp);
        }

        private void HandleHandsIKManagement()
        {
            // ikConstraint.data.target.position = position;
            // ikConstraint.weight = Mathf.Lerp(ikConstraint.weight, desiredWeight, Time.deltaTime * targetLerp);

            Vector3 targetedObjectPosition = targetedObject.position;

            currentAnimator.SetIKPosition(AvatarIKGoal.RightHand, targetedObjectPosition);
            // currentAnimator.SetIKRotation(AvatarIKGoal.RightHand, targetedObjectRotation);
            
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

            if (enableFootPositionIK)
            {
                // Vector3 leftLerped = Vector3.Lerp(currentAnimator.GetIKPosition(AvatarIKGoal.LeftFoot),
                //     leftFootPosIK, Time.deltaTime * lerpFoot);
                // Vector3 rightLerped = Vector3.Lerp(currentAnimator.GetIKPosition(AvatarIKGoal.RightFoot),
                //     rightFootPosIK, Time.deltaTime * lerpFoot);
                // currentAnimator.SetIKPosition(AvatarIKGoal.LeftFoot, leftLerped);
                // currentAnimator.SetIKPosition(AvatarIKGoal.RightFoot, rightLerped);
                
                currentAnimator.SetIKPosition(AvatarIKGoal.LeftFoot, _leftFootPosIK);
                currentAnimator.SetIKPosition(AvatarIKGoal.RightFoot, _rightFootPosIK);
            }

            if (enableFootRotationIK)
            {
                // currentAnimator.SetIKRotation(AvatarIKGoal.LeftFoot, lerpLeft);
                // currentAnimator.SetIKRotation(AvatarIKGoal.RightFoot, lerpRight);
            }
        }

        private void GetNewFootPosition(ref Vector3 footPosIK)
        {
            Vector3 origin = footPosIK + Vector3.up * rayHeightStartOffset;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayLength, CharacterLayerValue))
            {
                // Correction
                if (footPosIK.y - footHeight < hit.point.y) footPosIK.y = hit.point.y + footHeight;
            }
            
            if (debug)
            {
                Debug.DrawRay(footPosIK, hit.normal, Color.green);
            }
        }
    }
}