using Domain.Enums;

[System.Serializable]
public struct EffectBlueprint
{
    public EnemyEffect effectType;
    public EffectScalingType scalingType;
    public float power;
    public float duration;
    public float tickInterval;
}