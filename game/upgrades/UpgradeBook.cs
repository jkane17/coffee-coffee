using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

/// <summary>Which upgrades the shop owns. It doesn't handle money: the shop pays from the till, then calls <see cref="Add"/>.</summary>
public sealed class UpgradeBook
{
    private readonly List<Upgrade> _catalog;
    private readonly HashSet<Upgrade> _owned = new();

    public UpgradeBook(IEnumerable<Upgrade> catalog)
    {
        _catalog = catalog.ToList();
    }

    /// <summary>Every upgrade that can be bought, owned or not.</summary>
    public IReadOnlyList<Upgrade> Catalog => _catalog;

    /// <summary>The <see cref="Upgrade.Id"/> of each owned upgrade, for saving.</summary>
    public IEnumerable<string> OwnedIds => _owned.Select(upgrade => upgrade.Id);

    /// <summary>Raised whenever upgrades become owned.</summary>
    public event Action? Changed;

    public bool IsOwned(Upgrade upgrade) => _owned.Contains(upgrade);

    /// <summary>Whatever this upgrade builds on has been bought (or it doesn't build on anything).</summary>
    public bool IsUnlocked(Upgrade upgrade) => upgrade.Requires is null || IsOwned(upgrade.Requires);

    public void Add(Upgrade upgrade)
    {
        if (!_catalog.Contains(upgrade))
        {
            throw new ArgumentException($"{upgrade.DisplayName} isn't in the upgrade catalog.", nameof(upgrade));
        }

        if (!_owned.Add(upgrade))
        {
            throw new InvalidOperationException($"{upgrade.DisplayName} is already owned.");
        }

        Changed?.Invoke();
    }

    /// <summary>Own the upgrades with these ids, e.g. from a save. Ids that aren't in the catalog any more are skipped.</summary>
    public void Restore(IEnumerable<string> ids)
    {
        foreach (string id in ids)
        {
            Upgrade? upgrade = _catalog.FirstOrDefault(candidate => candidate.Id == id);
            if (upgrade is null)
            {
                GD.PushWarning($"Skipping saved upgrade '{id}': it isn't in the catalog.");
                continue;
            }

            _owned.Add(upgrade);
        }

        Changed?.Invoke();
    }

    /// <summary>The combined <see cref="Upgrade.Amount"/> of every owned upgrade with this effect; 0 if none.</summary>
    public int Total(UpgradeEffect effect)
    {
        return _owned.Where(upgrade => upgrade.Effect == effect).Sum(upgrade => upgrade.Amount);
    }
}
