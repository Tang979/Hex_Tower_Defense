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

                    // int newMovementCostToNeighbor = current.G + 1;
                    float newMovementCostToNeighbor = current.G + 1f;
                    if (newMovementCostToNeighbor < neighbor.G)
                    {
                        neighbor.G = newMovementCostToNeighbor;
                        // neighbor.H = GetDistance(neighbor, end);
                        neighbor.H = GetHeuristicWithTieBreaker(neighbor, start, end);
                        neighbor.Parent = current;
                        // int cost = neighbor.G + neighbor.H;
                        float priority = neighbor.G + neighbor.H;
                        _openSet.Enqueue(neighbor, priority);

                        if (!_openSetHash.Contains(neighbor))
                        {
                            _openSetHash.Add(neighbor);
                        }
                    }
                }
            }
            return null; // No path
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

        // Trong class AStarPathfinder của bạn

        // Thay vì dùng hàm GetDistance cũ cho H, hãy dùng hàm này:
        private float GetHeuristicWithTieBreaker(HexTile node, HexTile start, HexTile end)
        {
            // 1. Tính H chuẩn (Khoảng cách Hex)
            // Lưu ý: Phải cast sang float
            float h = (Math.Abs(node.Q - end.Q) + Math.Abs(node.R - end.R) + Math.Abs(node.S - end.S)) / 2f;

            // 2. Tính "Độ lệch" khỏi đường thẳng (Cross Product)
            // Ta coi tọa độ Hex (Q, R) như tọa độ 2D để tính tích chéo.

            // Vector từ Start -> End
            float dx1 = end.Q - start.Q;
            float dy1 = end.R - start.R;

            // Vector từ Start -> Node hiện tại
            float dx2 = node.Q - start.Q;
            float dy2 = node.R - start.R;

            // Tích chéo (Cross Product) cho biết độ lệch hướng
            // Giá trị này càng lớn -> Node càng lệch khỏi đường thẳng nối Start-End
            float crossProduct = Math.Abs(dx1 * dy2 - dx2 * dy1);

            // 3. Cộng điểm phạt (Nudge)
            // Cộng thêm một lượng siêu nhỏ (0.001) dựa trên độ lệch.
            // Node nào nằm đúng trên đường thẳng sẽ có crossProduct = 0 -> H nhỏ nhất -> Được ưu tiên chọn.
            return h + (crossProduct * 0.001f);
        }
    }
}