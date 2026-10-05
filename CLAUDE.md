# Project Guidelines

## Language
- C#
- Godot 4.7.2

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
