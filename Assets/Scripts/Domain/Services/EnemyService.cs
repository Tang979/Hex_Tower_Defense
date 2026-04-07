using System.Collections.Generic;
using Domain.Entities;
using Domain.Enums;
using Domain.Interface;
using Domain.Services.Pathfinding;
using Domain.ValueObject;

namespace Domain.Services
{
    public class EnemyService : IActiveEnemyProvider
    {
        private HexMap _map;
        private LaneService _laneService;
        private IPathfinder _pathfinder;
        private BaseHealthService _baseHealthService;
        private CurrencyService _currencyService;
        public List<Enemy> ActiveEnemies { get; private set; }

        public EnemyService(HexMap map, LaneService laneService, BaseHealthService baseHealthService, CurrencyService currencyService)
        {
            _map = map;
            _laneService = laneService;
            _pathfinder = new AStarPathfinder();
            _baseHealthService = baseHealthService;
            _currencyService = currencyService;
            ActiveEnemies = new List<Enemy>();
        }

        public List<Enemy> GetActiveEnemies()
        {
            return ActiveEnemies;
        }

        public void SpawnEnemy(Enemy enemy, int laneId)
        {
            var lane = _laneService.GetPath(laneId);
            enemy.Spawn(laneId, lane);
            ActiveEnemies.Add(enemy);
        }

        public void EnemyDie(Enemy enemy)
        {
            if (enemy.CurrentTile.State == HexState.Target)
            {
                _baseHealthService.TakeDamage(1);
                int reward = enemy.Reward/2;
                _currencyService.AddCurrency(reward);
            }
            else
            {
                _currencyService.AddCurrency(enemy.Reward);
            }

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
                    enemy.AddEffect(new ActiveEffect("SystemKnockBack", EnemyEffect.Stun, EffectScalingType.Flat, 0f, 1.5f, 0f));
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