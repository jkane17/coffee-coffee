using Godot;

/// <summary>The shop floor: spawns customers at the door and sends them to the counter.</summary>
public partial class Shop : Node2D
{
    [Export] public PackedScene CustomerScene { get; set; } = null!;
    [Export] public Marker2D Door { get; set; } = null!;
    [Export] public Marker2D Counter { get; set; } = null!;

    public override void _Ready()
    {
        SpawnCustomer();
    }

    private void SpawnCustomer()
    {
        Customer customer = CustomerScene.Instantiate<Customer>();
        AddChild(customer);
        customer.GlobalPosition = Door.GlobalPosition;
        customer.Arrived += OnCustomerArrived;
        customer.WalkTo(Counter.GlobalPosition);
    }

    private void OnCustomerArrived()
    {
        GD.Print("A customer reached the counter.");
    }
}
