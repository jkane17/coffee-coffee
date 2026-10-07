/// <summary>Everything that persists between play sessions. Bump <see cref="CurrentVersion"/> when older saves can no longer be read;
/// a new field with a default (like <see cref="OwnedUpgrades"/>) doesn't need it, because saves without it just get the default.</summary>
public sealed record SaveData(int Version, int Money, int DaysCompleted, string[]? OwnedUpgrades = null)
{
    public const int CurrentVersion = 1;
}
