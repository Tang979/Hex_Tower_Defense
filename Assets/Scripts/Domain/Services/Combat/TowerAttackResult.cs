using System.Collections.Generic;
using Domain.Entities;

namespace Domain.Services.Combat
{
    public class TowerAttackResult
    {
        public bool IsSuccess;
        public Enemy Target;          // Mục tiêu chính (để vẽ đạn bay tới)
        public List<Enemy> AffectedEnemies; // Danh sách bị trúng đòn (cho AoE/Chain)
        public string ProjectileId;   // Loại đạn (Lửa/Băng...)
    }
}