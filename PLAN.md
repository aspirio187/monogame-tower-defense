# Tower Defense Game — Implementation Plan

## Context

MonoGame 3.8 (DesktopGL, .NET 9) tower defense game. All assets are ready in `Content/`.

---

## Game Design

- **Resolution:** 1280x720
- **Grid:** 20 columns x 10 rows of 64x64 tiles (play area: 1280x640), HUD in bottom 80px
- **Starting currency:** 150 | **Starting lives:** 20
- **10 waves**, auto-starting with 5s pause between waves

### Map (procedurally generated)

Maps are generated at runtime using a **random waypoint connection** algorithm:

1. Pick a random spawn tile on the left edge (column 0) and an exit tile on the right edge (column 19)
2. Generate 4-6 random intermediate waypoints spread across the grid (evenly distributed by column range to ensure the path spans the map)
3. Connect waypoints with alternating horizontal then vertical straight segments (L-shaped connections)
4. Validate: path tiles must not overlap or touch themselves diagonally, and enough grass tiles (~60%+) remain for tower placement
5. If validation fails, regenerate (retry up to 10 times, then fall back to a hardcoded snake layout)

Hardcoded snake fallback:

```text
Row 0: 0 0 2 1 1 1 1 0 0 0 0 0 0 0 0 0 0 0 0 0
Row 1: 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0
Row 2: 0 0 0 0 0 0 1 1 1 1 1 1 1 0 0 0 0 0 0 0
Row 3: 0 0 0 0 0 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0
Row 4: 0 0 0 1 1 1 1 1 1 1 1 1 1 0 0 0 0 0 0 0
Row 5: 0 0 0 1 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
Row 6: 0 0 0 1 1 1 1 1 1 1 0 0 0 0 0 0 0 0 0 0
Row 7: 0 0 0 0 0 0 0 0 0 1 0 0 0 0 0 0 0 0 0 0
Row 8: 0 0 0 0 0 0 0 0 0 1 1 1 1 1 1 1 1 1 0 0
Row 9: 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 1 3 0
```

0=grass (buildable), 1=path, 2=spawn, 3=exit

### Towers

| Tower  | Cost | Damage | Range (px) | Fire Rate | Bullet Speed | Special         |
| ------ | ---- | ------ | ---------- | --------- | ------------ | --------------- |
| Arrow  | 50   | 10     | 150        | 2.0/s     | 400 px/s     | —               |
| Cannon | 100  | 40     | 120        | 0.5/s     | 250 px/s     | —               |
| Ice    | 75   | 5      | 130        | 1.5/s     | 350 px/s     | 50% slow for 2s |

### Enemies

| Enemy  | HP  | Speed (px/s) | Reward |
| ------ | --- | ------------ | ------ |
| Goblin | 30  | 60           | $10    |
| Orc    | 80  | 40           | $20    |
| Wolf   | 20  | 100          | $15    |
| Troll  | 200 | 30           | $50    |

### Waves (10 total)

| Wave | Enemies                    | Interval |
| ---- | -------------------------- | -------- |
| 1    | 5 Goblin                   | 1.5s     |
| 2    | 8 Goblin                   | 1.2s     |
| 3    | 5 Goblin + 3 Orc           | 1.2s     |
| 4    | 4 Wolf + 4 Goblin          | 1.0s     |
| 5    | 6 Orc + 4 Wolf             | 1.0s     |
| 6    | 10 Goblin + 5 Orc          | 0.8s     |
| 7    | 8 Wolf + 6 Orc             | 0.8s     |
| 8    | 10 Orc + 5 Wolf + 1 Troll  | 0.8s     |
| 9    | 8 Orc + 8 Wolf + 2 Troll   | 0.7s     |
| 10   | 10 Orc + 10 Wolf + 3 Troll | 0.6s     |

### Key Design Decisions

- **Tower targeting:** Pick the enemy furthest along the path (closest to exit) within range
- **Projectiles:** Homing (track target position each frame). If target dies mid-flight, projectile disappears
- **HP bars:** Drawn with a runtime-created 1x1 white pixel texture, no asset needed
- **Fallback art:** If assets fail to load, generate solid-color placeholder textures at runtime

---

## Assets — DONE

All assets are generated, resized, and in `Content/`:

| Asset        | Size  | File                  |
| ------------ | ----- | --------------------- |
| Grass tile   | 64x64 | `grass.png`           |
| Path tile    | 64x64 | `path.png`            |
| Arrow tower  | 48x48 | `tower_arrow.png`     |
| Cannon tower | 48x48 | `tower_cannon.png`    |
| Ice tower    | 48x48 | `tower_ice.png`       |
| Goblin       | 32x32 | `enemy_goblin.png`    |
| Orc          | 32x32 | `enemy_orc.png`       |
| Wolf         | 32x32 | `enemy_wolf.png`      |
| Troll        | 32x32 | `enemy_troll.png`     |
| Projectile   | 8x8   | `projectile.png`      |
| HUD font     | —     | `hud_font.spritefont` |

Still need to be registered in `Content/Content.mgcb` during implementation.

---

## Agent Sections

Each section below is owned by one agent. File ownership is explicit — only the owning agent may create/edit those files. `TowerDefenseGame.cs` is shared and requires lock coordination (see copilot-instructions.md).

---

### @map-agent

**Owns:** `Map.cs`, `Content/Content.mgcb` (grass.png + path.png entries only)
**Touches (shared):** `TowerDefenseGame.cs`

