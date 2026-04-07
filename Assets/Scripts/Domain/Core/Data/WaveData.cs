using System.Collections.Generic;

namespace Domain.Core.Data
{
    [System.Serializable]
    public class WaveData
    {
        public const float DelayBeforeStart = 5f;
        public List<EnemyWaveData> SpawnList;
    }
}
