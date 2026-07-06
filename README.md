# Custom Minecraft Implementation

A small sandbox voxel game built in Unity as a technical assignment. The world is procedurally generated from seeded Perlin noise, built from cubes, and playable in first person with mining and building mechanics.

## Requirements

- **Unity 6000.4.0f1** (Unity 6, URP)
- Open the project root in Unity Hub and load the main scene from `Assets/Scenes/`

## Controls

| Input | Action |
|---|---|
| **W / A / S / D** | Move |
| **Mouse** | Look around |
| **Space** | Jump |
| **Hold Left Mouse Button** | Mine the targeted cube (takes time based on cube type) |
| **Right Mouse Button** | Place the selected cube (grid-snapped, wireframe preview shows the target cell) |
| **Mouse Wheel / 1–9** | Select hotbar slot |
| **F5** | Save the world |

The world also saves automatically when the application quits.

## Implemented Features

### Required
- **Randomly generated world** on every new game, based on multi-octave Perlin noise with a random seed
- **Three cube types by height** — gray (rock, deepest, slowest to mine), green (grass, normal), white (snow, highest, fastest to mine)
- **Mining** — hold the mouse button; each cube type has its own mining time, shown on a progress bar
- **Building** — one click, snapped to the world grid
- **World limits** — an unbreakable bottom layer and a build height ceiling

### Bonus
- **Chunk streaming** — chunks spawn and despawn around the player based on view distance; chunk data persists in memory so player edits survive travel
- **Inventory** — mined cubes go into a stacking inventory, displayed as a hotbar
- **Wireframe placement preview** — a wireframe cube shows exactly where the next block will be placed
- **Save/Load system** — the seed plus only player-modified ("dirty") chunks are written to disk as binary files; unmodified chunks are regenerated from the seed on load. Saves live in `Application.persistentDataPath/save/`
- **UI (HUD)** — crosshair, mining progress bar and inventory hotbar, built at runtime with UI Toolkit (code-driven, no UXML/USS)

## Architecture Overview

- `World` — central manager; owns chunk data (single source of truth) and streams chunk renderers around the player
- `Chunk` — plain C# data class; a flat array of block type enums per 16×64×16 chunk
- `WorldGenerator` — static, seeded, deterministic terrain generation
- `ChunkGenerator` — pure view layer; builds a face-culled mesh with vertex colors per chunk
- `SaveWorld` — static persistence layer; one binary file per modified chunk plus the seed
- `PlayerController` — first-person movement, mining and building (new Input System)
- `HUD` — read-only UI Toolkit view over player state

## Notes

- Rendering uses a custom Shader Graph unlit shader; vertex colors carry the cube color and UVs drive the block edge outline
- The **Delete** keybind is a dev-only helper that wipes the save on disk
