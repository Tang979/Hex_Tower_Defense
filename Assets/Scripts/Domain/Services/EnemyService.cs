using System.Collections.Generic;
using Domain.Entities;
using Domain.Enums;
using Domain.Services.Pathfinding;
using Domain.ValueObject;

namespace Domain.Services
{
    public class EnemyService
    {
        private HexMap _map;
        private LaneService _laneService;
        private IPathfinder _pathfinder;
        public List<Enemy> ActiveEnemies { get; private set; }

        public EnemyService(HexMap map, LaneService laneService)
        {
            _map = map;
            _laneService = laneService;
            _pathfinder = new AStarPathfinder();
            ActiveEnemies = new List<Enemy>();
        }

        public void SpawnEnemy(Enemy enemy, int laneId)
        {
            var lane = _laneService.GetPath(laneId);
            enemy.Spawn(laneId, lane);
            ActiveEnemies.Add(enemy);
        }

        public void EnemyDie(Enemy enemy)
        {
            enemy.Die();
            ActiveEnemies.Remove(enemy);
        }

        public void UpdateEnemies(float deltaTime)
        {
            foreach (var enemy in ActiveEnemies)
            {
                enemy.Tick(deltaTime);
            }
        }

        public void HandleOnMapChanged(HexTile blockedTile)
        {
            foreach (var enemy in ActiveEnemies)
            {
                if (enemy.NextTile == blockedTile)
                {
                    enemy.TriggerKnockBack();
                    enemy.AddEffect(new ActiveEffect("StunKnockBack", EnemyEffect.Stun, 0f, 1.5f));
                    var newPath = _pathfinder.FindPath(_map, enemy.CurrentTile, _map.TargetTile);
                    if (newPath != null)
                    {
                        enemy.UpdatePath(newPath);
                    }
                    continue;
                }
                if (enemy.CurrentPath.Contains(blockedTile))
                {
                    var newPath = _pathfinder.FindPath(_map, enemy.CurrentTile, _map.TargetTile);
                    if (newPath != null)
                    {
                        enemy.UpdatePath(newPath);
                    }
                }
            }
        }
    }
}