using System;
using UnityEngine;

namespace MainGame.Helpers
{
    public class FollowPlayer : ScriptableObject
    {
        public struct RangeCollider
        {
            public float radius
            {
                get => collider.radius;
                set => collider.radius = value;
            }
            public SphereCollider collider;
            public bool isColliding;
            public Quest _quest;
        }

        public RangeCollider goodRadiusCollider, warningRadiusCollider;
        public float _goodRadius;
        public float _warningRadius;

        public void SetupPlayerColliders(GameObject playerToFollow)
        {
            GameObject go = new GameObject("QuestColliders");
            go.transform.SetParent(playerToFollow.transform);

            AddCollider(go, _goodRadius, "goodRadius", out goodRadiusCollider);
            AddCollider(go, _warningRadius, "warningRadius", out warningRadiusCollider);
        }

        private static void AddCollider(GameObject gameObjectToAddColliderTo, float radius, string name_, out RangeCollider rangeCollider)
        {
            GameObject go = new GameObject(name_);
            go.transform.SetParent(gameObjectToAddColliderTo.transform);
            SphereCollider goCollider = go.AddComponent<SphereCollider>();

            rangeCollider = new RangeCollider
            {
                collider = goCollider,
                radius = radius
            };
        }
    }

    public class UpdateCollision : MonoBehaviour
    {
        private FollowPlayer.RangeCollider _rangeCollider;
        private QuestManager.Quest _quest;

        private void OnTriggerEnter(Collider other)
        {
            _rangeCollider.isColliding = true;
        }

        private void OnTriggerExit(Collider other)
        {
            _rangeCollider.isColliding = false;
        }
    }
}