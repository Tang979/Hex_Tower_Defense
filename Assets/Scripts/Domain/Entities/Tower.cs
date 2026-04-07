using System;
using System.Collections.Generic;
using Domain.Enums;
using Domain.Interface;
using Domain.Services.Combat;

namespace Domain.Entities
{
    public class Tower
    {
        public string Id { get; }
        public string Name { get; }
        public float Range { get; private set; }
        public float Damage { get; private set; }
        public Enemy CurrentTarget { get; private set; }
        
        public bool Trap { get; private set;}
        public AttackType AttackType { get; private set; }
        public float AttackCooldown { get; private set; }
        public TargetPriority TargetPriority { get; private set; }
        public List<EffectBlueprint> Effects { get; private set; }


        // Biến đếm ngược nội bộ
        private float _currentCooldownTimer;

        private IAttackStrategy _attackStrategy;
        public event Action<TowerAttackResult> OnAttack;

        public Tower(string id, string name, float range, float damage, float attackCooldown, AttackType attackType, TargetPriority targetPriority, List<EffectBlueprint> effects, bool isTrap = false)
        {
            Id = id;
            Name = name;
            Range = range;
            Damage = damage;
            Trap = isTrap;
            AttackType = attackType;
            TargetPriority = targetPriority;
            AttackCooldown = attackCooldown;
            Effects = effects;
            _currentCooldownTimer = 0f;
        }

        public void SetAttackStrategy(IAttackStrategy attackStrategy)
        {
            _attackStrategy = attackStrategy;
        }

        public void SetTargetPriority(TargetPriority targetPriority)
        {
            TargetPriority = targetPriority;
        }

        // THÊM HÀM KIỂM TRA ĐIỀU KIỆN: Đạn đã nạp xong và có mục tiêu chưa?
        public bool CanAttack(List<Enemy> enemies)
        {
            return _currentCooldownTimer <= 0 && (CurrentTarget != null || (AttackType == AttackType.AoE && enemies.Count > 0));
        }

        public void Tick(float deltaTime, List<Enemy> enemies)
        {
            if (CurrentTarget != null && (CurrentTarget.IsDead || !enemies.Contains(CurrentTarget)))
            {
                CurrentTarget = null;
            }

            if (CurrentTarget == null && enemies.Count > 0)
            {
                CurrentTarget = TargetPicker.Picker(TargetPriority, enemies);
            }

            // 2. LOGIC CHỜ NẠP ĐẠN (COOLDOWN)
            if (_currentCooldownTimer > 0)
            {
                _currentCooldownTimer -= deltaTime;
            }
            
            // XÓA ĐOẠN TỰ ĐỘNG BẮN Ở ĐÂY ĐI. Việc bắn giờ do View quyết định.
        }

        // 3. THÊM HÀM BÓP CÒ (Do TowerView gọi khi đã ngắm xong)
        public void PullTrigger(List<Enemy> enemies)
        {
            if (!CanAttack(enemies) || _attackStrategy == null) return;

            var result = _attackStrategy.ExecuteAttack(this, CurrentTarget, enemies);

            if (result != null && result.IsSuccess)
            {
                _currentCooldownTimer = AttackCooldown; // Reset lại thời gian nạp đạn
                OnAttack?.Invoke(result); // Kích hoạt sự kiện để View đẻ ra viên đạn
            }
        }
    }
}