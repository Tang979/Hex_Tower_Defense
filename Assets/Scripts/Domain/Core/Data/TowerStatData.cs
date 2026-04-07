using System.Collections.Generic;
using Domain.Enums;
using Domain.Interface;

namespace Domain.Core.Data
{
    [System.Serializable]
    public class TowerStatData
    {
        public string Id;
        public float Damage;
        public float Range;
        public float AttackCooldown;
        public int Cost;
        public AttackType AttackType;
        public List<ModifierConfig> Modifiers;
        public TargetPriority TargetPriority;
        public bool IsTrap;
        public List<EffectBlueprint> Effects;
    }

    [System.Serializable]
    public class TowerDatabase
    {
        public List<TowerStatData> Towers;
    }
    [System.Serializable]
    public class ModifierConfig
    {
        public string ModifierName; 
        public Dictionary<string, float> Args;
        public Dictionary<string, string> StringArgs;
    }
}