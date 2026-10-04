# Harvest Systems

Harvest Systems is a small top-down farming simulation built as a gameplay and systems-engineering portfolio project. Its scope is intentionally narrow: demonstrate clean domain modeling, Unity integration, data-driven configuration, testing, and an incremental path toward developer tooling.

## Playable farming slice

The first slice supports:

- WASD/arrow-key player movement
- contextual interaction with `E`
- tilling a soil plot
- planting a configured seed from inventory
- cycling between carrot and turnip seeds with `Q` or the gamepad right shoulder
- watering planted crops
- advancing one hour at the orange time marker or starting the next day at the blue marker
- a deterministic clock with time-of-day and a compact year/day calendar
- four seven-day seasons with data-driven planting availability and clear rejection feedback
- deterministic growth for watered crops and daily moisture reset
- harvesting a mature crop into inventory
- selling all harvested produce at the gold station for data-driven prices
- buying one selected seed at the purple shop using earned currency

Inventory capacity, crop death, save/load, NPCs, and custom authoring tools are intentionally deferred.

## Technology baseline

- Unity `6000.6.0f1`
- Universal Render Pipeline `17.6.0` with the 2D Renderer and Render Graph enabled
- Input System `1.20.0` with an authored action asset for keyboard and gamepad
- UI Toolkit for runtime debug UI
- Unity Test Framework `1.8.0`

The project tracks the newest stable packages supported by its pinned Unity editor. Preview and experimental packages are added only when a concrete requirement justifies their maintenance cost.

## Open and run

1. Open the repository folder in Unity Hub using Unity `6000.6.0f1`.
2. Open `Assets/_Project/HarvestSystems/Scenes/Phase1.unity`.
3. Enter Play Mode.
4. Use WASD, arrow keys, or the left stick to move. Use `E` or the gamepad south button to interact. Use `Q` or the gamepad right shoulder to select the next seed. The orange marker advances one hour, the blue marker advances to 06:00 on the next day, the gold station sells harvested produce, and the purple shop buys one selected seed.

The scene contains one bootstrap component. It creates simple programmer-art geometry at runtime so the vertical slice stays reviewable without committing opaque binary art assets.

## Tests

Open **Window > General > Test Runner** and run the EditMode tests. A small PlayMode smoke test verifies that the Unity composition root creates a playable simulation. Most behavioral coverage belongs in EditMode because the tested domain assembly has no Unity dependency.

See [Architecture.md](Docs/Architecture.md) for the system boundaries, dependency direction, decisions, and milestone plan.
