/// <summary>Works out what to suggest next while the player makes their first coffee.
/// It only looks at the current state, never at what happened before, so it copes with steps done out of order.</summary>
public static class CoffeeTutorial
{
    /// <summary>The hint for the next step, or null when there's nothing to suggest.</summary>
    public static string? NextHint(CustomerState guestState, CoffeeBar bar, string guestName)
    {
        switch (guestState)
        {
            case CustomerState.Queueing:
            case CustomerState.Thinking:
                return $"{guestName} is deciding what to have...";
            case CustomerState.ReadyToOrder:
                return $"Click {guestName} to take their order.";
            case CustomerState.Ordered:
                break;
            default:
                return null;
        }

        Kettle kettle = bar.Kettle;

        if (bar.HeldDrink is not null)
        {
            return $"Click {guestName} to hand over the coffee.";
        }

        if (kettle.State == KettleState.Filling)
        {
            return "Filling the kettle...";
        }

        if (bar.IsHoldingKettle)
        {
            return kettle.State == KettleState.Empty
                ? "Click the sink to fill the kettle."
                : "Click the kettle's base to put it back and boil it.";
        }

        if (kettle.State == KettleState.Empty)
        {
            return bar.HeldCup switch
            {
                null => "Click the kettle to pick it up.",
                { IsEmpty: true } => "Your hands are full. Put the cup back on the stack, then pick up the kettle.",
                _ => "Your hands are full. Tip the cup out at the sink, then pick up the kettle.",
            };
        }

        // The kettle is on its base, boiling or boiled.
        if (bar.HeldCup is null)
        {
            return kettle.State == KettleState.Boiling
                ? "While the kettle boils, click the cup stack to grab a cup."
                : "Click the cup stack to grab a cup.";
        }

        if (bar.HeldCup.IsEmpty)
        {
            return "Click the coffee jar to add granules.";
        }

        return kettle.State == KettleState.Boiled
            ? "Click the kettle to pour the hot water."
            : "Wait for the kettle to boil...";
    }
}
