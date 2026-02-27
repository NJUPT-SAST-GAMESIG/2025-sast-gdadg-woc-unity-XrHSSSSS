namespace BehaviorDesigner.Runtime.Tasks.Custom.Enemy
{
    [TaskCategory("Custom/Enemy")]
    public class IsTargetInAttackRange : EnemyConditionalBase
    {
        [RequiredField]
        public SharedTransform target;

        public override TaskStatus OnUpdate()
        {
            if (Controller == null || target.Value == null)
            {
                return TaskStatus.Failure;
            }

            return Controller.IsInAttackRange(target.Value) ? TaskStatus.Success : TaskStatus.Failure;
        }

        public override void OnReset()
        {
            target = null;
        }
    }
}
