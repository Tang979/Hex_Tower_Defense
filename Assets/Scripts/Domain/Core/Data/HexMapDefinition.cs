using System.Collections.Generic;

namespace Domain.Core.Data
{
    [System.Serializable]
    public class HexMapDefinition
    {
        public string Name;
        public List<List<int>> Cells;
        public List<LaneData> Lanes;
    }
}