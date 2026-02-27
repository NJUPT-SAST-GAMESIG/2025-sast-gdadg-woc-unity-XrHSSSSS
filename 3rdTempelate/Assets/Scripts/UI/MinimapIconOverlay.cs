using System.Collections.Generic;
using Character.Player;
using UnityEngine;
using UnityEngine.UI;
using EnemyUnit = Character.Enemy.Enemy;

namespace UI.Minimap
{
    public class MinimapIconOverlay : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private Camera minimapCamera;
        [SerializeField] private RectTransform iconRoot;
        [SerializeField] private Transform playerTarget;

        [Header("图标资源")]
        [SerializeField] private Sprite playerSprite;
        [SerializeField] private Sprite enemySprite;
        [SerializeField] private Vector2 iconSize = new Vector2(14f, 14f);

        [Header("刷新")]
        [SerializeField] private float scanInterval = 1f;  // 扫描敌人列表的时间间隔
        [SerializeField] private bool clampInCircle = true;  // 是否将图标位置限制在圆形范围内
        [SerializeField] private float edgePadding = 6f;     // 当clampInCircle为true时，图标距离圆形边缘的最小距离

        private readonly Dictionary<EnemyUnit, Image> _enemyIconMap = new Dictionary<EnemyUnit, Image>();
        private Image _playerIcon;
        private float _nextScanTime;

        private void Awake()
        {
            if (iconRoot == null)
            {
                iconRoot = transform as RectTransform;
            }
        }

        private void OnEnable()
        {
            TryFindPlayer();
            EnsurePlayerIcon();
            RebuildIcons();
        }

        private void LateUpdate()
        {
            if (minimapCamera == null || iconRoot == null)
            {
                return;
            }

            if (Time.time >= _nextScanTime)
            {
                _nextScanTime = Time.time + scanInterval;
                RebuildIcons();
            }

            UpdatePlayerIcon();
            UpdateEnemyIconPositions();
        }

        private void RebuildIcons()
        {
            HashSet<EnemyUnit> enemySet = new HashSet<EnemyUnit>(EnemyUnit.ActiveEnemies);

            List<EnemyUnit> staleKeys = new List<EnemyUnit>();
            foreach (KeyValuePair<EnemyUnit, Image> pair in _enemyIconMap)
            {
                if (!enemySet.Contains(pair.Key) || pair.Key == null)
                {
                    if (pair.Value != null)
                    {
                        Destroy(pair.Value.gameObject);
                    }
                    staleKeys.Add(pair.Key);
                }
            }

            for (int index = 0; index < staleKeys.Count; index++)
            {
                _enemyIconMap.Remove(staleKeys[index]);
            }

            for (int index = 0; index < EnemyUnit.ActiveEnemies.Count; index++)
            {
                EnemyUnit enemy = EnemyUnit.ActiveEnemies[index];
                if (enemy == null || _enemyIconMap.ContainsKey(enemy))
                {
                    continue;
                }

                Image iconImage = CreateIcon(enemySprite, "Enemy_MinimapIcon");
                _enemyIconMap.Add(enemy, iconImage);
            }
        }

        private Image CreateIcon(Sprite iconSprite, string objectName)
        {
            GameObject iconObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObject.transform.SetParent(iconRoot, false);

            Image image = iconObject.GetComponent<Image>();
            image.raycastTarget = false;
            image.sprite = iconSprite;

            RectTransform rectTransform = image.rectTransform;
            rectTransform.sizeDelta = iconSize;
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            return image;
        }

        private void EnsurePlayerIcon()
        {
            if (_playerIcon != null)
            {
                return;
            }

            _playerIcon = CreateIcon(playerSprite, "Player_MinimapIcon");
        }

        private void TryFindPlayer()
        {
            if (playerTarget != null)
            {
                return;
            }

            PlayerMovementControl player = FindObjectOfType<PlayerMovementControl>();
            if (player != null)
            {
                playerTarget = player.transform;
            }
        }

        private void UpdatePlayerIcon()
        {
            if (_playerIcon == null)
            {
                return;
            }

            TryFindPlayer();
            if (playerTarget == null || !playerTarget.gameObject.activeInHierarchy)
            {
                _playerIcon.enabled = false;
                return;
            }

            _playerIcon.enabled = true;
            UpdateSingleIconPosition(_playerIcon, playerTarget.position);
        }

        private void UpdateEnemyIconPositions()
        {
            foreach (KeyValuePair<EnemyUnit, Image> pair in _enemyIconMap)
            {
                EnemyUnit enemy = pair.Key;
                Image iconImage = pair.Value;

                if (enemy == null || iconImage == null || !enemy.gameObject.activeInHierarchy)  
                {
                    if (iconImage != null)
                    {
                        iconImage.enabled = false;
                    }
                    continue;
                }

                iconImage.enabled = true;
                UpdateSingleIconPosition(iconImage, enemy.LockPoint.position);
            }
        }

        private void UpdateSingleIconPosition(Image iconImage, Vector3 worldPosition)  
        {
            float width = iconRoot.rect.width;
            float height = iconRoot.rect.height;
            float radius = Mathf.Min(width, height) * 0.5f - edgePadding;

            Vector3 viewportPosition = minimapCamera.WorldToViewportPoint(worldPosition);
            if (viewportPosition.z < 0f)
            {
                iconImage.enabled = false;
                return;
            }

            iconImage.enabled = true;

            Vector2 localPosition = new Vector2(
                (viewportPosition.x - 0.5f) * width,
                (viewportPosition.y - 0.5f) * height);

            if (clampInCircle)
            {
                float distance = localPosition.magnitude;
                if (distance > radius && distance > 0f)
                {
                    localPosition = localPosition / distance * radius;
                }
            }

            iconImage.rectTransform.anchoredPosition = localPosition;
        }
    }
}
