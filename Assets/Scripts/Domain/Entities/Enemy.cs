using System;
using System.Collections.Generic;
using Domain.Enums;
using Domain.ValueObject;

namespace Domain.Entities
{
    public class Enemy
    {
        // 1. Định danh
        public string Id { get; }

        // 2. Chỉ số
        public float MaxHealth { get; private set; }
        public float CurrentHealth { get; private set; }
        public float BaseSpeed { get; }
        public float SpeedModifier { get; private set; } = 1f;
        public float CurrentSpeed => BaseSpeed * SpeedModifier; // Tốc độ thực tế

        // 3. Vị trí & Map Logic (Quan trọng cho Hybrid 1.5)
        public int CurrentLaneId { get; set; }
        public List<HexTile> CurrentPath { get; private set; }
        private int PathIndex = 0;
        public HexTile CurrentTile { get; private set; }
        public HexTile NextTile { get; private set; }
        public bool HeadInNextTile { get; set; } = false;

        // 4. Trạng thái & Hiệu ứng
        public EnemyState CurrentState { get; private set; } // Chỉ 1 trạng thái tại 1 thời điểm
        private List<ActiveEffect> _activeEffects;
        private List<ActiveEffect> _deleteEffects;
        public event Action KnockBack;

        public bool IsDead => CurrentHealth <= 0;
        public bool IsStunned = false;

        public Enemy(string id, float health, float speed)
        {
            Id = id;
            MaxHealth = health;
            BaseSpeed = speed;
            _activeEffects = new List<ActiveEffect>();
            _deleteEffects = new List<ActiveEffect>();

            // Chưa active ngay, đợi Reset/Spawn mới active
            CurrentState = EnemyState.Spawning;
        }

        public Enemy()
        {
            Id = "Enemy_01";
            MaxHealth = 100f;
            BaseSpeed = 1f;
            _activeEffects = new List<ActiveEffect>();
            _deleteEffects = new List<ActiveEffect>();

            // Chưa active ngay, đợi Reset/Spawn mới active
            CurrentState = EnemyState.Spawning;
        }

        public void Spawn(int laneId, List<HexTile> path)
        {
            CurrentLaneId = laneId;
            CurrentPath = path;
            CurrentTile = path[0];
            CurrentTile.EnemyCount++;
            NextTile = path[1];
            CurrentState = EnemyState.Moving;
        }

        public void MoveNexTile()
        {
            if (PathIndex + 1 < CurrentPath.Count)
            {
                CurrentTile.EnemyCount--;
                PathIndex++;
                CurrentTile = CurrentPath[PathIndex];
                CurrentTile.EnemyCount++;
                NextTile = (PathIndex + 1 < CurrentPath.Count) ? CurrentPath[PathIndex + 1] : null;
            }
        }

        public void TriggerKnockBack()
        {
            KnockBack?.Invoke();
        }

        public void UpdatePath(List<HexTile> newPath)
        {
            CurrentPath = newPath;
            PathIndex = 0;
            // Luôn an toàn khi lấy phần tử đầu tiên (vì path != null)
            CurrentTile = newPath[0];

            // Kiểm tra an toàn trước khi lấy phần tử thứ 2
            if (newPath.Count > 1)
            {
                NextTile = newPath[1];
            }
            else
            {
                // Nếu path chỉ có 1 điểm (đang đứng tại đích), NextTile là null hoặc chính là đích
                NextTile = null;
            }

        }

        public void Tick(float deltaTime)
        {

            if (_activeEffects.Count > 0)
            {
                foreach (var effect in _activeEffects)
                {
                    effect.Update(deltaTime);
                    if (effect.RemainingTime <= 0)
                    {
                        // Xoá hiệu ứng hết hạn
                        _deleteEffects.Add(effect);
                    }
                }
            }

            RemoveEffect();
        }

        public void AddEffect(ActiveEffect effect)
        {
            foreach (var existingEffect in _activeEffects)
            {
                if (existingEffect.SourceId == effect.SourceId && existingEffect.EffectType == effect.EffectType)
                {
                    // Cập nhật lại thời gian và sức mạnh nếu đã tồn tại hiệu ứng cùng loại từ cùng nguồn
                    existingEffect.SetRemainingTime(effect.RemainingTime);
                    return;
                }
            }
            _activeEffects.Add(effect);
            RecalculateStats();
        }

        public void RemoveEffect()
        {
            if (_deleteEffects.Count > 0)
            {
                foreach (var effect in _deleteEffects)
                {
                    _activeEffects.Remove(effect);
                }
                RecalculateStats();
                _deleteEffects.Clear();
            }
        }

        private void RecalculateStats()
        {
            SpeedModifier = 1f;
            IsStunned = false;

            foreach (var effect in _activeEffects)
            {
                if (effect.EffectType == EnemyEffect.Stun)
                    IsStunned = true;
                if (effect.EffectType == EnemyEffect.Slow)
                    SpeedModifier -= effect.Power;
            }

            if (IsStunned)
                SpeedModifier = 0f;
            else
            {
                if (SpeedModifier < 0.1f)
                    SpeedModifier = 0.1f;
            }
        }

        public void TakeDamage(float amount)
        {
            CurrentHealth -= amount;
        }

        public void ResetEnemy()
        {
            CurrentHealth = MaxHealth;
            HeadInNextTile = false;
            SpeedModifier = 1f;
            CurrentState = EnemyState.Spawning;
            CurrentPath = null;
            PathIndex = 0;
            _activeEffects.Clear();
            _deleteEffects.Clear();
        }

        public void Die()
        {
            CurrentTile.EnemyCount--;
            ResetEnemy();
        }

        public int GetDistanceToTarget()
        {
            return CurrentPath.Count - PathIndex;
        }
    }
}