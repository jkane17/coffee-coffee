/// <summary>Everything that persists between play sessions. Bump <see cref="CurrentVersion"/> when older saves can no longer be read as they are,
/// and teach <see cref="SaveStore"/> to upgrade them. A new field with a default (like <see cref="OwnedUpgrades"/>) doesn't need it,
/// because saves without it just get the default.</summary>
/// <param name="Money">The till's balance in cents. Version 1 saves stored whole dollars.</param>
public sealed record SaveData(int Version, int Money, int DaysCompleted, string[]? OwnedUpgrades = null)
{
    public const int CurrentVersion = 2;
}
