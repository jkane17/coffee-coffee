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

## Drink orders (step 4)
- **Custom Resources + [GlobalClass]**: a `Resource` subclass holds data; `[GlobalClass]` lets you create it as a `.tres` file from the FileSystem dock and edit it in the Inspector. Used in: `game/orders/DrinkRecipe.cs`, `resources/drinks/`
- **Exporting a typed array**: `Godot.Collections.Array<DrinkRecipe>` shows as an editable list in the Inspector. Used in: `Shop.cs`
- **Label**: a Control that shows text; Controls can be children of Node2D scenes. Used in: `scenes/customer/customer.tscn`
- **Plain C# systems**: gameplay logic with no Godot types (e.g. `Till`) uses C# `event`s instead of `[Signal]`. Used in: `game/economy/Till.cs`

## Money display (step 5)
- **CanvasLayer**: draws its children on a separate layer above the game world, unaffected by cameras. Standard root for HUDs. Used in: `scenes/ui/hud.tscn`
- **Instancing scenes in the editor**: the chain-link button adds a saved scene as a child of another scene. Used in: `scenes/main/main.tscn`
- **Main scene as composition root**: a top-level scene that owns gameplay and UI scenes and wires them together, so neither references the other. Used in: `scenes/main/Main.cs`
- **_Ready order**: children are ready before their parent, so a parent's `_Ready` can safely use its children. Used in: `Main.cs`
- **Unsubscribing C# events in _ExitTree**: plain C# objects can outlive nodes; unsubscribe to avoid calls into freed nodes. Used in: `scenes/ui/Hud.cs`
- **Theme overrides**: per-Control tweaks like font size under Inspector → Theme Overrides. Used in: `scenes/ui/hud.tscn`
- **Debugging an unassigned export**: a `NullReferenceException` points at the line that *used* the null reference, not where setup was missed. Check the Inspector (or the `.tscn` for a missing `NodePath`) for unassigned exports. Hit in: `scenes/ui/hud.tscn`

## Brewing (step 6)
- **Ticking a plain C# system**: `Brewer` has no `_Process` of its own; its owner calls `Tick(delta)` each frame, keeping it engine-independent. Used in: `game/orders/Brewer.cs`, `Shop.cs`
- **Events vs polling**: use events for one-off changes (brew started/finished), poll for values that change every frame (progress). Used in: `scenes/ui/BrewStatus.cs`
- **Containers (VBoxContainer)**: arrange child Controls automatically; children's position/size are managed by the container. Used in: `scenes/ui/hud.tscn`
- **ProgressBar**: a Control showing `Value` between `MinValue` and `MaxValue`. Used in: `scenes/ui/hud.tscn`
- **PropertyHint suffix**: `"0.1,30,0.1,suffix:s"` shows units in the Inspector. Used in: `DrinkRecipe.cs`

## Customer patience (step 7)
- **Modulate**: tints a CanvasItem and its children by multiplying their colours; `Colors.White` means no tint. Used in: `scenes/customer/Customer.cs`
- **Color.Lerp**: blends between two colours by a 0–1 weight, handy for showing a value visually. Used in: `Customer.cs`
- **Exporting a base type**: `[Export] CanvasItem Body` accepts any CanvasItem (ColorRect now, Sprite2D later), so the art can change without code changes. Used in: `Customer.cs`

## Day cycle (step 8)
- **Starting and stopping a Timer from code**: `Start()` / `Stop()` instead of Autostart, so the owner controls when it runs. Used in: `Shop.cs`
- **Button + Pressed signal**: the built-in signal fired when a Button is clicked or activated by keyboard. Used in: `scenes/ui/DaySummary.cs`
- **Focus (GrabFocus)**: the focused Control receives keyboard input; a focused Button is pressed with Space/Enter, and that input never reaches `_UnhandledInput`. Used in: `DaySummary.cs`
- **PanelContainer**: a container that draws a background panel behind its single child. Used in: `scenes/ui/hud.tscn`
- **Anchors / layout presets**: pin a Control to a screen edge or centre so it stays put when the window resizes. Used in: `scenes/ui/hud.tscn`

