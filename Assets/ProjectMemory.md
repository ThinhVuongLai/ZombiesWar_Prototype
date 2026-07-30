# Project Memory — ZombiesWar

## Last Updated
2026-07-30 (Refactor Attack system: Player + Enemy dùng chung Strategy classes, namespace App.Combat.Attack; IThrowConfig interface chung; xóa Enemy/Attack/)

## Architecture

### Pattern
- MVP (Model-View-Presenter) với VContainer DI
- R3 cho reactive/event streams
- EventBus cho pub/sub communication decoupled
- ServiceLocator là static facade trên VContainer container

### Tech Stack
| Layer | Technology |
|-------|-----------|
| DI Container | VContainer 1.19.0 |
| Reactive | R3 1.3.1 |
| Render Pipeline | URP 14.x |
| Unity Version | 2022.3 LTS |
| ECS | Unity.Entities 1.4.8 |
| AI Navigation | com.unity.ai.navigation 1.1.7 |

## Directory Structure
```
Assets/App/Scripts/
├── WeaponType.cs                              ← Shared enum: Melee=0, Range=1, Throwing=2 (DÙNG CHUNG Player + Enemy)
├── Combat/
│   └── Attack/
│       ├── IAttackStrategy.cs                   ← Interface chung Player + Enemy
│       ├── AttackStrategyRegistry.cs             ← CreateForEnemy/CreateForPlayer factory + Replace()
│       ├── MeleeAttackStrategy.cs                ← Shared Melee (FaceTarget + HealthAccessor)
│       ├── RangedAttackStrategy.cs               ← Shared Range (BulletSpawner)
│       ├── ThrowAttackStrategy.cs                ← Shared Throw (ballistics + ThrownObject)
│       ├── IHealthAccessor.cs                    ← bool TryApplyDamage(Entity, float)
│       ├── PlayerHealthAccessor.cs               ← Ghi PlayerHealth ECS (dùng cho Enemy attack)
│       ├── EnemyHealthAccessor.cs                ← Ghi EnemyHealth ECS (dùng cho Player attack)
│       └── IThrowConfig.cs                      ← Interface chung cho ThrowWeaponConfig + EnemyThrowWeaponConfig
├── Core/
├── JoystickInput/
├── Player/
│   ├── PlayerView.cs                          ← [SerializeField] WeaponConfigRegistry + BulletConfigRegistry
│   └── ...
├── Enemy/
│   ├── EnemyView.cs                           ← [SerializeField] WeaponType _attackType (dùng WeaponType)
│   ├── EnemyPresenter.cs                      ← _attackRegistry.Get(_attackType) + ExecuteAttack()
│   ├── EnemySpawner.cs                        ← AttackStrategyRegistry.CreateForEnemy()
│   ├── EnemyMessages.cs                       ← EnemyDealtDamageMessage.Damage + WeaponType AttackType
│   ├── Weapon/
│   │   ├── EnemyWeaponConfigRegistry.cs       ← EnemyWeaponConfig (abstract) + EnemyMelee/Ranged/ThrowWeaponConfig + IThrowConfig
│   │   └── Editor/
│   │       └── EnemyWeaponConfigPropertyDrawer.cs ← Dropdown WeaponType, type-switching, null guard
│   └── ...
├── Weapon/
│   ├── WeaponConfigRegistry.cs                ← WeaponBase (abstract) + Melee/Range/ThrowWeaponConfig + [SerializeReference]
│   └── Editor/
│       └── WeaponBasePropertyDrawer.cs
├── ThrowingWeapon/
│   ├── ThrowActionType.cs
│   ├── IThrowAction.cs
│   ├── ThrowActionRegistry.cs
│   ├── ExplosionThrowAction.cs
│   ├── IDamageable.cs
│   └── ThrownObject.cs                        ← Initialize(lifespan, radius, damage, gravityScale, action, velocity) — raw params
└── Bullet/
```

## WeaponType Enum (Shared)
```csharp
public enum WeaponType : byte { Melee = 0, Range = 1, Throwing = 2 }
```
- Dùng chung cho cả Player và Enemy
- Đã xóa `EnemyAttackType` enum cũ (None=0, Melee=1, Ranged=2)

