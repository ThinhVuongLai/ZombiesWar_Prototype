using System;
using Unity.Entities;
using UnityEngine;
using ZombiesWar.ThrowingWeapon;

namespace App.Combat.Attack
{
    public class ThrowAttackStrategy : IAttackStrategy
    {
        readonly IThrowConfig _config;
        readonly ThrowActionRegistry _throwActionRegistry;

        public WeaponType AttackType => WeaponType.Throwing;

        public ThrowAttackStrategy(IThrowConfig config)
        {
            _config = config;
            _throwActionRegistry = new ThrowActionRegistry();
        }

        public void Execute(Vector3 attackerPos, Transform attackerTransform,
            Entity targetEntity, Vector3 targetPos, float damage,
            IHealthAccessor healthAccessor, bool faceTarget,
            Action<float> fallbackDamageDealer = null)
        {
            if (_config == null || _config.ObjectPrefab == null) return;

            var throwPos = attackerPos + new Vector3(0, 1.5f, 0);

            var velocity = CalculateThrowVelocity(throwPos, targetPos, _config);
            if (!velocity.HasValue) return;

            var thrownGo = UnityEngine.Object.Instantiate(_config.ObjectPrefab, throwPos, Quaternion.identity);
            var thrownObj = thrownGo.GetComponent<ThrownObject>();
            if (thrownObj == null)
                thrownObj = thrownGo.AddComponent<ThrownObject>();

            var throwAction = _throwActionRegistry.GetAction(_config.ActionType);
            thrownObj.Initialize(_config.ObjectLifespan, _config.ActionRadius, damage,
                _config.GravityScale, throwAction, velocity.Value);
        }

        static Vector3? CalculateThrowVelocity(Vector3 throwPos, Vector3 targetPos, IThrowConfig config)
        {
            var horizontalDist = Vector3.Distance(
                new Vector3(throwPos.x, 0, throwPos.z),
                new Vector3(targetPos.x, 0, targetPos.z));
            var heightDiff = targetPos.y - throwPos.y;

            var angleRad = config.ThrowAngle * Mathf.Deg2Rad;
            var angleCos = Mathf.Cos(angleRad);
            var angleSin = Mathf.Sin(angleRad);

            var denominator = 2f * (horizontalDist * angleSin * angleCos -
                heightDiff * angleCos * angleCos);
            if (Mathf.Abs(denominator) < 0.001f) return null;

            var gMagnitude = Mathf.Abs(Physics.gravity.y) * config.GravityScale;
            var speedSq = (gMagnitude * horizontalDist * horizontalDist) / denominator;
            if (speedSq <= 0f) return null;
            var speed = Mathf.Sqrt(speedSq);

            speed = Mathf.Clamp(speed, config.MinThrowForce, config.MaxThrowForce);

            var dirToTarget = new Vector3(targetPos.x - throwPos.x, 0, targetPos.z - throwPos.z).normalized;
            var velocity = dirToTarget * (speed * angleCos);
            velocity.y = speed * angleSin;

            return velocity;
        }
    }
}
