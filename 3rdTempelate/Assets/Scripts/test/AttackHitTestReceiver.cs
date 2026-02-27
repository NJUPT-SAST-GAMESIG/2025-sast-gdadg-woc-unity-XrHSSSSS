using UnityEngine;
using System.Collections;

namespace Test
{
    public class AttackHitTestReceiver : MonoBehaviour
    {
        [Header("受击测试")]
        [SerializeField] private bool enableLog = true;
        [SerializeField] private int hitCount = 0;

        [Header("受击闪烁")]
        [SerializeField] private bool enableBlinkOnHit = true;
        [SerializeField, Min(1)] private int blinkTimes = 2;
        [SerializeField, Min(0.01f)] private float blinkInterval = 0.06f;

        private Renderer[] _renderers;
        private Coroutine _blinkCoroutine;

        public int HitCount => hitCount;

        private void Awake()
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
        }

        public void OnPlayerAttackHit()
        {
            hitCount++;

            if (enableBlinkOnHit)
            {
                if (_blinkCoroutine != null)
                {
                    StopCoroutine(_blinkCoroutine);
                    SetRenderersVisible(true);
                }

                _blinkCoroutine = StartCoroutine(BlinkRoutine());
            }

            if (enableLog)
            {
                Debug.Log($"[AttackHitTestReceiver] {name} 被命中，累计次数: {hitCount}", this);
            }
        }

        [ContextMenu("Reset Hit Count")]
        private void ResetHitCount()
        {
            hitCount = 0;
            if (enableLog)
            {
                Debug.Log($"[AttackHitTestReceiver] {name} 命中计数已重置", this);
            }
        }

        private IEnumerator BlinkRoutine()
        {
            if (_renderers == null || _renderers.Length == 0)
            {
                yield break;
            }

            for (int i = 0; i < blinkTimes; i++)
            {
                SetRenderersVisible(false);
                yield return new WaitForSeconds(blinkInterval);
                SetRenderersVisible(true);
                yield return new WaitForSeconds(blinkInterval);
            }

            _blinkCoroutine = null;
        }

        private void SetRenderersVisible(bool visible)
        {
            if (_renderers == null)
            {
                return;
            }

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null)
                {
                    _renderers[i].enabled = visible;
                }
            }
        }

        private void OnDisable()
        {
            if (_blinkCoroutine != null)
            {
                StopCoroutine(_blinkCoroutine);
                _blinkCoroutine = null;
            }

            SetRenderersVisible(true);
        }
    }
}