## Player Weapon Config — Polymorphic Architecture

### Class Hierarchy
```
WeaponBase (abstract)                          ← _weaponId, _weaponType, _attackCooldown, _damage, _attackRange
├── MeleeWeaponConfig : WeaponBase             ← Không có field riêng
├── RangeWeaponConfig : WeaponBase             ← _bulletId (int)
└── ThrowWeaponConfig : WeaponBase             ← _throwAngle, _minThrowForce, _maxThrowForce, _throwSpeed,
                                                  _actionType (ThrowActionType), _actionRadius,
                                                  _objectLifespan, _gravityScale, _objectPrefab (GameObject)
```

### WeaponConfigRegistry
- `[SerializeReference] WeaponBase[] _configs`
- `Dictionary<int, WeaponBase> _lookup`
- `GetConfig(int weaponId)` → `WeaponBase`

### PlayerPresenter
- `_currentWeaponConfig` kiểu `WeaponBase`
- Pattern cast: `weaponConfig is RangeWeaponConfig rangeConfig`, `_currentWeaponConfig as ThrowWeaponConfig`
- `ThrownObject.Initialize(lifespan, actionRadius, damage, gravityScale, action, velocity)` — raw params

### PlayerView
- `[SerializeField] WeaponConfigRegistry + BulletConfigRegistry`

## Enemy Weapon Config — Polymorphic Architecture (same style as Player)

### Class Hierarchy
```
EnemyWeaponConfig (abstract)                   ← _weaponType (WeaponType), _damage, _attackRange, _attackCooldown
├── EnemyMeleeWeaponConfig : EnemyWeaponConfig ← Không có field riêng
├── EnemyRangedWeaponConfig : EnemyWeaponConfig← _bulletId (int)
└── EnemyThrowWeaponConfig : EnemyWeaponConfig ← _throwAngle, _minThrowForce, _maxThrowForce,
                                                  _actionType, _actionRadius, _objectLifespan,
                                                  _gravityScale, _objectPrefab
```

### EnemyWeaponConfigRegistry
- `[SerializeReference] EnemyWeaponConfig[] _configs`
- `Dictionary<WeaponType, EnemyWeaponConfig> _lookup`
- `GetConfig(WeaponType type)` → `EnemyWeaponConfig`

### EnemyWeaponConfigPropertyDrawer
- Dropdown WeaponType → auto-switch instance type (Melee↔Range↔Throwing)
- Null guard + nút "Create New (Melee)"
- Throwing: 8 extra fields

## Enemy Attack Strategies (Shared)
Xem thư mục `Assets/App/Scripts/Combat/Attack/` — dùng chung cho cả Player + Enemy.

### IAttackStrategy
```csharp
public interface IAttackStrategy
{
    WeaponType AttackType { get; }
    void Execute(Vector3 attackerPos, Transform attackerTransform,
        Entity targetEntity, Vector3 targetPos, float damage,
        IHealthAccessor healthAccessor, bool faceTarget,
        Action<float> fallbackDamageDealer = null);
}
```

### AttackStrategyRegistry
```csharp
// 2 factory methods:
AttackStrategyRegistry.CreateForEnemy(EnemyWeaponConfigRegistry, BulletConfigRegistry)
AttackStrategyRegistry.CreateForPlayer(int weaponId, WeaponConfigRegistry, BulletConfigRegistry)

// Methods:
registry.Get(WeaponType) → IAttackStrategy
registry.Replace(WeaponType, IAttackStrategy)  // Cho Player đổi vũ khí runtime
AttackStrategyRegistry.RegisterFromConfig(registry, WeaponBase, BulletConfigRegistry) // Static helper
```

### IHealthAccessor + Implementations
```csharp
interface IHealthAccessor { bool TryApplyDamage(Entity entity, float damage); }
PlayerHealthAccessor → ghi PlayerHealth ECS (dùng cho Enemy)
EnemyHealthAccessor  → ghi EnemyHealth ECS (dùng cho Player)
```

