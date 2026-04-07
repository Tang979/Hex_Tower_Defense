using System;
using System.Collections.Generic;
using Domain.Entities;
using Domain.Interface;

namespace Domain.Services.Combat.Modifier
{
    public class SplashModifier : IAttackModifier
    {
        Random random;
        float _chance, _damageRatio, _radius;

        public SplashModifier()
        {
            random = new Random();
        }
        public void Initialize(Domain.Core.Data.ModifierConfig config)
        {
            var args = config?.Args ?? new Dictionary<string, float>();

            _chance = args.TryGetValue("chance", out float c) ? c : 0.3f;
            _radius = args.TryGetValue("radius", out float r) ? r : 1.5f;
            _damageRatio = args.TryGetValue("damageRatio", out float d) ? d : 0.5f;
        }
        public void ExecuteOnFire(TowerAttackResult currentResult, Tower tower, IActiveEnemyProvider enemyProvider) { }
        public void ExecuteOnHit(TowerAttackResult currentResult, Tower tower, float damage, Enemy targetHit, IActiveEnemyProvider enemyProvider)
        {
            if (!isSplashTriggered()) return;

            var allEnemies = enemyProvider.GetActiveEnemies();

            for (int i = allEnemies.Count - 1; i >= 0; i--)
            {
                var enemy = allEnemies[i];

                if (enemy == targetHit || enemy.IsDead) continue;

                if (enemy.Position.GetDistance(targetHit.Position) <= _radius)
                {
                    enemy.TakeDamage(damage * _damageRatio);
                }
            }
        }

        public bool isSplashTriggered()
        {
            return random.NextDouble() < _chance;
        }

    }
}