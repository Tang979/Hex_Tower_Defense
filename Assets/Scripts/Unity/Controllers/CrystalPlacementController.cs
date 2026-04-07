using System;
using System.Collections.Generic;
using UnityEngine;
using Domain.Entities;
using Domain.Core.Data;
using Domain.Enums;
using Domain.Interface;
using Domain.Services.Combat.Modifier;
using Domain.Services.Combat.Strategy; // Thêm thư viện chứa IAttackModifier

public class CrystalPlacementController : MonoBehaviour
{
    [SerializeField] private Camera _cam;
    [SerializeField] private LayoutAdapter _layoutAdapter;

    private Ray ray;
    private Plane plane = new Plane(Vector3.up, Vector3.zero);

    [Header("Team Loadout (Visuals Only)")]
    private List<TowerOS> _teamTowers;

    private Dictionary<string, TowerStatData> _towerStatsDict;
    private TowerOS _currentSelectedTowerOS;
    private GameObject _currentGhost;
    private bool _isDragging = false;
    public bool IsMapDragging { get; private set; }
    private TowerView _originTowerView;
    private InputService _inputService;

    // [THÊM MỚI] Nhà máy đúc Modifier
    private Dictionary<string, Func<IAttackModifier>> _modifierFactory;

    public void Initialize(InputService inputService, Dictionary<string, TowerStatData> statsDict, List<TowerOS> teamTowers)
    {
        _inputService = inputService;
        _towerStatsDict = statsDict;
        _teamTowers = teamTowers;

        _modifierFactory = new Dictionary<string, Func<IAttackModifier>>
        {
            { "SplashModifier", () => new SplashModifier() },
            { "StatusEffectModifier", () => new StatusEffectModifier() },
            { "ChainModifier", () => new ChainModifier() },
            { "AlternatingEffectModifier", () => new AlternatingEffectModifier() }
        };
    }

    void Update()
    {
        if (_isDragging && _currentGhost != null)
        {
            Dragging();
        }
    }

    public void StartDragging(string towerId)
    {
        if (Time.timeScale == 0f) return;

        _currentSelectedTowerOS = _teamTowers.Find(t => t.Id == towerId);

        if (_currentSelectedTowerOS != null && _currentSelectedTowerOS.ghostPrefab != null)
        {
            _currentGhost = Instantiate(_currentSelectedTowerOS.ghostPrefab);
            _isDragging = true;
            IsMapDragging = false;
            _originTowerView = null;

            var colliders = _currentGhost.GetComponentsInChildren<Collider>();
            foreach (var col in colliders) col.enabled = false;

            GameController.Instance.CameraController.SetDragEnabled(false);
            MoveGhostToMouse();
        }
    }

    public void StartMapDragging(TowerView originTower)
    {
        if (Time.timeScale == 0f || originTower == null) return;
        
        _currentSelectedTowerOS = _teamTowers.Find(t => t.Id == originTower.Tower.Id);
        
        if (_currentSelectedTowerOS != null && _currentSelectedTowerOS.ghostPrefab != null)
        {
            _originTowerView = originTower;
            _currentGhost = Instantiate(_currentSelectedTowerOS.ghostPrefab);
            _isDragging = true;
            IsMapDragging = true;
            
            var colliders = _currentGhost.GetComponentsInChildren<Collider>();
            foreach (var col in colliders) col.enabled = false;

            GameController.Instance.CameraController.SetDragEnabled(false);
            MoveGhostToMouse();
        }
    }

    private void MoveGhostToMouse()
    {
        Vector2 screenPos = _inputService.GetPointerPosition();
        ray = _cam.ScreenPointToRay(screenPos);

        if (plane.Raycast(ray, out float dist))
        {
            _currentGhost.transform.position = ray.GetPoint(dist);
        }
    }

    public void Dragging()
    {
        Vector2 screenPos = _inputService.GetPointerPosition();
        ray = _cam.ScreenPointToRay(screenPos);

        if (plane.Raycast(ray, out float dist))
        {
            Vector3 worldPos = ray.GetPoint(dist);
            _currentGhost.transform.position = worldPos;

            var (q, r) = _layoutAdapter.WorldToHexCoords(worldPos);
            var tile = GameController.Instance.Map.GetTile(q, r);

            if (tile != null)
            {
                _currentGhost.transform.position = _layoutAdapter.HexToWorld(tile);
            }
        }
    }

    public void EndMapDraggingCancel()
    {
        if (_currentGhost != null) Destroy(_currentGhost);
        _currentGhost = null;
        _currentSelectedTowerOS = null;
        _isDragging = false;
        IsMapDragging = false;
        _originTowerView = null;
        GameController.Instance.CameraController.SetDragEnabled(true);
    }

    public void EndDragging()
    {
        if (!_isDragging) return;
        _isDragging = false;

        if (TryPlaceCrystal())
        {
            Destroy(_currentGhost);
        }
        else
        {
            Destroy(_currentGhost);
        }
        _currentGhost = null;
        _currentSelectedTowerOS = null;
        IsMapDragging = false;
        _originTowerView = null;
        GameController.Instance.CameraController.SetDragEnabled(true);
    }

