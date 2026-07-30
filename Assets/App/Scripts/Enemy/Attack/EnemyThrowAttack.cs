using App.Enemy.Weapon;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using ZombiesWar.ThrowingWeapon;

namespace App.Enemy.Attack
{
    public class EnemyThrowAttack : IEnemyAttackStrategy
    {
        readonly EnemyThrowWeaponConfig _config;
        readonly ThrowActionRegistry _throwActionRegistry;

        public WeaponType AttackType => WeaponType.Throwing;

        public EnemyThrowAttack(EnemyThrowWeaponConfig config)
        {
            _config = config;
            _throwActionRegistry = new ThrowActionRegistry();
        }

        public void Execute(IEnemyView view, IPlayerTargetProvider target, float damage)
        {
            if (_config == null || _config.ObjectPrefab == null) return;

            var playerTransform = target.PlayerTransform;
            var throwPos = view.Transform.position + new Vector3(0, 1.5f, 0);
            var targetPos = playerTransform.position + new Vector3(0, 1f, 0);

            var horizontalDist = Vector3.Distance(
                new Vector3(throwPos.x, 0, throwPos.z),
                new Vector3(targetPos.x, 0, targetPos.z));
            var heightDiff = targetPos.y - throwPos.y;

            var angleRad = _config.ThrowAngle * Mathf.Deg2Rad;
            var angleCos = Mathf.Cos(angleRad);
            var angleSin = Mathf.Sin(angleRad);

            var denominator = 2f * (horizontalDist * angleSin * angleCos -
                heightDiff * angleCos * angleCos);
            if (Mathf.Abs(denominator) < 0.001f) return;

            var gMagnitude = Mathf.Abs(Physics.gravity.y) * _config.GravityScale;
            var speedSq = (gMagnitude * horizontalDist * horizontalDist) / denominator;
            if (speedSq <= 0f) return;
            var speed = Mathf.Sqrt(speedSq);

            speed = Mathf.Clamp(speed, _config.MinThrowForce, _config.MaxThrowForce);

            var dirToTarget = new Vector3(targetPos.x - throwPos.x, 0, targetPos.z - throwPos.z).normalized;
            var velocity = dirToTarget * (speed * angleCos);
            velocity.y = speed * angleSin;

            var thrownGo = Object.Instantiate(_config.ObjectPrefab, throwPos, Quaternion.identity);
            var thrownObj = thrownGo.GetComponent<ThrownObject>();
            if (thrownObj == null)
            {
                thrownObj = thrownGo.AddComponent<ThrownObject>();
            }

            var throwAction = _throwActionRegistry.GetAction(_config.ActionType);
            thrownObj.Initialize(_config.ObjectLifespan, _config.ActionRadius, _config.Damage,
                _config.GravityScale, throwAction, velocity);
        }
    }
}
