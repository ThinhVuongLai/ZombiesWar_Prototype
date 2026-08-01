using App.Combat.Attack;
using App.Core;
using App.Core.Services;
using App.Enemy.Weapon;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using ZombiesWar.Bullet;

namespace App.Enemy
{
    public class EnemySpawner : MonoBehaviour
    {
        AttackStrategyRegistry _attackStrategyRegistry;

        void Start()
        {
            var configManager = ServiceLocator.Resolve<ConfigManager>();
            _attackStrategyRegistry = AttackStrategyRegistry.CreateForEnemy(
                configManager.EnemyWeaponConfigRegistry, configManager.BulletConfigRegistry);
        }

        public EnemyView SpawnEnemy(Vector3 position, int enemyId)
        {
            var configManager = ServiceLocator.Resolve<ConfigManager>();
            var enemyInfor = configManager.EnemyConfig?.GetEnemyInfor(enemyId);
            if (enemyInfor?.EnemyPrefab == null) return null;

            var enemyInstance = Instantiate(enemyInfor.EnemyPrefab, position, Quaternion.identity);
            var enemyView = enemyInstance.GetComponent<EnemyView>();

            if (enemyView == null)
            {
                Destroy(enemyInstance);
                return null;
            }

            var playerTarget = ServiceLocator.Resolve<IPlayerTargetProvider>();

            var viewConfig = enemyView.Config;
            var config = new EnemyViewConfig(viewConfig.AttackType, viewConfig.MoveSpeed,
                viewConfig.Health, viewConfig.DetectionRange,
                enemyInfor.IdleAnimationName, enemyInfor.MoveAnimationName,
                enemyInfor.AttackAnimationName, enemyInfor.DeadAnimationName);
            enemyView.SetConfig(config);

            var presenter = new EnemyPresenter(enemyView, config,
                _attackStrategyRegistry, configManager.EnemyWeaponConfigRegistry, playerTarget);

            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entity = entityManager.CreateEntity(
                typeof(EnemyStats),
                typeof(EnemyHealth),
                typeof(EnemyCombatState),
                typeof(LocalTransform)
            );

            entityManager.SetComponentData(entity, LocalTransform.FromPosition(position));
            presenter.SetEntity(entity);

            return enemyView;
        }
    }
}