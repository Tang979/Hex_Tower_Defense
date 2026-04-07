using System.Collections.Generic;
using Domain.Entities;
using Domain.Interface;

namespace Domain.Services.Combat.Modifier
{
    public class ChainModifier : IAttackModifier
    {
        private int _maxBounces;
        private float _bounceRadius;
        private float _damageMultiplier;

        public void Initialize(Domain.Core.Data.ModifierConfig config)
        {
            var args = config?.Args ?? new Dictionary<string, float>();
            _maxBounces = (int)(args.TryGetValue("maxBounces", out float mb) ? mb : 2);
            _bounceRadius = args.TryGetValue("bounceRadius", out float br) ? br : 3f;
            _damageMultiplier = args.TryGetValue("damageMultiplier", out float dm) ? dm : 0.7f;
        }

        public void ExecuteOnFire(TowerAttackResult currentResult, Tower tower, IActiveEnemyProvider enemyProvider)
        {
            if (currentResult.AffectedEnemies.Count == 0 || enemyProvider == null) return;

            Enemy currentTarget = currentResult.AffectedEnemies[0];
            float currentDamage = currentResult.DamageList[0];
            
            List<Enemy> chainedTargets = currentResult.AffectedEnemies; 
            List<Enemy> allEnemies = enemyProvider.GetActiveEnemies();

            for (int i = 0; i < _maxBounces; i++)
            {
                Enemy nextTarget = null;
                float minDistance = float.MaxValue;

                foreach (var enemy in allEnemies)
                {
                    if (enemy.IsDead || chainedTargets.Contains(enemy)) continue;

                    float distance = currentTarget.Position.GetDistance(enemy.Position);
                    if (distance < minDistance && distance <= _bounceRadius)
                    {
                        minDistance = distance;
                        nextTarget = enemy;
                    }
                }

                if (nextTarget != null)
                {
                    currentDamage *= _damageMultiplier;
                    
                    currentResult.AffectedEnemies.Add(nextTarget);
                    currentResult.DamageList.Add(currentDamage);
                    
                    currentTarget = nextTarget;
                }
                else break;
            }
        }

        public void ExecuteOnHit(TowerAttackResult currentResult, Tower tower, float damage, Enemy targetHit, IActiveEnemyProvider enemyProvider)
        {
        }
    }
}