using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Custom.Enemy
{
    [TaskCategory("Custom/Enemy")]
    public class FindTarget : EnemyActionBase
    {
        public SharedTransform target;

        public override TaskStatus OnUpdate()
        {
            if (Controller == null)
            {
                return TaskStatus.Failure;
            }

            if (Controller.RefreshCurrentTargetVisibility())
            {
                target.Value = Controller.CurrentTarget;
                return TaskStatus.Success;
            }

            Transform foundTarget = Controller.SearchNearestVisibleTarget();
            if (foundTarget == null)
            {
                target.Value = null;
                return TaskStatus.Failure;
            }

            Controller.SetTarget(foundTarget);
            target.Value = foundTarget;
            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            target = null;
        }
    }
}
