using System;
using App.Combat.Attack;
using App.Core;
using App.Core.Services;
using App.Enemy.Weapon;
using MagicTile.Pool;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using ZombiesWar.Bullet;

namespace App.Enemy
{
    public class EnemySpawner : MonoBehaviour
    {
        public EnemyView SpawnEnemy(Vector3 position, int enemyId)
        {
            var configManager = ServiceLocator.Resolve<ConfigManager>();
            var enemyInfor = configManager.EnemyConfig?.GetEnemyInfor(enemyId);
            if (enemyInfor?.EnemyPrefab == null) return null;

            var poolService = ServiceLocator.Resolve<PoolService>();
            var enemyInstance = poolService.Get(enemyInfor.EnemyPrefab);
            enemyInstance.transform.position = position;
            enemyInstance.transform.rotation = Quaternion.identity;

            var enemyView = enemyInstance.GetComponent<EnemyView>();

            if (enemyView == null)
            {
                poolService.Release(enemyInstance);
                return null;
            }

            var weaponConfig = configManager.EnemyWeaponConfigRegistry?.GetConfig(enemyInfor.WeaponId);
            var registry = CreateAttackRegistry(weaponConfig, configManager.BulletConfigRegistry);

            var playerTarget = ServiceLocator.Resolve<IPlayerTargetProvider>();

            var viewConfig = enemyView.Config;
            var config = new EnemyViewConfig(viewConfig.MoveSpeed,
                viewConfig.Health, viewConfig.DetectionRange,
                enemyInfor.IdleAnimationName, enemyInfor.MoveAnimationName,
                enemyInfor.AttackAnimationName, enemyInfor.DeadAnimationName);
            enemyView.SetConfig(config);

            var presenter = new EnemyPresenter(enemyView, config,
                weaponConfig, registry, playerTarget,
                enemyInfor.DissolveDuration);

            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entity = entityManager.CreateEntity(
                typeof(EnemyStats),
                typeof(EnemyHealth),
                typeof(EnemyCombatState),
                typeof(LocalTransform)
            );

            entityManager.SetComponentData(entity, LocalTransform.FromPosition(position));
            presenter.SetEntity(entity);

            enemyView.CurrentPresenter = presenter;

            return enemyView;
        }

        static AttackStrategyRegistry CreateAttackRegistry(EnemyWeaponConfig weaponConfig,
            BulletConfigRegistry bulletRegistry)
        {
            return AttackStrategyRegistry.CreateForEnemy(weaponConfig, bulletRegistry);
        }
    }
}
