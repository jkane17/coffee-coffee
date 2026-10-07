/// <summary>What happened during one shop day.</summary>
public sealed class DayStats
{
    public DayStats(int dayNumber, int customersExpected)
    {
        DayNumber = dayNumber;
        CustomersExpected = customersExpected;
    }

    public int DayNumber { get; }
    /// <summary>How many customers were due to come in that day.</summary>
    public int CustomersExpected { get; }
    public int CustomersServed { get; private set; }
    public int CustomersLost { get; private set; }
    /// <summary>Everything taken, tips included.</summary>
    public Money Earnings { get; private set; }
    public Money Tips { get; private set; }

    public void RecordSale(Money price, Money tip)
    {
        CustomersServed++;
        Earnings += price + tip;
        Tips += tip;
    }

    public void RecordLostCustomer()
    {
        CustomersLost++;
    }
}