    private bool TryPlaceCrystal()
    {
        if (_currentSelectedTowerOS == null) return false;

        if (!_towerStatsDict.TryGetValue(_currentSelectedTowerOS.Id, out TowerStatData stats))
        {
            Debug.LogError($"Thiếu dữ liệu JSON cho Tower ID: {_currentSelectedTowerOS.Id}");
            return false;
        }

        if (!GameController.Instance.CurrencyService.CanAfford(stats.Cost))
        {
            Debug.Log($"Không đủ tiền!");
            return false;
        }

        Vector3 worldPos = _currentGhost.transform.position;
        var (q, r) = _layoutAdapter.WorldToHexCoords(worldPos);
        var tile = GameController.Instance.Map.GetTile(q, r);

        if (tile == null) return false;

        TowerView targetTower = GameController.Instance.GetTowerAt(tile);
        if (targetTower != null && targetTower != _originTowerView)
        {
            return TryMergeTowers(targetTower);
        }

        if (IsMapDragging) 
        {
            return false;
        }

        int currentEnemyCount = GameController.Instance.EnemyService.ActiveEnemies.Count;
        bool success = GameController.Instance.PlacementService.TryPlaceCrystal(tile, currentEnemyCount, stats.IsTrap);

        if (success)
        {
            GameController.Instance.CurrencyService.SpendCurrency(stats.Cost);
            SpawnTowerAtTile(tile, stats, _currentSelectedTowerOS);
            return true;
        }

        return false;
    }

    private bool TryMergeTowers(TowerView targetTower)
    {
        if (_currentSelectedTowerOS == null || targetTower == null) return false;
        
        string dragTowerId = _currentSelectedTowerOS.Id;
        string targetTowerId = targetTower.Tower.Id;
        
        var recipe = GameController.Instance.ValidMergeRecipes.Find(r => 
            (r.ComponentA_Id == dragTowerId && r.ComponentB_Id == targetTowerId) ||
            (r.ComponentA_Id == targetTowerId && r.ComponentB_Id == dragTowerId)
        );
        
        if (recipe == null) return false;
        
        int totalCost = recipe.MergeCost;
        if (!IsMapDragging)
        {
            if (_towerStatsDict.TryGetValue(dragTowerId, out TowerStatData dragStats))
            {
                totalCost += dragStats.Cost;
            }
        }

        if (!GameController.Instance.CurrencyService.CanAfford(totalCost))
        {
            Debug.Log($"Không đủ tiền ghép tháp! Cần {totalCost}");
            return false; 
        }
        
        if (!_towerStatsDict.TryGetValue(recipe.ResultTower_Id, out TowerStatData resultStats))
        {
            Debug.LogError($"Thiếu dữ liệu JSON cho tháp kết quả: {recipe.ResultTower_Id}");
            return false;
        }
        
        TowerOS resultTowerOS = _teamTowers.Find(t => t.Id == recipe.ResultTower_Id);
        if (resultTowerOS == null)
        {
            resultTowerOS = ResourceManager.GetTowerOS(recipe.ResultTower_Id);
            if (resultTowerOS == null) return false;
        }

        GameController.Instance.CurrencyService.SpendCurrency(totalCost);
        
        HexTile targetTile = targetTower.CurrentTile;
        GameController.Instance.DestroyTowerViewSilently(targetTower);
        
        if (IsMapDragging && _originTowerView != null)
        {
            GameController.Instance.DestroyTowerViewSilently(_originTowerView);
        }
        
        bool success = GameController.Instance.PlacementService.TryPlaceCrystal(targetTile, GameController.Instance.EnemyService.ActiveEnemies.Count, resultStats.IsTrap);
        if (success)
        {
            SpawnTowerAtTile(targetTile, resultStats, resultTowerOS);
        }
        return true;
    }

    private void SpawnTowerAtTile(HexTile tile, TowerStatData stats, TowerOS towerOS)
    {
        Tower domainTower = new Tower(
            stats.Id,
            towerOS.towerName,
            stats.Range,
            stats.Damage,
            stats.AttackCooldown,
            stats.AttackType,
            stats.TargetPriority,
            stats.Effects,
            stats.IsTrap
        );

        AssignAttackStrategy(domainTower, stats.AttackType, stats);

        GameObject towerObject = GameController.Instance.PoolManager.GetObject(towerOS.towerPrefab);
        towerObject.transform.position = _layoutAdapter.HexToWorld(tile);
        towerObject.transform.rotation = Quaternion.identity;

        TowerView towerView = towerObject.GetComponent<TowerView>();
        if (towerView != null)
        {
            towerView.Initialize(domainTower, towerOS.bulletPrefab, tile, towerOS.towerPrefab);
            GameController.Instance.RegisterTower(tile, towerView);
        }
        towerObject.SetActive(true);
        GameController.Instance.OnMapChanged();
    }

    private void AssignAttackStrategy(Tower tower, AttackType type, TowerStatData stats)
    {
        List<IAttackModifier> activeModifiers = new List<IAttackModifier>();

        if (stats.Modifiers != null)
        {
            foreach (ModifierConfig config in stats.Modifiers)
            {
                if (_modifierFactory.TryGetValue(config.ModifierName, out var createModifier))
                {
                    IAttackModifier newModifier = createModifier();
                    newModifier.Initialize(config);
                    activeModifiers.Add(newModifier);
                }
            }
        }

        switch (type)
        {
            case AttackType.Projectile:
                tower.SetAttackStrategy(new ProjectileAttackStrategy(activeModifiers, GameController.Instance.EnemyService));
                break;
            case AttackType.AoE:
                tower.SetAttackStrategy(new AoEAttackStrategy(activeModifiers));
                break;
            case AttackType.Instant:
                tower.SetAttackStrategy(new InstantAttackStrategy(activeModifiers, GameController.Instance.EnemyService));
                break;
            default:
                tower.SetAttackStrategy(new ProjectileAttackStrategy(activeModifiers, GameController.Instance.EnemyService));
                break;
        }
    }
}