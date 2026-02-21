using System;
using System.Collections.Generic;
using Domain.Enums;
using Domain.Services.Combat;

namespace Domain.Entities
{
    public class Tower
    {
        public string Id { get; }
        public string Name { get; }
        public float Range { get; private set; }
        public float Damage { get; private set; }

        // [THAY ĐỔI 1] Đổi tên biến cho đúng nghĩa "Thời gian chờ"
        // Đơn vị: Giây (Seconds)
        public float AttackCooldown { get; private set; }
        private TargetPriority _targetPriority = TargetPriority.First;
        public Enemy EnemyTargeted { get; private set; }

        // Biến đếm ngược nội bộ
        private float _currentCooldownTimer;

        private IAttackStrategy _attackStrategy;
        public event Action<TowerAttackResult> OnAttack;

        public Tower(string id, string name, float range, float damage, float attackCooldown, TargetPriority targetPriority)
        {
            Id = id;
            Name = name;
            Range = range;
            Damage = damage;
            _targetPriority = targetPriority;
            AttackCooldown = attackCooldown;
            _currentCooldownTimer = 0f;
        }

        public void SetAttackStrategy(IAttackStrategy attackStrategy)
        {
            _attackStrategy = attackStrategy;
        }

        public void SetTargetPriority(TargetPriority targetPriority)
        {
            _targetPriority = targetPriority;
        }

        public void Tick(float deltaTime, List<Enemy> enemies)
        {
            if (_currentCooldownTimer > 0)
            {
                _currentCooldownTimer -= deltaTime;
            }

            if (_currentCooldownTimer <= 0)
            {
                if (_attackStrategy == null) return;

                

                var result = _attackStrategy.ExecuteAttack(this, enemies);

                if (result != null && result.IsSuccess)
                {
                    _currentCooldownTimer = AttackCooldown;
                    EnemyTargeted = result.Target;
                    OnAttack?.Invoke(result);
                }
            }
        }
    }
}