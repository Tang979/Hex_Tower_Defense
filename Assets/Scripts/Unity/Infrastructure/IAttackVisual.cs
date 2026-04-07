using System.Collections.Generic;
using Domain.Entities;
using Domain.Services.Combat;
using UnityEngine;

public interface IAttackVisual
{
    void Initialize(TowerAttackResult result, Tower tower, List<EnemyView> targetViews, Transform firePoint, GameObject prefab);
    void UpdateTargetsAndDamage(TowerAttackResult result, List<EnemyView> targetViews);
    void TurnOff();
}