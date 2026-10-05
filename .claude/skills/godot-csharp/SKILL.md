---
name: godot-csharp
description: Godot 4 C# conventions, patterns and gotchas for this project. Use before writing or editing any .cs script, .tscn scene or .tres resource, and when debugging Godot C# errors (missing exports, signals not firing, GetNode nulls, partial class errors).
---

# Godot 4 C# reference

Godot's docs and most online examples use GDScript. Translate them to C#. Never write GDScript-style C# (snake_case methods, `func`, `$Node`).

## Scripts
- Every node script is a `partial` class, and the class name **must match the file name**:
  `public partial class Customer : CharacterBody2D` in `Customer.cs`.
- Engine members are PascalCase in C#: `Position`, `GetNode`, `QueueFree`, `GlobalPosition`.
- When a property name is needed as a string (tweens, `Set`), use the generated constants, e.g. `Node2D.PropertyName.Position`, instead of `"position"`.
- Logging: `GD.Print`, `GD.PushWarning`, `GD.PushError`.

## Lifecycle
| Method | When |
|---|---|
| `_EnterTree()` | Node added to the tree (children may not be ready) |
| `_Ready()` | Node and all its children are in the tree. Do setup here. |
| `_Process(double delta)` | Every frame (visuals, timers, UI) |
| `_PhysicsProcess(double delta)` | Fixed physics tick (movement, collisions) |
| `_UnhandledInput(InputEvent @event)` | Input not consumed by UI |
| `_ExitTree()` | Leaving the tree. Disconnect from longer-lived objects here. |

`delta` is a `double`; cast when mixing with `float` (`(float)delta`).

## Nullable references
Node references assigned in `_Ready` or by the Inspector aren't set in the constructor:
```csharp
[Export] private Label _priceLabel = null!;   // assigned in Inspector; fails loudly if missing
private Timer _brewTimer = null!;             // assigned in _Ready
public override void _Ready() => _brewTimer = GetNode<Timer>("BrewTimer");
```
Use `T?` only when the value can genuinely be absent at runtime.

## Getting nodes
Order of preference:
1. `[Export]` node reference, wired in the Inspector. Survives renames and moves.
2. Scene-unique name: mark the node *% Access as Unique Name* in the editor, then `GetNode<Label>("%PriceLabel")`.
3. Relative path `GetNode<Timer>("BrewTimer")` for direct children only.
Avoid long `../../` paths and `GetTree().Root` lookups. They make scenes depend on where they're placed.

## Exports
```csharp
[Export] public float WalkSpeed { get; set; } = 80f;
[Export(PropertyHint.Range, "0,100,1")] public int Patience { get; set; } = 50;
[Export] public PackedScene CustomerScene { get; set; } = null!;
[ExportGroup("Pricing")] [Export] public int BasePrice { get; set; } = 3;
```
New or changed exports only appear in the Inspector **after a build** (Alt+B in the editor).

## Signals
```csharp
[Signal] public delegate void OrderPlacedEventHandler(DrinkRecipe recipe);  // name MUST end in EventHandler

EmitSignal(SignalName.OrderPlaced, recipe);   // emit
customer.OrderPlaced += OnOrderPlaced;         // connect
customer.OrderPlaced -= OnOrderPlaced;         // disconnect (in _ExitTree if the emitter outlives you)
```
- Signal parameters must be Variant-compatible: primitives, `string`, Godot structs, `GodotObject` subclasses (Nodes, Resources), and Godot collections. Plain C# classes are not allowed.
- Built-in signals: `button.Pressed += OnBuyPressed;`, `timer.Timeout += OnBrewDone;`.
- For **pure C# gameplay systems** in `game/` that aren't nodes, plain C# `event Action<T>` is fine and keeps them engine-light. Use `[Signal]` when the editor needs to see or connect it.

