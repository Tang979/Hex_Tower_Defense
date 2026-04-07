using System.Collections.Generic;
using Domain.Entities;
using Domain.Services.Combat;

namespace Domain.Interface
{
    public interface IAttackStrategy
    {
        // Trả về null nếu không bắn được (không có mục tiêu), trả về object nếu bắn thành công
        TowerAttackResult ExecuteAttack(Tower tower, Enemy target, List<Enemy> enemies);
    }
}