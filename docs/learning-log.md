# Learning log

Godot concepts covered so far, so they don't get re-explained. Claude appends to this as new concepts come up.

## 2026-10-05
- **Project setup**: Godot 4.7.2 .NET build, C# scripts, 2D game. Used in: `project.godot`

## Shop scene (step 1)
- **Nodes & scenes**: a scene is a saved tree of nodes. Scenes can be instanced inside other scenes. Used in: `scenes/shop/shop.tscn`, `scenes/customer/customer.tscn`
- **Instancing from code**: `PackedScene.Instantiate<T>()` + `AddChild`. Used in: `scenes/shop/Shop.cs`
- **Marker2D**: invisible position marker for spawn points and targets. Used in: `scenes/shop/shop.tscn`
- **[Export]**: exposes a property in the Inspector, including node references. Rebuild before it appears. Used in: `Shop.cs`, `Customer.cs`
- **_Process & delta**: per-frame update; multiply speeds by delta for frame-rate independence. Used in: `Customer.cs`
- **Custom [Signal]**: `ArrivedEventHandler` declared, emitted with `EmitSignal`, connected with `+=`. Used in: `Customer.cs`, `Shop.cs`

## Customer spawning (step 2)
- **Timer node**: fires its built-in `Timeout` signal every `Wait Time` seconds. `Autostart` starts it when the scene loads. Used in: `scenes/shop/shop.tscn`
- **Built-in signals**: engine nodes emit signals too (`Timeout`, `Pressed`, ...); see them in the Node → Signals tab, connect in C# with `+=`. Used in: `Shop.cs`
- **[ExportGroup] / PropertyHint.Range**: group related exports in the Inspector and limit values to a range. Used in: `Shop.cs`

## Serving customers (step 3)
- **Input Map**: named actions (e.g. `serve`) bound to keys in Project Settings, so code never hard-codes keys. Used in: `project.godot`, `Shop.cs`
- **_UnhandledInput**: receives input events that UI controls didn't consume; check them with `IsActionPressed`. Used in: `Shop.cs`
- **QueueFree**: safely deletes a node (and its children) at the end of the current frame. Used in: `Customer.cs`
