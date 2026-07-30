using Unity.Entities;
using Unity.Collections;

namespace ZombiesWar.Bullet.ECS
{
    [RequireMatchingQueriesForUpdate]
    [UpdateInGroup(typeof(BulletSystemGroup))]
    [UpdateAfter(typeof(BulletHitDetectionSystem))]
    [UpdateAfter(typeof(BulletRayCastSystem))]
    public partial struct BulletCleanupSystem : ISystem
    {
        EntityQuery _bulletQuery;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<BulletData>();

            _bulletQuery = state.GetEntityQuery(
                ComponentType.ReadOnly<BulletData>(),
                ComponentType.ReadOnly<BulletLifeData>()
            );
        }

        public void OnUpdate(ref SystemState state)
        {
            var bulletLifeDatas = _bulletQuery.ToComponentDataArray<BulletLifeData>(Allocator.Temp);
            var bulletEntities = _bulletQuery.ToEntityArray(Allocator.Temp);

            int destroyCount = 0;
            var toDestroy = new NativeArray<Entity>(bulletEntities.Length, Allocator.Temp);

            for (int i = 0; i < bulletEntities.Length; i++)
            {
                if (bulletLifeDatas[i].HasHit)
                {
                    toDestroy[destroyCount++] = bulletEntities[i];
                }
            }

            for (int i = 0; i < destroyCount; i++)
            {
                state.EntityManager.DestroyEntity(toDestroy[i]);
            }

            toDestroy.Dispose();
            bulletLifeDatas.Dispose();
            bulletEntities.Dispose();
        }
    }
}
