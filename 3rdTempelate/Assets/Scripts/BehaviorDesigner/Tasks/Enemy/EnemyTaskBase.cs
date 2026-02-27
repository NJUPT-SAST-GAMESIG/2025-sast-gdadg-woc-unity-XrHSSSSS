using Character.Enemy;
using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Custom.Enemy
{
    public abstract class EnemyActionBase : Action
    {
        protected EnemyAIController Controller;

        public override void OnAwake()
        {
            Controller = GetComponent<EnemyAIController>();
            if (Controller == null)
            {
                Debug.LogError($"[{GetType().Name}] Missing EnemyAIController on {gameObject.name}.", gameObject);
            }
        }
    }

    public abstract class EnemyConditionalBase : Conditional
    {
        protected EnemyAIController Controller;

        public override void OnAwake()
        {
            Controller = GetComponent<EnemyAIController>();
            if (Controller == null)
            {
                Debug.LogError($"[{GetType().Name}] Missing EnemyAIController on {gameObject.name}.", gameObject);
            }
        }
    }
}
