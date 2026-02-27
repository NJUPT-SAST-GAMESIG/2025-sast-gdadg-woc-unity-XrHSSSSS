using Tools;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameInput
{
    public class GameInputManager : Singleton<GameInputManager>
    {
        private GameInputAction _gameInputAction;
    
        public Vector2 Movement => _gameInputAction.GameInput.Movement.ReadValue<Vector2>();
        public Vector2 CameraLook => _gameInputAction.GameInput.CameraLook.ReadValue<Vector2>();
        public bool Run => _gameInputAction.GameInput.Run.phase == InputActionPhase.Performed;
    
        public bool Climb => _gameInputAction.GameInput.Climb.triggered;
    
        public bool Grab => _gameInputAction.GameInput.Grab.triggered;
    
        public bool LAttack => _gameInputAction.GameInput.LAttack.triggered;
    
        public bool RAttack => _gameInputAction.GameInput.RAttack.triggered;
    
        public bool TakeOut => _gameInputAction.GameInput.TakeOut.triggered;
        //按住
        public bool Parry => _gameInputAction.GameInput.Parry.phase == InputActionPhase.Performed;

        public bool ExecuteParry => _gameInputAction.GameInput.Parry.triggered;
    
        public bool LockEnemy => _gameInputAction.GameInput.LockEnemy.triggered;

        public bool Ex1 => Keyboard.current != null && Keyboard.current.digit1Key.wasPressedThisFrame;

        public bool Ex2 => Keyboard.current != null && Keyboard.current.digit2Key.wasPressedThisFrame;
    
    
    
        protected override void Awake()
        {
            if (_gameInputAction == null)
            {
                _gameInputAction = new GameInputAction();
            }
        }
    
        private void OnEnable()
        {
            _gameInputAction.Enable();
        }

        private void OnDisable()
        {
            _gameInputAction.Disable();
        }

    }
}
