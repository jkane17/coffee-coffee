/// <summary>Where a customer is in their visit. They only ever move forward through these.</summary>
public enum CustomerState
{
    /// <summary>Walking in or waiting in line behind someone.</summary>
    Queueing,

    /// <summary>At the front, deciding what to have.</summary>
    Thinking,

    /// <summary>Decided, and waiting for the barista to take their order. Patience starts here.</summary>
    ReadyToOrder,

    /// <summary>Order taken, waiting for the drink to be made.</summary>
    Ordered,

    /// <summary>Their drink is being made or handed over. Patience is paused.</summary>
    BeingServed,

    /// <summary>Walking out, served or not.</summary>
    Leaving,
}
