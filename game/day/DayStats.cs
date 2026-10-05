/// <summary>What happened during one shop day.</summary>
public sealed class DayStats
{
    public DayStats(int dayNumber)
    {
        DayNumber = dayNumber;
    }

    public int DayNumber { get; }
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