### IThrowConfig
```csharp
interface IThrowConfig { ThrowAngle, MinThrowForce, MaxThrowForce, GravityScale, ObjectLifespan, ActionRadius, ActionType, ObjectPrefab }
// Implemented by: ThrowWeaponConfig (Player) + EnemyThrowWeaponConfig (Enemy)
```

### 3 Strategy Classes (stateless, shared)
- **MeleeAttackStrategy()** — FaceTarget → healthAccessor.TryApplyDamage → fallback
- **RangedAttackStrategy(BulletConfig, float bulletDamage)** — FaceTarget → BulletSpawner
- **ThrowAttackStrategy(IThrowConfig)** — ballistics → Instantiate ThrownObject

## ThrownObject (Refactored)
```csharp
public void Initialize(float lifespan, float actionRadius, float damage,
    float gravityScale, IThrowAction action, Vector3 velocity)
```
- Không còn phụ thuộc `ThrowWeaponConfig` → dùng chung cho cả Player và Enemy

## Combat Stats: Single Source of Truth
- **Player**: `WeaponConfigRegistry.GetConfig(weaponId)` → `Damage`, `AttackRange`, `AttackCooldown`
- **Enemy**: `EnemyWeaponConfigRegistry.GetConfig(weaponType)` → `Damage`, `AttackRange`, `AttackCooldown`
- **BulletConfig**: chỉ chứa di chuyển/visual (Speed, Range, MovementType, VisualPrefab) — **không chứa Damage**
- **EnemyView + EnemyViewConfig**: chỉ chứa di chuyển/sinh tồn (MoveSpeed, Health, DetectionRange) — **không chứa combat stats**

## ECS System Execution Order
```
SimulationSystemGroup
  ├── EnemySystemGroup
  │     ├── EnemyDetectionSystem               ← distance→DetectionState
  │     └── EnemyCombatSystem                  ← cooldown→NeedsCombatResult
  ├── PlayerWeaponDetectionSystem              ← find closest enemy in radius
  └── BulletSystemGroup
        ├── BulletMovementSystem
        ├── BulletHitDetectionSystem
        ├── BulletRayCastSystem
        └── BulletCleanupSystem
```

## Data Flow (Enemy)
```
Spawner → EnemyPresenter(view, config, registry, enemyWeaponRegistry, playerTarget)
  → weaponConfig = enemyWeaponRegistry.GetConfig(config.AttackType)  // WeaponType key
  → AttackStrategy = registry.Get(config.AttackType)
  → AttackDamage = weaponConfig.Damage (không còn AttackDamage)
  → SetEntity → sync ECS EnemyStats

LateUpdate → EnemyStateAttack:
  CachedNeedsCombatResult → Strategy.Execute(view, target, presenter.AttackDamage)
    → Melee: PlayerHealth.Value -= damage (ECS)
    → Range: BulletSpawner.SpawnBullet(config, damage, firePos, PlayerEntity)
    → Throwing: ballistics → Instantiate ObjectPrefab → ThrownObject
```

## Startup Flow
```
GameManager.Awake() → Configure(builder) → CoreInstaller + PlayerInstaller + EnemyInstaller(empty) + WaveInstaller
GameManager.Start() → EnemyECSWorldBootstrap.Initialize() → ECS singletons + systems
  → WaveSpawnerManager.StartWaves() → ProcessWave(0) → spawn enemies
```

## Setup Guide
```
1. Assets → Create → ZombiesWar → Weapon Config Registry (Player)
   - Inspector: thêm element, chọn Type qua dropdown WeaponType
   - Melee: WeaponId, Damage, AttackRange, AttackCooldown
   - Range: + BulletId; Throw: + ThrowAngle, ActionType, ObjectPrefab,...

2. Assets → Create → ZombiesWar → Enemy Weapon Config Registry (Enemy)
   - Inspector: thêm element, chọn Type qua dropdown WeaponType (cùng style)
   - Assign vào EnemySpawner trong Scene

3. EnemyView Prefab: WeaponType _attackType = Melee/Range/Throwing

4. PlayerView: assign WeaponConfigRegistry + BulletConfigRegistry
```
