#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;

namespace Character.Player.Editor
{
#if ODIN_INSPECTOR
    [CustomEditor(typeof(PlayerCombatControl))]
    public class PlayerCombatSystemOdinEditor : OdinEditor
    {
        private PlayerCombatControl _combat;
        private PlayerAttackDetection _attackDetection;
        private VFXControl _vfxControl;
        private PlayerStatus _playerStatus;
        private CombatLock _combatLock;

        private SerializedObject _attackDetectionSO;
        private SerializedObject _vfxControlSO;
        private SerializedObject _playerStatusSO;
        private SerializedObject _combatLockSO;

        [ShowInInspector, ReadOnly, LabelText("攻击检测")]
        private PlayerAttackDetection AttackDetectionRef => _attackDetection;

        [ShowInInspector, ReadOnly, LabelText("特效组件")]
        private VFXControl VFXControlRef => _vfxControl;

        [ShowInInspector, ReadOnly, LabelText("玩家状态")]
        private PlayerStatus PlayerStatusRef => _playerStatus;

        [ShowInInspector, ReadOnly, LabelText("战斗锁定")]
        private CombatLock CombatLockRef => _combatLock;

        private bool _foldCombatSkill = false;
        private bool _foldPlayerStatus = false;
        private bool _foldCombatLock = false;
        private bool _foldAttackRange = false;
        private bool _foldVfxAudioVoice = false;
        private bool _foldRuntimeDebug = true;

        private static readonly Color AttackDetectButtonColor = new Color(0.32f, 0.7f, 1f);
        private static readonly Color AtkTriggerButtonColor = new Color(0.45f, 0.85f, 0.45f);
        private static readonly Color ClearLockButtonColor = new Color(1f, 0.65f, 0.3f);

        protected override void OnEnable()
        {
            base.OnEnable();
            CacheReferences();
        }

        public override void OnInspectorGUI()
        {
            CacheReferences();

            SirenixEditorGUI.Title("战斗系统总控", "包含技能、范围检测、特效/音效/语音条目等", TextAlignment.Left, true);
            SirenixEditorGUI.InfoMessageBox("同时包含部分调试功能! OwO", true);

            DrawCombatSkillSection();
            DrawPlayerStatusSection();
            DrawCombatLockSection();
            DrawAttackRangeSection();
            DrawVFXAudioVoiceSection();
            DrawRuntimeDebugSection();
        }

        private void ExecuteAttackDetectionOnce()
        {
            if (_attackDetection == null) return;
            if (!Application.isPlaying) return;

            int hitCount = _attackDetection.ExecuteAttackDetection();
            Debug.Log($"[PlayerCombatSystemOdinEditor] 攻击检测完成，命中数量: {hitCount}", _combat);
        }

        private void TriggerATKEvent()
        {
            if (_combat == null) return;
            _combat.gameObject.SendMessage("ATK", SendMessageOptions.DontRequireReceiver);
        }

        private void ClearLockTarget()
        {
            if (_combatLock == null) return;
            _combatLock.gameObject.SendMessage("ClearLockTarget", SendMessageOptions.DontRequireReceiver);
        }

        private void DrawCombatSkillSection()
        {
            _foldCombatSkill = EditorGUILayout.Foldout(_foldCombatSkill, "攻击技能（PlayerCombatControl）", true);
            if (!_foldCombatSkill)
            {
                return;
            }

            SirenixEditorGUI.BeginBox("攻击技能（PlayerCombatControl）");
            if (serializedObject != null)
            {
                serializedObject.Update();
                DrawPropertyIfExists(serializedObject, "maxComboCount");
                DrawPropertyIfExists(serializedObject, "comboResetTime");
                DrawPropertyIfExists(serializedObject, "attackInputBufferTime");
                DrawPropertyIfExists(serializedObject, "comboLinkWindowStart");
                DrawPropertyIfExists(serializedObject, "comboLinkWindowEnd");
                SirenixEditorGUI.HorizontalLineSeparator();
                EditorGUILayout.LabelField("EX技能参数", EditorStyles.boldLabel);
                DrawPropertyIfExists(serializedObject, "exTriggerHoldTime");
                DrawPropertyIfExists(serializedObject, "ex1SpCost");
                DrawPropertyIfExists(serializedObject, "ex2SpCost");
                serializedObject.ApplyModifiedProperties();
            }

            SirenixEditorGUI.EndBox();
        }