## Instancing scenes
```csharp
Customer customer = CustomerScene.Instantiate<Customer>();
customer.Position = _spawnPoint.Position;
AddChild(customer);
// later
customer.QueueFree();   // never Free() mid-frame; check GodotObject.IsInstanceValid(x) for stale refs
```

## Custom Resources (data)
Use these for drink recipes, ingredients and upgrades. Designers edit them as `.tres` in the Inspector.
```csharp
[GlobalClass]
public partial class DrinkRecipe : Resource
{
    [Export] public string DisplayName { get; set; } = "";
    [Export] public int Price { get; set; }
    [Export] public float BrewSeconds { get; set; } = 2f;
    [Export] public Texture2D? Icon { get; set; }
}
```
`[GlobalClass]` makes it appear in *FileSystem → right-click → New → Resource…*. Resources need a parameterless constructor. Loaded resources are shared, so call `Duplicate()` before mutating per-instance state.

## Timers, tweens, async
```csharp
GetTree().CreateTimer(2.0).Timeout += OnBrewDone;
await ToSignal(GetTree().CreateTimer(2.0), SceneTreeTimer.SignalName.Timeout);
Tween tween = CreateTween();
tween.TweenProperty(cup, Node2D.PropertyName.Position.ToString(), target, 0.4);
```

## Input
Define actions in *Project → Project Settings → Input Map* (e.g. `interact`), then use
`Input.IsActionJustPressed("interact")` or check `@event.IsActionPressed("interact")` in `_UnhandledInput`.

### Mouse clicks
```csharp
public override void _UnhandledInput(InputEvent @event)
{
    if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: true })
    {
        Vector2 point = GetGlobalMousePosition();   // world position, on any CanvasItem
        GetViewport().SetInputAsHandled();
    }
}
```
- Controls (UI) see clicks **before** `_UnhandledInput`. A Control with *Mouse → Filter* = **Stop** swallows clicks over its rect. Set **Ignore** on decorative Controls (backgrounds, icons, progress bars, editor-only rects). Leave **Stop** on panels that should block the game, like modal dialogs.
- To find what's under a click, give it an `Area2D` + `CollisionShape2D` and query the physics space:
```csharp
PhysicsPointQueryParameters2D query = new() { Position = point, CollideWithAreas = true, CollideWithBodies = false };
foreach (Godot.Collections.Dictionary hit in GetWorld2D().DirectSpaceState.IntersectPoint(query))
{
    if (hit["collider"].AsGodotObject() is Area2D area) { /* clicked area */ }
}
```

## Collections
Use `System.Collections.Generic` (`List<T>`, `Dictionary<K,V>`) normally. Use `Godot.Collections.Array<T>` / `Dictionary` only for `[Export]`s and signal parameters.

## Global state
Autoloads (*Project Settings → Globals → Autoload*) are singletons. Per CLAUDE.md, avoid them unless state truly is global (e.g. save data, audio bus manager). Prefer passing references or exported dependencies.

## Scene files (.tscn / .tres)
- Text format: `[ext_resource]` entries carry `uid="uid://..."` and an `id`. Keep them consistent when editing, and never invent UIDs for files that already have them (read the `.uid` file or existing references).
- Prefer the user making scene changes in the editor (see CLAUDE.md *Learning mode*). If you edit a `.tscn` while the editor has it open, tell the user to reload it when prompted.
- Never edit anything under `.godot/`.

## Common errors
| Symptom | Likely cause |
|---|---|
| Script won't attach / "class not found" | Class name ≠ file name, or not `partial`, or not built yet |
| Export missing in Inspector | Not rebuilt; or the type isn't Variant-compatible |
| Signal not in Node → Signals tab | Delegate name doesn't end in `EventHandler`, or not rebuilt |
| `NullReferenceException` in `_Ready` | Wrong `GetNode` path, or export not assigned in Inspector |
| Changes ignored at runtime | Running stale build. Rebuild (Alt+B) before F5. |
