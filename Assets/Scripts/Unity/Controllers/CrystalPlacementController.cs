using System.Collections.Generic;
using UnityEngine;
using Domain.Entities;
using Domain.Core.Data;

public class CrystalPlacementController : MonoBehaviour
{
    [SerializeField] private GameController _game;
    [SerializeField] private Camera _cam;
    [SerializeField] private LayoutAdapter _layoutAdapter;

    private Ray ray;
    private Plane plane = new Plane(Vector3.up, Vector3.zero);

    [Header("Team Loadout (Visuals)")]
    [SerializeField] private List<TowerOS> _teamTowers; // Loadout lấy từ ScriptableObject

    private Dictionary<string, TowerStatData> _towerStatsDict; // Dữ liệu cân bằng lấy từ JSON

    private TowerOS _currentSelectedTowerOS;
    private GameObject _currentGhost;
    private bool _isDragging = false;
    private InputService _inputService;

    public void Initialize(InputService inputService, Dictionary<string, TowerStatData> statsDict, List<TowerOS> teamTowers)
    {
        _inputService = inputService;
        _towerStatsDict = statsDict; // Nhận dữ liệu JSON từ GameController
        _teamTowers = teamTowers;   // Nhận loadout từ GameController
    }

    void Update()
    {
        if (_isDragging && _currentGhost != null)
        {
            Dragging();
        }
    }

    // Nút UI sẽ gọi hàm này và truyền vào Id của TowerOS
    public void StartDragging(string towerId)
    {
        _currentSelectedTowerOS = _teamTowers.Find(t => t.Id == towerId);

        Debug.Log($"Start dragging tower: {_currentSelectedTowerOS?.ghostPrefab} with ID: {towerId}");
        if (_currentSelectedTowerOS != null && _currentSelectedTowerOS.ghostPrefab != null)
        {
            _currentGhost = Instantiate(_currentSelectedTowerOS.ghostPrefab);
            _isDragging = true;

            // Đảm bảo Ghost không block raycast
            var colliders = _currentGhost.GetComponentsInChildren<Collider>();
            foreach(var col in colliders) col.enabled = false;

            Vector2 screenPos = _inputService.GetPointerPosition();
            ray = _cam.ScreenPointToRay(screenPos);

            if (plane.Raycast(ray, out float dist))
            {
                _currentGhost.transform.position = ray.GetPoint(dist);
            }
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
            var tile = _game.Map.GetTile(q, r);

            if (tile != null)
            {
                _currentGhost.transform.position = _layoutAdapter.HexToWorld(tile);
            }
        }
    }

    public void EndDragging()
    {
        if (!_isDragging) return;
        _isDragging = false;

        if (TryPlaceCrystal())
        {
            // Build thành công -> Bỏ ghost
            Destroy(_currentGhost); 
            _currentGhost = null;
        }
        else
        {
            // Build thất bại -> Hủy ghost
            Destroy(_currentGhost);
            _currentGhost = null;
        }
        _currentSelectedTowerOS = null;
    }

    private bool TryPlaceCrystal()
    {
        if (_currentSelectedTowerOS == null) return false;

        Vector3 worldPos = _currentGhost.transform.position;
        var (q, r) = _layoutAdapter.WorldToHexCoords(worldPos);
        if (q < 0 || r < 0) return false;
        
        var tile = _game.Map.GetTile(q, r);
        if (tile == null) return false;

        // 1. Dùng Domain kiểm tra có đặt được không (Không bị Deadlock)
        int currentEnemyCount = _game.EnemyService.ActiveEnemies.Count;
        bool success = _game.PlacementService.TryPlaceCrystal(tile, currentEnemyCount);

        if (success)
        {
            // --- HYBRID DATA IN ACTION ---
            
            // 2. Lấy dữ liệu Logic từ JSON Dictionary
            if (!_towerStatsDict.TryGetValue(_currentSelectedTowerOS.Id, out TowerStatData stats))
            {
                Debug.LogError($"Thiếu dữ liệu JSON cân bằng cho Tower ID: {_currentSelectedTowerOS.Id}");
                return false;
            }

            // 3. Khởi tạo Domain Entity (Thuần C#) với dữ liệu JSON
            Tower domainTower = new Tower(
                _currentSelectedTowerOS.Id, 
                _currentSelectedTowerOS.towerName, 
                stats.Range, 
                stats.Damage, 
                stats.AttackCooldown, 
                _currentSelectedTowerOS.targetPriority
            );

            // Gắn Strategy cho Tower (dựa trên AttackType trong SO)
            // Tạm thời giả sử bạn có class Factory, hoặc map cứng ở đây:
            // domainTower.SetAttackStrategy(new ProjectileAttackStrategy()); 

            // 4. Khởi tạo Unity View (Prefab)
            GameObject towerObject = Instantiate(_currentSelectedTowerOS.towerPrefab, _layoutAdapter.HexToWorld(tile), Quaternion.identity);
            
            // 5. Kết nối View với Domain
            TowerView towerView = towerObject.GetComponent<TowerView>();
            if (towerView != null)
            {
                towerView.Initialize(domainTower);
            }

            // 6. Thông báo Map thay đổi
            _game.OnMapChanged(); 
            return true;
        }

        return false;
    }
}