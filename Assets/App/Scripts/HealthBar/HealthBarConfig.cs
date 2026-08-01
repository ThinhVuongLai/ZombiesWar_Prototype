using UnityEngine;

namespace App.HealthBar
{
    [CreateAssetMenu(fileName = "HealthBarConfig", menuName = "ZombiesWar/Health Bar Config")]
    public class HealthBarConfig : ScriptableObject
    {
        [SerializeField] Material _material;
        [SerializeField] Mesh _mesh;
        [SerializeField] Vector3 _offset = new(0f, 2.5f, 0f);
        [SerializeField] Vector2 _backgroundSize = new(2f, 0.25f);
        [SerializeField] Vector2 _fillSize = new(1.9f, 0.15f);
        [SerializeField] Color _backgroundColor = new(0.2f, 0.2f, 0.2f, 1f);
        [SerializeField] Color _fillColor = new(0f, 1f, 0f, 1f);
        [SerializeField, Range(1, 3)] int _fillSortingOffset = 1;

        public Material Material => _material;
        public Mesh Mesh => _mesh;
        public Vector3 Offset => _offset;
        public Vector2 BackgroundSize => _backgroundSize;
        public Vector2 FillSize => _fillSize;
        public Color BackgroundColor => _backgroundColor;
        public Color FillColor => _fillColor;
        public int FillSortingOffset => _fillSortingOffset;
    }
}
