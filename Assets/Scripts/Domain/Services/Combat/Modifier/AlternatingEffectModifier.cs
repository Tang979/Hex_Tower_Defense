using System.Collections.Generic;
using Domain.Entities;
using Domain.Interface;
using Domain.ValueObject;
using Domain.Core.Data;

namespace Domain.Services.Combat.Modifier
{
    public class AlternatingEffectModifier : IAttackModifier
    {
        private int _shotIndex = 0;
        private List<string> _visuals = new List<string>();

        public void Initialize(ModifierConfig config)
        {
            if (config != null && config.StringArgs != null)
            {
                int idx = 0;
                while (config.StringArgs.ContainsKey("Visual" + idx))
                {
                    _visuals.Add(config.StringArgs["Visual" + idx]);
                    idx++;
                }
            }
        }

        public void ExecuteOnFire(TowerAttackResult currentResult, Tower tower, IActiveEnemyProvider enemyProvider)
        {
            int count = _visuals.Count;
            if (count > 0)
            {
                // Nếu có Effects, lấy độ dài ngắn nhất của Visuals và Effects để đi cặp, tránh lỗi lố
                int maxPairs = (tower.Effects != null && tower.Effects.Count > 0) ? UnityEngine.Mathf.Min(count, tower.Effects.Count) : count;
                
                int currentIndex = _shotIndex % maxPairs;
                currentResult.OverrideVisualId = _visuals[currentIndex];
                currentResult.AlternatingStateIndex = currentIndex;
            }
            
            _shotIndex++;
        }

        public void ExecuteOnHit(TowerAttackResult currentResult, Tower tower, float damage, Enemy targetHit, IActiveEnemyProvider enemyProvider)
        {
            if (tower.Effects == null || tower.Effects.Count == 0 || targetHit.IsDead) return;

            int effectIndex = currentResult.AlternatingStateIndex;
            if (effectIndex < 0 || effectIndex >= tower.Effects.Count) effectIndex = 0;
            
            var blueprint = tower.Effects[effectIndex];
            ActiveEffect activeEffect = new ActiveEffect(blueprint, tower.Id, tower.Damage);
                
            targetHit.AddEffect(activeEffect); 
        }
    }
}
