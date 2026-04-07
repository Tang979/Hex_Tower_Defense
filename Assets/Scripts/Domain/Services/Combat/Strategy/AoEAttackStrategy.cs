using System.Collections.Generic;
using Domain.Entities;
using Domain.Interface;

namespace Domain.Services.Combat.Strategy
{
    public class AoEAttackStrategy : IAttackStrategy
    {
        private List<IAttackModifier> _modifiers;

        public AoEAttackStrategy(List<IAttackModifier> modifiers = null)
        {
            _modifiers = modifiers ?? new List<IAttackModifier>();
        }

        public TowerAttackResult ExecuteAttack(Tower tower, Enemy mainTarget, List<Enemy> enemiesInRange)
        {
            if (enemiesInRange == null || enemiesInRange.Count == 0)
            {
                TowerAttackResult failedResult = AttackResultPool.GetPool();
                failedResult.IsSuccess = false;
                return failedResult;
            }

            TowerAttackResult result = AttackResultPool.GetPool();
            result.IsSuccess = true;

            foreach (var enemy in enemiesInRange)
            {
                result.AffectedEnemies.Add(enemy);
                result.DamageList.Add(tower.Damage);
            }

            if (_modifiers != null)
            {
                result.Modifiers.AddRange(_modifiers);
                
                foreach (var modifier in _modifiers)
                {
                    modifier.ExecuteOnFire(result, tower, null);
                }
            }

            return result;
        }
    }
}