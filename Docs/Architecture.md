# Harvest Systems architecture

## Design target

Harvest Systems is a compact systems portfolio, not a content-heavy farming game. The architecture therefore optimizes for readable rules, deterministic simulation, testability, and future tooling while keeping the first playable slice small.

The dependency rule is:

```text
Unity scene/input/views -> Unity composition and data adapters -> plain C# domain
                                                            |
ScriptableObject assets ------------------------------------+
```

The domain never references `UnityEngine`. Unity-facing code may translate authored assets into immutable domain definitions and render domain state. This makes game rules usable by EditMode tests and, later, a command-line simulation tool.

## Major system boundaries

| Area | Plain C# domain models/services | ScriptableObject configuration | Unity adapters/controllers |
|---|---|---|---|
| Time | `GameClock`, later `GameDate` and calendar rules | Later calendar/season configuration if needed | clock HUD, day-advance interaction |
| Farming | `SoilPlot`, `CropState`, `CropDefinition`, `FarmSimulation` | `CropDefinitionSO` | `SoilPlotView` |
| Items/inventory | `StableId`, `Inventory`, inventory change data | `ItemDefinitionSO` | inventory presenter/UI (later) |
| Interaction | action methods on the relevant domain system | optional interaction prompts (later) | `IInteractable`, `PlayerInteractor`, player controller |
| Economy | later pure pricing and transaction services | shop/catalog assets | shop UI/controller |
| NPC schedules | later schedule entries and resolver | NPC and schedule assets | navigation/animation adapter |
| Persistence | later snapshot DTOs and reconstruction services | schema/version policy if useful | file storage adapter |

`FarmSimulation` is the Phase 1 application facade. It coordinates the clock, crop catalog, plots, and inventory. It is not a global singleton and does not know about scenes, input, rendering, or ScriptableObjects.

## Proposed Unity folder structure

```text
Assets/_Project/HarvestSystems/
  Runtime/
    Domain/
      Common/
      Farming/
      Inventory/
      Time/
    Unity/
      Composition/
      Data/
      Farming/
      Interaction/
      Player/
      Presentation/
      Time/
  Data/
    Definitions/
      Crops/
      Items/
    Resources/
      Input/
  Scenes/
  Settings/               # URP renderer and pipeline assets
  Tests/
    EditMode/
    PlayMode/
  Editor/                 # added when the first custom tool is implemented
Docs/
```

Separate assembly definitions enforce the dependency boundary: `HarvestSystems.Domain` has `noEngineReferences`, while `HarvestSystems.Unity` references it. Tests reference the smallest relevant assembly.

## Dependencies and data flow

```text
PlayerInteractor
  -> IInteractable (SoilPlotView / DayAdvanceInteractable)
      -> HarvestGameController (scene composition boundary)
          -> FarmSimulation
              -> GameClock
              -> SoilPlot aggregates
              -> Inventory
              -> CropDefinition catalog

CropDefinitionSO -> ToDomain() -> CropDefinition
ItemDefinitionSO ----------------> stable item identifiers

GameClock.DayAdvanced -> FarmSimulation advances every SoilPlot
SoilPlot.Changed ------> SoilPlotView refreshes its presentation
Inventory.Changed -----> future inventory presenter
FarmSimulation.CropHarvested -> HUD / future analytics hooks
```

The `HarvestGameController` is intentionally a thin scene-level composition root and input-facing facade. It owns the lifetime of one simulation; tests can instantiate the simulation directly without it.

## Configuration versus runtime state

Configuration answers "what kind of thing is this?" Crop duration, yield, names, and item relationships are authored in ScriptableObjects. On startup, adapters validate and convert them into plain, read-only domain definitions.

Runtime state answers "what happened in this playthrough?" Current day, item quantities, tilled plots, planted crop IDs, and accumulated growth live in ordinary C# objects. Runtime state never mutates a ScriptableObject.

This conversion has a small amount of mapping code, but it prevents scene or asset lifetime from leaking into business rules. An alternative is to let rules consume ScriptableObjects directly. That is quicker at first, but it ties tests and offline simulation to Unity asset loading and makes accidental asset mutation more likely.

## Persistent identifiers

Every persistent definition and world entity uses a stable, human-readable ID such as `crop.carrot`, `item.carrot_seed`, or `plot.northwest`. Domain code wraps the string in `StableId` to centralize validation and equality. Save data will store the string value, never a Unity object reference, instance ID, display name, or array index.

Authored IDs should be treated as immutable after release. A future validation pass will detect blanks, duplicates, missing references, and renamed IDs; migrations or an alias table can handle intentional renames. GUIDs are an alternative and make collisions unlikely, but opaque GUIDs are harder to inspect in saves, tests, logs, and designer tools. Human-readable namespaced IDs are a better fit for this small project, provided validation is strict.

Plot IDs are authored/world IDs and crop/item IDs are definition IDs. They share the value-object representation but remain separated by the APIs that consume them. If cross-category ID mistakes become common, distinct typed wrappers are a reasonable later refactor.

## Save/load seam

Save/load is not implemented in Phase 1. Later, each stateful boundary will expose plain versioned snapshots, for example `ClockSnapshot`, `InventorySnapshot`, and `SoilPlotSnapshot`. A `GameStateSnapshot` will aggregate them. A persistence coordinator will:

1. ask systems for snapshots;
2. serialize DTOs through an injected storage/serializer adapter;
3. load and migrate DTOs;
4. reconstruct or restore domain objects through validated methods.

Systems will not call disk APIs and MonoBehaviours will not serialize authoritative gameplay state. This keeps persistence out of ordinary rule methods. The alternative—an `ISaveable` interface on every component—looks convenient but distributes file-format concerns across the codebase and couples saves to scene composition.

