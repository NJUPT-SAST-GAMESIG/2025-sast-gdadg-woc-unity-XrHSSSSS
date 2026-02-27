using CameraController;
using GameInput;
using UnityEngine;
using EnemyUnit = Character.Enemy.Enemy;

namespace Character.Player
{
	public class CombatLock : MonoBehaviour
	{
		[Header("引用")]
		[SerializeField] private TP_CameraController tpCameraController;
		[SerializeField] private PlayerMovementControl playerMovementControl;
		[SerializeField] private Transform lockOrigin;

		[Header("锁敌参数")]
		[SerializeField] private float maxLockDistance = 16f;
		[SerializeField, Range(0f, 180f)] private float maxLockAngle = 45f;
		[SerializeField, Range(1f, 3f)] private float unlockDistanceMultiplier = 1.35f;     // 锁定状态下允许的最大距离倍率
		[SerializeField, Range(1f, 3f)] private float unlockAngleMultiplier = 1.6f;         // 锁定状态下允许的最大角度倍率

		private EnemyUnit _currentTarget;

		private void Awake()
		{
			if (lockOrigin == null)
			{
				lockOrigin = transform;
			}

			if (tpCameraController == null)
			{
				tpCameraController = FindObjectOfType<TP_CameraController>();
			}

			if (playerMovementControl == null)
			{
				playerMovementControl = GetComponent<PlayerMovementControl>();
			}
		}

		private void Update()
		{
			HandleLockInput();
			UpdateLockState();
		}

		private void OnDisable()
		{
			ClearLockTarget();
		}

		private void HandleLockInput()
		{
			if (!GameInputManager.MainInstance.LockEnemy)
			{
				return;
			}

			if (_currentTarget != null)
			{
				ClearLockTarget();
				return;
			}

			TryAcquireLockTarget();
		}

		private void UpdateLockState()
		{
			if (_currentTarget == null)
			{
				return;
			}

			if (!IsTargetValid(_currentTarget, true))
			{
				ClearLockTarget();
				return;
			}

			SetCameraLockTarget(_currentTarget.LockPoint);
		}

		private void TryAcquireLockTarget()
		{
			EnemyUnit bestTarget = FindBestTarget();
			if (bestTarget == null)
			{
				return;
			}

			_currentTarget = bestTarget;
			SetCameraLockTarget(bestTarget.LockPoint);
		}

		private EnemyUnit FindBestTarget()
		{
			Vector3 pointerDirection = GetPointerDirection();
			Vector3 origin = lockOrigin.position;

			float bestScore = float.MaxValue;
			EnemyUnit bestTarget = null;

			for (int index = 0; index < EnemyUnit.ActiveEnemies.Count; index++)
			{
				EnemyUnit enemy = EnemyUnit.ActiveEnemies[index];
				if (!IsTargetValid(enemy, false))
				{
					continue;
				}

				Vector3 toEnemy = enemy.LockPoint.position - origin;
				toEnemy.y = 0f;

				float distance = toEnemy.magnitude;
				if (distance <= 0.001f || distance > maxLockDistance)
				{
					continue;
				}

				Vector3 direction = toEnemy / distance;
				float angle = Vector3.Angle(pointerDirection, direction);
				if (angle > maxLockAngle)
				{
					continue;
				}

				// 优先角度更正的目标，距离作为次要权重
				float score = angle + distance * 0.2f;
				if (score < bestScore)
				{
					bestScore = score;
					bestTarget = enemy;
				}
			}

			return bestTarget;
		}

		private bool IsTargetValid(EnemyUnit enemy, bool useUnlockRange)
		{
			if (enemy == null || !enemy.CanBeLocked)
			{
				return false;
			}

			Vector3 origin = lockOrigin != null ? lockOrigin.position : transform.position;
			Vector3 toEnemy = enemy.LockPoint.position - origin;
			toEnemy.y = 0f;

			float distance = toEnemy.magnitude;
			if (distance <= 0.001f)
			{
				return false;
			}

			// 已锁定目标允许更宽松的距离和角度，避免镜头抖动导致频繁丢锁
			float allowedDistance = useUnlockRange ? maxLockDistance * unlockDistanceMultiplier : maxLockDistance;
			if (distance > allowedDistance)
			{
				return false;
			}

			float allowedAngle = useUnlockRange ? maxLockAngle * unlockAngleMultiplier : maxLockAngle;
			float angle = Vector3.Angle(GetPointerDirection(), toEnemy / distance);

			return angle <= allowedAngle;
		}

		private Vector3 GetPointerDirection()
		{
			Transform pointer = Camera.main != null ? Camera.main.transform : transform;
			Vector3 direction = pointer.forward;
			direction.y = 0f;

			if (direction.sqrMagnitude <= 0.0001f)
			{
				direction = transform.forward;
				direction.y = 0f;
			}

			return direction.normalized;
		}

		private void ClearLockTarget()
		{
			_currentTarget = null;

			tpCameraController?.ClearLockTarget();

			if (playerMovementControl != null)
			{
				playerMovementControl.isLock = false;
			}
		}

		private void SetCameraLockTarget(Transform targetPoint)
		{
			tpCameraController?.SetLockTarget(targetPoint);
		}
	}
}
