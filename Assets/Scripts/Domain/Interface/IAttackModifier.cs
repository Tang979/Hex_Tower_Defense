using System.Collections.Generic;
using Domain.Entities;
using Domain.Services.Combat;

namespace Domain.Interface
{
    public interface IAttackModifier
    {
        void Initialize(Domain.Core.Data.ModifierConfig config);
        
        void ExecuteOnHit(TowerAttackResult currentResult, Tower tower, float damage, Enemy targetHit, IActiveEnemyProvider enemyProvider);
        void ExecuteOnFire(TowerAttackResult currentResult, Tower tower, IActiveEnemyProvider enemyProvider);
    }
}