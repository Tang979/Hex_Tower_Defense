using System.Collections.Generic;

namespace Domain.Core.Data
{
    [System.Serializable]
    public class WaveData
    {
        public string WaveId;
        public const float DelayBeforeStart = 2f;
        public List<EnemyWaveData> SpawnList;
    }
}
