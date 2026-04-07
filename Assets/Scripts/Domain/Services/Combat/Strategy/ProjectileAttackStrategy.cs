using System.Collections.Generic;
using Domain.Entities;
using Domain.Interface;

namespace Domain.Services.Combat.Strategy
{
    public class ProjectileAttackStrategy : IAttackStrategy
    {
        private List<IAttackModifier> _modifiers;
        private readonly IActiveEnemyProvider _enemyProvider;

        public ProjectileAttackStrategy(List<IAttackModifier> modifiers = null, IActiveEnemyProvider enemyProvider = null)
        {
            _modifiers = modifiers ?? new List<IAttackModifier>();
            _enemyProvider = enemyProvider;
        }

        public TowerAttackResult ExecuteAttack(Tower tower, Enemy mainTarget, List<Enemy> enemiesInRange)
        {
            if (mainTarget == null)
                return null;

            TowerAttackResult result = AttackResultPool.GetPool();
            result.IsSuccess = true;

            result.AffectedEnemies.Add(mainTarget);
            result.DamageList.Add(tower.Damage);

            if (_modifiers != null)
            {
                result.Modifiers.AddRange(_modifiers);
                foreach (var modifier in _modifiers)
                {
                    modifier.ExecuteOnFire(result, tower, _enemyProvider);
                }
            }

            return result;
        }
    }
}