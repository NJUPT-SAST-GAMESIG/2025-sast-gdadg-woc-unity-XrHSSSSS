using System.Collections.Generic;
using UnityEngine;
using Tools;

namespace Character.Player
{
    public class PlayerAttackDetection : MonoBehaviour
    {
        [Header("攻击检测")]
        [SerializeField] private Transform attackOrigin;
        [SerializeField] private Transform forwardReference;
        [SerializeField] private LayerMask targetLayerMask = ~0;  
        [SerializeField, Min(0.01f)] private float detectRadius = 2.2f;        // 碰撞检测半径
        [SerializeField, Min(0.01f)] private float maxDetectDistance = 2.2f;   
        [SerializeField, Range(1f, 180f)] private float maxDetectAngle = 90f;  
        [SerializeField] private bool includeTriggerCollider = false;          

        private readonly HashSet<Transform> _filteredTargets = new HashSet<Transform>();

        private void Awake()
        {
            if (attackOrigin == null)
            {
                attackOrigin = transform;
            }

            if (forwardReference == null)
            {
                forwardReference = transform;
            }
        }

        public int ExecuteAttackDetection()
        {
            Transform originTransform = attackOrigin != null ? attackOrigin : transform;
            Transform facingTransform = forwardReference != null ? forwardReference : transform;
            Vector3 origin = originTransform.position;
            QueryTriggerInteraction queryTrigger = includeTriggerCollider ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;

            Collider[] overlaps = Physics.OverlapSphere(origin, detectRadius, targetLayerMask, queryTrigger);
            _filteredTargets.Clear();

            for (int i = 0; i < overlaps.Length; i++)
            {
                Collider current = overlaps[i];
                if (current == null)
                {
                    continue;
                }

                Transform target = current.attachedRigidbody != null ? current.attachedRigidbody.transform : current.transform;
                target = target.root;

                // 以root去重，避免同一角色多个碰撞体被重复命中
                if (target == transform.root || _filteredTargets.Contains(target))
                {
                    continue;
                }

                Vector3 toTarget = target.position - origin;
                float distance = DevelopmentTools.DistanceForTarget(target, originTransform);
                if (distance > maxDetectDistance)
                {
                    continue;
                }

                if (distance <= 0.0001f)
                {
                    continue;
                }

                float deltaAngle = Mathf.Abs(DevelopmentTools.GetDeltaAngle(facingTransform, toTarget.normalized));
                // 扇形
                if (deltaAngle > maxDetectAngle * 0.5f)
                {
                    continue;
                }

                _filteredTargets.Add(target);
            }

            foreach (Transform target in _filteredTargets)
            {
                Debug.Log($"Hit target: {target.name}", target);
                target.gameObject.SendMessage("OnPlayerAttackHit", SendMessageOptions.DontRequireReceiver);
            }

            Debug.Log($"[PlayerAttackDetection] Detect complete. Hit Count: {_filteredTargets.Count}", this);
            return _filteredTargets.Count;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 origin = attackOrigin != null ? attackOrigin.position : transform.position;
            Vector3 forward = forwardReference != null ? forwardReference.forward : transform.forward;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(origin, detectRadius);

            Vector3 leftLimit = Quaternion.Euler(0f, -maxDetectAngle * 0.5f, 0f) * forward;
            Vector3 rightLimit = Quaternion.Euler(0f, maxDetectAngle * 0.5f, 0f) * forward;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(origin, origin + leftLimit * maxDetectDistance);
            Gizmos.DrawLine(origin, origin + rightLimit * maxDetectDistance);
            Gizmos.DrawLine(origin, origin + forward * maxDetectDistance);
        }
    }
}
