using System.Collections.Generic;
using Domain.Entities;
using Domain.Enums;
using Domain.Services.Combat;
using UnityEngine;

public class TowerView : MonoBehaviour
{
    public Tower Tower { get; private set; }
    public HexTile CurrentTile { get; private set; }

    private List<Enemy> _enemiesInRange = new List<Enemy>();
    private GameObject _bulletPrefab;
    public GameObject OriginalPrefab { get; private set; }

    [SerializeField] private GameObject towerModel;
    [SerializeField] private Transform firePoint;
    [SerializeField] private SphereCollider sphereCollider;

    [Header("Settings")]
    private float rotationSpeed = 500f;

    // Biến lưu trữ tia laser chiếu liên tục (nếu có)
    private IAttackVisual _persistentVisual;

    public void Initialize(Tower tower, GameObject bulletPrefab, HexTile tile, GameObject originalPrefab)
    {
        if (Tower != null)
        {
            Tower.OnAttack -= HandleAttack; // Hủy theo dõi tháp cũ
        }

        _enemiesInRange.Clear(); // Xóa sạch danh sách quái cũ đang kẹt trong nòng

        // Trả nòng súng về góc mặc định (không bị méo mó từ kiếp trước)
        if (towerModel != null)
        {
            towerModel.transform.localRotation = Quaternion.identity;
        }

        if (_persistentVisual != null)
        {
            (_persistentVisual as MonoBehaviour)?.gameObject.SetActive(false);
            _persistentVisual = null;
        }

        Tower = tower;
        Debug.Log($"[TowerView] Tháp {Tower.Id} khởi tạo thành công");
        CurrentTile = tile;
        sphereCollider.radius = Tower.Range;
        Tower.OnAttack += HandleAttack;
        _bulletPrefab = bulletPrefab;
        OriginalPrefab = originalPrefab;
    }

    private void Update()
    {
        if (Tower == null) return;

        _enemiesInRange.RemoveAll(e => e == null || e.IsDead || !GameController.Instance.EnemyRegistry.ContainsKey(e));
        Tower.Tick(Time.deltaTime, _enemiesInRange);

        if (Tower.AttackType == AttackType.Instant && Tower.CurrentTarget == null && _persistentVisual != null)
        {
            _persistentVisual.TurnOff();
            _persistentVisual = null;
        }

        bool isAimingDone = HandleSmoothRotation();
        if (Tower.CanAttack(_enemiesInRange) && isAimingDone)
        {
            Tower.PullTrigger(_enemiesInRange);
        }
    }

    private bool HandleSmoothRotation()
    {
        if (Tower.Trap || Tower.CurrentTarget == null || Tower.AttackType == AttackType.AoE) return true;

        if (GameController.Instance.EnemyRegistry.TryGetValue(Tower.CurrentTarget, out EnemyView targetView))
        {
            if (targetView != null)
            {
                Vector3 direction = targetView.transform.position - towerModel.transform.position;
                direction.y = 0;

                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);

                    towerModel.transform.rotation = Quaternion.RotateTowards(
                        towerModel.transform.rotation,
                        targetRotation,
                        rotationSpeed * Time.deltaTime
                    );

                    float angleToTarget = Quaternion.Angle(towerModel.transform.rotation, targetRotation);
                    return angleToTarget <= 5f;
                }
            }
        }

        return false;
    }

    private void HandleAttack(TowerAttackResult result)
    {
        if (!result.IsSuccess || result.AffectedEnemies == null || result.AffectedEnemies.Count == 0) return;

        List<EnemyView> targetViews = new List<EnemyView>();
        foreach (var enemy in result.AffectedEnemies)
        {
            if (GameController.Instance.EnemyRegistry.TryGetValue(enemy, out EnemyView view) && view != null)
                targetViews.Add(view);
        }

        GameObject bulletToUse = _bulletPrefab;
        if (!string.IsNullOrEmpty(result.OverrideVisualId))
        {
            GameObject overridePrefab = ResourceManager.GetVisual(result.OverrideVisualId);
            if (overridePrefab != null)
            {
                bulletToUse = overridePrefab;
            }
        }

        if (bulletToUse != null && targetViews.Count > 0)
        {
            if (Tower.AttackType == AttackType.Instant && _persistentVisual != null)
            {
                _persistentVisual.UpdateTargetsAndDamage(result, targetViews);
                return;
            }
            GameObject visualObj = GameController.Instance.PoolManager.GetObject(bulletToUse);
            visualObj.transform.position = firePoint.position;
            visualObj.SetActive(true);

            IAttackVisual attackVisual = visualObj.GetComponent<IAttackVisual>();
            if (attackVisual != null)
            {
                attackVisual.Initialize(result, Tower, targetViews, firePoint, bulletToUse);

                if (Tower.AttackType == AttackType.Instant)
                {
                    _persistentVisual = attackVisual;
                }
            }
        }
    }

    void OnDisable()
    {
        if (_persistentVisual != null)
        {
            _persistentVisual.TurnOff();
            _persistentVisual = null;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyView enemyView = other.GetComponent<EnemyView>();
            if (enemyView != null && enemyView.Enemy != null) _enemiesInRange.Add(enemyView.Enemy);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyView enemyView = other.GetComponent<EnemyView>();
            if (enemyView != null && enemyView.Enemy != null) _enemiesInRange.Remove(enemyView.Enemy);
        }
    }
}