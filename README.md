# 🏰 Hex Tower Defense

A strategic tower defense game built with **Unity** featuring hexagonal grid-based maps, tower merging mechanics, and advanced combat systems. The project demonstrates clean architecture principles with Domain-driven design and modern async patterns.

![Game](https://img.shields.io/badge/Platform-Unity%202022.3%2B-blueviolet)
![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-brightgreen)
![Language](https://img.shields.io/badge/Language-C%23%2010-blue)

---

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Project Architecture](#project-architecture)
- [Core Systems](#core-systems)
- [Installation & Setup](#installation--setup)
- [Development Guide](#development-guide)
- [API Reference](#api-reference)

---

## 🎮 Overview

**Hex Tower Defense** is a turn-based strategy game where players defend their base from waves of enemies by strategically placing and upgrading towers on a hexagonal grid. The unique **tower merging system** allows players to combine similar towers to unlock more powerful variants.

### Key Gameplay Loop
1. Place towers on hexagonal tiles
2. Configure tower targeting priorities
3. Defeat enemy waves to earn currency
4. Merge towers to unlock upgraded versions
5. Survive and defeat the boss wave
## 📥 Download Demo
You can download the latest playable APK [HERE](https://github.com/Tang979/Aetheric_Mobile/releases/download/v1.0.0/HexTowerDefense.apk)
---

## ✨ Features

### Core Gameplay
- **Hexagonal Grid System**: Strategic placement on hex-based maps with optimal pathfinding
- **Tower Merging**: Combine two towers to create upgraded variants with new attack types
- **Dynamic Wave System**: Configurable enemy waves with spawn scheduling
- **Multi-lane Paths**: Enemies can attack from multiple directions

### Tower System
- **5+ Attack Strategies**: 
  - `Projectile` - Single target projectiles
  - `MultiProjectile` - Burst projectiles
  - `AoE` - Area of effect attacks
  - `Instant` - Continuous laser attacks ⭐ (Fixed memory leak)
  - `Chain` - Bouncing attacks between enemies

- **Targeting Priorities**:
  - `First` - Closest to exit
  - `Last` - Closest to entrance
  - `Strongest` - Highest health
  - `Weakest` - Lowest health

- **Status Effects**:
  - `Stun` - Disable enemy movement
  - `Slow` - Reduce enemy speed
  - `DOT` (Damage Over Time) - Continuous damage application

- **Attack Modifiers**: Customize behavior with composable modifiers:
  - Splash damage
  - Chaining attacks
  - Status effects
  - Alternating visuals

### Infrastructure
- **Object Pooling**: Efficient memory management for projectiles and visuals
- **Async Resource Loading**: Lazy loading towers and prefabs
- **Event-Driven Architecture**: Decoupled systems via events
- **Enemy Registry**: Real-time tracking of active enemy entities

---

## 🛠️ Tech Stack

### Core Technologies
| Component | Technology | Version |
|-----------|-----------|---------|
| **Engine** | Unity | 2022.3+ |
| **Language** | C# | 10+ |
| **.NET** | .NET Standard | 2.1 |
| **Serialization** | Newtonsoft.Json | Latest |
| **UI** | TextMesh Pro | 3.0+ |
| **Rendering** | URP | Latest |

### Design Patterns
- **Clean Architecture** - Separation of Domain, Infrastructure layers
- **SceneSingleton** - Lifecycle management for global systems
- **Object Pool** - Projectile and prefab management
- **Strategy Pattern** - Attack type polymorphism
- **Observer Pattern** - Event-driven communication
- **Factory Pattern** - Resource creation and instantiation

---

## 🏗️ Project Architecture

### Directory Structure
```
Assets/
├── Scripts/
│   ├── Domain/                          # Business logic (independent of Unity)
│   │   ├── Core/Data/                   # Data structures & serialization
│   │   │   ├── TowerStatData.cs         # Tower configuration
│   │   │   ├── EnemyStatData.cs         # Enemy configuration
│   │   │   ├── MergeRecipeData.cs       # Tower merge recipes
│   │   │   ├── HexMapDefinition.cs      # Map layout definition
│   │   │   ├── WaveData.cs              # Enemy wave configuration
│   │   │   └── LaneData.cs              # Lane path definition
│   │   │
│   │   ├── Entities/                    # Domain models
│   │   │   ├── Tower.cs                 # Tower entity with combat logic
│   │   │   ├── Enemy.cs                 # Enemy entity with pathfinding
│   │   │   ├── HexMap.cs                # Hex grid representation
│   │   │   └── HexTile.cs               # Individual hex tile
│   │   │
│   │   ├── Enums/                       # Enumeration types
│   │   │   ├── AttackType.cs            # Attack strategies
│   │   │   ├── TargetPriority.cs        # Targeting algorithms
│   │   │   ├── HexState.cs              # Tile occupation states
│   │   │   ├── EnemyEffect.cs           # Status effects
│   │   │   └── TargetStatus.cs          # Enemy conditions
│   │   │
│   │   ├── Services/                    # Domain services
│   │   │   ├── Combat/                  # Attack system
│   │   │   │   ├── IAttackStrategy.cs   # Attack interface
│   │   │   │   ├── InstantAttackStrategy.cs
│   │   │   │   ├── ProjectileAttackStrategy.cs
│   │   │   │   ├── AoEAttackStrategy.cs
│   │   │   │   ├── TowerAttackResult.cs # Attack result data
│   │   │   │   ├── IAttackModifier.cs   # Modifier interface
│   │   │   │   ├── AttackResultPool.cs  # Result object pooling
│   │   │   │   └── TargetPicker.cs      # Target selection logic
│   │   │   │
│   │   │   ├── EnemyService.cs          # Enemy management
│   │   │   ├── PlacementService.cs      # Tower placement validation
│   │   │   ├── CurrencyService.cs       # Resource management
│   │   │   ├── BaseHealthService.cs     # Base health tracking
│   │   │   └── Pathfinding/
│   │   │       ├── IPathfinder.cs       # Pathfinding interface
│   │   │       └── AStarPathfinder.cs   # A* implementation
│   │   │
│   │   ├── Interface/                   # Contracts
│   │   │   ├── IAttackModifier.cs       # Modifier interface
│   │   │   └── IActiveEnemyProvider.cs  # Query interface
│   │   │
│   │   └── ValueObject/                 # Value types
│   │       └── ActiveEffect.cs          # Effect application data
│   │
│   └── Unity/                           # Unity-specific implementation
│       ├── Controllers/                 # High-level orchestration
│       │   ├── GameController.cs        # Main game coordinator (SceneSingleton)
│       │   ├── CrystalPlacementController.cs  # Tower placement UI/logic
│       │   ├── WaveController.cs        # Enemy wave spawn management
│       │   ├── CameraController.cs      # Camera control
│       │   └── UIController.cs          # UI state management
│       │
│       ├── Views/                       # UI representation
│       │   ├── TowerView.cs             # Tower visual + attack handler ⭐
│       │   ├── EnemyView.cs             # Enemy visual + movement handler
│       │   ├── LaserView.cs             # Instant attack visual (persistent)
│       │   ├── ProjectileView.cs        # Projectile visual
│       │   ├── AoEAttackView.cs         # AoE explosion visual
│       │   ├── BouncingProjectileView.cs # Bouncing projectile visual
│       │   ├── BurstLaserView.cs        # Burst laser visual
│       │   ├── HexMeshView.cs           # Hex grid rendering
│       │   └── HexOutlineView.cs        # Hex selection outline
│       │
│       ├── Infrastructure/              # System utilities
│       │   ├── PoolManager.cs           # Object pooling (generic)
│       │   ├── ResourceManager.cs       # Asset loading & caching
│       │   ├── InputSystem.cs           # Input handling abstraction
│       │   ├── MapLoader.cs             # Hex map loader from JSON
│       │   ├── IAttackVisual.cs         # Visual attack interface
│       │   ├── SceneSingleton.cs        # Generic singleton pattern
│       │   ├── TowerOS.cs               # Tower scriptable object
│       │   ├── MapLoader.cs             # Map JSON loader
│       │   └── CrystalButton.cs         # Tower button UI component
│       │
│       └── Utils/                       # Helper utilities
│           └── MapGenerator.cs          # Runtime map generation
│
├── Resources/                           # Runtime-loadable assets
│   ├── Towers/                          # Tower ScriptableObjects
│   ├── Enemies/                         # Enemy prefabs & configs
│   ├── Maps/                            # Map JSON definitions
│   ├── Prefabs/                         # Visual prefabs
│   │   ├── Towers/
│   │   └── Enemies/
│   ├── Textures/                        # Sprite textures
│   ├── Materials/                       # 3D materials
│   └── Models/                          # 3D models
│
├── Scenes/
│   └── SampleScene.unity                # Main gameplay scene
│
├── Settings/                            # Project settings
│   └── URP Profiles/
│
└── Tests/                               # Unit tests
    └── ScriptTest/
```

### Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│                    Unity View Layer                         │
│  (TowerView, EnemyView, UI Controllers)                     │
└─────────────────────┬───────────────────────────────────────┘
                      │ MVC Pattern
┌─────────────────────▼───────────────────────────────────────┐
│              Unity Infrastructure Layer                      │
│  (PoolManager, ResourceManager, InputSystem)                │
└─────────────────────┬───────────────────────────────────────┘
                      │ Dependency Injection
┌─────────────────────▼───────────────────────────────────────┐
│            Domain Logic (Business Rules)                     │
│  (Tower.cs, Enemy.cs, Combat Services)                      │
│                                                              │
│  ✓ No Unity dependencies                                    │
│  ✓ Independently testable                                   │
│  ✓ Portable to other engines                                │
└──────────────────────────────────────────────────────────────┘
```

---

## 🎯 Core Systems

### 1. **Tower Combat System**

#### Attack Flow
```
TowerView.Update()
    ↓
Tower.Tick() → Check cooldown + select target
    ↓
Tower.CanAttack() → Validate conditions
    ↓
Tower.PullTrigger() → Execute attack strategy
    ↓
AttackStrategy.ExecuteAttack() → Calculate damage + effects
    ↓
AttachModifier.ExecuteOnFire() → Apply pre-hit modifiers
    ↓
IAttackVisual.Initialize() → Spawn projectile/visual from pool
    ↓
IAttackVisual.UpdateTargetsAndDamage() → Apply damage + modifiers
```

#### Attack Result Pool (Memory Optimization)
```csharp
// Pre-allocated result objects reused across attacks
TowerAttackResult result = AttackResultPool.GetPool();
result.IsSuccess = true;
result.DamageList = new List<float> { 50f, 50f };
result.AffectedEnemies = new List<Enemy> { enemy1, enemy2 };

// Damage application
foreach (var enemy in result.AffectedEnemies)
    enemy.TakeDamage(damage);

// Return to pool for reuse
AttackResultPool.ReturnPool(result);
```

### 2. **Object Pooling System**

```csharp
// Get from pool (reuse or instantiate if needed)
GameObject projectile = PoolManager.GetObject(projectilePrefab);

// Use it...
projectile.SetActive(true);
projectile.transform.position = firePoint.position;

// Return to pool when done
PoolManager.ReturnPool(projectilePrefab, projectile);
```

**Benefits**:
- ✅ Eliminates instantiate/destroy garbage
- ✅ Prevents instant tower visual memory leak (⭐ Fixed)
- ✅ Smoother frame rate in wave combat

### 3. **Tower Merging System**

#### Recipe Definition (JSON)
```json
{
  "Recipes": [
    {
      "RecipeId": "fire_to_inferno",
      "ComponentA_Id": "fire_tower",
      "ComponentB_Id": "fire_tower",
      "ResultTower_Id": "inferno_tower",
      "MergeCost": 100
    },
    {
      "RecipeId": "fire_ice_merge",
      "ComponentA_Id": "fire_tower",
      "ComponentB_Id": "ice_tower",
      "ResultTower_Id": "frozen_flame_tower",
      "MergeCost": 200
    }
  ]
}
```

#### Merge Validation Logic
```csharp
// Only show recipes where player has both components
foreach (var recipe in allRecipes)
{
    bool hasComponentA = teamLoadout.Exists(t => t.Id == recipe.ComponentA_Id);
    bool hasComponentB = teamLoadout.Exists(t => t.Id == recipe.ComponentB_Id);
    
    // Support same tower merges (A == B only need one copy)
    bool canMerge = (recipe.ComponentA_Id == recipe.ComponentB_Id) 
        ? hasComponentA 
        : (hasComponentA && hasComponentB);
    
    if (canMerge)
        validRecipes.Add(recipe);
}
```

### 4. **Pathfinding (A* Algorithm)**

```csharp
// A* finds optimal path through hex grid
List<HexTile> path = pathfinder.FindPath(startTile, endTile, hexMap);

// Enemy follows path with obstacle avoidance
foreach (var tile in path)
{
    yield return MoveToTile(tile);
}
```

**Grid Support**:
- Hexagonal offset coordinates
- 6-directional movement
- Dynamic obstacle avoidance

### 5. **Enemy Wave System**

#### Wave Configuration (JSON)
```json
{
  "Waves": [
    {
      "DelayBeforeStart": 5.0,
      "SpawnList": [
        { "EnemyId": "goblin", "Quantity": 10, "SpawnDelay": 0.5 }
      ]
    },
    {
      "DelayBeforeStart": 15.0,
      "SpawnList": [
        { "EnemyId": "orc", "Quantity": 5, "SpawnDelay": 1.0 },
        { "EnemyId": "boss", "Quantity": 1, "SpawnDelay": 0 }
      ]
    }
  ]
}
```

#### Spawning Logic
```csharp
foreach (var wave in laneData.Waves)
{
    yield return new WaitForSeconds(wave.DelayBeforeStart);
    
    foreach (var enemySpawn in wave.SpawnList)
    {
        for (int i = 0; i < enemySpawn.Quantity; i++)
        {
            SpawnEnemy(enemySpawn.EnemyId);
            yield return new WaitForSeconds(enemySpawn.SpawnDelay);
        }
    }
}
```

### 6. **Instant Tower Fix** ⭐

**Problem**: Laser visual didn't return to pool when tower was sold.

**Solution**:
```csharp
// TowerView.OnDisable() - Called when tower GameObject is pooled
private void OnDisable()
{
    if (_persistentVisual != null)
    {
        _persistentVisual.TurnOff();  // Returns laser to pool
        _persistentVisual = null;
    }
}

// Also check in Update() if target lost
if (Tower.AttackType == AttackType.Instant && Tower.CurrentTarget == null && _persistentVisual != null)
{
    _persistentVisual.TurnOff();
    _persistentVisual = null;
}
```

---

## 📦 Installation & Setup

### Requirements
- **Unity** 2022.3 or higher
- **Git** for version control
- **.NET 6.0** SDK (for development tools)

### Clone & Setup
```bash
# Clone repository
git clone https://github.com/your-org/Hex_Tower_Defense.git
cd Hex_Tower_Defense

# Open in Unity Hub or command line
unity -projectPath . -logFile -
```

### Project Setup
1. Open `SampleScene.unity` in Unity Editor
2. Configure Scriptable Objects in `Assets/Resources/Towers/`
3. Add map JSON files to `Assets/Resources/Maps/`
4. Assign tower button prefabs in `CrystalPlacementController`

---

## 🔧 Development Guide

### Adding a New Tower

1. **Create Tower Stat Data**
```csharp
// Create ScriptableObject in Resources/Towers/
var towerData = ScriptableObject.CreateInstance<TowerOS>();
towerData.Id = "frost_tower";
towerData.Name = "Frost Tower";
towerData.Damage = 45f;
towerData.Range = 8f;
towerData.AttackCooldown = 1.2f;
towerData.AttackType = AttackType.Projectile;
```

2. **Create Attack Modifiers (if custom)**
```csharp
public class FrostModifier : IAttackModifier
{
    public void ExecuteOnHit(TowerAttackResult result, Tower tower, 
        float damage, Enemy target, IActiveEnemyProvider provider)
    {
        // Apply slow effect
        var slowEffect = new ActiveEffect
        {
            Type = EnemyEffect.Slow,
            Duration = 3f,
            Magnitude = 0.5f
        };
        target.ApplyEffect(slowEffect);
    }
}
```

3. **Add Merge Recipe**
```json
{
  "RecipeId": "fire_frost_merge",
  "ComponentA_Id": "fire_tower",
  "ComponentB_Id": "frost_tower",
  "ResultTower_Id": "tempest_tower",
  "MergeCost": 300
}
```

### Adding a New Attack Type

1. **Create Strategy**
```csharp
public class SpikeAttackStrategy : IAttackStrategy
{
    public TowerAttackResult ExecuteAttack(Tower tower, Enemy mainTarget, List<Enemy> enemies)
    {
        var result = AttackResultPool.GetPool();
        result.IsSuccess = true;
        result.AffectedEnemies = enemies;
        result.DamageList = enemies.Select(_ => tower.Damage).ToList();
        return result;
    }
}
```

2. **Add Attack Type Enum**
```csharp
public enum AttackType
{
    Projectile,
    MultiProjectile,
    AoE,
    Instant,
    Chain,
    Spike  // New!
}
```

3. **Create Visual**
```csharp
public class SpikeAttackView : MonoBehaviour, IAttackVisual
{
    public void Initialize(TowerAttackResult result, Tower tower, 
        List<EnemyView> targets, Transform firePoint, GameObject prefab)
    {
        // Spawn spikes, play animation, etc.
    }
}
```

### Debugging Tips

```csharp
// Enable combat logging
Debug.Log($"[Tower] {tower.Name} attacking {target.Name}");
Debug.Log($"[Damage] {tower.Damage} to {enemies.Count} enemies");

// Check tower state
Debug.Assert(tower.CanAttack(enemies), "Tower cooldown active");

// Verify pool health
Debug.Log($"[Pool] {PoolManager.GetPoolSize(prefab)} objects available");
```

### Running Tests

```bash
# Run test framework from Unity
Window → General → Test Runner → Play Mode

# Or via command line
unity -projectPath . -runTests -testPlatform playmode
```

---

## 📖 API Reference

### ScriptableObject Data Types

#### TowerOS (Tower Object Specification)
```csharp
public class TowerOS : ScriptableObject
{
    public string Id;                          // Unique identifier
    public string Name;
    public float Damage;
    public float Range;
    public float AttackCooldown;
    public AttackType AttackType;
    public TargetPriority TargetPriority;
    public GameObject towerPrefab;
    public GameObject bulletPrefab;
    public List<EffectBlueprint> Effects;
}
```

#### MergeRecipeData
```csharp
public class MergeRecipeData
{
    public string RecipeId;
    public string ComponentA_Id;           // First tower ID
    public string ComponentB_Id;           // Second tower ID
    public string ResultTower_Id;          // Result tower ID
    public int MergeCost;                  // Currency cost
}
```

#### TowerAttackResult
```csharp
public class TowerAttackResult
{
    public bool IsSuccess;                 // Attack connected?
    public List<Enemy> AffectedEnemies;    // Damage targets
    public List<float> DamageList;         // Damage per enemy
    public List<ActiveEffect> AppliedEffects;
    public List<IAttackModifier> Modifiers;
    public string OverrideVisualId;        // Custom visual ID
}
```

### Key Interfaces

#### IAttackStrategy
```csharp
public interface IAttackStrategy
{
    TowerAttackResult ExecuteAttack(Tower tower, Enemy target, List<Enemy> enemies);
}
```

#### IAttackVisual
```csharp
public interface IAttackVisual
{
    void Initialize(TowerAttackResult result, Tower tower, 
        List<EnemyView> targets, Transform firePoint, GameObject prefab);
    void UpdateTargetsAndDamage(TowerAttackResult result, List<EnemyView> targets);
    void TurnOff();  // Returns to pool
}
```

#### IAttackModifier
```csharp
public interface IAttackModifier
{
    void ExecuteOnFire(TowerAttackResult result, Tower tower, IActiveEnemyProvider provider);
    void ExecuteOnHit(TowerAttackResult result, Tower tower, float damage, 
        Enemy target, IActiveEnemyProvider provider);
}
```

### Key Services

#### GameController (Singleton)
```csharp
static GameController Instance { get; }
Dictionary<Enemy, EnemyView> EnemyRegistry { get; }
Dictionary<HexTile, TowerView> ActiveTowers { get; }
PoolManager PoolManager { get; }
CurrencyService CurrencyService { get; }
BaseHealthService BaseHealthService { get; }
EnemyService EnemyService { get; }
PlacementService PlacementService { get; }
```

#### PoolManager
```csharp
GameObject GetObject(GameObject prefab);
void ReturnPool(GameObject prefab, GameObject obj);
void Prewarm(GameObject prefab, int count);
```

---

## 🐛 Common Issues & Solutions

### Issue: Memory leak with instant towers
**Solution**: Ensured `OnDisable()` calls `_persistentVisual.TurnOff()` ✅

### Issue: Projectiles not pooling properly
**Try**: Check `OnDisable()` in attack visual - must call `ReturnPool()`

### Issue: Pathfinding fails on map load
**Try**: Verify hex coordinates in map JSON match grid dimensions

### Issue: Tower merge recipes not showing
**Try**: Check JSON format in `mergeRecipesJson` - must match `MergeRecipeDatabase` structure

---

## 📄 License

This project uses the **AmyElliott Priority Queue** implementation which is free for personal & commercial use.
See [Assets/ThirdParty/PriorityQueue/LICENSE.md](Assets/Scripts/Domain/AmyElliott/PriorityQueue/LICENSE.md)

---

## 🤝 Contributing

1. Create feature branch: `git checkout -b feature/tower-system`
2. Commit changes: `git commit -m "feat: Add new tower type"`
3. Push branch: `git push origin feature/tower-system`
4. Open Pull Request

### Commit Convention
- `feat:` New feature
- `fix:` Bug fix (like instant tower visual cleanup)
- `refactor:` Code restructuring (like GameController singleton)
- `chore:` Cleanup (like removing deprecated assets)
- `docs:` Documentation changes

---

## 📞 Support & Contact

For questions or issues:
- Open an issue on GitHub
- Check existing documentation
- Review code comments for implementation details

---

**Last Updated**: April 2026  
**Maintainer**: @GameDev Team  
**Current Version**: 1.0.0