        private void DrawPlayerStatusSection()
        {
            _foldPlayerStatus = EditorGUILayout.Foldout(_foldPlayerStatus, "玩家状态（PlayerStatus）", true);
            if (!_foldPlayerStatus)
            {
                return;
            }

            SirenixEditorGUI.BeginBox("玩家状态（PlayerStatus）");

            if (_playerStatusSO == null)
            {
                SirenixEditorGUI.EndBox();
                return;
            }

            _playerStatusSO.Update();
            DrawPropertyIfExists(_playerStatusSO, "maxHp");
            DrawPropertyIfExists(_playerStatusSO, "startHp");
            DrawPropertyIfExists(_playerStatusSO, "maxSp");
            DrawPropertyIfExists(_playerStatusSO, "spRecoverPerSecond");
            DrawPropertyIfExists(_playerStatusSO, "inspectorHpNormalized");
            DrawPropertyIfExists(_playerStatusSO, "inspectorSpNormalized");
            _playerStatusSO.ApplyModifiedProperties();

            if (Application.isPlaying && _playerStatus != null)
            {
                SirenixEditorGUI.HorizontalLineSeparator();
                EditorGUILayout.LabelField($"当前HP: {_playerStatus.CurrentHp:0.##} / {_playerStatus.MaxHp:0.##}");
                EditorGUILayout.LabelField($"当前SP: {_playerStatus.CurrentSp:0.##} / {_playerStatus.MaxSp:0.##}");
            }

            SirenixEditorGUI.EndBox();
        }

        private void DrawCombatLockSection()
        {
            _foldCombatLock = EditorGUILayout.Foldout(_foldCombatLock, "战斗锁定（CombatLock）", true);
            if (!_foldCombatLock)
            {
                return;
            }

            SirenixEditorGUI.BeginBox("战斗锁定（CombatLock）");

            if (_combatLockSO == null)
            {
                SirenixEditorGUI.EndBox();
                return;
            }

            _combatLockSO.Update();
            DrawPropertyIfExists(_combatLockSO, "tpCameraController");
            DrawPropertyIfExists(_combatLockSO, "playerMovementControl");
            DrawPropertyIfExists(_combatLockSO, "lockOrigin");
            DrawPropertyIfExists(_combatLockSO, "maxLockDistance");
            DrawPropertyIfExists(_combatLockSO, "maxLockAngle");
            DrawPropertyIfExists(_combatLockSO, "unlockDistanceMultiplier");
            DrawPropertyIfExists(_combatLockSO, "unlockAngleMultiplier");
            _combatLockSO.ApplyModifiedProperties();

            SirenixEditorGUI.EndBox();
        }

        private void DrawAttackRangeSection()
        {
            _foldAttackRange = EditorGUILayout.Foldout(_foldAttackRange, "攻击范围（PlayerAttackDetection）", true);
            if (!_foldAttackRange)
            {
                return;
            }

            SirenixEditorGUI.BeginBox("攻击范围（PlayerAttackDetection）");

            if (_attackDetectionSO == null)
            {
                SirenixEditorGUI.EndBox();
                return;
            }

            _attackDetectionSO.Update();
            DrawPropertyIfExists(_attackDetectionSO, "attackOrigin");
            DrawPropertyIfExists(_attackDetectionSO, "forwardReference");
            DrawPropertyIfExists(_attackDetectionSO, "targetLayerMask");
            DrawPropertyIfExists(_attackDetectionSO, "detectRadius");
            DrawPropertyIfExists(_attackDetectionSO, "maxDetectDistance");
            DrawPropertyIfExists(_attackDetectionSO, "maxDetectAngle");
            DrawPropertyIfExists(_attackDetectionSO, "includeTriggerCollider");
            _attackDetectionSO.ApplyModifiedProperties();

            SirenixEditorGUI.EndBox();
        }

