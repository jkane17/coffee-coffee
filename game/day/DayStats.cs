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
    public int Earnings { get; private set; }

    public void RecordSale(int price)
    {
        CustomersServed++;
        Earnings += price;
    }

    public void RecordLostCustomer()
    {
        CustomersLost++;
    }
}
