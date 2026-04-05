# Copilot Instructions

## Environment Setup

.NET 9 SDK is at `~/.dotnet`. You **must** set the PATH before running dotnet commands:

```bash
export PATH="$HOME/.dotnet:$PATH"
export DOTNET_ROOT="$HOME/.dotnet"
```

The project is pinned to .NET 9 via `global.json`. The system may have .NET 10 at `/usr/lib/dotnet` — it is **incompatible** with MonoGame 3.8 MGCB tooling. Do not use it.

## Build & Run

```bash
dotnet build        # Build the project
dotnet run          # Run the game
dotnet tool restore # Restore local tools (MGCB content pipeline)
```

No test project or linter is configured.

## Project

Tower defense game built with MonoGame 3.8 (DesktopGL) on .NET 9. Single-project, flat file structure — all `.cs` files live in the project root under namespace `monogame_funny_game`.

MonoGame lifecycle: `Program.cs` instantiates `Game1`, which follows the standard loop: `Initialize()` → `LoadContent()` → `Update()`/`Draw()` at ~60 FPS.

Content pipeline: assets must be registered in `Content/Content.mgcb` (platform: DesktopGL, profile: Reach) and loaded via `Content.Load<T>("AssetName")` in `LoadContent()`.

Resolution: 1280x720. Play area is 1280x640 (20x10 grid), bottom 80px is HUD.

## Agent Orchestration

`PLAN.md` is the single source of truth. It is organized into sections, each owned by a specific agent with explicit file ownership.

### Available Agents

| Agent           | Domain                                                              |
| --------------- | ------------------------------------------------------------------- |
| `@orchestrator` | Reads `PLAN.md`, delegates work, runs parallel jobs, enforces locks |
| `@map-agent`    | Tile grids, procedural map generation, grid rendering               |
| `@enemy-agent`  | Enemy entities, pathfinding, status effects                         |
| `@tower-agent`  | Tower placement, input, currency                                    |
| `@combat-agent` | Projectiles, targeting, damage                                      |
| `@wave-agent`   | Wave spawning, progression, win/lose                                |
| `@hud-agent`    | HUD, health bars, placement preview                                 |
| `@menu-agent`   | Screen flow, menus, state transitions                               |

### Lock Protocol

Agents must not edit the same file concurrently. Before an agent edits a file, a `.locks/{filename}.lock` file must be created with the agent name. When the agent is done, the lock is removed. If a lock exists, another agent must wait.

### Parallelism Rules

Agents whose file ownership in `PLAN.md` does not overlap may run in parallel. The orchestrator determines parallelism by reading the file ownership table in each plan section.

### Shared File: TowerDefenseGame.cs

`TowerDefenseGame.cs` is touched by multiple agents. It must be edited sequentially — only one agent may hold its lock at a time. The orchestrator is responsible for ordering access.
