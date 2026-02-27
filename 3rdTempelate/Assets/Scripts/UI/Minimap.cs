using UnityEngine;
using Character.Player;

namespace UI.Minimap
{
    public class Minimap : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private Transform followTarget;

        [Header("位置")]
        [SerializeField] private float cameraHeight = 30f;
        [SerializeField] private float positionSmoothTime = 10f;

        [Header("旋转")]
        [SerializeField] private bool rotateWithTarget = true;
        [SerializeField] private float rotationSmoothTime = 12f;
        [SerializeField] private float yawOffset = 0f;

        private void Awake()
        {
            TryFindFollowTarget();
        }

        private void LateUpdate()
        {
            if (followTarget == null)
            {
                TryFindFollowTarget();
                if (followTarget == null)
                {
                    return;
                }
            }

            UpdatePosition();
            UpdateRotation();
        }

        private void TryFindFollowTarget()
        {
            if (followTarget != null)
            {
                return;
            }

            PlayerMovementControl player = FindObjectOfType<PlayerMovementControl>();
            if (player != null)
            {
                followTarget = player.transform;
            }
        }

        private void UpdatePosition()
        {
            Vector3 targetPosition = followTarget.position;
            targetPosition.y = followTarget.position.y + cameraHeight;

            transform.position = Vector3.Lerp(
                transform.position,
                targetPosition,
                1f - Mathf.Exp(-positionSmoothTime * Time.deltaTime));
        }

        private void UpdateRotation()
        {
            Quaternion targetRotation;
            if (rotateWithTarget)
            {
                float targetYaw = followTarget.eulerAngles.y + yawOffset;
                targetRotation = Quaternion.Euler(90f, targetYaw, 0f);
            }
            else
            {
                targetRotation = Quaternion.Euler(90f, yawOffset, 0f);
            }

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                1f - Mathf.Exp(-rotationSmoothTime * Time.deltaTime));
        }
    }
}
