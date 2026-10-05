---
name: godot-learn
description: Teach a single Godot concept in the context of the coffee shop game, with editor steps, a small C# example and a hands-on exercise. Use when the user runs /godot-learn <topic>, or asks "how does X work in Godot" / "explain X".
argument-hint: <topic, e.g. signals, scenes, custom resources, tweens>
---

# Godot lesson: $ARGUMENTS

Give a short, practical lesson on **$ARGUMENTS** for someone returning to Godot after a break, using Godot 4 with C#.

1. Read `docs/learning-log.md` and skim the project (`scenes/`, `game/`, `resources/`) so the lesson builds on what the user already knows and on code that actually exists. Use the `godot-csharp` skill for correct C# patterns.
2. Structure the lesson as follows. Keep it tight and skimmable, around one screen:
   - **What it is**: 2–3 sentences, plain language.
   - **Why it matters here**: tie it to a concrete coffee shop mechanic (orders, customers, brewing, money, UI).
   - **In the editor**: numbered steps with exact menu paths and dock names (Scene, FileSystem, Inspector, Node → Signals).
   - **In C#**: a minimal snippet (≤ 25 lines). Call out the Godot-specific parts.
   - **Gotcha**: the most common mistake with this concept in C#.
   - **Try it**: one small exercise the user can do themselves in 5–15 minutes, with a hint they can expand on if stuck. Don't do it for them unless they ask.
   - **Read more**: one link to the relevant page on https://docs.godotengine.org/en/stable/.
3. Don't modify project files during a lesson unless the user asks.
4. Append an entry to `docs/learning-log.md` (create it if missing) under today's date:
   `- **<Concept>**: one-line summary. Used in: <file or "not yet">`
