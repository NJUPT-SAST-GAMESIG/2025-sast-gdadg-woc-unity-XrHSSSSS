using UnityEngine;

namespace UI.Minimap
{
    public class MinimapEntityIcon : MonoBehaviour
    {
        public enum IconType
        {
            Player,
            Enemy
        }

        [SerializeField] private IconType iconType = IconType.Enemy;
        [SerializeField] private Vector3 worldOffset = Vector3.up;

        public IconType Type => iconType;

        public Vector3 WorldPosition => transform.position + worldOffset;
    }

}