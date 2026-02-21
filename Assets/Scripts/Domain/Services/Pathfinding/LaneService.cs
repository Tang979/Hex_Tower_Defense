using System;
using System.Collections.Generic;
using Domain.Entities;
using Domain.Services.Pathfinding;

namespace Domain.Services
{
    public class LaneService
    {
        private readonly HexMap _map;
        private readonly IPathfinder _pathfinder;
        private readonly Dictionary<int, List<HexTile>> _lanePaths = new Dictionary<int, List<HexTile>>();

        public LaneService(HexMap map, IPathfinder pathfinder)
        {
            _map = map;
            _pathfinder = pathfinder;
        }

        public void UpdatePath(Dictionary<int,List<HexTile>> newPath)
        {
            _lanePaths.Clear();
            foreach (var path in newPath)
            {
                _lanePaths[path.Key] = path.Value;
            }
        }

        public void RecalculateAllPaths()
        {
            _lanePaths.Clear();
            for (int i = 0; i < _map.SpawnTiles.Count; i++)
            {
                var path = _pathfinder.FindPath(_map, _map.SpawnTiles[i], _map.TargetTile);
                if (path != null)
                {
                    _lanePaths[i] = path;
                }
            }
        }

        public List<HexTile> GetPath(int laneId)
        {
            return _lanePaths.TryGetValue(laneId, out var path) ? path : null;
        }
        
        public Dictionary<int, List<HexTile>> GetAllPaths() => _lanePaths;

        public HexTile GetTileAtPathIndex(int currentLaneId, int pathIndex)
        {
            List<HexTile> path = GetPath(currentLaneId);

            if (path != null && pathIndex >= 0 && pathIndex < path.Count)
            {
                return path[pathIndex];
            }
            return null;
        }
    }
}