- Create `Map.cs`: constants (`TileSize=64`, `Cols=20`, `Rows=10`), `int[,] Grid`, `List<Vector2> Waypoints`, `bool[,] Occupied`, `IsBuildable(col, row)`, procedural generator, hardcoded fallback, `Draw(SpriteBatch, Texture2D, Texture2D)`
- Register `grass.png` and `path.png` in `Content.mgcb`
- In `TowerDefenseGame.cs`: set resolution 1280x720, load grass/path textures, instantiate Map, draw it, `R` key to regenerate
- **Verify:** different path each launch

---

### @enemy-agent

**Owns:** `EnemyType.cs`, `Enemy.cs`, `Content/Content.mgcb` (enemy PNG entries only)
**Touches (shared):** `TowerDefenseGame.cs`

- Create `EnemyType.cs`: enum + static stats lookup (see Enemies table)
- Create `Enemy.cs`: position, HP, speed, reward, waypoint index, slow timer/factor, `IsAlive`, `ReachedEnd`, `Update(GameTime, waypoints)`, `TakeDamage(float)`, `ApplySlow(float, float)`, `Draw(SpriteBatch, Texture2D)`
- Register 4 enemy PNGs in `Content.mgcb`
- In `TowerDefenseGame.cs`: load enemy textures, add `List<Enemy>`, spawn test enemies, update/draw them
- **Verify:** enemies walk the path

---

### @tower-agent

**Owns:** `TowerType.cs`, `Tower.cs`, `GameState.cs`, `Content/Content.mgcb` (tower PNG entries only)
**Touches (shared):** `TowerDefenseGame.cs`

- Create `TowerType.cs`: enum + static stats lookup (see Towers table)
- Create `Tower.cs`: type, col, row, position (tile center), stats fields, `Draw(SpriteBatch, Texture2D)`
- Create `GameState.cs`: `Lives=20`, `Currency=150`, `GameOver`, `GameWon`, `TryPurchase(int)`, `EnemyKilled(int)`, `EnemyReachedEnd()`
- Register 3 tower PNGs in `Content.mgcb`
- In `TowerDefenseGame.cs`: load tower textures, add `List<Tower>`, `GameState`, `selectedTower`, keys 1/2/3 select type, left-click places on buildable tiles, deduct currency
- **Verify:** towers placed on grass, rejected on path/occupied/broke

---

### @combat-agent

**Owns:** `Projectile.cs`, `Content/Content.mgcb` (projectile.png entry only)
**Modifies:** `Tower.cs`
**Touches (shared):** `TowerDefenseGame.cs`

- Create `Projectile.cs`: position, speed, damage, target (Enemy ref), slow factor/duration, `IsActive`, `Update(GameTime)` (homing, hit at <5px, deactivate if target dead), `Draw(SpriteBatch, Texture2D)`
- Register `projectile.png` in `Content.mgcb`
- Add to `Tower.cs`: fire cooldown, `Update(GameTime, List<Enemy>, List<Projectile>)` — target furthest-along enemy in range, spawn projectile on cooldown
- In `TowerDefenseGame.cs`: load projectile texture, add `List<Projectile>`, update towers/projectiles, handle enemy death (reward) and removal
- **Verify:** towers shoot and kill enemies

---

### @wave-agent

**Owns:** `WaveManager.cs`
**Touches (shared):** `TowerDefenseGame.cs`

- Create `WaveManager.cs`: `WaveDefinition` struct (enemy list + spawn interval), all 10 waves hardcoded (see Waves table), `CurrentWave`, spawn timer, 5s pause timer, `AllWavesComplete`, `Update(GameTime, List<Enemy>, List<Vector2> waypoints)` — spawns at first waypoint, advances waves when all enemies cleared
- In `TowerDefenseGame.cs`: add `WaveManager`, call its Update, check GameOver/GameWon, remove test enemy spawning from enemy-agent phase
- **Verify:** waves progress, win after 10, lose at 0 lives

---

### @hud-agent

**Owns:** `Content/Content.mgcb` (hud_font.spritefont entry only)
**Touches (shared):** `TowerDefenseGame.cs`

- Register `hud_font.spritefont` in `Content.mgcb`
- In `TowerDefenseGame.cs`: load SpriteFont, create 1x1 white pixel texture, draw HUD bar at bottom (wave number, lives, currency, selected tower + cost), draw HP bars above enemies (red bg + green proportional), draw range circle on hover before placing, green/red tile tint for valid/invalid placement
- **Verify:** HUD readable, HP bars visible, placement preview works

---

### @menu-agent

**Touches (shared):** `TowerDefenseGame.cs`

- In `TowerDefenseGame.cs`: add `GameScreen` enum (`Menu`, `Playing`, `GameOver`, `Victory`), start on `Menu` screen, draw title + "Play"/"Quit" buttons (spritefont + rectangle hitboxes), click Play → generate map + reset state + start waves, ESC during play → return to menu, game over/victory → show result text then return to menu
- **Verify:** full flow — menu → play → win/lose → menu, repeatable

---

## Execution Order

Agents that do NOT share files can run in parallel. Suggested groupings:

1. **Parallel:** `@map-agent` + `@enemy-agent` + `@tower-agent` (each own separate .cs files, share TowerDefenseGame.cs sequentially via locks)
2. **Sequential after group 1:** `@combat-agent` (modifies Tower.cs from tower-agent)
3. **Sequential after combat:** `@wave-agent` (replaces test spawning)
4. **Sequential after wave:** `@hud-agent`
5. **Last:** `@menu-agent` (wraps everything in screen flow)

---

## Verification

After each agent completes: `dotnet build`. After all agents: `dotnet build && dotnet run`. Final test: play through all 10 waves, confirm win/lose conditions.
