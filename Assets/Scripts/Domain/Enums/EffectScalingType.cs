namespace Domain.Enums
{
    public enum EffectScalingType
    {
        Flat,                           // Sát thương cố định (ví dụ: Power = 10 -> Trừ 10 máu)
        PercentageTowerDamage,          // % Sát thương của Tháp (Power = 0.5 -> Trừ 50% damage tháp)
        PercentageEnemyMaxHealth,       // % Máu tối đa của Quái (Power = 0.05 -> Trừ 5% Max HP)
        PercentageEnemyCurrentHealth,   // % Máu hiện tại của Quái
        PercentageEnemyMissingHealth    // % Máu đã mất của Quái
    }
}