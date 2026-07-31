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
            var cm = ServiceLocator.Resolve<ConfigManager>();
            _attackStrategyRegistry = AttackStrategyRegistry.CreateForEnemy(
                cm.EnemyWeaponConfigRegistry, cm.BulletConfigRegistry);
        }

        public EnemyView SpawnEnemy(Vector3 position, int enemyId)
        {
            var cm = ServiceLocator.Resolve<ConfigManager>();
            var enemyInfor = cm.EnemyConfig?.GetEnemyInfor(enemyId);
            if (enemyInfor?.EnemyPrefab == null) return null;

            var go = Instantiate(enemyInfor.EnemyPrefab, position, Quaternion.identity);
            var enemyView = go.GetComponent<EnemyView>();

            if (enemyView == null)
            {
                Destroy(go);
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
                _attackStrategyRegistry, cm.EnemyWeaponConfigRegistry, playerTarget);

            var em = World.DefaultGameObjectInjectionWorld.EntityManager;
            var entity = em.CreateEntity(
                typeof(EnemyStats),
                typeof(EnemyHealth),
                typeof(EnemyCombatState),
                typeof(LocalTransform)
            );

            em.SetComponentData(entity, LocalTransform.FromPosition(position));
            presenter.SetEntity(entity);

            return enemyView;
        }
    }
}