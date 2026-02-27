using GameInput;
using Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace CameraController
{
    public class TP_CameraController : MonoBehaviour
    {
        //相机的移动速度
        [FormerlySerializedAs("_controlSpeed")] [SerializeField, Header("相机参数配置")] private float controlSpeed;  //摄像机移动速度
        [FormerlySerializedAs("_cameraVerticalMaxAngle")] [SerializeField, Header("最大俯仰角")] private float cameraVerticalMaxAngle;  //限制相机上下最大旋转角度
        [FormerlySerializedAs("_cameraVerticalMinAngle")] [SerializeField, Header("最小俯仰角")] private float cameraVerticalMinAngle;  //限制相机上下最小转角度
        [FormerlySerializedAs("_smoothRotationTime")] [SerializeField, Header("相机旋转平滑时间")] private float smoothRotationTime;   //摄像机平滑速度
        [FormerlySerializedAs("_positionOffset")] [SerializeField, Header("TP_Camera偏移值")] private float positionOffset;   //摄像机与目标物体的距离偏移
        [FormerlySerializedAs("_positionSmoothTime")] [SerializeField, Header("相机位置平滑插值系数")] private float positionSmoothTime;    //摄像机位置平滑时间


        [FormerlySerializedAs("_lookTarget")] [SerializeField , Header("跟随目标")]
        public Transform lookTarget;
        [SerializeField, Header("锁定参数")] private float lockYawSmoothTime = 0.08f;
        private Vector3 _smoothDampVelocity = Vector3.zero;
        private Vector2 _input;    //相机的输入 旋转角度
        private Vector3 _cameraRotation;   //当前摄像机的旋转角度
        private Transform _lockTarget;
        private float _lockYawVelocity;

        public Transform CurrentLockTarget => _lockTarget;

        public bool IsLockingTarget => _lockTarget != null;

        private void Update()
        {
            if(lookTarget == null)
                return;
            CameraInput();
        }

        private void LateUpdate()
        {
            if(lookTarget == null)
                return;
            UpdateCameraRotation();
            CameraPosition();
        }

        private void CameraInput()
        {
            if (IsLockingTarget)
            {
                LockCameraYawToTarget();
            }
            else
            {
                _input.y += GameInputManager.MainInstance.CameraLook.x * controlSpeed;
            }

            _input.x -= GameInputManager.MainInstance.CameraLook.y * controlSpeed;

            _input.x = Mathf.Clamp(_input.x, cameraVerticalMinAngle, cameraVerticalMaxAngle);
        }

        public void SetLockTarget(Transform target)
        {
            _lockTarget = target;
        }

        public void ClearLockTarget()
        {
            _lockTarget = null;
            _lockYawVelocity = 0f;
        }

        //将相机的Yaw平滑地锁定到目标上
        private void LockCameraYawToTarget()  
        {
            if (_lockTarget == null || lookTarget == null)
            {
                return;
            }

            Vector3 direction = _lockTarget.position - lookTarget.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            float targetYaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            _input.y = Mathf.SmoothDampAngle(_input.y, targetYaw, ref _lockYawVelocity, lockYawSmoothTime);
        }

        //更新相机的旋转
        private void UpdateCameraRotation()
        {
            _cameraRotation = Vector3.SmoothDamp(_cameraRotation , new Vector3(_input.x, _input.y, 0), ref _smoothDampVelocity, smoothRotationTime);
            transform.eulerAngles = _cameraRotation;
        }

        private void CameraPosition()
        {
            var newPos = (lookTarget.position + (-transform.forward * positionOffset));
            transform.position = Vector3.Lerp(transform.position , newPos , DevelopmentTools.UnTetheredLerp(positionSmoothTime));
        }
    
    
    
    }
}
