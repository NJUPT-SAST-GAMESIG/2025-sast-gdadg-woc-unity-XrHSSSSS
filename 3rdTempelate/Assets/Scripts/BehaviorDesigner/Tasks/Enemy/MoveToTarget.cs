using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Custom.Enemy
{
    [TaskCategory("Custom/Enemy")]
    public class MoveToTarget : EnemyActionBase
    {
        [RequiredField]
        public SharedTransform target;

        public SharedFloat stopDistance;

        public override TaskStatus OnUpdate()
        {
            if (Controller == null)
            {
                return TaskStatus.Failure;
            }

            Transform currentTarget = target.Value;
            if (currentTarget == null)
            {
                Controller.ClearTarget();
                return TaskStatus.Failure;
            }

            Controller.SetTarget(currentTarget);
            float desiredStopDistance = stopDistance.Value > 0f ? stopDistance.Value : Controller.AttackRange;
            if (Controller.DistanceTo(currentTarget) <= desiredStopDistance)
            {
                Controller.StopMove();
                return TaskStatus.Success;
            }

            Controller.MoveTo(currentTarget.position);
            return TaskStatus.Running;
        }

        public override void OnEnd()
        {
            Controller?.StopMove();
        }

        public override void OnReset()
        {
            target = null;
            stopDistance = 0f;
        }
    }
}
