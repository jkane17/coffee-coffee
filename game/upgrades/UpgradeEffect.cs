/// <summary>What an upgrade changes. The name says what its <see cref="Upgrade.Amount"/> means.
/// Owning several upgrades with the same effect adds their amounts together.</summary>
public enum UpgradeEffect
{
    /// <summary>Nothing beyond how the shop looks (see <see cref="UpgradeVisibility"/>).</summary>
    None,
    WalkSpeedPercent,
    PatiencePercent,
    FillSpeedPercent,
    BoilSpeedPercent,
    ExtraKettleCups,
}
