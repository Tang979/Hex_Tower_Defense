using System;
using System.Collections.Generic;
using Domain.Entities;

namespace Domain.Services.Pathfinding
{
    public class AStarPathfinder : IPathfinder
    {
        private int _currentSearchId = 0;
        private PriorityQueue<HexTile, float> _openSet = new PriorityQueue<HexTile, float>();
        private HashSet<HexTile> _openSetHash = new HashSet<HexTile>();
        private HashSet<HexTile> _closedSet = new HashSet<HexTile>();

        public List<HexTile> FindPath(HexMap map, HexTile start, HexTile end)
        {
            if (start == null || end == null) return null;

            _currentSearchId++;
            _openSet.Clear();
            _openSetHash.Clear();
            _closedSet.Clear();

            _openSet.Enqueue(start, 0);
            _openSetHash.Add(start);
            start.searchId = _currentSearchId;
            start.G = 0;
            start.H = GetHeuristicWithTieBreaker(start, start, end);

            while (_openSet.Count > 0)
            {
                var current = _openSet.Dequeue();
                _openSetHash.Remove(current);

                if (current == end) return RetracePath(start, end);

                if (_closedSet.Contains(current)) continue;
                _closedSet.Add(current);

                foreach (var neighbor in map.GetNeighbors(current))
                {
                    if (!neighbor.IsWalkable() || _closedSet.Contains(neighbor)) continue;

                    if (neighbor.searchId != _currentSearchId)
                    {
                        neighbor.G = int.MaxValue;
                        neighbor.H = 0;
                        neighbor.Parent = null;
                        neighbor.searchId = _currentSearchId;
                    }

                    float newMovementCostToNeighbor = current.G + 1f;
                    if (newMovementCostToNeighbor < neighbor.G)
                    {
                        neighbor.G = newMovementCostToNeighbor;
                        neighbor.H = GetHeuristicWithTieBreaker(neighbor, start, end);
                        neighbor.Parent = current;
                        float priority = neighbor.G + neighbor.H;
                        _openSet.Enqueue(neighbor, priority);

                        if (!_openSetHash.Contains(neighbor))
                        {
                            _openSetHash.Add(neighbor);
                        }
                    }
                }
            }
            return null;
        }

        private List<HexTile> RetracePath(HexTile start, HexTile end)
        {
            var path = new List<HexTile>();
            var curr = end;
            while (curr != start)
            {
                path.Add(curr);
                curr = curr.Parent;
            }
            path.Add(start);
            path.Reverse();
            return path;
        }
        private float GetHeuristicWithTieBreaker(HexTile node, HexTile start, HexTile end)
        {
            float h = (Math.Abs(node.Q - end.Q) + Math.Abs(node.R - end.R) + Math.Abs(node.S - end.S)) / 2f;

            float dx1 = end.Q - start.Q;
            float dy1 = end.R - start.R;

            float dx2 = node.Q - start.Q;
            float dy2 = node.R - start.R;

            float crossProduct = Math.Abs(dx1 * dy2 - dx2 * dy1);
            return h + (crossProduct * 0.001f);
        }
    }
}