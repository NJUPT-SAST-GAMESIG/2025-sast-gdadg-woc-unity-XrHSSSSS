namespace BehaviorDesigner.Runtime.Tasks.Custom.Enemy
{
    [TaskCategory("Custom/Enemy")]
    public class AttackTarget : EnemyActionBase
    {
        [RequiredField]
        public SharedTransform target;

        public override TaskStatus OnUpdate()
        {
            if (Controller == null)
            {
                return TaskStatus.Failure;
            }

            if (target.Value == null)
            {
                return TaskStatus.Failure;
            }

            if (!Controller.IsInAttackRange(target.Value))
            {
                return TaskStatus.Failure;
            }

            Controller.StopMove();
            if (Controller.TryAttackTarget(target.Value))
            {
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }

        public override void OnReset()
        {
            target = null;
        }
    }
}