## Event architecture

Use local, typed C# events owned by the object that produces the event:

- `GameClock.DayAdvanced`
- `SoilPlot.Changed`
- `Inventory.Changed`
- `FarmSimulation.CropHarvested`

The composition root wires subscriptions and owns their lifetime. Events are notifications of completed facts; direct method calls remain preferable for commands that need a result, such as plant or harvest. This avoids a global event bus with hidden dependencies and hard-to-trace ordering.

If cross-cutting consumers later multiply (analytics, quests, audio, achievements), a small scoped domain-event dispatcher injected into one simulation could be justified. The tradeoff is looser coupling at the cost of indirection and ordering complexity. Phase 1 does not need it.

## Significant Phase 1 decisions

### Aggregate soil and crop state

`SoilPlot` owns its optional `CropState` and enforces till/plant/grow/harvest invariants. This prevents invalid combinations such as a crop on untilled soil. A data-oriented alternative would store soil and crop in separate arrays keyed by plot ID; that can be faster at very large scale, but four to hundreds of plots do not justify the synchronization cost.

### Clock emits a completed day transition

`GameClock.AdvanceDay` changes state and then emits `DayAdvanced`. `FarmSimulation` subscribes and advances crops. A central update loop that calls every system explicitly is simpler to step through and remains a reasonable option if event ordering becomes complex. Here the single meaningful transition demonstrates the intended event boundary without creating an event framework.

### Contextual interaction for the slice

One soil interaction tills an untilled plot, plants the selected Phase 1 crop on empty tilled soil, or harvests a mature crop. This minimizes UI/tool-selection work while validating the full system loop. A command/tool system will become worthwhile when watering and multiple tools arrive.

### Growth requires watering from the first Phase 2 slice

Crops gain one growth day only when their plot is watered. A day transition applies growth first and then resets moisture, so the rule has deterministic ordering and a dry day simply pauses growth. Soil owns `IsWatered` because moisture belongs to the plot even when no crop is present. An alternative is storing `LastWateredDay`; that becomes useful if weather, irrigation history, or multi-day moisture is introduced, but a boolean expresses the current rule more directly and serializes cleanly later.

### Programmer-art scene bootstrap

The checked-in Phase 1 scene contains a bootstrap that creates simple colored geometry. This makes the repository runnable before art/prefab workflows exist. The bootstrap is a temporary composition convenience; production scenes and prefabs can replace it without changing the domain.

### Supported modern Unity baseline

The project pins Unity 6.6 and the stable package versions that Unity 6.6 supports: URP 17.6 with its 2D Renderer, Input System 1.20, UI Toolkit, and Test Framework 1.8. URP uses its current Render Graph path. This removes dependencies on the deprecated Built-in Render Pipeline, legacy `UnityEngine.Input`, and runtime IMGUI while keeping those choices outside the domain layer.

The rendering alternative was Built-in, which would have reduced initial setup but is now on Unity's deprecation path and would force a later material/rendering migration. The input alternative was direct device polling; an authored action asset adds a small adapter and configuration file, but supports multiple devices, rebinding, and testable input boundaries. For the simple HUD, UI Toolkit costs more setup than `OnGUI`, but it matches the planned runtime debugging and editor-tool workflow. Preview packages are intentionally excluded: "new" here means the latest supported stable baseline, not experimental churn.

## Milestones

### Phase 1 — first vertical slice

- plain C# clock, inventory, soil, crop state, and simulation facade
- item/crop ScriptableObject definitions mapped to domain definitions
- player movement and proximity interaction
- till, plant, advance day, deterministic growth, and harvest
- simple runtime HUD and visuals
- EditMode rule tests and one PlayMode composition smoke test

### Phase 2 — farming depth and calendar

- watering and daily moisture reset (implemented as the first Phase 2 slice)
- explicit game time/date, season length, and time controls
- multiple crop definitions and seed selection
- crop growth/death policy defined from play requirements
- clearer interaction feedback and focused PlayMode integration coverage

### Phase 3 — inventory and economy

- inventory capacity/stack rules and usable UI
- shop catalog, buy/sell transactions, and currency ledger
- pure economy metrics such as seed cost, yield value, and profit per day
- transaction and economy tests

### Phase 4 — persistence and world state

- versioned snapshot DTOs, migrations, and JSON storage adapter
- stable-ID validation and missing-definition handling
- save/load integration tests and deterministic round trips

### Phase 5 — NPC schedule slice

- plain schedule data, validation, and deterministic resolver
- one NPC with time/location movement adapters
- EditMode schedule edge-case coverage

### Phase 6 — developer and editor tooling

- runtime inspector/commands for clock, plots, inventory, and event tracing
- Crop Balance Editor with validation, comparisons, and economic metrics
- NPC Schedule Editor with overlap and invalid-entry diagnostics

### Phase 7 — offline simulation and portfolio polish

- headless multi-day simulation using the same domain definitions
- seeded scenarios, statistical summaries, balance outlier detection, and report export
- profiling, documentation diagrams, CI test execution, demo capture, and a concise engineering case study

## Tooling-friendly choices

The Crop Balance Editor and offline simulator benefit from the same choices:

- crop rules are immutable plain data after authoring conversion;
- economics will be pure functions rather than UI calculations;
- stable IDs make reports and cross-asset joins reproducible;
- definitions can be validated in batches without entering Play Mode;
- simulation advances through explicit deterministic commands, not wall-clock time;
- runtime state does not modify source assets;
- assembly boundaries let an editor assembly reuse domain logic without depending on scene code.

The future editor should call the same metric functions as tests and simulation reports. It should not duplicate formulas in custom inspector code. For scale, an offline runner may load exported definition DTOs instead of Unity assets; the domain API remains the same.
