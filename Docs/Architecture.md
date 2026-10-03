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
| Time | `GameClock`, `GameDate`, `GameTime`, `SeasonDate` | Later calendar configuration asset if needed | clock HUD and time controls |
| Farming | `SoilPlot`, `CropState`, `CropDefinition`, `FarmSimulation` | `CropDefinitionSO` | `SoilPlotView` |
| Items/inventory | `StableId`, `ItemDefinition`, `Inventory`, inventory change data | `ItemDefinitionSO` | inventory presenter/UI (later) |
| Interaction | action methods on the relevant domain system | optional interaction prompts (later) | `IInteractable`, `PlayerInteractor`, player controller |
| Economy | `EconomyService`, `CurrencyWallet`, `SaleReceipt` | sell prices on item assets | sell-station interaction and HUD |
| NPC schedules | later schedule entries and resolver | NPC and schedule assets | navigation/animation adapter |
| Persistence | later snapshot DTOs and reconstruction services | schema/version policy if useful | file storage adapter |

`FarmSimulation` is the Phase 1 application facade. It coordinates the clock, crop catalog, plots, and inventory. It is not a global singleton and does not know about scenes, input, rendering, or ScriptableObjects.

## Proposed Unity folder structure

```text
Assets/_Project/HarvestSystems/
  Runtime/
    Domain/
      Common/
      Economy/
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
  -> IInteractable (SoilPlotView / time controls)
      -> HarvestGameController (scene composition boundary)
          -> FarmSimulation
              -> GameClock
              -> SoilPlot aggregates
              -> Inventory
              -> CropDefinition catalog
          -> EconomyService
              -> Inventory
              -> CurrencyWallet
              -> ItemDefinition catalog

CropDefinitionSO -> ToDomain() -> CropDefinition
ItemDefinitionSO ----------------> stable item identifiers

GameClock.DayAdvanced -> FarmSimulation advances every SoilPlot
SoilPlot.Changed ------> SoilPlotView refreshes its presentation
Inventory.Changed -----> future inventory presenter
FarmSimulation.CropHarvested -> HUD / future analytics hooks
EconomyService.SaleCompleted -> HUD / future analytics hooks
```

The `HarvestGameController` is intentionally a thin scene-level composition root and input-facing facade. It owns the lifetime of one simulation; tests can instantiate the simulation directly without it.

## Configuration versus runtime state

Configuration answers "what kind of thing is this?" Crop duration, yield, planting seasons, item prices, names, and item relationships are authored in ScriptableObjects. On startup, adapters validate and convert them into plain, read-only domain definitions.

Runtime state answers "what happened in this playthrough?" Current day, currency balance, item quantities, tilled plots, planted crop IDs, and accumulated growth live in ordinary C# objects. Runtime state never mutates a ScriptableObject.

This conversion has a small amount of mapping code, but it prevents scene or asset lifetime from leaking into business rules. An alternative is to let rules consume ScriptableObjects directly. That is quicker at first, but it ties tests and offline simulation to Unity asset loading and makes accidental asset mutation more likely.

## Persistent identifiers

Every persistent definition and world entity uses a stable, human-readable ID such as `crop.carrot`, `item.carrot_seed`, or `plot.northwest`. Domain code wraps the string in `StableId` to centralize validation and equality. Save data will store the string value, never a Unity object reference, instance ID, display name, or array index.

Authored IDs should be treated as immutable after release. A future validation pass will detect blanks, duplicates, missing references, and renamed IDs; migrations or an alias table can handle intentional renames. GUIDs are an alternative and make collisions unlikely, but opaque GUIDs are harder to inspect in saves, tests, logs, and designer tools. Human-readable namespaced IDs are a better fit for this small project, provided validation is strict.

Plot IDs are authored/world IDs and crop/item IDs are definition IDs. They share the value-object representation but remain separated by the APIs that consume them. If cross-category ID mistakes become common, distinct typed wrappers are a reasonable later refactor.

## Save/load seam

Save/load is not implemented yet. Later, each stateful boundary will expose plain versioned snapshots, for example `ClockSnapshot`, `WalletSnapshot`, `InventorySnapshot`, and `SoilPlotSnapshot`. A `GameStateSnapshot` will aggregate them. A persistence coordinator will:

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
- `EconomyService.SaleCompleted`

The composition root wires subscriptions and owns their lifetime. Events are notifications of completed facts; direct method calls remain preferable for commands that need a result, such as plant or harvest. This avoids a global event bus with hidden dependencies and hard-to-trace ordering.

