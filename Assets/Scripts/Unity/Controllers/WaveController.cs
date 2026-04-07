using System;
using System.Collections;
using System.Collections.Generic;
using Domain.Core.Data;
using Domain.Entities;
using UnityEngine;

public class WaveController : MonoBehaviour
{
    private Dictionary<string, EnemyStatData> _enemyStatsDict;
    [SerializeField] private LayoutAdapter layoutAdapter;

    public event Action OnAllWavesSpawned;
    public event Action<int, int> OnWaveChanged;

    private int _totalWaves = 0;
    private int _currentGlobalWave = 0;
    private int _activeLanes = 0;
    private float _multiplierEnemy = 1f;

    public void Initialize(List<LaneData> laneData, Dictionary<string, EnemyStatData> enemyStatsDict)
    {
        _enemyStatsDict = enemyStatsDict;

        if (laneData != null)
        {
            foreach (var lane in laneData)
            {
                if (lane.Waves.Count > _totalWaves)
                    _totalWaves = lane.Waves.Count;
            }
            _activeLanes = laneData.Count;

            foreach (var lane in laneData)
            {
                StartCoroutine(SpawnLaneRoutine(lane, layoutAdapter));
            }
        }
    }

    private IEnumerator SpawnLaneRoutine(LaneData laneData, LayoutAdapter layoutAdapter)
    {
        if (laneData == null)
        {
            CheckLaneComplete();
            yield break;
        }

        int _currentWave = 1;
        foreach (var wave in laneData.Waves)
        {
            yield return new WaitUntil(() => GameController.Instance.EnemyService.ActiveEnemies.Count == 0);

            if (_currentGlobalWave < _currentWave)
            {
                _currentGlobalWave = _currentWave;
                OnWaveChanged?.Invoke(_currentGlobalWave, _totalWaves);
            }

            yield return new WaitForSeconds(WaveData.DelayBeforeStart);

            if (wave.SpawnList != null)
            {
                foreach (var enemy in wave.SpawnList)
                {
                    for (int i = 0; i < enemy.Count; i++)
                    {
                        SpawnEnemy(enemy.EnemyId, _currentGlobalWave, laneData.LaneId, layoutAdapter);
                        yield return new WaitForSeconds(enemy.Interval);
                    }
                }
            }

            _multiplierEnemy += 0.2f;
            _currentWave++;
        }

        // Thay vì gọi Invoke trực tiếp, ta chuyển qua hàm kiểm tra
        CheckLaneComplete();
    }

    // THÊM: Hàm kiểm tra đồng bộ
    private void CheckLaneComplete()
    {
        _activeLanes--;
        if (_activeLanes <= 0)
        {
            OnAllWavesSpawned?.Invoke();
        }
    }

    private void SpawnEnemy(string enemyId, int wave, int laneId, LayoutAdapter layoutAdapter)
    {
        EnemyOS enemyOS = ResourceManager.GetEnemyOS(enemyId);
        if (enemyOS == null) return;

        Enemy domainEnemy = new Enemy(enemyId, _enemyStatsDict[enemyId].MaxHealth, _enemyStatsDict[enemyId].BaseSpeed, _enemyStatsDict[enemyId].Reward);
        domainEnemy.SetHealth(domainEnemy.MaxHealth * (_multiplierEnemy + wave * 0.2f));
        domainEnemy.SetSpeed(domainEnemy.BaseSpeed * (_multiplierEnemy + wave * 0.05f));
        GameController.Instance.EnemyService.SpawnEnemy(domainEnemy, laneId);

        GameObject enemyObj = GameController.Instance.PoolManager.GetObject(enemyOS.prefab);
        EnemyView view = enemyObj.GetComponent<EnemyView>();

        if (view != null)
        {
            view.Initialize(domainEnemy, layoutAdapter, enemyOS.prefab);
            enemyObj.SetActive(true);
            GameController.Instance.EnemyRegistry.Add(domainEnemy, view);
        }
    }
}