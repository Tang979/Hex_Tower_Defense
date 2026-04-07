using System.Collections.Generic;
using Domain.Entities;
using Domain.Interface;
using Domain.ValueObject;

namespace Domain.Services.Combat.Modifier
{
    public class StatusEffectModifier : IAttackModifier
    {
        public void Initialize(Domain.Core.Data.ModifierConfig config)
        {
            // Hiện tại hiệu ứng lấy 100% từ Tower.Effects nên không cần tham số JSON.
            // (Sau này bạn có thể thêm args "effectChance" nếu muốn đánh 30% dính độc)
        }

        public void ExecuteOnFire(TowerAttackResult currentResult, Tower tower, IActiveEnemyProvider enemyProvider)
        {
            
        }

        public void ExecuteOnHit(TowerAttackResult currentResult, Tower tower, float damage, Enemy targetHit, IActiveEnemyProvider enemyProvider)
        {
            if (tower.Effects == null || tower.Effects.Count == 0 || targetHit.IsDead) return;

            foreach (var blueprint in tower.Effects)
            {
                ActiveEffect activeEffect = new ActiveEffect(blueprint, tower.Id, tower.Damage);
                
                targetHit.AddEffect(activeEffect); 
            }
        }
    }
}