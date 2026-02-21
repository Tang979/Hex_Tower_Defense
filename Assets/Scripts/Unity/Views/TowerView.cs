using System.Collections.Generic;
using Domain.Entities;
using UnityEngine;

public class TowerView : MonoBehaviour
{
    private Tower _tower;

    [SerializeField] private SphereCollider collider;

    public void Initialize(Tower tower)
    {
        _tower = tower;
        collider.radius = _tower.Range;
        // Đăng ký các event (OnAttack) từ _tower tại đây
    }

    private void Update()
    {
        if (_tower == null) return;
        
        // Gọi _tower.Tick(Time.deltaTime, listEnemies) ở đây hoặc từ 1 TowerService tổng.
    }

}