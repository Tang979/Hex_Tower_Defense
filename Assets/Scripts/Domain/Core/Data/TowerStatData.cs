using System.Collections.Generic;

namespace Domain.Core.Data
{
    [System.Serializable]
    public class TowerStatData
    {
        public string Id;
        public float Damage;
        public float Range;
        public float AttackCooldown;
    }

    [System.Serializable]
    public class TowerDatabase
    {
        public List<TowerStatData> Towers;
    }
}