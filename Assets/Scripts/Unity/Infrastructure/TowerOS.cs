using System.Collections.Generic;
using Domain.Enums;
using Domain.ValueObject;
using UnityEngine;

[CreateAssetMenu(fileName = "TowerOS", menuName = "Tower Defense/Tower OS")]
public class TowerOS : ScriptableObject
{
    [Header("Identity (Must match JSON Id)")]
    public string Id;
    public string towerName;

    [Header("Visuals")]
    public GameObject towerPrefab;
    public GameObject ghostPrefab;
    public Sprite towerIcon;
    public GameObject bulletPrefab;
}