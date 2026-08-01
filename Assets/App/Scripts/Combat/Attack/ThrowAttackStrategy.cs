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

        public void Execute(Vector3 attackerPosition, Transform attackerTransform,
            Entity targetEntity, Vector3 targetPosition, float damage,
            IHealthAccessor healthAccessor, bool faceTarget,
            Action<float> fallbackDamageDealer = null)
        {
            if (_config == null || _config.ObjectPrefab == null) return;

            var throwPosition = attackerPosition + new Vector3(0, 1.5f, 0);

            var velocity = CalculateThrowVelocity(throwPosition, targetPosition, _config);
            if (!velocity.HasValue) return;

            var thrownGameObject = UnityEngine.Object.Instantiate(_config.ObjectPrefab, throwPosition, Quaternion.identity);
            var thrownObject = thrownGameObject.GetComponent<ThrownObject>();
            if (thrownObject == null)
                thrownObject = thrownGameObject.AddComponent<ThrownObject>();

            var throwAction = _throwActionRegistry.GetAction(_config.ActionType);
            thrownObject.Initialize(_config.ObjectLifespan, _config.ActionRadius, damage,
                _config.GravityScale, throwAction, velocity.Value);
        }

        static Vector3? CalculateThrowVelocity(Vector3 throwPosition, Vector3 targetPosition, IThrowConfig config)
        {
            var horizontalDistance = Vector3.Distance(
                new Vector3(throwPosition.x, 0, throwPosition.z),
                new Vector3(targetPosition.x, 0, targetPosition.z));
            var heightDifference = targetPosition.y - throwPosition.y;

            var angleRadians = config.ThrowAngle * Mathf.Deg2Rad;
            var angleCosine = Mathf.Cos(angleRadians);
            var angleSine = Mathf.Sin(angleRadians);

            var denominator = 2f * (horizontalDistance * angleSine * angleCosine -
                heightDifference * angleCosine * angleCosine);
            if (Mathf.Abs(denominator) < 0.001f) return null;

            var gravityMagnitude = Mathf.Abs(Physics.gravity.y) * config.GravityScale;
            var speedSquared = (gravityMagnitude * horizontalDistance * horizontalDistance) / denominator;
            if (speedSquared <= 0f) return null;
            var speed = Mathf.Sqrt(speedSquared);

            speed = Mathf.Clamp(speed, config.MinimumThrowForce, config.MaximumThrowForce);

            var directionToTarget = new Vector3(targetPosition.x - throwPosition.x, 0, targetPosition.z - throwPosition.z).normalized;
            var velocity = directionToTarget * (speed * angleCosine);
            velocity.y = speed * angleSine;

            return velocity;
        }
    }
}
