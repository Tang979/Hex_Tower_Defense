using System.Collections.Generic;
using Domain.Entities;

namespace Domain.Services.Pathfinding
{
    public interface IPathfinder
    {
        List<HexTile> FindPath(HexMap map, HexTile start, HexTile end);
    }
}