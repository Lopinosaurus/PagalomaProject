using UnityEngine;

namespace MainGame.PlayerScripts
{
    public partial class PlayerAnimation
    {
        // Foot IK
        [Space, Header("IK Setup")]
        public bool enableFootPositionIK;
        public bool enableFootRotationIK;
        public bool debug;
        [SerializeField] private Transform LeftFoot, RightFoot;
        public float lowestPointSoFar;
        [SerializeField, Range(0, 2)] private float RayToFloorDistance;
        [SerializeField, Range(0, 2)] private float RayStartOffset;
        [SerializeField, Range(-1f, 1f)] private float footHeightOffset;
        [SerializeField, Range(0, 1)] private float lerpFactor;
        private readonly int _characterLayerValue = 7;
        [SerializeField] private Vector3 IKHint;

        private void OnAnimatorIK(int layerIndex)
        {
            currentAnimator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
            currentAnimator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
            
            currentAnimator.SetIKHintPosition(AvatarIKHint.LeftKnee, IKHint);
            currentAnimator.SetIKHintPosition(AvatarIKHint.RightKnee, IKHint);
            
            // Get new foot placements
            Vector3 leftFootPos = currentAnimator.GetIKPosition(AvatarIKGoal.LeftFoot);
            Vector3 rightFootPos = currentAnimator.GetIKPosition(AvatarIKGoal.RightFoot);
            Vector3 fixedLeftFootPos = leftFootPos;
            Vector3 fixedRightFootPos = rightFootPos;
            
            Quaternion leftFootRot = currentAnimator.GetIKRotation(AvatarIKGoal.LeftFoot);
            Quaternion rightFootRot = currentAnimator.GetIKRotation(AvatarIKGoal.RightFoot);
            Quaternion fixedLeftFootRot = leftFootRot;
            Quaternion fixedRightFootRot = rightFootRot;
            
            Ray leftFootRay = new Ray(leftFootPos + RayStartOffset * Vector3.up, Vector3.down * 10);
            Ray rightFootRay = new Ray(rightFootPos+RayStartOffset * Vector3.up, Vector3.down * 10);

            // Left foot
            if (Physics.Raycast(leftFootRay, out RaycastHit hit, RayToFloorDistance, _characterLayerValue))
            {
                if (debug) Debug.DrawRay(leftFootRay.origin, leftFootRay.direction, Color.red);
                
                leftFootRot = Quaternion.LookRotation(LeftFoot.forward, hit.normal);                
                
                Vector3 correctedLeftFootPos = hit.point + Vector3.up * footHeightOffset;
                if (leftFootRot.y < correctedLeftFootPos.y) leftFootPos = correctedLeftFootPos;
            }
            
            // Right foot
            if (Physics.Raycast(rightFootRay, out RaycastHit hit2, RayToFloorDistance, _characterLayerValue))
            {
                if (debug) Debug.DrawRay(rightFootRay.origin, rightFootRay.direction, Color.red);
                
                rightFootRot = Quaternion.LookRotation(RightFoot.forward, hit2.normal);                
            
                Vector3 correctedRightFootPos = hit2.point + Vector3.up * footHeightOffset;
                if (rightFootPos.y < correctedRightFootPos.y) rightFootPos = correctedRightFootPos;
            }
            
            if (enableFootPositionIK)
            {
                var lerpLeft = Vector3.Lerp(fixedLeftFootPos, leftFootPos, lerpFactor);
                var lerpRight = Vector3.Lerp(fixedRightFootPos, rightFootPos, lerpFactor);
                currentAnimator.SetIKPosition(AvatarIKGoal.LeftFoot, lerpLeft);
                currentAnimator.SetIKPosition(AvatarIKGoal.RightFoot, lerpRight);
            }
            
            if (enableFootRotationIK)
            {
                var lerpLeft = Quaternion.Lerp(fixedLeftFootRot, leftFootRot, lerpFactor);
                var lerpRight = Quaternion.Lerp(fixedRightFootRot, rightFootRot, lerpFactor);
                currentAnimator.SetIKRotation(AvatarIKGoal.LeftFoot, lerpLeft);
                currentAnimator.SetIKRotation(AvatarIKGoal.RightFoot, lerpRight);
            }
            
            if (debug)
            {
                Debug.DrawRay(hit.point, hit.normal, Color.green);
                Debug.DrawRay(hit2.point, hit2.normal, Color.green);
            }
        }
    }
}