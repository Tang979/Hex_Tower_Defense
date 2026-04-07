using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Domain.Entities;
using Domain.Services.Combat;

public class AoEAttackView : MonoBehaviour, IAttackVisual
{
    private TowerAttackResult _result;
    private Tower _tower;
    private GameObject _prefab;
    
    [SerializeField] private float _damageDelay = 0.2f;
    [SerializeField] private float _visualDuration = 1.0f;

    // Hàm này được TowerView gọi khi Tháp bắn

    public void Initialize(TowerAttackResult result, Tower tower, List<EnemyView> targetViews, Transform firePoint, GameObject prefab)
    {
        _result = result;
        _tower = tower;
        _prefab = prefab;

        transform.position = firePoint.position;

        // Bắt đầu quy trình nổ
        StartCoroutine(ExplodeRoutine());
    }

    public void TurnOff()
    {
    }

    public void UpdateTargetsAndDamage(TowerAttackResult result, List<EnemyView> targetViews)
    {
    }

    private IEnumerator ExplodeRoutine()
    {
        // 1. Chờ Delay sát thương (nếu có)
        if (_damageDelay > 0f)
        {
            yield return new WaitForSeconds(_damageDelay);
        }

        // 2. KHUI CỐC VÀ TRỪ MÁU
        if (_result != null && _result.IsSuccess)
        {
            for (int i = 0; i < _result.AffectedEnemies.Count; i++)
            {
                var enemy = _result.AffectedEnemies[i];
                if (enemy == null || enemy.IsDead) continue; 

                // Trừ sát thương gốc
                enemy.TakeDamage(_result.DamageList[i]);

                // KÍCH HOẠT MODIFIER
                if (_result.Modifiers != null)
                {
                    foreach (var modifier in _result.Modifiers)
                    {
                        modifier.ExecuteOnHit(_result, _tower, _tower.Damage, enemy, GameController.Instance.EnemyService);
                    }
                }
            }
        }

        yield return new WaitForSeconds(_visualDuration);

        GameController.Instance.PoolManager.ReturnPool(_prefab, this.gameObject);
    }

    private void OnDisable()
    {
        // An toàn dự phòng: lỡ object bị tắt đột ngột thì vẫn phải trả Cốc
        if (_result != null)
        {
            _result.Clear();
            AttackResultPool.ReturnPool(_result);
            _result = null;
        }
    }
}