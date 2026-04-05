# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Environment Setup

.NET 9 SDK is installed at `~/.dotnet`. You must set the PATH before running dotnet commands:

```bash
export PATH="$HOME/.dotnet:$PATH"
export DOTNET_ROOT="$HOME/.dotnet"
```

The project is pinned to .NET 9 via `global.json`. The system has .NET 10 at `/usr/lib/dotnet` which is **incompatible** with MonoGame 3.8 MGCB tooling.

## Build & Run

```bash
dotnet build                # Build the project
dotnet run                  # Run the game
dotnet tool restore         # Restore local tools (MGCB, MGCB editor)
dotnet mgcb-editor-linux    # Open content pipeline editor (Linux/WSL)
```

## Project Overview

Simple tower defense game using MonoGame 3.8 (DesktopGL) on .NET 9. Single-project structure.

## Architecture

- **Program.cs** — Entry point; instantiates and runs `TowerDefenseGame`.
- **TowerDefenseGame.cs** — Main game class. Standard MonoGame lifecycle: `Initialize()` → `LoadContent()` → game loop (`Update()` / `Draw()`).
- **Content/Content.mgcb** — Content pipeline manifest (platform: DesktopGL, profile: Reach). Assets go here.

## MonoGame Conventions

- Game assets (textures, fonts, sounds) must be registered in `Content.mgcb` and loaded via `Content.Load<T>("AssetName")` in `LoadContent()`.
- The game loop runs at ~60 FPS. `Update()` handles logic/input, `Draw()` handles rendering.
- Namespace: `monogame_funny_game`.
