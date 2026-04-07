using System.Collections.Generic;
using Domain.Entities;
using Domain.Services.Combat;
using UnityEngine;

public class BouncingProjectileView : MonoBehaviour, IAttackVisual
{
    private TowerAttackResult _result;
    private Tower _tower;
    private List<EnemyView> _targets;
    private int _currentIndex = 0;
    private float _speed = 10f;
    private GameObject _prefab;

    public void Initialize(TowerAttackResult result, Tower tower, List<EnemyView> targetViews, Transform firePoint, GameObject prefab)
    {
        _result = result;
        _tower = tower;
        _targets = targetViews;
        _prefab = prefab;
        _currentIndex = 0;
    }

    private void Update()
    {
        if (_targets == null || _currentIndex >= _targets.Count)
        {
            GameController.Instance.PoolManager.ReturnPool(_prefab, this.gameObject);
            return;
        }

        EnemyView currentTarget = _targets[_currentIndex];

        if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy || currentTarget.Enemy == null || currentTarget.Enemy.IsDead)
        {
            _currentIndex++;
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, currentTarget.transform.position, _speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, currentTarget.transform.position) < 0.01f)
        {
            HitCurrentTarget(currentTarget);
            _currentIndex++;
        }
    }

    void OnDisable()
    {
        if(_result != null)
        {
            AttackResultPool.ReturnPool(_result);
            _result = null;
        }
    }

    private void HitCurrentTarget(EnemyView targetView)
    {
        float damage = _result.DamageList[_currentIndex];
        targetView.Enemy.TakeDamage(damage);

        if (_result.Modifiers.Count > 0)
        {
            foreach (var modifier in _result.Modifiers)
            {
                modifier.ExecuteOnHit(_result, _tower, damage, targetView.Enemy, GameController.Instance.EnemyService);
            }
        }

        if (_result.AppliedEffects != null)
        {
            foreach (var effect in _result.AppliedEffects) 
            {
                targetView.Enemy.AddEffect(effect);
                
            }
        }
    }

    public void UpdateTargetsAndDamage(TowerAttackResult result, List<EnemyView> targetViews)
    {
    }

    public void TurnOff()
    {
    }
}