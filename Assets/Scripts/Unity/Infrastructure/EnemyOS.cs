using UnityEngine;

[CreateAssetMenu(fileName = "EnemyOS", menuName = "Tower Defense/Enemy OS")]
public class EnemyOS : ScriptableObject
{
    [Header("Identity (Must match JSON Id)")]
    public string Id;

    [Header("Visuals")]
    public GameObject prefab;
    // ĐÃ XÓA MaxHealth và BaseSpeed
}