## Placeholder art (step 9)
- **Importing assets**: files dropped into the project are imported automatically; SVGs become textures. Settings live in the Import dock and the `.import` file next to the asset. Used in: `assets/art/`
- **Sprite2D**: draws a texture. `Centered` + `Offset` control where the origin sits (feet, for characters). Used in: `scenes/customer/customer.tscn`, `scenes/shop/shop.tscn`
- **Change Type**: right-click a node → Change Type swaps its class but keeps its name, children and compatible properties. Used in: `customer.tscn`, `shop.tscn`
- **TextureRect with Stretch Mode Tile**: repeats a texture to fill a Control's rect, good for floors and backgrounds. Used in: `shop.tscn`
- **Draw order**: siblings later in the tree draw on top; runtime-added children go last, so customers draw over the scenery. Used in: `shop.tscn`

## Drink icons (exercise)
- **Exports and access**: `[Export]` works on private members too, so the Inspector can set a value other scripts can't read. Members without a modifier are private; use `public` when other classes need them. Hit in: `game/orders/DrinkRecipe.cs`
- **Visibility is inherited**: a hidden parent hides all its children whatever their own `Visible` says, so toggle the container, not its parts. Used in: `scenes/customer/customer.tscn`
- **TextureRect in a container**: shows a texture as a Control so it can sit in an HBoxContainer next to a Label. Used in: `customer.tscn`

## Point-and-click barista (step 10)
- **Mouse input**: `InputEventMouseButton` with pattern matching, `GetGlobalMousePosition()` for the world position. Used in: `scenes/shop/Shop.cs`
- **Mouse filter**: Controls receive clicks before `_UnhandledInput`; `Stop` swallows them, `Ignore` lets them through. Used in: `shop.tscn`, `customer.tscn`, `hud.tscn`
- **Area2D + CollisionShape2D**: an invisible shape other systems can detect; here, what the player clicked. Used in: `customer.tscn`, `shop.tscn`
- **Physics point query**: `DirectSpaceState.IntersectPoint` lists the areas/bodies under a point. Used in: `Shop.cs`
- **ReferenceRect**: an outline that only draws in the editor; handy for marking zones like the work area. Used in: `shop.tscn`
- **Callbacks with Action**: `WalkTo(target, onArrived)` runs code when the walk finishes; a new walk replaces the old callback. Used in: `scenes/barista/Barista.cs`

## Hover feedback (step 11)
- **Shaders (canvas_item)**: small GPU programs run per pixel; `fragment()` sets each pixel's `COLOR`, `vertex()` runs per corner. Used in: `scenes/common/outline.gdshader`
- **Uniforms + ShaderMaterial**: a shader's `uniform`s show in the Inspector under the material; set them from C# with `SetShaderParameter`. Used in: `scenes/common/Clickable.cs`
- **Local To Scene**: resources are shared between instances by default; ticking *Local To Scene* gives each scene instance its own copy (here, one material per customer). Used in: `scenes/customer/customer.tscn`
- **Cursor shapes**: `Input.SetDefaultCursorShape` switches the OS cursor, e.g. to a pointing hand. Used in: `Shop.cs`
- **IsInstanceValid**: checks whether a stored node reference has been freed before touching it. Used in: `Shop.cs`
- **Shader values flow into COLOR**: uniforms are read-only inputs, and a calculation only matters if it feeds the final `COLOR`. `TIME` gives seconds since start for animation. Shaders recompile on save; errors show in the shader editor and Output. Used in: `outline.gdshader`

## Walk animation (step 12)
- **Component nodes**: shared behaviour (walking) lives in its own node (`Walker`) added as a child, instead of being copied or inherited. Used in: `scenes/common/Walker.cs`, `customer.tscn`, `barista.tscn`
- **AnimationPlayer**: keyframes any property over time (here `Body:position` and `Body:rotation`). Track paths are relative to the AnimationPlayer's root (its parent by default). Used in: `customer.tscn`, `barista.tscn`
- **Sharing animations as resources**: an AnimationPlayer holds libraries (`[Global]` is the unnamed default) of animations. Save a single animation to a `.tres` and *Load* it into other players to share it; loading a whole library adds it under a name, which prefixes its animations (`library/walk`). `RESET` is an editor helper storing resting values. Used in: `scenes/common/walk.tres`
- **Looping and Stop()**: loop mode repeats an animation; `Stop()` returns to its first frame, so the resting pose belongs at time 0. Used in: `Walker.cs`

