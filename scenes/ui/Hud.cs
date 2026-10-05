using Godot;

/// <summary>On-screen overlay. Observes gameplay systems and displays their state; never changes them.</summary>
public partial class Hud : CanvasLayer
{
    [Export] public Label MoneyLabel { get; set; } = null!;

    private Till? _till;

    /// <summary>Start displaying the given till's balance, replacing any till shown before.</summary>
    public void ShowTill(Till till)
    {
        StopShowingTill();
        _till = till;
        _till.BalanceChanged += OnBalanceChanged;
        OnBalanceChanged(_till.Balance);
    }

    public override void _ExitTree()
    {
        // Till is plain C# and could outlive this node; unsubscribe so it never calls into a freed HUD.
        StopShowingTill();
    }

    private void StopShowingTill()
    {
        if (_till is not null)
        {
            _till.BalanceChanged -= OnBalanceChanged;
            _till = null;
        }
    }

    private void OnBalanceChanged(int balance)
    {
        MoneyLabel.Text = $"$ {balance}";
    }
}
