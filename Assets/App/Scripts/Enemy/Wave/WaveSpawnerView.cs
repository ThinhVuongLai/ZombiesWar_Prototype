using App.Core.Services;
using UnityEngine;

namespace App.Enemy.Wave
{
    public class WaveSpawnerView : MonoBehaviour, IWaveSpawnerView
    {
        [SerializeField] Camera _gameCamera;
        [SerializeField] WaveSpawnerConfig _config;
        [SerializeField] EnemySpawner _enemySpawner;

        WaveSpawnerManager _manager;

        public Vector3 WorldCenter
        {
            get
            {
                if (_gameCamera == null) return transform.position;
                if (_gameCamera.orthographic) return _gameCamera.transform.position;

                var groundPlane = new Plane(Vector3.up, Vector3.zero);
                var camTrans = _gameCamera.transform;
                if (groundPlane.Raycast(new Ray(camTrans.position, camTrans.forward), out var dist))
                    return camTrans.position + camTrans.forward * dist;

                return _gameCamera.transform.position;
            }
        }
        public Camera GameCamera => _gameCamera;

        public Vector2 ScreenExtents
        {
            get
            {
                if (_gameCamera == null)
                    return Vector2.zero;

                if (_gameCamera.orthographic)
                {
                    var height = _gameCamera.orthographicSize;
                    var width = height * _gameCamera.aspect;
                    return new Vector2(width, height);
                }

                var camPos = _gameCamera.transform.position;
                var camForward = _gameCamera.transform.forward;
                var groundPlane = new Plane(Vector3.up, Vector3.zero);

                if (groundPlane.Raycast(new Ray(camPos, camForward), out var distance))
                {
                    var halfHeight = distance * Mathf.Tan(_gameCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
                    var halfWidth = halfHeight * _gameCamera.aspect;
                    return new Vector2(halfWidth, halfHeight);
                }

                return Vector2.zero;
            }
        }

        void Awake()
        {
            _manager = ServiceLocator.Resolve<WaveSpawnerManager>();
            _manager.Initialize(this, _config, _enemySpawner);
        }

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            if (_config == null || _config.Waves == null)
                return;

            var cam = _gameCamera;
            if (cam == null) return;

            var center = cam.transform.position;

            Gizmos.color = Color.yellow;
            foreach (var wave in _config.Waves)
            {
                if (wave == null) continue;
                Gizmos.DrawWireSphere(center, wave.SpawnRadius);
            }

            var extents = ScreenExtents;
            Gizmos.color = Color.green;
            var screenCenter = center;

            if (!cam.orthographic)
            {
                var groundPlane = new Plane(Vector3.up, Vector3.zero);
                if (groundPlane.Raycast(new Ray(center, cam.transform.forward), out var dist))
                {
                    screenCenter = center + cam.transform.forward * dist;
                }
            }

            Gizmos.DrawWireCube(screenCenter, new Vector3(extents.x * 2f, 0f, extents.y * 2f));
        }
#endif
    }
}
