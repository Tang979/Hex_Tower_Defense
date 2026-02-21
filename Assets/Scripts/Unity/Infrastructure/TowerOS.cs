using Domain.Enums;
using UnityEngine;

[CreateAssetMenu(fileName = "TowerOS", menuName = "Tower Defense/Tower OS")]
public class TowerOS : ScriptableObject
{
    [Header("Identity (Must match JSON Id)")]
    public string Id;
    public string towerName;

    [Header("Visuals")]
    public GameObject towerPrefab; // Tháp thật (có TowerView)
    public GameObject ghostPrefab; // Tháp mờ (dùng khi kéo thả)
    public Sprite towerIcon;

    [Header("Combat Strategy")]
    public AttackType attackType;
    public GameObject projectilePrefab; // Nếu dùng chiến thuật bắn đạn, gán prefab đạn vào đây
    public TargetPriority targetPriority;
}