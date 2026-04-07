using System;
using System.Collections.Generic;
using Domain.Enums;
using Domain.ValueObject;

namespace Domain.Entities
{
    public class Enemy
    {
        public string Id { get; }

        public float MaxHealth { get; private set; }
        public float CurrentHealth { get; private set; }
        public float BaseSpeed { get; private set;}
        public float SpeedModifier { get; private set; } = 1f;
        public float CurrentSpeed => BaseSpeed * SpeedModifier;
        public int Reward { get; private set; }

        public Position Position { get; set; }
        public int CurrentLaneId { get; set; }
        public List<HexTile> CurrentPath { get; private set; }
        private int PathIndex = 0;
        public HexTile CurrentTile { get; private set; }
        public HexTile NextTile { get; private set; }
        public bool HeadInNextTile { get; set; } = false;

        public EnemyState CurrentState { get; private set; } // Chỉ 1 trạng thái tại 1 thời điểm
        public List<ActiveEffect> _activeEffects;
        private List<ActiveEffect> _deleteEffects;
        public event Action KnockBack;
        public event Action<float, float> OnHealthChanged;

        public bool IsDead => CurrentHealth <= 0;
        public bool IsStunned = false;

        public Enemy(string id, float health, float speed, int reward)
        {
            Id = id;
            MaxHealth = health;
            CurrentHealth = health;
            BaseSpeed = speed;
            Reward = reward;
            _activeEffects = new List<ActiveEffect>();
            _deleteEffects = new List<ActiveEffect>();

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

        public void SetHealth(float health)
        {
            CurrentHealth = health;
        }

        public void SetSpeed(float speed)
        {
            BaseSpeed = speed;
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
            CurrentTile = newPath[0];

            if (newPath.Count > 1)
            {
                NextTile = newPath[1];
            }
            else
            {
                NextTile = null;
            }

        }

        public void Tick(float deltaTime)
        {
            if (_activeEffects.Count > 0)
            {
                foreach (var effect in _activeEffects)
                {
                    bool shouldTriggerTick = effect.UpdateTick(deltaTime);

                    if (shouldTriggerTick)
                    {
                        effect.ApplyTickEffect(this);
                    }

                    if (effect.RemainingTime <= 0)
                        _deleteEffects.Add(effect);
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
                IsStunned |= effect.IsStunning(); 
                
                SpeedModifier -= effect.GetSpeedReduction();
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
            OnHealthChanged?.Invoke(CurrentHealth, MaxHealth);
        }

        public void ResetEnemy()
        {
            CurrentHealth = MaxHealth;
            HeadInNextTile = false;
            SpeedModifier = 1f;
            Reward = 0;
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