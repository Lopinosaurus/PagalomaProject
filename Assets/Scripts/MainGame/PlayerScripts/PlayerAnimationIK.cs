using System;
using UnityEngine;

namespace MainGame.PlayerScripts
{
    public partial class PlayerAnimation : MonoBehaviour
    {
        // Foot IK
        [Space, Header("IK Setup")]
        public bool enableFootIK;
        [SerializeField] private Transform LeftFoot, RightFoot;
        [SerializeField, Range(0, 1)] private float RayToFloorDistance;
        [SerializeField, Range(-1, 1)] private float RayOffset;
        [SerializeField, Range(-0.2f, 0.2f)] private float footHeightOffset;
        [SerializeField, Range(-120, 120)] private float rotationOffset;
        private readonly int _characterLayerValue = 7;

        private void OnAnimatorIK(int layerIndex)
        {
            currentAnimator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
            currentAnimator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
            currentAnimator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
            currentAnimator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
            
            // Get new foot placement
            Vector3 leftFootPos = currentAnimator.GetIKPosition(AvatarIKGoal.LeftFoot);
            Quaternion leftFootRot = currentAnimator.GetIKRotation(AvatarIKGoal.LeftFoot);
            Ray leftFootRay = new Ray(leftFootPos + Vector3.up * RayOffset, Vector3.down);
            
            if (Physics.Raycast(leftFootRay, out RaycastHit hit, RayToFloorDistance, _characterLayerValue))
            {
                Debug.DrawRay(leftFootRay.origin, leftFootRay.direction, Color.red);
                leftFootPos = hit.point + Vector3.up * footHeightOffset;
                leftFootRot = Quaternion.LookRotation(LeftFoot.forward, hit.normal);
                leftFootRot *= Quaternion.AngleAxis(rotationOffset, Vector3.right);
            }
            
            currentAnimator.SetIKPosition(AvatarIKGoal.LeftFoot, leftFootPos);
            currentAnimator.SetIKRotation(AvatarIKGoal.LeftFoot, leftFootRot); 
            
            // Get new foot placement
            Vector3 rightFootPos = currentAnimator.GetIKPosition(AvatarIKGoal.RightFoot);
            Quaternion rightFootRot = currentAnimator.GetIKRotation(AvatarIKGoal.RightFoot);
            Ray rightFootRay = new Ray(rightFootPos + Vector3.up * RayOffset, Vector3.down);
            
            if (Physics.Raycast(rightFootRay, out RaycastHit hit2, RayToFloorDistance, _characterLayerValue))
            {
                Debug.DrawRay(rightFootRay.origin, rightFootRay.direction, Color.red);
                rightFootPos = hit2.point + Vector3.up * footHeightOffset;
                rightFootRot = Quaternion.LookRotation(RightFoot.forward, hit2.normal);
                rightFootRot *= Quaternion.AngleAxis(rotationOffset, Vector3.right);
            }
            
            currentAnimator.SetIKPosition(AvatarIKGoal.RightFoot, rightFootPos);
            currentAnimator.SetIKRotation(AvatarIKGoal.RightFoot, rightFootRot); 
        }
    }
}