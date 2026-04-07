using System.Collections.Generic;

namespace Domain.Services.Combat
{
    public static class AttackResultPool
    {
        private static readonly Queue<TowerAttackResult> _pool;

        static AttackResultPool()
        {
            _pool = new Queue<TowerAttackResult>();
            for (int i = 0; i < 10; i++) 
                _pool.Enqueue(new TowerAttackResult());
        }

        public static TowerAttackResult GetPool()
        {
            if (_pool.Count == 0)
                return new TowerAttackResult();
            return _pool.Dequeue();
        }

        public static void ReturnPool(TowerAttackResult result)
        {
            result.Clear();
            _pool.Enqueue(result);
        }

    }
}