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

## Architecture

Tower defense game built with MonoGame 3.8 (DesktopGL) on .NET 9. Single-project, flat file structure — all `.cs` files live in the project root under namespace `monogame_funny_game`.

**MonoGame lifecycle:** `Program.cs` instantiates `Game1`, which follows the standard MonoGame loop: `Initialize()` → `LoadContent()` → `Update()`/`Draw()` at ~60 FPS.

**Planned classes** (see `PLAN.md` for full design, being built in phases):

- `Map.cs` — 20×10 grid of 64×64 tiles, procedural path generation with hardcoded snake fallback
- `Tower.cs` / `TowerType.cs` — Placement, targeting (furthest-along-path enemy in range), cooldowns
- `Enemy.cs` / `EnemyType.cs` — Path-following with HP, speed, slow effects
- `Projectile.cs` — Homing projectiles, disappear if target dies
- `WaveManager.cs` — 10-wave definitions with spawn timing
- `GameState.cs` — Lives, currency, win/lose tracking
- `Game1.cs` orchestrates everything: input, game screens (Menu → Playing → GameOver/Victory), HUD

## Key Conventions

- **Content pipeline:** Assets must be registered in `Content/Content.mgcb` (platform: DesktopGL, profile: Reach) and loaded via `Content.Load<T>("AssetName")` in `LoadContent()`. The `.mgcb` file currently has no assets registered — they need to be added as implementation progresses.
- **Resolution:** 1280×720. Play area is 1280×640 (20×10 grid), bottom 80px is HUD.
- **HP bars:** Drawn with a runtime-created 1×1 white pixel texture, no asset needed.
- **Fallback art:** If image assets aren't available, generate solid-color placeholder textures at runtime.
