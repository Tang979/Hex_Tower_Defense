using System.Collections.Generic;

namespace Domain.Core.Data
{
    [System.Serializable]
    public class LaneData
    {
        public int LaneId;
        public List<WaveData> Waves;
    }
}
