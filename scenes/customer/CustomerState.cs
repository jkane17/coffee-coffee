/// <summary>Where a customer is in their visit. They only ever move forward through these.</summary>
public enum CustomerState
{
    /// <summary>Walking in to their spot at the counter.</summary>
    Queueing,

    /// <summary>At the counter, deciding what to have.</summary>
    Thinking,

    /// <summary>Decided, and waiting for the barista to take their order. Patience starts here.</summary>
    ReadyToOrder,

    /// <summary>Order taken, waiting for the drink. Patience keeps running until they're served.</summary>
    Ordered,

    /// <summary>Has their drink.</summary>
    Served,

    /// <summary>Walking out, served or not.</summary>
    Leaving,
}
