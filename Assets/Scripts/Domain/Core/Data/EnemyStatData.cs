using System.Collections.Generic;

namespace Domain.Core.Data
{
    [System.Serializable]
    public class EnemyStatData
    {
        public string Id;
        public float MaxHealth;
        public float BaseSpeed;
        public int Reward;
    }

    [System.Serializable]
    public class EnemyDatabase
    {
        public List<EnemyStatData> Enemies;
    }
}