using System.Collections.Generic;
using Domain.Enums;

namespace Domain.Entities
{
    public class HexMap
    {
        public List<List<HexTile>> Grid { get; }
        public int Rows => Grid.Count;
        public int Cols => (Grid.Count > 0 && Grid[0] != null) ? Grid[0].Count : 0;

        public HexTile TargetTile { get; private set; }
        public List<HexTile> SpawnTiles { get; } = new List<HexTile>();

        public HexMap()
        {
            Grid = new List<List<HexTile>>();
        }

        public HexTile GetTile(int q, int r)
        {
            if (q < 0 || q >= Rows || r < 0 || r >= Cols) return null;
            return Grid[q][r];
        }

        public void RegisterSpecialTiles()
        {
            SpawnTiles.Clear();
            foreach (var row in Grid)
            {
                foreach (var tile in row)
                {
                    if (tile == null) continue;
                    if (tile.State == HexState.Target) TargetTile = tile;
                    if (tile.State == HexState.Spawn) SpawnTiles.Add(tile);
                }
            }
        }

        public IEnumerable<HexTile> GetNeighbors(HexTile tile)
        {
            // Offset coordinates neighbors logic
            var directions = (tile.Q & 1) == 1
                ? OddDirs
                : EvenDirs;

            foreach (var (dq, dr) in directions)
            {
                var neighbor = GetTile(tile.Q + dq, tile.R + dr);
                if (neighbor != null && neighbor.State != HexState.None)
                    yield return neighbor;
            }
        }

        public HexTile GetNeighbor(HexTile tile, int dir)
        {
            var dirs = (tile.Q & 1) == 1 ? OddDirs : EvenDirs;
            var (dq, dr) = dirs[dir];
            return GetTile(tile.Q + dq, tile.R + dr);
        }

        // private static readonly (int, int)[] EvenDirs = { (1, 0), (0, -1), (-1, -1), (-1, 0), (1, -1), (0, 1) };
        private static readonly (int, int)[] EvenDirs = { (-1, 0), (0, 1), (1, 0), (1, -1), (0, -1), (-1, -1) };
        // private static readonly (int, int)[] OddDirs = { (1, 0), (-1, 1), (0, -1), (-1, 0), (0, 1), (1, 1) };
        private static readonly (int, int)[] OddDirs = { (-1, 1), (0, 1), (1, 1), (1, 0), (0, -1), (-1, 0) };
    }
}