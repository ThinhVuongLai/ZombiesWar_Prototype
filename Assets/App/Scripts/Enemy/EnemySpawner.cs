using App.Core.Services;
using App.Enemy.Attack;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace App.Enemy
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] GameObject _enemyPrefab;

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

            var registry = ServiceLocator.Resolve<AttackStrategyRegistry>();
            var playerTarget = ServiceLocator.Resolve<IPlayerTargetProvider>();

            var presenter = new EnemyPresenter(enemyView, enemyView.Config, registry, playerTarget);

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