using System.Collections.Generic;
using Domain.Entities;
using Domain.Services.Combat;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BurstLaserView : MonoBehaviour, IAttackVisual
{
    private LineRenderer _lineRenderer;
    private TowerAttackResult _result;
    private Tower _tower;
    private GameObject _prefab;

    private List<EnemyView> _targetViews;
    private Transform _firePoint;

    [Header("Burst Settings")]
    [SerializeField] private float burstDuration = 0.15f; // Thời gian tia chớp tồn tại (giây)

    public void Initialize(TowerAttackResult result, Tower tower, List<EnemyView> targetViews, Transform firePoint, GameObject originalPrefab)
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 0;
        _tower = tower;
        _firePoint = firePoint;
        _prefab = originalPrefab;

        UpdateTargetsAndDamage(result, targetViews);
        Invoke(nameof(TurnOff), burstDuration);
    }

    public void UpdateTargetsAndDamage(TowerAttackResult newResult, List<EnemyView> newTargetViews)
    {
        if (_result != null && _result != newResult)
        {
            _result.Clear();
            AttackResultPool.ReturnPool(_result);
        }

        _result = newResult;
        _targetViews = newTargetViews;

        if (_lineRenderer != null)
        {
            _lineRenderer.positionCount = _targetViews.Count + 1;
        }

        ApplyDamageAndEffects();
    }

    private void ApplyDamageAndEffects()
    {
        if (_result == null || _targetViews == null) return;
        for (int i = 0; i < _targetViews.Count; i++)
        {
            if (_targetViews[i] != null && _targetViews[i].Enemy != null && !_targetViews[i].Enemy.IsDead)
            {
                var enemy = _targetViews[i].Enemy;
                enemy.TakeDamage(_result.DamageList[i]);
                if (_result.Modifiers != null)
                {
                    foreach (var modifier in _result.Modifiers)
                        modifier.ExecuteOnHit(_result, _tower, _result.DamageList[i], enemy, GameController.Instance.EnemyService);
                }
            }
        }
    }

    private void Update()
    {
        if (_lineRenderer == null || _firePoint == null || _targetViews == null || _targetViews.Count == 0)
        {
            if (_lineRenderer != null) _lineRenderer.positionCount = 0;
            return;
        }

        _lineRenderer.SetPosition(0, _firePoint.position);
        int validPoints = 1;

        for (int i = 0; i < _targetViews.Count; i++)
        {
            if (_targetViews[i] != null && _targetViews[i].Enemy != null && !_targetViews[i].Enemy.IsDead)
            {
                if (_lineRenderer.positionCount < validPoints + 1)
                    _lineRenderer.positionCount = validPoints + 1;

                _lineRenderer.SetPosition(validPoints, _targetViews[i].transform.position);
                validPoints++;
            }
        }
        _lineRenderer.positionCount = validPoints;
    }

    public void TurnOff() 
    {
        GameController.Instance.PoolManager.ReturnPool(_prefab, gameObject);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(TurnOff));
        
        if (_lineRenderer != null) _lineRenderer.positionCount = 0;
        if (_result != null)
        {
            _result.Clear();
            AttackResultPool.ReturnPool(_result);
            _result = null;
        }
        _targetViews = null;
    }
}