If cross-cutting consumers later multiply (analytics, quests, audio, achievements), a small scoped domain-event dispatcher injected into one simulation could be justified. The tradeoff is looser coupling at the cost of indirection and ordering complexity. Phase 1 does not need it.

## Significant Phase 1 decisions

### Aggregate soil and crop state

`SoilPlot` owns its optional `CropState` and enforces till/plant/grow/harvest invariants. This prevents invalid combinations such as a crop on untilled soil. A data-oriented alternative would store soil and crop in separate arrays keyed by plot ID; that can be faster at very large scale, but four to hundreds of plots do not justify the synchronization cost.

### Clock is deterministic and command-driven

`GameClock` advances only through explicit commands, never `Update` or wall-clock time. It exposes value-type `GameDate` and `GameTime` views while retaining an absolute day counter for simple growth and future save data. Large time jumps publish every crossed `DayAdvanced` event, so crop simulation cannot silently skip days. `AdvanceDay` means "next day at 06:00," which makes the interaction predictable even when the player has already advanced time.

The calendar has four equal seasons and defaults to seven days per season. `SeasonDate` is derived from the same absolute day as `GameDate`, so there is one authoritative counter rather than synchronized calendar fields. A dedicated configurable calendar definition would support variable season lengths, festivals, or leap rules, but those requirements do not yet exist. A real-time Unity clock would make the demo feel more continuous, but it couples rule progression to frame time and complicates deterministic tests and offline simulation. A central simulation loop remains a reasonable alternative if event ordering grows beyond this small set of local subscriptions.

### Contextual interaction for the slice

One soil interaction tills an untilled plot, plants the selected crop on empty tilled soil, waters a growing crop, or harvests a mature crop. Seed selection is a Unity input concern that changes only the controller's selected crop ID; planting still goes through the same domain command and stable-ID catalog lookup. Carrot and turnip are separate authored assets but require no crop-specific code.

The slice cycles selection with one input action instead of implementing a hotbar. A hotbar is the likely long-term UI because it makes more inventory items directly addressable, but it also requires slot assignment, focus/navigation, and presentation rules. Cycling proves that data-driven crop selection works while keeping those concerns in the inventory/UI milestone.

### Planting returns an explicit outcome

`FarmSimulation.Plant` returns a `PlantResult` instead of a boolean so the Unity adapter can distinguish untilled soil, an occupied plot, missing seed, and an out-of-season crop without duplicating domain rules. Exceptions remain reserved for invalid identifiers and broken invariants. A result object carrying richer context could replace the enum if planting later needs costs, substitutions, or multiple validation messages; an enum is sufficient for the current command and is easy to test exhaustively.

Season rules currently limit planting only. Existing crops continue to grow when watered after a season transition. Killing crops at a boundary is a materially different player-loss policy, so it is deferred until that gameplay requirement and its feedback are designed.

### Growth requires watering from the first Phase 2 slice

Crops gain one growth day only when their plot is watered. A day transition applies growth first and then resets moisture, so the rule has deterministic ordering and a dry day simply pauses growth. Soil owns `IsWatered` because moisture belongs to the plot even when no crop is present. An alternative is storing `LastWateredDay`; that becomes useful if weather, irrigation history, or multi-day moisture is introduced, but a boolean expresses the current rule more directly and serializes cleanly later.

### Economy transactions are domain commands

`EconomyService` owns the sell transaction across `Inventory`, immutable `ItemDefinition` prices, and `CurrencyWallet`. It calculates and overflow-checks the complete sale before removing inventory, then returns a `SaleReceipt` suitable for UI, tests, analytics, and later simulation reports. Zero-price items are explicitly not sellable, allowing seeds to share the same item definition without a separate item hierarchy.

An alternative is placing `SellPrice` on `CropDefinition` and calculating sales in the MonoBehaviour. That removes one domain type, but it incorrectly treats harvested items as inseparable from crops and makes economy analysis depend on Unity scene code. Item-level pricing is the cleaner seam for a future balance editor. Buying, variable shop modifiers, and a transaction ledger remain deferred until those requirements are implemented.

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

### Phase 2 — farming depth and calendar (in progress)

- watering and daily moisture reset (implemented as the first Phase 2 slice)
- explicit deterministic game time/date and time controls (implemented)
- four-season calendar and seasonal planting rules (implemented)
- multiple crop definitions and seed selection (implemented)
- crop growth/death policy defined from play requirements
- clearer interaction feedback and focused PlayMode integration coverage

### Phase 3 — inventory and economy (in progress)

- inventory capacity/stack rules and usable UI
- sell transactions, data-driven prices, and currency wallet (implemented)
- shop catalog, buying, and transaction ledger
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
