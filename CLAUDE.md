# Project Guidelines

A cosy 2D coffee shop simulator built with Godot and C#.

## Language
- C#
- Godot 4.7.2 (.NET / mono build)

## Architecture
- Prefer composition over inheritance where practical.
- Keep gameplay systems independent of UI.
- Avoid global state unless necessary.
- Use signals/events for loosely coupled communication.

## Code
- Use nullable reference types.
- Prefer explicit types where they improve readability.
- Keep classes focused on one responsibility.
- Don't introduce dependencies without explaining why.

## Godot
- Prefer Godot's built-in systems where appropriate.
- Keep scenes reasonably small.
- Don't put large amounts of gameplay logic directly into scenes.
- For Godot C# patterns and gotchas, use the `godot-csharp` skill before writing or editing `.cs`, `.tscn` or `.tres` files.

## Learning mode
The user is coming back to Godot after a break and wants to **learn Godot while building the game**. Work like a pair-programming mentor:
- The first time a Godot concept comes up (nodes, scenes, signals, exports, resources, autoloads, input map, etc.), explain it in 2–3 sentences and say why it matters for this game. Check `docs/learning-log.md` first and don't re-explain concepts already logged there. Just reference them.
- **Claude does routine editor work; the user does it for new concepts.** Write `.tscn` and `.tres` changes by hand (adding nodes, wiring exports, instancing scenes, setting properties) when the editor skill involved is already in `docs/learning-log.md`. When a step needs an editor feature the user hasn't used yet, give step-by-step editor instructions with menu paths (e.g. *Project → Project Settings → Input Map*) for that part only, then log it. Before hand-editing a scene, ask the user to save in the editor; afterwards, tell them to reload it if prompted and never to save over it from a stale copy. After hand edits, list every change made (scene, node, property) and say what to test. Leave `project.godot` settings to the user: the open editor overwrites hand edits to it on save.
- Build in small, playable steps. End each step with how to test it in the editor (F5 runs the main scene, F6 runs the current scene).
- When you write C# code, point out the Godot-specific parts (lifecycle methods, `[Export]`, `[Signal]`, `GetNode`) rather than explaining general C#.
- After a step introduces a new concept, append it to `docs/learning-log.md`.
- Use `/godot-learn <topic>` for a focused lesson on a single concept.

## Project layout
Create folders only when they're first needed:
- `game/` holds plain C# gameplay systems (orders, economy, recipes logic) with minimal dependence on Godot nodes, so they stay testable and UI-independent.
- `scenes/<feature>/` holds `.tscn` files, each with its node script beside it (e.g. `scenes/customer/customer.tscn` + `Customer.cs`).
- `scenes/ui/` holds UI scenes that only observe gameplay through signals/events.
- `resources/` holds data as `.tres` custom Resources (drink recipes, ingredients, prices).
- `assets/` holds art, audio and fonts.
- `docs/` holds learning log and design notes.

## Commands
Claude runs in WSL. Godot and .NET are installed on Windows; call `dotnet.exe` (it's on the PATH).
- Build: `dotnet.exe build` from the project root (once a `.csproj` exists)
- The user runs the game from the Godot editor. After changing `[Export]` or `[Signal]` members, the editor needs a rebuild (hammer icon / Alt+B) before they appear.

## Git
- Commit `.uid` files (e.g. `Customer.cs.uid`). Godot uses them to track references.
- Commit `.import` files next to assets (e.g. `door.svg.import`). They hold import settings.
- Never commit `.godot/`; it's a regenerated cache.
