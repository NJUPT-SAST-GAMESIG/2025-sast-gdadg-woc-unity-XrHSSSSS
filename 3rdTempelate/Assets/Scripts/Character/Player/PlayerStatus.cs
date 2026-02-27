using System;
using Config;
using UnityEngine;

namespace Character.Player
{
    public class PlayerStatus : MonoBehaviour
    {
        [Header("HP")]
        [SerializeField] private float maxHp = 100f;
        [SerializeField] private float startHp = 100f;

        [Header("SP")]
        [SerializeField] private float maxSp = 100f;
        [SerializeField] private float startSp = 0f;
        [SerializeField, Min(0f)] private float spRecoverPerSecond = 10f;

        [Header("调试")]
        [SerializeField, Range(0f, 1f)] private float inspectorHpNormalized = 1f;
        [SerializeField, Range(0f, 1f)] private float inspectorSpNormalized = 0f;

        private float _currentHp;
        private float _currentSp;
        private Animator _animator;

        public float CurrentHp => _currentHp;
        public float CurrentSp => _currentSp;
        public float MaxHp => maxHp;
        public float MaxSp => maxSp;
        public float HpNormalized => maxHp <= 0f ? 0f : _currentHp / maxHp;
        public float SpNormalized => maxSp <= 0f ? 0f : _currentSp / maxSp;

        public event Action<float, float> OnHpChanged;
        public event Action<float, float> OnSpChanged;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            maxHp = Mathf.Max(1f, maxHp);
            maxSp = Mathf.Max(1f, maxSp);
            _currentHp = Mathf.Clamp(startHp, 0f, maxHp);
            _currentSp = 0f;
            inspectorHpNormalized = HpNormalized;
            inspectorSpNormalized = SpNormalized;

            NotifyAll();
        }

        private void Update()
        {
            if (spRecoverPerSecond <= 0f)
            {
                return;
            }

            if (_currentSp >= maxSp)
            {
                return;
            }

            RecoverSp(spRecoverPerSecond * Time.deltaTime);
        }

        public void TakeDamage(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            SetHp(_currentHp - amount);
        }

        public void RecoverHp(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            SetHp(_currentHp + amount);
        }

        public bool ConsumeSp(float amount)
        {
            if (amount <= 0f)
            {
                return true;
            }

            if (_currentSp < amount)
            {
                return false;
            }

            SetSp(_currentSp - amount);
            return true;
        }

        public void RecoverSp(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            SetSp(_currentSp + amount);
        }

        public void ResetStatus()
        {
            SetHp(maxHp);
            SetSp(maxSp);
        }

        private void SetHp(float value)
        {
            float clampedValue = Mathf.Clamp(value, 0f, maxHp);  
            if (Mathf.Approximately(clampedValue, _currentHp))
            {
                return;
            }

            _currentHp = clampedValue;
            inspectorHpNormalized = HpNormalized;
            OnHpChanged?.Invoke(_currentHp, maxHp);
        }

        private void SetSp(float value)
        {
            float clampedValue = Mathf.Clamp(value, 0f, maxSp);  
            if (Mathf.Approximately(clampedValue, _currentSp))
            {
                return;
            }

            _currentSp = clampedValue;
            inspectorSpNormalized = SpNormalized;
            UpdateAnimatorSpParameters();
            OnSpChanged?.Invoke(_currentSp, maxSp);
        }

        private void NotifyAll()
        {
            inspectorHpNormalized = HpNormalized;  
            inspectorSpNormalized = SpNormalized;
            OnHpChanged?.Invoke(_currentHp, maxHp);
            UpdateAnimatorSpParameters();
            OnSpChanged?.Invoke(_currentSp, maxSp);
        }

        private void UpdateAnimatorSpParameters()
        {
            if (_animator == null)
            {
                return;
            }

            _animator.SetInteger(AnimationID.Sp, Mathf.RoundToInt(_currentSp));  
        }

        private void OnValidate()
        {
            maxHp = Mathf.Max(1f, maxHp);
            maxSp = Mathf.Max(1f, maxSp);
            startHp = Mathf.Clamp(startHp, 0f, maxHp);
            startSp = 0f;
            spRecoverPerSecond = Mathf.Max(0f, spRecoverPerSecond);

            inspectorHpNormalized = Mathf.Clamp01(inspectorHpNormalized);
            inspectorSpNormalized = Mathf.Clamp01(inspectorSpNormalized);

            if (Application.isPlaying)
            {
                float debugHpValue = inspectorHpNormalized * maxHp;
                float debugSpValue = inspectorSpNormalized * maxSp;
                SetHp(debugHpValue);
                SetSp(debugSpValue);
            }
            else
            {
                _currentHp = Mathf.Clamp(startHp, 0f, maxHp);
                _currentSp = 0f;
                inspectorHpNormalized = HpNormalized;
                inspectorSpNormalized = SpNormalized;
            }
        }
    }

}