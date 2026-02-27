using Config;
using GameInput;
using UnityEngine;
using UnityEngine.Serialization;

namespace Character.Player
{
    public class PlayerCombatControl : MonoBehaviour
    {
        [Header("攻击参数")]
        [FormerlySerializedAs("_maxComboCount")][SerializeField] private int maxComboCount;  //最大连击数
        [FormerlySerializedAs("_comboResetTime")][SerializeField] private float comboResetTime;  //连击重置时间
        [SerializeField] private float comboResetNoInputTime = 1.0f; //无输入自动清空连击时间
        [FormerlySerializedAs("_attackInputBufferTime")][SerializeField] private float attackInputBufferTime;  //攻击输入缓冲时间
        [SerializeField, Range(0f, 1f)] private float comboLinkWindowStart = 0.5f; //连击衔接窗口开始时间
        [SerializeField, Range(0f, 1f)] private float comboLinkWindowEnd = 0.95f;  //连击衔接窗口结束时间
        [SerializeField] private float exTriggerHoldTime = 0.1f;

        [Header("EX技能SP消耗")]
        [SerializeField, Min(0f)] private float ex1SpCost = 20f;
        [SerializeField, Min(0f)] private float ex2SpCost = 30f;

        private Animator _animator;
        private PlayerAttackDetection _attackDetection;
        private PlayerStatus _playerStatus;

        private int _comboIndex = 0;
        private float _comboTimer = 0f;
        private bool _attackQueued = false;
        private float _attackBufferTimer = 0f;
        private int _currentAttackStateHash = 0;
        private bool _lastAttackInputRaw = false;
        private float _ex1TriggerTimer = 0f;
        private float _ex2TriggerTimer = 0f;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _attackDetection = GetComponentInChildren<PlayerAttackDetection>();
            _playerStatus = GetComponent<PlayerStatus>();
            maxComboCount = Mathf.Max(1, maxComboCount);
            comboResetNoInputTime = Mathf.Max(0f, comboResetNoInputTime);
            comboLinkWindowEnd = Mathf.Max(comboLinkWindowStart, comboLinkWindowEnd);
            exTriggerHoldTime = Mathf.Max(0.01f, exTriggerHoldTime);
            ex1SpCost = Mathf.Max(0f, ex1SpCost);
            ex2SpCost = Mathf.Max(0f, ex2SpCost);
        }

        private void Update()
        {
            HandleAttackInput();
            UpdateAttackState();
            UpdateBufferedInput();
            UpdateAttackInputParameter();
            UpdateExSkillTriggerParameters();
            UpdateComboTimer();
        }

        private void HandleAttackInput()
        {
            bool attackInputRaw = GameInputManager.MainInstance.LAttack;
            bool attackPressedThisFrame = attackInputRaw && !_lastAttackInputRaw;
            _lastAttackInputRaw = attackInputRaw;

            if (!attackPressedThisFrame || _attackQueued)
            {
                return;
            }

            _comboTimer = comboResetNoInputTime;

            bool isInAttack = IsInAttackState();
            if (isInAttack && _comboIndex >= maxComboCount)
            {
                return;
            }

            _attackQueued = true;
            _attackBufferTimer = attackInputBufferTime;
        }

        private void UpdateBufferedInput()
        {
            if (!_attackQueued)
            {
                return;
            }

            // 不在攻击状态时 靠缓冲计时等待下一个可衔接时机
            if (!IsInAttackState())
            {
                if (_attackBufferTimer > 0f)
                {
                    _attackBufferTimer -= Time.deltaTime;
                }
                else
                {
                    ClearBufferedAttack();
                }

                return;
            }

            float normalizedTime = GetCurrentAttackNormalizedTime();
            if (normalizedTime > comboLinkWindowEnd)
            {
                ClearBufferedAttack();
            }
        }

        private void ResetCombo()
        {
            _comboIndex = 0;
            ClearBufferedAttack();
            _animator.SetInteger(AnimationID.ComboIndex, _comboIndex);
        }

        private bool IsInAttackState()
        {
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

            return stateInfo.IsTag("Attack");
        }

        private void UpdateComboTimer()
        {
            if (_comboIndex <= 0)
            {
                return;
            }

            if (IsInAttackState())
            {
                return;
            }

            if (_comboTimer > 0)
            {
                _comboTimer -= Time.deltaTime;
                if (_comboTimer <= 0)
                {
                    ResetCombo();
                }
            }
        }

