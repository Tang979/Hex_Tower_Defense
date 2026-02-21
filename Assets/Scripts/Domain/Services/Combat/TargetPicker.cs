using System.Collections.Generic;
using System.Linq;
using Domain.Entities;
using Domain.Enums;

namespace Domain.Services.Combat
{
    public class TargetPicker
    {
        public static Enemy Picker(TargetPriority priority, List<Enemy> enemies)
        {
            if (enemies == null || enemies.Count == 0)
                return null;
            
            if (enemies.Count == 1)
                return enemies[0];

            Enemy enemyPicked = enemies[0];
            for (int i = 0; i < enemies.Count; i++)
            {
                switch (priority)
                {
                    case TargetPriority.Strongest:
                        if (enemies[i].CurrentHealth > enemyPicked.CurrentHealth)
                            enemyPicked = enemies[i];
                        break;
                    case TargetPriority.Weakest:
                        if (enemies[i].CurrentHealth < enemyPicked.CurrentHealth)
                            enemyPicked = enemies[i];
                        break;
                    case TargetPriority.First:
                        if (enemies[i].GetDistanceToTarget() < enemyPicked.GetDistanceToTarget())
                            enemyPicked = enemies[i];
                        break;
                    case TargetPriority.Last:
                        if (enemies[i].GetDistanceToTarget() > enemyPicked.GetDistanceToTarget())
                            enemyPicked = enemies[i];
                        break;
                }
            }
            return enemyPicked;
        }
    }
}