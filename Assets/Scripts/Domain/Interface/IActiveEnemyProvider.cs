using System.Collections.Generic;
using Domain.Entities;

namespace Domain.Interface
{
    public interface IActiveEnemyProvider
    {
        List<Enemy> GetActiveEnemies();
    }
}