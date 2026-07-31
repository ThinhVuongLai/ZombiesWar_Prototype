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
        [SerializeField] GameObject _enemyPrefab;

        AttackStrategyRegistry _attackStrategyRegistry;

        void Start()
        {
            var cm = ServiceLocator.Resolve<ConfigManager>();
            _attackStrategyRegistry = AttackStrategyRegistry.CreateForEnemy(
                cm.EnemyWeaponConfigRegistry, cm.BulletConfigRegistry);
        }

        public EnemyView SpawnEnemy(Vector3 position, GameObject overridePrefab = null)
        {
            var prefab = overridePrefab != null ? overridePrefab : _enemyPrefab;
            if (prefab == null) return null;

            var go = Instantiate(prefab, position, Quaternion.identity);
            var enemyView = go.GetComponent<EnemyView>();

            if (enemyView == null)
            {
                Destroy(go);
                return null;
            }

            var cm = ServiceLocator.Resolve<ConfigManager>();
            var playerTarget = ServiceLocator.Resolve<IPlayerTargetProvider>();

            var presenter = new EnemyPresenter(enemyView, enemyView.Config,
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