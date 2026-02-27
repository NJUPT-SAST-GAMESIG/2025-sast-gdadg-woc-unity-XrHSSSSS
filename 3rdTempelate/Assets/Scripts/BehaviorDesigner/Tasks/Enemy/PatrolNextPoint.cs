using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Custom.Enemy
{
    [TaskCategory("Custom/Enemy")]
    public class PatrolNextPoint : EnemyActionBase
    {
        public SharedVector3 currentPatrolPoint;

        private bool _moving;
        private float _waitTimer;

        public override void OnStart()
        {
            _moving = true;
            _waitTimer = 0f;

            Vector3 patrolPoint = Controller != null ? Controller.GetNextPatrolPoint() : Vector3.zero;
            currentPatrolPoint.Value = patrolPoint;
            Controller?.MoveTo(patrolPoint);
        }

        public override TaskStatus OnUpdate()
        {
            if (Controller == null)
            {
                return TaskStatus.Failure;
            }

            if (_moving)
            {
                if (!Controller.HasReachedDestination())
                {
                    return TaskStatus.Running;
                }

                Controller.StopMove();
                _moving = false;
            }

            if (_waitTimer < Controller.PatrolWaitTime)
            {
                _waitTimer += Time.deltaTime;
                return TaskStatus.Running;
            }

            return TaskStatus.Success;
        }

        public override void OnEnd()
        {
            _moving = false;
            _waitTimer = 0f;
        }

        public override void OnReset()
        {
            currentPatrolPoint = Vector3.zero;
        }
    }
}
