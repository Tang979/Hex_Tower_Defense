using System.Collections.Generic;
using Domain.Entities;

namespace Domain.Services.Combat // Hoặc Domain.Strategies
{
    public interface IAttackStrategy
    {
        // Trả về null nếu không bắn được (không có mục tiêu), trả về object nếu bắn thành công
        TowerAttackResult ExecuteAttack(Tower tower, List<Enemy> enemies);
    }
}