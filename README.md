# Harvest Systems

Harvest Systems is a small top-down farming simulation built as a gameplay and systems-engineering portfolio project. Its scope is intentionally narrow: demonstrate clean domain modeling, Unity integration, data-driven configuration, testing, and an incremental path toward developer tooling.

## Phase 1 vertical slice

The first slice supports:

- WASD/arrow-key player movement
- contextual interaction with `E`
- tilling a soil plot
- planting a configured seed from inventory
- advancing one day at the blue day marker
- deterministic crop growth on each day transition
- harvesting a mature crop into inventory

Watering, selling, seasons, save/load, NPCs, and custom authoring tools are intentionally deferred.

## Open and run

1. Open the repository folder in Unity Hub using Unity `6000.6.0f1`.
2. Open `Assets/_Project/HarvestSystems/Scenes/Phase1.unity`.
3. Enter Play Mode.
4. Use WASD or the arrow keys to move and `E` to interact.

The scene contains one bootstrap component. It creates simple programmer-art geometry at runtime so the vertical slice stays reviewable without committing opaque binary art assets.

## Tests

Open **Window > General > Test Runner** and run the EditMode tests. A small PlayMode smoke test verifies that the Unity composition root creates a playable simulation. Most behavioral coverage belongs in EditMode because the tested domain assembly has no Unity dependency.

See [Architecture.md](Docs/Architecture.md) for the system boundaries, dependency direction, decisions, and milestone plan.
