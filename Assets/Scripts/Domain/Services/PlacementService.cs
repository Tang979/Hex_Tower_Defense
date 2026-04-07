using System;
using System.Collections.Generic;
using Domain.Entities;
using Domain.Enums;
using Domain.Services.Pathfinding;

namespace Domain.Services
{
    public class PlacementService
    {
        private readonly HexMap _map;
        private readonly IPathfinder _pathfinder;
        private LaneService _laneService;

        public Action<HexTile> OnMapChanged;

        public PlacementService(HexMap map, IPathfinder pathfinder, LaneService laneService)
        {
            _map = map;
            _pathfinder = pathfinder;
            _laneService = laneService;
        }

        public bool CheckPlace(Dictionary<int, List<HexTile>> tempPath)
        {

            for (int i = 0; i < _map.SpawnTiles.Count; i++)
            {
                var path = _pathfinder.FindPath(_map, _map.SpawnTiles[i], _map.TargetTile);
                if (path == null)
                {
                    return false;
                }
                tempPath[i] = path;
            }
            return true;
        }

        public bool TryPlaceCrystal(HexTile tile, int totalActiveEnemies, bool isTrap)
        {
            if (!tile.IsWalkable() || tile.State == HexState.Spawn || tile.State == HexState.Target) return false;

            if (tile.EnemyCount > 0 && !isTrap) return false;

            if(isTrap)
            {
                tile.SetState(HexState.Trap);
                return true;
            }

            var originalState = tile.State;
            tile.SetState(HexState.Tower);

            var tempPath = new Dictionary<int, List<HexTile>>();
            var canPlace = CheckPlace(tempPath);

            if (!canPlace || !CheckDeadlockEnemy(totalActiveEnemies))
            {
                tile.SetState(originalState);
                return false;
            }

            _laneService.UpdatePath(tempPath);
            OnMapChanged?.Invoke(tile);
            return true;
        }

        public void RemoveCrystal(HexTile tile)
        {
            if (tile != null && tile.State == HexState.Tower)
            {
                tile.SetState(HexState.Walkable);
                
                _laneService.RecalculateAllPaths();
                
                OnMapChanged?.Invoke(tile);
            }
        }

        private bool CheckDeadlockEnemy(int totalActiveEnemies)
        {
            int reachableEnemiesCount = 0;

            // Queue để duyệt
            Queue<HexTile> frontier = new Queue<HexTile>();
            // Hashset để đánh dấu đã duyệt (tránh lặp vô tận)
            HashSet<HexTile> visited = new HashSet<HexTile>();

            // Bắt đầu từ Đích (Target)
            frontier.Enqueue(_map.TargetTile);
            visited.Add(_map.TargetTile);

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                // Cộng dồn số quái đang đứng trên ô này (nếu có)
                reachableEnemiesCount += current.EnemyCount;

                // Duyệt hàng xóm
                foreach (var neighbor in _map.GetNeighbors(current))
                {
                    // Logic lọc:
                    // 1. Phải đi được (IsWalkable đã bao gồm check Blocked)
                    // 2. Chưa từng duyệt qua
                    if (neighbor.IsWalkable() && !visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        frontier.Enqueue(neighbor);
                    }
                }
            }

            // Duyệt hết bản đồ mà số quái tìm được vẫn ít hơn tổng số -> Có con bị kẹt
            return reachableEnemiesCount == totalActiveEnemies;
        }
    }
}