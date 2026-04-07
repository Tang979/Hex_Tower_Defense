using System;
using Domain.Entities;
using Domain.Enums;

namespace Domain.ValueObject
{
    public class ActiveEffect
    {
        public string SourceId { get; private set; }
        public EnemyEffect EffectType { get; private set; }
        public EffectScalingType ScalingType { get; private set; } // Thêm
        public float Power { get; private set; }
        public float SourceDamage { get; private set; } // Thêm: Ghi nhớ sát thương của tháp
        
        public float RemainingTime { get; private set; }
        public float TickInterval { get; private set; } 
        private float _currentTickTimer;

        // Cập nhật Constructor
        public ActiveEffect(string sourceId, EnemyEffect effect, EffectScalingType scalingType, float power, float duration, float tickInterval, float sourceDamage = 0f)
        {
            this.SourceId = sourceId;
            this.EffectType = effect;
            this.ScalingType = scalingType;
            this.Power = power;
            this.SourceDamage = sourceDamage; // Lưu sát thương tháp vào đây
            this.RemainingTime = duration;
            this.TickInterval = tickInterval;
            this._currentTickTimer = tickInterval; 
        }

        public ActiveEffect(EffectBlueprint blueprint, string sourceId, float sourceDamage = 0f)
        {
            this.SourceId = sourceId;
            this.EffectType = blueprint.effectType;
            this.ScalingType = blueprint.scalingType;
            this.Power = blueprint.power;
            this.SourceDamage = sourceDamage; // Lưu sát thương tháp vào đây
            this.RemainingTime = blueprint.duration;
            this.TickInterval = blueprint.tickInterval;
            this._currentTickTimer = blueprint.tickInterval; 
        }

        public ActiveEffect SystemKnockBackEffect(float duration)
        {
            return new ActiveEffect(
                new EffectBlueprint
                {
                    effectType = EnemyEffect.Stun,
                    scalingType = EffectScalingType.Flat,
                    power = 0f,
                    duration = duration,
                    tickInterval = 0f
                },
                SourceId,
                SourceDamage
            );
        }

        // ĐỔI TÊN & KIỂU TRẢ VỀ: Trả về TRUE nếu đã đến lúc hiệu ứng này phát huy tác dụng
        public bool UpdateTick(float deltaTime)
        {
            RemainingTime -= deltaTime;

            // Nếu hiệu ứng không có chu kỳ (như Stun/Slow), luôn trả về false vì nó là hiệu ứng tĩnh
            if (TickInterval <= 0) return false;

            _currentTickTimer -= deltaTime;
            if (_currentTickTimer <= 0)
            {
                _currentTickTimer += TickInterval; 
                return true; // Báo hiệu: "Đã đến lúc giật damage!"
            }

            return false;
        }

        public void SetRemainingTime(float time)
        {
            RemainingTime = time;
        }

        public void ApplyTickEffect(Enemy target)
        {
            switch (EffectType)
            {
                case EnemyEffect.DOT:
                    float damageToTake = CalculateEffectDamage(target);
                    target.TakeDamage(damageToTake);
                    break;
            }
        }

        public bool IsStunning()
        {
            return EffectType == EnemyEffect.Stun;
        }

        public float GetSpeedReduction()
        {
            if (EffectType == EnemyEffect.Slow)
            {
                return Power;
            }
            return 0f;
        }
        
        private float CalculateEffectDamage(Enemy enemy)
        {
            switch (ScalingType)
            {
                case EffectScalingType.Flat:
                    return Power; // Gây đúng số damage đã ghi

                case EffectScalingType.PercentageTowerDamage:
                    return SourceDamage * Power; // Lấy sát thương tháp nhân với %

                case EffectScalingType.PercentageEnemyMaxHealth:
                    return enemy.MaxHealth * Power;

                case EffectScalingType.PercentageEnemyCurrentHealth:
                    return enemy.CurrentHealth * Power;

                case EffectScalingType.PercentageEnemyMissingHealth:
                    return (enemy.MaxHealth - enemy.CurrentHealth) * Power;

                default:
                    return Power;
            }
        }
    }
}