        private void UpdateAttackState()
        {
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            bool isInAttack = stateInfo.IsTag("Attack");

            if (isInAttack)
            {
                int stateHash = stateInfo.fullPathHash;
                if (stateHash != _currentAttackStateHash)
                {
                    // 清空旧输入 避免残留输入串到下一段
                    ClearBufferedAttack();
                    _currentAttackStateHash = stateHash;
                }

                return;
            }

            if (_currentAttackStateHash != 0)
            {
                _currentAttackStateHash = 0;
                ClearBufferedAttack();
            }
        }

        private void UpdateAttackInputParameter()
        {
            bool isInAttack = IsInAttackState();
            bool attackInput = _attackQueued &&
                               (!isInAttack
                                   ? _attackBufferTimer > 0f
                                   : _comboIndex <= maxComboCount && IsInComboLinkWindow());  // 只有在攻击状态且满足衔接条件时才持续保持AttackInput为true，其他情况即使有输入也不传递给动画参数
            _animator.SetBool(AnimationID.AttackInput, attackInput);
        }


        private bool IsInComboLinkWindow()
        {
            float normalizedTime = GetCurrentAttackNormalizedTime();
            return normalizedTime >= comboLinkWindowStart && normalizedTime <= comboLinkWindowEnd;
        }

        private float GetCurrentAttackNormalizedTime()
        {
            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
            float normalizedTime = stateInfo.normalizedTime;
            // 避免循环动画下normalizedTime持续累加
            return normalizedTime - Mathf.Floor(normalizedTime);
        }

        private void ClearBufferedAttack()
        {
            _attackQueued = false;
            _attackBufferTimer = 0f;
        }

        private void UpdateExSkillTriggerParameters()
        {
            UpdateExTrigger(ref _ex1TriggerTimer, GameInputManager.MainInstance.Ex1, AnimationID.Ex1Trigger);
            UpdateExTrigger(ref _ex2TriggerTimer, GameInputManager.MainInstance.Ex2, AnimationID.Ex2Trigger);
        }

        private void UpdateExTrigger(ref float triggerTimer, bool isPressedThisFrame, int animatorParam)
        {
            if (isPressedThisFrame)
            {
                triggerTimer = exTriggerHoldTime;
            }

            if (triggerTimer > 0f)
            {
                triggerTimer -= Time.deltaTime;
            }

            _animator.SetBool(animatorParam, triggerTimer > 0f);
        }

        private void ConsumeSpForExSkill(float spCost, string eventName)
        {
            PlayerStatus playerStatus = GetOrCachePlayerStatus();
            if (playerStatus == null)
            {
                Debug.LogWarning($"[PlayerCombatControl] Missing PlayerStatus when handling animation event {eventName}.", this);
                return;
            }

            if (!playerStatus.ConsumeSp(spCost))
            {
                Debug.Log($"[PlayerCombatControl] Not enough SP for {eventName}. Need {spCost}, current {playerStatus.CurrentSp}.", this);
            }
        }

        private PlayerStatus GetOrCachePlayerStatus()
        {
            if (_playerStatus == null)
            {
                _playerStatus = GetComponent<PlayerStatus>();
            }

            return _playerStatus;
        }

        //Events
        private void ATK()
        {
            _comboIndex++;
            if (_comboIndex > maxComboCount)
            {
                _comboIndex = 0;
            }

            _animator.SetInteger(AnimationID.ComboIndex, _comboIndex);

            if (_attackDetection != null)
            {
                _attackDetection.ExecuteAttackDetection();
            }


            Debug.Log("Attack Frame");
        }

        private void Ex1_ATK()
        {
            if (_attackDetection != null)
            {
                _attackDetection.ExecuteAttackDetection();
            }

            Debug.Log("Ex1 Attack Frame");
        }

        private void Ex2_ATK()
        {
            if (_attackDetection != null)
            {
                _attackDetection.ExecuteAttackDetection();
            }

            Debug.Log("Ex2 Attack Frame");
        }

        private void Ex1_ConsumeSP()
        {
            ConsumeSpForExSkill(ex1SpCost, "Ex1_ConsumeSP");
        }

        private void Ex2_ConsumeSP()
        {
            ConsumeSpForExSkill(ex2SpCost, "Ex2_ConsumeSP");
        }

        private void DisableLinkCombo()
        {
            ClearBufferedAttack();
        }


        private void CancelAttackColdTime()
        {
            Debug.Log("Cancel Attack Cooldown");
        }

        private void EnablePreInput()
        {
            Debug.Log("Enable PreInput");
        }




    }
}