/// <summary>Everything that persists between play sessions. Bump <see cref="CurrentVersion"/> whenever the shape changes.</summary>
public sealed record SaveData(int Version, int Money, int DaysCompleted)
{
    public const int CurrentVersion = 1;
}
