using UnityEngine;

namespace MainGame.Helpers
{
    public class FollowPlayer : ScriptableObject
    {
        public SphereCollider _goodRadiusCollider, _warningRadiusCollider, _tooFarRadiusCollider;
        public float _goodRadius;
        public float _warningRadius;
        public float _tooFarRadius;

        public void SetupPlayer(GameObject playerToFollow)
        {
            GameObject go = new GameObject("QuestColliders");
            go.transform.SetParent(playerToFollow.transform);

            AddCollider(go, _goodRadius, "goodRadius");
            AddCollider(go, _warningRadius, "warningRadius");
            AddCollider(go, _tooFarRadius, "tooFarRadius");
        }

        private static void AddCollider(GameObject gameObjectToAddColliderTo, float radius, string name_)
        {
            GameObject go = new GameObject(name_);
            go.transform.SetParent(gameObjectToAddColliderTo.transform);
            SphereCollider goCollider = go.AddComponent<SphereCollider>();
            goCollider.radius = radius;
            goCollider.isTrigger = true;
        }
    }
}