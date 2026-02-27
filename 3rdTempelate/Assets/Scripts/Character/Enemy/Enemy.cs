using System.Collections.Generic;
using UnityEngine;

namespace Character.Enemy
{
	public class Enemy : MonoBehaviour
	{
		private static readonly List<Enemy> RegisteredEnemies = new List<Enemy>();

		[Header("锁定参数")]
		[SerializeField] private Transform lockPoint;
		[SerializeField] private bool canBeLocked = true;

		public static IReadOnlyList<Enemy> ActiveEnemies => RegisteredEnemies;

		public Transform LockPoint => lockPoint != null ? lockPoint : transform;

		public bool CanBeLocked => canBeLocked && enabled && gameObject.activeInHierarchy;

		private void OnEnable()
		{
			if (!RegisteredEnemies.Contains(this))
			{
				RegisteredEnemies.Add(this);
			}
		}

		private void OnDisable()
		{
			RegisteredEnemies.Remove(this);
		}

		private void OnDestroy()
		{
			RegisteredEnemies.Remove(this);
		}
	}
}
