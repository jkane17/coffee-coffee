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