        private void DrawVFXAudioVoiceSection()
        {
            _foldVfxAudioVoice = EditorGUILayout.Foldout(_foldVfxAudioVoice, "特效/音效/语音（VFXControl）", true);
            if (!_foldVfxAudioVoice)
            {
                return;
            }

            SirenixEditorGUI.BeginBox("特效/音效/语音（VFXControl）");

            if (_vfxControlSO == null)
            {
                SirenixEditorGUI.EndBox();
                return;
            }

            _vfxControlSO.Update();
            DrawPropertyIfExists(_vfxControlSO, "vfxEventEntries");
            DrawPropertyIfExists(_vfxControlSO, "sfxAudioSource");
            DrawPropertyIfExists(_vfxControlSO, "voiceAudioSource");
            DrawPropertyIfExists(_vfxControlSO, "enableLog");
            _vfxControlSO.ApplyModifiedProperties();

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("进入 Play 模式后可直接播放选中事件进行预览。", MessageType.Info);
            }

            SirenixEditorGUI.EndBox();
        }

        private void DrawRuntimeDebugSection()
        {
            _foldRuntimeDebug = EditorGUILayout.Foldout(_foldRuntimeDebug, "运行时调试", true);
            if (!_foldRuntimeDebug)
            {
                return;
            }

            SirenixEditorGUI.BeginBox("运行时调试");

            GUI.enabled = Application.isPlaying;

            EditorGUILayout.BeginHorizontal();
            DrawColoredButton("攻击检测", AttackDetectButtonColor, ExecuteAttackDetectionOnce);
            DrawColoredButton("ATK触发", AtkTriggerButtonColor, TriggerATKEvent);
            DrawColoredButton("清除锁敌", ClearLockButtonColor, ClearLockTarget);
            EditorGUILayout.EndHorizontal();

            SirenixEditorGUI.HorizontalLineSeparator();
            if (_playerStatusSO != null)
            {
                _playerStatusSO.Update();

                SerializedProperty hpSlider = _playerStatusSO.FindProperty("inspectorHpNormalized");
                SerializedProperty spSlider = _playerStatusSO.FindProperty("inspectorSpNormalized");

                if (hpSlider != null)
                {
                    hpSlider.floatValue = EditorGUILayout.Slider("HP调试", hpSlider.floatValue, 0f, 1f);
                }

                if (spSlider != null)
                {
                    spSlider.floatValue = EditorGUILayout.Slider("SP调试", spSlider.floatValue, 0f, 1f);
                }

                _playerStatusSO.ApplyModifiedProperties();
            }

            GUI.enabled = true;

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("请在运行时调试。", MessageType.Info);
            }

            SirenixEditorGUI.EndBox();
        }

        private static void DrawColoredButton(string label, Color color, System.Action onClick)
        {
            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            if (GUILayout.Button(label, GUILayout.Height(24)))
            {
                onClick?.Invoke();
            }
            GUI.backgroundColor = originalColor;
        }

        private void CacheReferences()
        {
            _combat = target as PlayerCombatControl;

            if (_combat == null)
            {
                _attackDetectionSO = null;
                _vfxControlSO = null;
                _playerStatusSO = null;
                _combatLockSO = null;
                return;
            }

            _attackDetection = _combat.GetComponentInChildren<PlayerAttackDetection>(true);
            _vfxControl = _combat.GetComponentInChildren<VFXControl>(true);
            _playerStatus = _combat.GetComponent<PlayerStatus>();
            _combatLock = _combat.GetComponent<CombatLock>();

            _attackDetectionSO = _attackDetection != null ? new SerializedObject(_attackDetection) : null;
            _vfxControlSO = _vfxControl != null ? new SerializedObject(_vfxControl) : null;
            _playerStatusSO = _playerStatus != null ? new SerializedObject(_playerStatus) : null;
            _combatLockSO = _combatLock != null ? new SerializedObject(_combatLock) : null;
        }

        private static void DrawPropertyIfExists(SerializedObject so, string propertyName)
        {
            SerializedProperty property = so.FindProperty(propertyName);
            if (property != null)
            {
                EditorGUILayout.PropertyField(property, true);
            }
        }
    }
#endif
}

#endif

