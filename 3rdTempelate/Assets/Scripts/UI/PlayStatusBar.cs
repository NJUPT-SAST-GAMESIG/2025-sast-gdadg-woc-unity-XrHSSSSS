using Character.Player;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI.StatusBar
{
    public class PlayStatusBar : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private PlayerStatus playerStatus;
        [SerializeField] private Image hpFillImage;
        [SerializeField] private Image spFillImage;
        [SerializeField] private Image hpLagFillImage;
        [SerializeField] private Image spLagFillImage;
        [SerializeField] private Text hpValueText;

        [Header("动画")]
        [SerializeField] private float tweenDuration = 0.25f;        // 主填充动画持续时间
        [SerializeField] private Ease tweenEase = Ease.OutCubic;     // 主填充动画缓动类型
        [SerializeField] private float lagDelay = 0.25f;             // 滞后填充开始动画的延迟时间
        [SerializeField] private float lagTweenDuration = 0.35f;     // 滞后填充动画持续时间
        [SerializeField] private Ease lagTweenEase = Ease.OutCubic;  // 滞后填充动画缓动类型

        [Header("闪烁")]
        [SerializeField] private bool enableBlinkOnDecrease = true;  // 是否在HP/SP减少时启用闪烁效果
        [SerializeField] private Image hpBlinkTarget;                   
        [SerializeField] private Image spBlinkTarget;
        [SerializeField, Range(1, 6)] private int blinkCount = 2;    
        [SerializeField] private float blinkSingleDuration = 0.08f;  // 单次闪烁动画持续时间
        [SerializeField, Range(0f, 1f)] private float blinkMinAlpha = 0.35f;  
        private Tweener _hpTweener;
        private Tweener _spTweener;
        private Tweener _hpLagTweener;  
        private Tweener _spLagTweener;
        private Tween _hpLagDelayTween;
        private Tween _spLagDelayTween;
        private Tweener _hpBlinkTweener;
        private Tweener _spBlinkTweener;
        private bool _isSubscribed;

        private void Awake()
        {
            if (playerStatus == null)
            {
                playerStatus = FindObjectOfType<PlayerStatus>();
            }

            if (hpBlinkTarget == null)
            {
                hpBlinkTarget = hpFillImage;
            }

            if (spBlinkTarget == null)
            {
                spBlinkTarget = spFillImage;
            }
        }

        private void OnEnable()
        {
            RegisterStatusEvents();
            RefreshBarImmediately();
        }

        private void OnDisable()
        {
            UnregisterStatusEvents();
            KillTweens();
        }

        private void OnDestroy()
        {
            KillTweens();
        }

        private void RegisterStatusEvents()
        {
            if (_isSubscribed || playerStatus == null)
            {
                return;
            }

            playerStatus.OnHpChanged += HandleHpChanged;
            playerStatus.OnSpChanged += HandleSpChanged;
            _isSubscribed = true;
        }

        private void UnregisterStatusEvents()
        {
            if (!_isSubscribed || playerStatus == null)
            {
                return;
            }

            playerStatus.OnHpChanged -= HandleHpChanged;
            playerStatus.OnSpChanged -= HandleSpChanged;
            _isSubscribed = false;
        }

        private void RefreshBarImmediately()
        {
            if (playerStatus == null)
            {
                return;
            }

            if (hpFillImage != null)
            {
                hpFillImage.fillAmount = playerStatus.HpNormalized;
            }

            if (hpLagFillImage != null)
            {
                hpLagFillImage.fillAmount = playerStatus.HpNormalized;
            }

            if (spFillImage != null)
            {
                spFillImage.fillAmount = playerStatus.SpNormalized;
            }

            if (spLagFillImage != null)
            {
                spLagFillImage.fillAmount = playerStatus.SpNormalized;
            }

            UpdateHpText(playerStatus.CurrentHp, playerStatus.MaxHp);
        }

        private void HandleHpChanged(float current, float max)
        {
            float target = max <= 0f ? 0f : current / max;
            bool isDecrease = hpFillImage != null && target < hpFillImage.fillAmount;

            AnimateFillAmount(hpFillImage, target, ref _hpTweener);
            AnimateLagFillAmount(hpLagFillImage, target, isDecrease, true);
            UpdateHpText(current, max);

            if (isDecrease)
            {
                PlayDecreaseBlink(hpBlinkTarget, ref _hpBlinkTweener);
            }
        }

        private void HandleSpChanged(float current, float max)
        {
            float target = max <= 0f ? 0f : current / max;
            bool isDecrease = spFillImage != null && target < spFillImage.fillAmount;

            AnimateFillAmount(spFillImage, target, ref _spTweener);
            AnimateLagFillAmount(spLagFillImage, target, isDecrease, false);

            if (isDecrease)
            {
                PlayDecreaseBlink(spBlinkTarget, ref _spBlinkTweener);
            }
        }

        private void AnimateFillAmount(Image targetImage, float targetValue, ref Tweener tweener)
        {
            if (targetImage == null)
            {
                return;
            }

            float finalValue = Mathf.Clamp01(targetValue);
            tweener?.Kill();
            tweener = targetImage
                .DOFillAmount(finalValue, tweenDuration)
                .SetEase(tweenEase)
                .SetUpdate(true);
        }

        private void AnimateLagFillAmount(                      
            Image targetImage,        
            float targetValue,
            bool withDelay,
            bool isHp)
        {
            if (targetImage == null)
            {
                return;
            }

            float finalValue = Mathf.Clamp01(targetValue);

            if (isHp)
            {
                _hpLagDelayTween?.Kill();
                _hpLagTweener?.Kill();
            }
            else
            {
                _spLagDelayTween?.Kill();
                _spLagTweener?.Kill();
            }

            if (!withDelay)  
            {
                if (isHp)
                {
                    _hpLagTweener = targetImage
                        .DOFillAmount(finalValue, tweenDuration)
                        .SetEase(tweenEase)
                        .SetUpdate(true);
                }
                else
                {
                    _spLagTweener = targetImage
                        .DOFillAmount(finalValue, tweenDuration)
                        .SetEase(tweenEase)
                        .SetUpdate(true);
                }

                return;
            }

            if (isHp)
            {
                _hpLagDelayTween = DOVirtual.DelayedCall(lagDelay, () =>  
                {
                    _hpLagTweener?.Kill();                           
                    _hpLagTweener = targetImage
                        .DOFillAmount(finalValue, lagTweenDuration)
                        .SetEase(lagTweenEase)
                        .SetUpdate(true);
                }, true);
            }
            else
            {
                _spLagDelayTween = DOVirtual.DelayedCall(lagDelay, () =>
                {
                    _spLagTweener?.Kill();
                    _spLagTweener = targetImage
                        .DOFillAmount(finalValue, lagTweenDuration)
                        .SetEase(lagTweenEase)
                        .SetUpdate(true);
                }, true);
            }
        }

        private void PlayDecreaseBlink(Image targetImage, ref Tweener blinkTweener)   
        {
            if (!enableBlinkOnDecrease || targetImage == null)
            {
                return;
            }

            Color color = targetImage.color;
            blinkTweener?.Kill();

            targetImage.color = new Color(color.r, color.g, color.b, 1f);
            blinkTweener = targetImage
                .DOFade(blinkMinAlpha, blinkSingleDuration)
                .SetLoops(blinkCount * 2, LoopType.Yoyo)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    Color endColor = targetImage.color;
                    targetImage.color = new Color(endColor.r, endColor.g, endColor.b, 1f);
                });
        }

        private void UpdateHpText(float current, float max)
        {
            if (hpValueText == null)
            {
                return;
            }

            int currentInt = Mathf.RoundToInt(current);
            int maxInt = Mathf.RoundToInt(max);
            hpValueText.text = $"{currentInt}/{maxInt}";
        }

        private void KillTweens()
        {
            _hpTweener?.Kill();
            _spTweener?.Kill();
            _hpLagTweener?.Kill();
            _spLagTweener?.Kill();
            _hpLagDelayTween?.Kill();  
            _spLagDelayTween?.Kill();
            _hpBlinkTweener?.Kill();
            _spBlinkTweener?.Kill();
            _hpTweener = null;
            _spTweener = null;      
            _hpLagTweener = null;
            _spLagTweener = null;
            _hpLagDelayTween = null;
            _spLagDelayTween = null;
            _hpBlinkTweener = null;
            _spBlinkTweener = null;
        }
    }
}