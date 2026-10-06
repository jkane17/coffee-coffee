/// <summary>What the kettle is doing. A kettle goes round this cycle: filled at the sink, boiled on its base, then poured until empty.</summary>
public enum KettleState
{
    Empty,

    /// <summary>Under the tap at the sink.</summary>
    Filling,

    /// <summary>Full of cold water, ready to go back on its base.</summary>
    Full,

    Boiling,

    /// <summary>Hot water ready to pour.</summary>
    Boiled,
}
