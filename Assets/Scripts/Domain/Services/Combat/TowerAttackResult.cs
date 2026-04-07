using System.Collections.Generic;
using Domain.Entities;
using Domain.Interface;
using Domain.ValueObject;

namespace Domain.Services.Combat
{
    public class TowerAttackResult
    {
        public bool IsSuccess;
        public string OverrideVisualId;
        public int AlternatingStateIndex;
        public List<ActiveEffect> AppliedEffects; // Hiệu ứng đã áp dụng lên mục tiêu (cho hiển thị hiệu ứng)
        public List<float> DamageList;
        public List<Enemy> AffectedEnemies; // Danh sách bị trúng đòn (cho AoE/Chain)
        public List<IAttackModifier> Modifiers;

        public TowerAttackResult()
        {
            IsSuccess = false;
            OverrideVisualId = null;
            AppliedEffects = new List<ActiveEffect>();
            DamageList = new List<float>();
            AffectedEnemies = new List<Enemy>();
            Modifiers = new List<IAttackModifier>();
        }

        public void Clear()
        {
            IsSuccess = false;
            OverrideVisualId = null;
            AlternatingStateIndex = 0;
            AppliedEffects.Clear();
            if (AffectedEnemies.Capacity > 10)
            {
                AffectedEnemies.Capacity = 10;
            }
            AffectedEnemies.Clear();
            DamageList.Clear();
            Modifiers.Clear();
        }
    }
}