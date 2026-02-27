using Character.Player;
using UnityEngine;
using UnityEngine.AI;

namespace Character.Enemy
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAIController : MonoBehaviour
    {
        [Header("巡逻")]
        [SerializeField] private Transform[] patrolPoints;
        [SerializeField] private float patrolWaitTime = 1f;
        [SerializeField] private float patrolArriveDistance = 0.6f;

        [Header("索敌")]
        [SerializeField] private float searchRadius = 12f;
        [SerializeField, Range(0f, 360f)] private float viewAngle = 120f;
        [SerializeField] private LayerMask targetMask = ~0;
        [SerializeField] private LayerMask obstacleMask;
        [SerializeField] private float loseTargetDelay = 2f;

        [Header("攻击")]
        [SerializeField] private float attackRange = 1.8f;
        [SerializeField] private float attackCooldown = 1.2f;
        [SerializeField] private float attackDamage = 10f;
        [SerializeField] private string attackAnimationTrigger = "Attack";

        [Header("调试")]
        [SerializeField] private bool enableDebugLog;

        private NavMeshAgent _agent;
        private Animator _animator;

        private int _patrolIndex = -1;
        private float _nextAttackTime;
        private float _loseTargetTimer;
        private Transform _currentTarget;

        public Transform CurrentTarget => _currentTarget;
        public float PatrolWaitTime => patrolWaitTime;
        public float AttackRange => attackRange;
        public bool EnableDebugLog => enableDebugLog;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();

            patrolWaitTime = Mathf.Max(0f, patrolWaitTime);
            patrolArriveDistance = Mathf.Max(0.1f, patrolArriveDistance);
            searchRadius = Mathf.Max(0.1f, searchRadius);
            attackRange = Mathf.Max(0.1f, attackRange);
            attackCooldown = Mathf.Max(0f, attackCooldown);
            loseTargetDelay = Mathf.Max(0f, loseTargetDelay);
            attackDamage = Mathf.Max(0f, attackDamage);
        }

        public void SetTarget(Transform target)
        {
            bool changed = _currentTarget != target;
            _currentTarget = target;
            _loseTargetTimer = 0f;

            if (changed && target != null)
            {
                Log($"锁定目标: {target.name}");
            }
        }

        public void ClearTarget()
        {
            if (_currentTarget != null)
            {
                Log($"丢失目标: {_currentTarget.name}");
            }

            _currentTarget = null;
            _loseTargetTimer = 0f;
        }

        public void Log(string message)
        {
            if (!enableDebugLog)
            {
                return;
            }

            Debug.Log($"[EnemyAI:{name}] {message}", this);
        }

        public bool RefreshCurrentTargetVisibility()
        {
            if (_currentTarget == null)
            {
                return false;
            }

            if (CanSeeTarget(_currentTarget))
            {
                _loseTargetTimer = 0f;
                return true;
            }

            _loseTargetTimer += Time.deltaTime;
            if (_loseTargetTimer >= loseTargetDelay)
            {
                ClearTarget();
            }

            return _currentTarget != null;
        }

        public Transform SearchNearestVisibleTarget()
        {
            Collider[] candidates = Physics.OverlapSphere(transform.position, searchRadius, targetMask, QueryTriggerInteraction.Ignore);
            Transform nearestTarget = null;
            float nearestDistanceSqr = float.MaxValue;

            for (int i = 0; i < candidates.Length; i++)
            {
                PlayerStatus playerStatus = candidates[i].GetComponentInParent<PlayerStatus>();
                if (playerStatus == null)
                {
                    continue;
                }

                Transform candidate = playerStatus.transform;
                if (candidate == transform || candidate.IsChildOf(transform))
                {
                    continue;
                }

                if (!CanSeeTarget(candidate))
                {
                    continue;
                }

                float distanceSqr = (candidate.position - transform.position).sqrMagnitude;
                if (distanceSqr < nearestDistanceSqr)
                {
                    nearestDistanceSqr = distanceSqr;
                    nearestTarget = candidate;
                }
            }

            return nearestTarget;
        }

        public bool CanSeeTarget(Transform target)
        {
            if (target == null)
            {
                return false;
            }

            Vector3 origin = transform.position + Vector3.up * 1.2f;
            Vector3 targetPosition = target.position + Vector3.up * 1.0f;
            Vector3 direction = targetPosition - origin;
            float distance = direction.magnitude;

            if (distance > searchRadius)
            {
                return false;
            }

            if (Vector3.Angle(transform.forward, direction.normalized) > viewAngle * 0.5f)
            {
                return false;
            }

            if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, distance, obstacleMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform != target && !hit.transform.IsChildOf(target))
                {
                    return false;
                }
            }

            return true;
        }

        public Vector3 GetNextPatrolPoint()
        {
            if (patrolPoints == null || patrolPoints.Length == 0)
            {
                Log("巡逻点为空，保持原地。请在 patrolPoints 填入至少 2 个点。");
                return transform.position;
            }

            int total = patrolPoints.Length;
            for (int i = 0; i < total; i++)
            {
                _patrolIndex = (_patrolIndex + 1) % total;
                Transform patrolPoint = patrolPoints[_patrolIndex];
                if (patrolPoint != null)
                {
                    Log($"前往巡逻点[{_patrolIndex + 1}/{total}] {patrolPoint.name}");
                    return patrolPoint.position;
                }
            }

            
            return transform.position;
        }

        public void MoveTo(Vector3 destination)
        {
            if (_agent == null || !_agent.enabled)
            {
                return;
            }

            _agent.isStopped = false;
            _agent.SetDestination(destination);
        }

        public void StopMove()
        {
            if (_agent == null || !_agent.enabled)
            {
                return;
            }

            _agent.isStopped = true;
            _agent.ResetPath();
        }

        public bool HasReachedDestination()
        {
            if (_agent == null || !_agent.enabled)
            {
                return true;
            }

            if (_agent.pathPending)
            {
                return false;
            }

            return _agent.remainingDistance <= Mathf.Max(_agent.stoppingDistance, patrolArriveDistance);
        }

        public float DistanceTo(Transform target)
        {
            if (target == null)
            {
                return float.MaxValue;
            }

            Vector3 offset = target.position - transform.position;
            offset.y = 0f;
            return offset.magnitude;
        }

        public bool IsInAttackRange(Transform target)
        {
            return DistanceTo(target) <= attackRange;
        }

        public bool TryAttackTarget(Transform target)
        {
            if (target == null)
            {
                Log("攻击失败: 目标为空");
                return false;
            }

            if (Time.time < _nextAttackTime)
            {
                Log($"攻击冷却中: 剩余 {(_nextAttackTime - Time.time):F2}s");
                return false;
            }

            if (!IsInAttackRange(target))
            {
                Log($"攻击失败: 目标不在范围内，当前距离 {DistanceTo(target):F2}");
                return false;
            }

            _nextAttackTime = Time.time + attackCooldown;

            if (_animator != null && !string.IsNullOrEmpty(attackAnimationTrigger))
            {
                _animator.SetTrigger(attackAnimationTrigger);
            }

            PlayerStatus playerStatus = target.GetComponent<PlayerStatus>();
            if (playerStatus != null)
            {
                playerStatus.TakeDamage(attackDamage);
                Log($"攻击命中: {target.name}，伤害 {attackDamage:F1}");
            }
            else
            {
                Log($"攻击触发但目标没有 PlayerStatus: {target.name}");
            }

            return true;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, searchRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