## Title screen (step 13)
- **SceneTree**: the running game's tree of nodes; `GetTree()` reaches it from any node. Used in: `scenes/title/TitleScreen.cs`
- **Changing scenes**: `GetTree().ChangeSceneToPacked(scene)` frees the current scene and loads another; `GetTree().Quit()` closes the game. Used in: `TitleScreen.cs`
- **Control-based scenes**: menus use a Control root with the *Full Rect* anchor preset so they fill the window at any size. Used in: `scenes/title/title_screen.tscn`
- **CenterContainer**: keeps its child centred in its own rect. Used in: `title_screen.tscn`

## Save and load (step 14)
- **user:// paths**: a per-user writable data folder (on Windows under `%APPDATA%\Godot\app_userdata\<project>`). Open it with *Project → Open User Data Folder*. `res://` is read-only in exported games. Used in: `game/save/SaveStore.cs`
- **FileAccess**: Godot's file API; understands `user://` and `res://` and works on every platform. Used in: `SaveStore.cs`
- **Versioned save data**: a version number in the file lets future code detect and handle older formats. Used in: `game/save/SaveData.cs`
- **Changing scenes by hand**: `Instantiate` → configure → `Root.AddChild` → set `CurrentScene` → `QueueFree` the old scene. Unlike `ChangeSceneToPacked`, it lets you pass data in before the new scene's `_Ready`. Used in: `scenes/title/TitleScreen.cs`

## Pause menu (step 15)
- **Pausing**: `GetTree().Paused = true` stops processing and input for every node whose Process Mode is *Pausable* (the default via *Inherit*), including Timers and AnimationPlayers. Used in: `scenes/ui/PauseMenu.cs`
- **Process Mode**: per node (*Inherit*, *Pausable*, *When Paused*, *Always*, *Disabled*); children inherit it. A pause menu needs *Always* to keep working. Used in: `scenes/ui/pause_menu.tscn`
- **Pause outlives scenes**: `Paused` belongs to the SceneTree, so unpause before changing scene. Used in: `PauseMenu.cs`
- **Loading scenes by path**: `ChangeSceneToFile` + a `PropertyHint.File` string export avoids circular PackedScene references between scenes. Used in: `PauseMenu.cs`
- **CanvasLayer Layer**: higher layers draw on top of lower ones (pause menu above the HUD). Used in: `pause_menu.tscn`

## Starting store (step 16)
- **Renaming exports**: a renamed `[Export]` is a new property to Godot; the old saved value is dropped and must be reassigned in the Inspector. Hit in: `Shop.cs` (`BrewStationArea`, `BrewSpot`)
- **Keeping data for later**: resources and art not used yet (espresso, coffee machine) can stay in the project; only what's referenced in scenes gets used. Used in: `resources/drinks/`, `assets/art/shop/`

## Decoration (step 17)
- **Grouping with a Node2D**: a plain Node2D parent keeps related nodes together; moving or hiding the parent affects them all. Used in: `scenes/shop/shop.tscn` (`Decor`)
- **Sprite2D Centered**: off means the texture's top-left corner sits at the node's position, useful for things anchored to a corner. Used in: `shop.tscn` (cobweb)
- **CanvasModulate**: tints everything on its canvas layer (the shop) but not other CanvasLayers (the HUD), for cheap mood lighting. Used in: `shop.tscn`

## Taking orders (step 18)
- **State machines with an enum**: one `State` value (Queueing → Thinking → ReadyToOrder → …) replaces several bools, so impossible mixes (leaving *and* being served) can't happen, and `_Process` can `switch` on it to decide what to tick. Used in: `scenes/customer/CustomerState.cs`, `Customer.cs`
- **Countdowns in _Process**: subtracting `delta` from a field is the simplest one-off timer, and it stops automatically when the game is paused or the node is freed. Used in: `Customer.cs` (thinking time)

## Making instant coffee (step 19)
- **Use Parent Material**: a CanvasItem setting that draws the node with its parent's material, so one ShaderMaterial (the hover outline) covers a parent and its children. Used in: `scenes/shop/shop.tscn` (kettle on its base)
- **Area2D as a station**: a station is just a sprite with a `Clickable` child; the Shop maps each click area to what it does, and uses the area's position to work out where the barista should stand. Used in: `scenes/shop/Shop.cs`
- **Visible doesn't disable physics**: hiding a node stops it drawing, but its Area2D still answers point queries. That's why the click area sits on the kettle *base*, which stays clickable while the kettle is carried. Used in: `shop.tscn`
