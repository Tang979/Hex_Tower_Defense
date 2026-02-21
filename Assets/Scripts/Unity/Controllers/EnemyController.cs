using System.Collections.Generic;
using Domain.Entities;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private LayoutAdapter _layout;
    private List<HexTile> _path;
    private int _targetIndex;
    private Animator _animator;

    // Thay vì dùng LaneRuntime, giờ dùng dữ liệu trực tiếp
    public void Initialize(LayoutAdapter layout, List<HexTile> path)
    {
        _layout = layout;
        _animator = GetComponent<Animator>();
    }

    
}