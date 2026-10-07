using System;

/// <summary>How many customers come in on one day and how long to wait between them. Fewer customers means longer gaps,
/// so early days stay spread across opening hours instead of everyone arriving at once.</summary>
public sealed class CustomerSchedule
{
    private readonly double _averageIntervalSeconds;
    private readonly double _jitter;

    /// <param name="totalCustomers">How many customers come in today.</param>
    /// <param name="openSeconds">How long the shop is open, in real seconds.</param>
    /// <param name="minIntervalSeconds">The shortest average gap, however many customers there are.</param>
    /// <param name="jitter">How much each gap varies from the average, e.g. 0.3 for ±30%.</param>
    public CustomerSchedule(int totalCustomers, double openSeconds, double minIntervalSeconds, double jitter)
    {
        if (totalCustomers < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(totalCustomers), totalCustomers, "A day needs at least one customer.");
        }

        if (jitter is < 0 or >= 1)
        {
            throw new ArgumentOutOfRangeException(nameof(jitter), jitter, "Jitter must be from 0 up to (not including) 1.");
        }

        Total = totalCustomers;
        _averageIntervalSeconds = Math.Max(openSeconds / totalCustomers, minIntervalSeconds);
        _jitter = jitter;
    }

    public int Total { get; }
    public int Arrived { get; private set; }
    public bool AllArrived => Arrived >= Total;

    /// <summary>Customers on a given day: the first day's number, plus more each day after, up to a maximum.</summary>
    public static int CustomersOnDay(int dayNumber, int firstDay, int extraPerDay, int max)
    {
        return Math.Min(firstDay + (dayNumber - 1) * extraPerDay, max);
    }

    public void RecordArrival()
    {
        if (AllArrived)
        {
            throw new InvalidOperationException("Everyone due today has already arrived.");
        }

        Arrived++;
    }

    /// <summary>How long to wait before the next customer, varied a little so arrivals don't feel mechanical.</summary>
    public double NextIntervalSeconds(Random random)
    {
        double variation = (random.NextDouble() * 2 - 1) * _jitter;
        return _averageIntervalSeconds * (1 + variation);
    }
}
