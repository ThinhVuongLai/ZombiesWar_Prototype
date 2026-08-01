using UnityEngine;

namespace App.HealthBar
{
    public class HealthBarView : MonoBehaviour, IHealthBarView
    {
        HealthBarConfig _config;
        Material _backgroundMaterial;
        Material _fillMaterial;
        MaterialPropertyBlock _fillPropertyBlock;
        Transform _characterTransform;
        Vector3 _offset;
        float _fillAmount = 1f;
        bool _visible = true;
        Camera _camera;
        Mesh _quadMesh;

        void OnDestroy()
        {
            if (_backgroundMaterial != null) Destroy(_backgroundMaterial);
            if (_fillMaterial != null) Destroy(_fillMaterial);
            if (_quadMesh != null && _quadMesh.name == "HealthBarQuad")
                Destroy(_quadMesh);
        }

        public void Initialize(HealthBarConfig config, Transform character, Vector3? overrideOffset = null)
        {
            _config = config;
            _characterTransform = character;
            _offset = overrideOffset ?? config.Offset;
            _camera = Camera.main;

            _backgroundMaterial = new Material(config.Material);
            _fillMaterial = new Material(config.Material);
            _fillPropertyBlock = new MaterialPropertyBlock();
            _quadMesh = config.Mesh != null ? config.Mesh : CreateQuadMesh();
        }

        public void SetFillAmount(float ratio)
        {
            _fillAmount = Mathf.Clamp01(ratio);
        }

        public void SetVisible(bool visible)
        {
            _visible = visible;
        }

        void Update()
        {
            if (!_visible || _config == null || _characterTransform == null) return;

            var worldPos = _characterTransform.position + _offset;
            var billboardRot = Quaternion.LookRotation(
                _camera.transform.forward, _camera.transform.up);

            _backgroundMaterial.color = _config.BackgroundColor;
            var backgroundMatrix = Matrix4x4.TRS(
                worldPos, billboardRot, new Vector3(_config.BackgroundSize.x, _config.BackgroundSize.y, 1));
            Graphics.DrawMesh(_quadMesh, backgroundMatrix, _backgroundMaterial, 0);

            _fillMaterial.color = _config.FillColor;
            _fillPropertyBlock.SetFloat("_FillAmount", _fillAmount);
            var fillMatrix = Matrix4x4.TRS(
                worldPos, billboardRot, new Vector3(_config.FillSize.x, _config.FillSize.y, 1));
            Graphics.DrawMesh(_quadMesh, fillMatrix, _fillMaterial, 0, null, 0, _fillPropertyBlock);
        }

        Mesh CreateQuadMesh()
        {
            var mesh = new Mesh { name = "HealthBarQuad" };
            mesh.vertices = new[]
            {
                new Vector3(-0.5f, 0.5f, 0),
                new Vector3(0.5f, 0.5f, 0),
                new Vector3(-0.5f, -0.5f, 0),
                new Vector3(0.5f, -0.5f, 0),
            };
            mesh.uv = new[]
            {
                new Vector2(0, 1), new Vector2(1, 1),
                new Vector2(0, 0), new Vector2(1, 0),
            };
            mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
