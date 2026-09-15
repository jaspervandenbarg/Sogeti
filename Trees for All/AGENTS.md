# AGENTS.md

## Workload.
- Allowed to auto switch to Opus 5 with 1m context when deemed necessary.
- - Switch back to default after.

## Project
"Trees for All" is a VR tree-planting game for Sogeti. The game reflects
Sogeti's partnership with the Trees for All foundation. The player plants
and grows trees inside a limited play space.

- Engine: Unity 6000.3.24f1
- Render pipeline: URP
- XR stack: XR Interaction Toolkit 3.3.2, OpenXR, Meta XR SDK (com.meta.xr.sdk.all)
- Target device: Meta Quest 3, standalone
- Unity project folder: `Trees for All/`
- Repo host: GitHub
- Company name: `Sogeti`

Teleport, planting, the water meter, and the watering can all work and are
verified in Play Mode. See `## Progress` at the end of this file for the
current state.

- Work scene: `Assets/Scenes/DevelopmentScene.unity`.
- Player rig: `Assets/Prefabs/Player/PlayerRig.prefab`, a prefab variant of the
  XR Interaction Toolkit Starter Assets rig. Edit the variant, never the sample.
- World: `Assets/Prefabs/Terrain/World.prefab` holds the terrain tiles, the
  `TeleportationArea`, and the pond.
- The XR Interaction Toolkit Starter Assets sample is imported for reference
  under `Assets/Samples/`. Treat it as read-only.

## Testing constraint — no VR hardware
The developer has no Quest 3 or other headset. All testing happens on PC.

- Test interactions in Unity Play Mode using the XR Interaction Toolkit
  Device Simulator and/or the Meta XR Simulator, not on a physical device.
- Build interactions on standard OpenXR/Input System actions so the PC
  simulator and the Quest 3 build behave the same way.
- Avoid Quest-only APIs with no PC simulation path. If one is needed,
  isolate it behind an interface so the rest of the game stays testable
  without a headset.
- When a design decision affects testability (hand tracking vs.
  controllers, passthrough, haptics, headset-only sensors), state the
  tradeoff and how it gets verified without hardware.
- Treat a Quest 3 build as unverified beyond "it builds and installs"
  until the developer or someone else confirms it on-device.

## Assignment rules (fixed — do not relax without asking the user)
Source: `Opdrachtomschrijving XR developer Sogeti.pdf` (repo root).

1. The game runs in VR.
2. The game runs standalone on a Quest 3.
3. The player plants 2 or more seed types.
4. The plantable area or time is limited. The game has no infinite space.
5. A seed's planting spot depends on nearby factors (same-type or other-type
   seeds/sprouts/trees, obstacles). The candidate designs the limits and
   how the game shows them.
6. Each seed has 3 growth stages. The player must act to advance a stage
   (for example, water the seed for X seconds). A missed or late action
   kills the seed/sprout/tree.
7. The goal is the highest possible score. The candidate designs the
   scoring system.
8. Optional: add one small bonus feature (for example, weather). This adds
   little weight to the grade.

## Design freedom
The candidate decides:
- How planting limits work and how the game displays them.
- The growth-stage care action and its timing.
- The scoring system.
- The optional bonus feature.

## Constraints and grading notes
- A grid system is not required, but it is allowed.
- Placeholder art is allowed. A reasonable look is a plus.
- The target player has little to moderate game experience. Favor clear
  onboarding and simple controls over complex mechanics.
- Default to comfort-friendly VR locomotion (teleport and/or snap turn).
  Avoid forced smooth camera rotation/movement unless the user asks for it,
  since novice players are more prone to motion sickness.
- State each design decision as a short code comment near the relevant
  code. Keep the assignment's own intent (show design reasoning) but
  follow this repo's comment style: one line, state the why, not the what.
- List any external asset source in `README.md` when the asset is not part
  of the default Unity template or an installed package. That file sits at
  the git root, one level above this Unity folder. See `## Repo layout`.

## Layer conventions
Unity physics layers decide what a gameplay ray can hit. XRI interaction layers
are a separate system. They gate interactor and interactable matching only. Do
not mix the two.

| Layer | Meaning | Blocks teleport | Blocks planting |
| --- | --- | --- | --- |
| 2 `Ignore Raycast` | Non-interactive world UI. The water meter and the can level bar. Also the layer a dead plant moves to. | no | no |
| 3 `Terrain` | The only surface to stand on and plant on. Carries the `TeleportationArea`. | no, it is the target | no |
| 5 `UI` | Interactive uGUI. The tool menu panel and the seed readout. | only with a collider | only with a collider |
| 4 `Water` | Visual water and the watering can refill trigger. Colliders here are triggers. | no, the pond body blocks instead | yes |
| 6 `PlayerObstacle` | Blocks the player only. Pond body, boundary walls. | yes | no |
| 7 `UniversalObstacle` | Blocks the player and placement. Rocks, scenery, props. | yes | yes |

Rules:
- Teleport works by occlusion, not exclusion. Obstacle layers stay inside the
  teleport raycast mask. A closer collider with no interactable blocks the
  terrain behind it. Remove a layer from the mask and the ray passes through it.
- Teleport raycast mask on both Teleport Interactors: `Default`, `Terrain`,
  `UI`, `PlayerObstacle`, `UniversalObstacle`.
- Obstacle colliders must never be triggers. The teleport ray ignores triggers.
- An obstacle collider top sits at least 0.2 m above the terrain.
- Obstacles carry no XRI interactable component.
- Planting is an allow list on `Terrain`. Refuse any other hit. This covers
  water, rocks, and the pond body with one rule.
- The teleport raycast mask and the planting `occluderMask` cover the same layers.
  `occluderMask` is 233 and the teleport mask is 2147483881, which is 233 plus the
  XR Simulation bit. No layer blocks the player and passes the planting ray.
- The player camera culls layer 8 `Overlay UI`. The scene overrides the culling
  mask to 4294967039. Only an `OVROverlayCanvas` draws there. World UI the camera
  must draw belongs on layer 2 or layer 5.
- **A clickable canvas belongs on layer 5, never on layer 2.** The Near-Far
  Interactor hands its own mask to the UI raycast, and
  `Left_NearFarInteractor.prefab` sets `m_RaycastMask.m_Bits: 2147483689`. That
  covers layers 0, 3, 5 and 31 only. A panel on layer 2 never receives a hit.
- Layer 5 sits inside `occluderMask` and the teleport mask, but a Canvas carries
  no collider. Both masks gate colliders, so a panel there blocks no ray.
- Pass `QueryTriggerInteraction.Ignore` in every gameplay raycast.
  `Physics.queriesHitTriggers` is on, and the pond refill trigger overlaps the
  ground.

## Repo layout
The git root is one level above this Unity project.

```
Sogeti/                 <- git root, .gitignore, README.md, assignment PDF
└─ Trees for All/       <- Unity project, this AGENTS.md, Assets/, Packages/
```

`README.md` and `Opdrachtomschrijving XR developer Sogeti.pdf` are at the git
root, not next to `Assets/`.

## Repo conventions
- Put new gameplay scripts under `/Assets/Scripts/`.
- Do not commit `Library/`, `Temp/`, `Logs/`, `UserSettings/`, or other
  paths already listed in `.gitignore`.
- Do not commit secrets or personal API keys. Use `.env.example`-style
  placeholders if a script needs a key.

## Coding style
- Use namespaces e.g. `Assets/Scripts/Interaction` -> Sogeti.Interaction
- Use descriptive naming
- Small methods
- Single responsibility
- Explicit dependencies
- Readonly where appropriate
- Private by default
- Prefer set/get to get access to private variables over making the variable public.
- Prefer Events/Scriptable objects instead of direct references when possible.
- Prefer reusability and modularity.
- Define each seed type as a ScriptableObject, not an enum or hardcoded
  list, so new seed types don't need code changes.
- Avoid per-frame allocations and heavy work in `Update()`. The Quest 3 is
  mobile-class hardware; profile before adding costly logic to hot paths.

## Workflow
- Ask before installing packages.
- Verify features in Play Mode with the XR Device Simulator or Meta XR
  Simulator before calling them done. Do not claim a feature "works" based
  only on a successful Quest 3 build.
- Call out anything that cannot be verified without a physical headset
  (e.g. real haptics feel, real hand tracking) so the developer knows what
  still needs on-device confirmation.

## Comment / Documentation style
- Write comments that explain "why". A few high level comments explaining the purpose of classes or methods is very helpful. Comments explaining tricky code are also helpful.
- Avoid comments that are redundant with the code. Do not comment before each line of code explaining what it does unless there is something that is not obvious going on.
- Don't mention yourself or agents.md in commit messages

## Things to avoid
- Don't add third-party assets without asking.
- Giant MonoBehaviours.
- God classes.
- Static global state. This means mutable statics and singletons. A static pure
  function is fine, and the rules layer is built from them.
- Quest-only APIs with no PC simulation path, unless isolated behind an interface (see testing constraint above).

## High level architecture
Keep this current. It saves a rediscovery pass at the start of the next session.

### Assemblies
- `Sogeti.Planting` (`Assets/Scripts/Planting/`) holds the planting data and
  rules. It references no package. The EditMode tests force it.
- `Sogeti.Watering` (`Assets/Scripts/Watering/`) holds the pour rules, for the
  same reason. `PourFlow` and `WaterTank` need no scene and no package.
- `Sogeti.Game` (`Assets/Scripts/Game/`) holds the round rules: the clock, the
  score, the clock format and the phase machine. It references `Sogeti.Planting`
  alone, because `ScoreTally` keys its per-seed rows by `SeedDefinition`. Both
  assemblies are package-free, so the EditMode tests still run with no headset.
  Adding a package here would pull it into the test assembly for no gain.
- `Sogeti.Atoms` (`Assets/Scripts/Atoms/`) holds the hand-written Unity Atoms
  types the base package does not ship: the `SeedDefinition` and `GamePhase`
  families, plus `ScoreTallyEvent`. It references Unity Atoms, `Sogeti.Planting`
  and `Sogeti.Game`. The reference runs one way, so the three assemblies above
  stay package-free.
- `Sogeti.Atoms.Editor` (`Assets/Scripts/Atoms/Editor/`) holds one inspector per
  custom Variable type. Unity Atoms ships an editor for every type it generates,
  and a hand-written type without one gets Unity's default inspector, which
  edits the wrong field. See the wiring note in `### Atoms decoupling step`.
- All other gameplay code compiles into `Assembly-CSharp`. That assembly
  auto-references `Sogeti.Planting`, `Sogeti.Game` and `Sogeti.Atoms`, so glue
  code needs no new asmdef.
- Code that needs a package (Input System, TextMesh Pro, XRI) belongs in
  `Assembly-CSharp`. Adding those references to `Sogeti.Planting` would pull
  packages into the test assembly for no gain.

### The four layers of a gameplay feature
1. **Data.** `SeedDefinition`, `GrowthStage`. ScriptableObject assets, no behaviour.
2. **Rules.** `PlantingRules`, `GrowthPoints`, `PlantGrowth`. Pure C#. No scene,
   no physics, no `Time.deltaTime`. This layer carries the tests.
3. **Queries.** `PlantPlacementQuery`. Runs the raycast and the overlap test,
   then calls layer 2. Plain C#, but it needs a live scene.
4. **Scene.** `Plant`, `SeedPlanter`, `PlantingPreview`. MonoBehaviours only.

Put a new rule in layer 2, where a test can reach it without a headset.

### Communication
ScriptableObjects hold data. Unity Atoms assets carry shared state between
systems that must not know each other. Plain C# events carry per-instance state.

**Atoms are the broadcast channel, not the store.** `ScoreTally` still owns the
score. `GameCountdown` still owns the clock. `GameSession` still owns the phase
machine. Each writes its value out to an Atom, and consumers read the Atom only.
A missing Atom must cost the broadcast, never the system itself, so null-guard
every Atom field.

One exception: `SelectedSeed`. The Variable owns the seed because every
candidate owner gets deactivated. `RightHandToolSwitch` calls `SetActive` on the
tool objects, so `SeedPlanter` sleeps while the menu is open.

Rules that later steps must not re-derive:
- Keep Atoms in `Assembly-CSharp` and `Sogeti.Atoms`. `Sogeti.Game`,
  `Sogeti.Planting` and `Sogeti.Watering` stay package-free, so the EditMode
  tests run with no headset.
- **A value shown as text belongs in an `IntVariable`.** Atoms drops a write
  that changes nothing, so whole seconds throttle the TMP rebuild for free. A
  `FloatVariable` of raw seconds fires every frame, 90 times a second on Quest 3.
- **Commands use `VoidEvent` with the no-argument `Register(Action)`.** The
  `Action<T>` overload replays the last value to each new subscriber, which
  would reload the scene every time something registers for a restart.
- **Give every Variable asset a Changed Event sub-asset.** `SetValue` raises
  nothing while that field is null, so a write made before the first listener
  registers is lost.
- **Force the opening value out in `Start`**, with `SetValue(value,
  forceEvent: true)`. A plain write of the Initial Value changes nothing, leaves
  the replay buffer empty, and a consumer enabled later reads a blank channel.
  `GameSession` does this for `Score`, `SecondsRemaining` and `Phase`.
- **A Variable with no continuous producer needs the reverse fix: the consumer
  reads `.Value` directly on enable, alongside registering for `.Changed`.**
  `SelectedSeed` has no owner that writes its starting value — nothing calls
  `StartRound` before the first pick — so nothing is ever in the replay buffer
  until the player chooses a seed. `SelectedSeedReadout`,
  `WateringCanLevelMeter` and `HudMenuLink` all read their Atom's `.Value`
  once in `OnEnable`, in addition to registering, for exactly this reason. The
  bug from skipping this: the seed readout kept whatever the prefab shipped
  with, until the first menu pick, because `Register` only replays a value
  that was actually raised, and the Variable's own Initial Value never raises.
- **A listener must not live on the GameObject it toggles.** `HudMenuLink` hides
  the HUD root, so it lives elsewhere. `SetActive(false)` would unregister it,
  and the close write would find no listener to bring the HUD back.

Per-plant state stays on plain C# events. A shared asset cannot carry
per-instance state.
- `SeedPlanter` raises `PlantPlaced` and `PlantRefused`. `Plant` raises
  `Planted`, `StageAdvanced` and `Died`, and each event carries its point award.
  The scoring step subscribes to those. It must never poll every plant.

## Steps for the game (original list)
- The user can move using teleport on Terrain layers
- - Obstacle layers can not be teleported on
- A UI hovers in front of the user on start explaining the goal and controls
- The user can open a UI menu where he can pick tools to use or seeds to plant
- The user can plant trees on Terrain layers
- - Trees can be planted using the pointer
- - The user cannot plant trees on any other layers
- - Trees cannot be planted near other types of trees
- Trees can grow to different stage
- - Trees show a water meter that slowly drops
- - water meter starts at 50% for each growing stage except the last
- - The water meter dissapears when the tree is fully grown
- - The tree dies when the water meter is empty
- - The tree grows to the next stage when the water meter is full
- Users can water the trees using a watering can
- - Users tilt the watering can over/next to the trees to water the trees.
- - Optional: the watering can empties when pouring, refill using a pond or tap.
- The user sees which seed is selected
- - A visual on the hand or in the menu. Today the first seed is preselected and
- - nothing names it.
- The user gets points for each tree stage reached per tree
- - the longer it takes to reach a stage the fewer points the user receives.
- - later stages provide more points e.g. seed -> +10 points, sprout -> +20 points, fully grown tree +40
- Game has a timer after which the game ends
- - Timer time to be determined
- - Show user score
- - Show Restart button

## Next Step Claude progress (overwrite when finished)
Backlog complete. Polish only. Every step in `### Build order` is done and
verified in Play Mode, including the intro UI, which the start panel covers.
`## How to play` in `README.md` at the git root is current.

The HUD wrist binding is code-complete but not yet wired in the scene. See
`### HUD wrist binding, code done — needs scene wiring`: reparent `GameHud`
under `Left Controller` and wire `HudMenuLink.hudRoot`, then verify in Play
Mode and update that section's heading to "done".

The Atoms decoupling step is code-complete and needs Inspector wiring. See
`### Atoms decoupling step, code done — needs Inspector wiring`. Create the
three custom Atom assets, fill every new Atom field, then drop the scene
overrides the prefabs no longer need.

## Progress
Update this section at the end of every step. Keep one line per step.

### Build order
The list above is the backlog, not the build order. Two changes:
- Build the seed and tree data model before any UI. Steps 3 to 7 all read it.
- Build the intro UI last. It must describe the final goal and controls.

Order: teleport, seed data model, planting, growth stages, tool menu,
watering can, scoring, timer and end screen, intro UI.

### Status
- **Teleport: done, verified in Play Mode, left hand only.** The right hand
  plants now.
- **Seed and tree data model: done.** See `### Seed data model step, done`.
- **Planting: done, verified in Play Mode.** See `### Planting step, done`.
- **Growth stages: done, verified in Play Mode.** The stage visual, the death
  visual and the water meter all passed. See `### Water meter step, built`.
- **Watering can: done, wired into the rig, verified in Play Mode.** See
  `### Watering can step, built`.
- **Tool and seed menu: done, verified in Play Mode.** See
  `### Tool and seed menu step, done`.
- **Scoring: done, verified in Play Mode.** See
  `### Timer and end screen step, done`.
- **Timer and end screen: done, verified in Play Mode.** Same section.
- **Intro UI: done, verified in Play Mode.** The start panel covers it.
- **Atoms decoupling: code done, needs Inspector wiring.** See
  `### Atoms decoupling step, code done — needs Inspector wiring`.

### Atoms decoupling step, code done — needs Inspector wiring
Shared round state and hand state moved onto Unity Atoms assets. The rules are
in `### Communication`. Read those before touching an Atom.

Why: prefabs could not ship complete. `GameHud.prefab` stored `session:
{fileID: 0}` and a scene override patched it. `GamePanel.prefab` needed four.
Components also failed alone. A prefab that references an asset needs neither.

What each consumer lost:
- `GameHud` lost `GameSession`. It reads `Score` and `SecondsRemaining`.
- `GamePanelController` lost `session`, `gate` and `restarter`. It reads `Phase`
  and `RoundEnded`, and raises `StartRoundRequested` and `RestartRequested`.
- `PlayerActionGate` reads `Phase` and drives itself. The panel no longer calls
  `SetPlayerActionsEnabled`.
- `GameRestarter` listens for `RestartRequested`.
- `SeedPlanter`, `ToolMenuController` and `SelectedSeedReadout` share
  `SelectedSeed`. `SeedPlanter.SelectSeed` and `SeedChanged` are gone.
- `WateringCanLevelMeter` lost `WateringCan` and its per-frame poll.
- `HudMenuLink` lost `ToolMenuController`. `OpenChanged` is gone.

`ScoreCollector` keeps its `GameSession` and `SeedPlanter` references on
purpose. Per-plant events are not shared state, and its single planting hook is
what stops every seed scoring twice.

The base-type Atom assets exist under `Assets/ScriptableObjects/UnityAtoms/`
in `Round/` and `Hand/`. Each Variable already carries its Changed sub-asset.

Inspector wiring still needed (not done by editing files alone):
1. Create the three custom assets. Right-click in `Round/`, then
   `Create > Sogeti > Trees for All > Atoms`: a `Game Phase` Variable named
   `Phase`, and a `Score Tally` Event named `RoundEnded`. In `Hand/`, a
   `Seed Definition` Variable named `SelectedSeed`.
2. On `Phase` and `SelectedSeed`, add a Changed Event. Leave replay buffer 1.
   `RoundEnded` needs no Changed Event.
3. Set **Initial Value**, never Value: `Phase` to `Ready`, `SelectedSeed` to the
   seed the hand starts with. `SeedCatalog` no longer decides that.
   `AtomVariable.OnEnable` copies Initial Value over Value on entering Play
   Mode, so a seed set in Value alone is wiped on the first frame. Every
   Variable type needs an editor in `Assets/Scripts/Atoms/Editor/`, which locks
   Value outside Play Mode and makes that mistake impossible. Add one whenever
   a new Variable type is written.
4. Fill every new Atom field on `GameSession`, `GamePanelController`,
   `PlayerActionGate`, `GameRestarter`, `GameHud`, `SeedPlanter`,
   `ToolMenuController`, `SelectedSeedReadout`, `WateringCan`,
   `WateringCanLevelMeter` and `HudMenuLink`.
5. Drop the scene overrides the prefabs no longer need: `session` on `GameHud`,
   and `session`, `gate` and `restarter` on `GamePanel`. Keep `playerHead`.

### HUD wrist binding, code done — needs scene wiring
The HUD moved from a head-follow panel to the left wrist, sharing `ToolMenu`'s
spot: opening the tool menu hides the HUD, closing it shows the HUD again, so
exactly one is ever visible. `HudFollow` is deleted. `GameHud.prefab`'s root
now carries a fixed wrist offset, position `(0, 0.1, 0.05)`, rotation `-10`
about X — the same numbers as `ToolMenu`'s instance override in
`PlayerRig.prefab`, so the two panels sit in the same spot.

New type: `HudMenuLink` (`Assets/Scripts/UI/ToolMenu/`), added to
`ToolMenu.prefab`'s root next to `ToolMenuController`, not to `GameHud`'s
root.

Rule later steps must not re-derive:
- **A listener that toggles a GameObject must not live on that GameObject.**
  `HudMenuLink` reads the `ToolMenuOpen` Atom and sets `hudRoot.SetActive(!open)`.
  Putting that subscription on `GameHud`'s own root would unregister on the very
  `SetActive(false)` it triggers, so the close write would have no listener left
  to show the HUD again. Any object the gate leaves alone hosts it safely.
  `ToolMenu`'s root is one: only `panelRoot`, a child, and
  `ToolMenuController.enabled` ever switch off.

The Atoms step later replaced `ToolMenuController.OpenChanged` with the
`ToolMenuOpen` Variable, so `HudMenuLink` no longer needs to sit beside the
controller. It still must not sit on the HUD root.

Scene wiring still needed (not done by editing files alone):
1. In `DevelopmentScene.unity`, reparent the `GameHud` instance from
   `PlayerRig > Camera Offset` to `PlayerRig > ... > Left Controller`, the
   same parent as `ToolMenu`. The prefab's own local offset already matches
   `ToolMenu`'s slot.
2. On the scene's `ToolMenu` instance, drag the scene's `GameHud` object into
   the new `HudMenuLink.Hud Root` field.
3. `PlayerActionGate.objectsToDeactivate`'s `GameHud` entry survives the
   reparent untouched — it is an object reference, not a hierarchy path.

### Timer and end screen step, done
A round clock, a score, a head HUD, and one world panel that serves both the
start screen and the game over screen. Code in `Assets/Scripts/Game/`,
`Assets/Scripts/Session/`, `Assets/Scripts/UI/Hud/` and
`Assets/Scripts/UI/Panels/`.

Design answers from the user:
- **A head-locked HUD with a soft follow.** Up and left of centre, 1.2 m deep.
  A rigid head lock puts text at the lens edge, where it blurs and strains the
  eyes. Superseded later by a wrist-bound HUD — see
  `### HUD wrist binding, code done — needs scene wiring`.
- **The seed readout stays on the right hand.** The HUD carries global state,
  the hand carries hand state. `SelectedSeedReadout` is a child of `SeedPlanter`,
  so it already leaves with the can. A HUD copy would re-implement that.
- **The round is a serialized field, default 180 s.**
- **Restart reloads the scene.** Fresh by construction, so no reset list can rot.
- **Restart returns to the start panel.** A reload cannot carry a "skip the
  intro" flag without persistence, and the user asked for no saved state.
  `GameSession.autoStartOnLoad` flips this if the extra press annoys.
- **The start panel leads with the goal**, then `Controls`, then `Play` last.
- **The game over panel lists one row per seed type**, with grown and planted
  counts.

New types:
- `GameCountdown`, `ScoreTally`, `SeedTally`, `CountdownDisplay`, `GamePhase`,
  `GamePhaseRules`. All pure, all in `Sogeti.Game`.
- `GameSession`, `ScoreCollector`, `PlayerActionGate`, `GameRestarter`.
- `GameHud`, `GamePanelView`, `GamePanelController`, `WorldPanelPlacer`.

Rules that later steps must not re-derive:
- **`ControllerInputActionManager.enabled = false` is a clean teleport gate.**
  Its `OnDisable` only calls `TeardownInteractorEvents()`, so `OnStartTeleport`
  can never fire. Its `OnEnable` forces the Teleport Interactor GameObject
  inactive, so re-enabling self-heals. Do not restore that interactor by hand.
- **The gate still deactivates the left Teleport Interactor.** A player holding
  the teleport stick as the clock expires leaves a live arc, because the
  postponed deactivate in `Update` never runs on a disabled manager.
- **On the resume path, activate GameObjects before enabling Behaviours.**
  `ControllerInputActionManager.OnEnable` deactivates the teleport interactor.
  The reverse order revives a live interactor with nothing driving it.
- **`ToolMenuController.Close()` un-stows the hand.** `SetOpen` calls
  `SetToolsStowed(open)`. So the gate closes the menu first, then stows. The
  reverse order hands the player a live planter behind the game over panel.
- **`SeedPlanter.PlantPlaced` is the only planting hook.** `Plant.Planted`
  carries the same award and fires first, inside `Initialize`. Reading both
  counts every seed twice.
- **The HUD sits on layer 2 and carries no raycaster.** A raycaster 1.2 m from
  the eyes would eat the right hand ray. The panel is clickable, so it sits on
  layer 5 with both raycasters. See `## Layer conventions`.
- **`GameSession` raises the clock on whole seconds only.** A per-frame raise
  rebuilds the TMP mesh 90 times a second for a clock with no decimals.
- `GameSession.StartRound` resets the tally before it raises `PhaseChanged`.
  That event un-hides the HUD, and a HUD showing the old score for one frame
  reads as a failed restart.
- The panel places itself one frame after the request. The head pose is not
  settled on the frame a scene loads. `PlantWaterMeter` skips a frame for the
  same reason.
- A world-space Canvas faces **away** from the viewer.
  `WorldUILookAtPlayer.rotationOffset` stays `0,180,0` on the panel, or the
  text mirrors.
- Plants keep growing after the round ends, on purpose. It looks alive behind
  the panel. `ScoreCollector` drops every award unless the phase is `Playing`.

Tests: `GameCountdownTests`, `ScoreTallyTests`, `CountdownDisplayTests` and
`GamePhaseRulesTests`, 70 new cases. **241 total, all green**, run in batch mode
against Unity 6000.3.24f1.

**Wired into the rig and verified in Play Mode.** See
`### Timer step, scene wiring` for the field list a rebuilt rig would lose.

### Timer step, scene wiring
Facts about the rig and the scene. A rebuilt rig or a re-dragged prefab
instance loses all of these.

1. `GameSession` sits on its own root GameObject, named `GameSession`, next to
   `ScoreCollector`, `PlayerActionGate`, `GameRestarter` and
   `GamePanelController`. All five live on that one GameObject.
2. **`GamePanel.prefab` starts active. `GameHud.prefab` starts inactive.**
   `GamePanelController` and `GamePanelView` live on the `GamePanel` root, and
   `GamePanelView.Awake()` already hides its own child `Panel` canvas. A root
   set inactive in the Inspector never runs that `Awake`, never subscribes to
   `GameSession.PhaseChanged`, and so can never show itself again. `GameHud`
   has no such self-managed visibility: its root is the thing
   `PlayerActionGate.objectsToDeactivate` turns on and off directly, so it
   alone starts inactive. Getting this backwards was the one bug scene wiring
   found: an inactive `GamePanel` means the start screen never appears and the
   game never starts.
3. `GameHud.prefab` sits under `PlayerRig > ... > Left Controller`, the same
   wrist anchor as `ToolMenu`, at the same local offset. See
   `### HUD wrist binding, code done — needs scene wiring`.
4. `PlayerActionGate.behavioursToSuspend` holds three: `Left Controller >
   Controller Input Action Manager`, `Locomotion > Move > Dynamic Move
   Provider`, `Locomotion > Turn > Snap Turn Provider`.
5. `PlayerActionGate.objectsToDeactivate` holds two: `Left Controller >
   Teleport Interactor` and the `GameHud` root.
6. `PlayerActionGate.toolSwitch` is the `RightHandToolSwitch` on `Right
   Controller`. `.toolMenu` is the `ToolMenuController` on `Left Controller >
   ToolMenu`.
7. `GamePanelController.playerHead` points at `PlayerRig > Camera Offset >
   Main Camera`.
8. `ScoreCollector.planter` points at the `SeedPlanter` on `Right Controller`,
   the one source `PlayerActionGate` never has to touch directly, because
   `RightHandToolSwitch.SetToolsStowed` already empties that hand.
9. `GameSession.roundSeconds` is `180`. `autoStartOnLoad` stays unticked, so a
   restart returns to the start panel rather than skipping it.

Tests: `Assets/Tests/EditMode/`, unaffected by scene wiring. **241 total**,
still green after the wiring pass.

### Timer and end screen prefabs, built
`Assets/Prefabs/UI/GamePanel.prefab` and `Assets/Prefabs/UI/GameHud.prefab`,
hand-authored YAML like `ToolMenu.prefab`. Both import clean and every
reference inside them resolves.

- `GamePanel` is layer 5. The root carries `WorldPanelPlacer`,
  `WorldUILookAtPlayer`, `GamePanelView` and `GamePanelController`. The child
  `Panel` holds the world Canvas, both raycasters and the three groups.
  640 x 560 at scale 0.0015, so it reads about 25 degrees wide at 2 m.
- `GameHud` is layer 2 and carries no raycaster. The root carries `GameHud`
  alone; its Transform holds the fixed wrist offset instead of a follow
  component. The child `Panel` is 280 x 100 at scale 0.001.
- The game over rows reuse `ToolMenuEntry.prefab` in a `GridLayoutGroup`, two
  columns of 260 x 96. A narrower cell clips `Broadleaf 1 grown / 2 planted`.

Rules that later steps must not re-derive:
- **`GamePanelView` sits on the always-active root, not on `panelRoot`.** Its
  `Awake` hides `panelRoot`. A view living on that hidden canvas would run
  `Awake` on the frame `ShowStart` activates it, and hide itself again.
  `ToolMenuController` sits on its root for the same reason.
- **Each screen is a full-stretch group, and the action button anchors to the
  bottom edge.** Play, Back and Play again then land in the same spot, so the
  panel height can change without moving the primary action.

Scene fields left empty on purpose: `GamePanelController.session`, `.gate`,
`.restarter`, `.playerHead`, and `GameHud.session`.

### Tool and seed menu step, done
The player opens a left wrist panel, picks a seed or the watering can, and the
readout names the pick at the right hand. Code in
`Assets/Scripts/UI/ToolMenu/`, `Assets/Scripts/UI/WorldUI/`, and
`Assets/Editor/SeedIconBaker.cs`.

Design answers from the user:
- **A left wrist panel.** The left secondary button (Y, key `N`) opens and
  closes it. The Quest system owns the left menu button, so that stays free.
- **The right hand ray picks an entry.** The open panel stows every right hand
  tool, so the right trigger drives UI only.
- **Two labelled rows.** A Tools row and a Seeds row. Each entry shows an icon
  and a name.
- **A seed readout on the right hand, no tool readout.** The player already
  sees the planter or the can.
- **The panel stays open until Y.** A pick does not close it.
- **One mark at a time.** The hand holds one tool, so the panel lights one
  entry. The seed row goes dark while the can is out. `SelectedSeed` survives,
  so returning to the planter restores the same seed.
- **Jump is off.** The menu removed the B button, which left B on Jump. The
  game stays teleport only, for comfort.

New types:
- `SeedCatalog`, a ScriptableObject listing the four seeds. `SeedPlanter` and
  the menu both read it, so no second seed list can drift from the spacing
  rules.
- `RightHandToolSwitch.SetToolsStowed(bool)`. `ActiveIndex` survives a stow, so
  closing the menu restores the same tool.
- `ToolMenuEntry`, `SeedMenuRow`, `ToolMenuRow`, `ToolMenuController`.
- `SelectedSeedReadout`.
- `SeedIconBaker`, menu item `Trees for All/Bake Seed Icons`.

Rules that later steps must not re-derive:
- **`PreviewRenderUtility.Render()` defaults to the built-in pipeline.** The
  first argument is `allowScriptableRenderPipeline`, default false. URP
  SubShaders tag `"RenderPipeline" = "UniversalPipeline"`, so nothing matches
  and Unity draws the magenta error shader over correct geometry.
  `SeedIconBaker` passes `true`. Never drop that argument.
- **Frame an icon against mesh renderers only.** `WateringCanPrefab` carries a
  Particle System, and an idle `ParticleSystemRenderer` reports bounds far
  larger than its emitter. `SeedIconBaker.CollectMeshRenderers` filters and
  disables the rest.
- **The tool row entries carry authored art, the seed row does not.**
  `SeedMenuRow` builds its entries from the catalog and calls `SetContent`.
  `ToolMenuRow` reuses entries placed in `ToolMenu.prefab`, so a tool icon is a
  prefab instance override on the entry's `Icon` image, not a code assignment.
- **The Tools row holds the watering can alone.** A seed pick already switches
  the hand to the planter, so a Plant button adds nothing. `ToolMenuRow.entries`
  maps an array position to a tool index, so slot 0 stays empty and the can
  sits at slot 1. Keep that empty slot, or the can selects the planter.
- The left hand also has UI interaction on. Untick `m_EnableUIInteraction` on
  the left interactor in `PlayerRig.prefab`, or its ray hits its own panel.
- The rows build on first activation, so `ToolMenuController` refreshes the
  highlight inside `Open()`.
- Use a plain parented transform for the panel, not `LazyFollow`. `HandMenu`
  needs a palm-up pose, which the PC simulator cannot hold, and a tween would
  blur a pass or fail check.
- The scene `EventSystem` needs `XR UI Input Module`, not `Input System UI
  Input Module`, with empty action fields so the mouse fallback stays on.
  Without it, `m_EnableUIInteraction` does nothing and the panel reads as dead
  with no error.

Tests: EditMode suite, 160 total.

**Verified in Play Mode.** The player picks a seed from the panel, the ghost
changes to that seed, and the readout names it at the right hand.

### Watering can step, built
The player tilts a can over a plant and the meter rises. Code in
`Assets/Scripts/Watering/`, `Assets/Scripts/Interaction/` and
`Assets/Scripts/UI/WorldUI/`.

Design answers from the user:
- **The right hand carries every tool.** Planting, watering and refilling. The
  left hand keeps teleport only.
- **The right primary button swaps the tool.** Seeds or can, never both.
- **Limited charge.** About 10 seconds of full pour. Dip the spout in the pond.
- **Tilt pours.** `requireTriggerToPour` on the can prefab adds the trigger, so
  the developer can compare both. Default is tilt alone.

New types:
- `PourFlow`, static and pure. Tilt degrees to a 0 to 1 throttle.
- `WaterTank`, a plain class. Charge measured in seconds of full pour.
- `RightHandToolSwitch`, the scene component that keeps one tool active.
- `WateringCan`, the scene component. It only ever calls `Plant.ApplyWaterFlow`.
- `WateringCanLevelMeter`, the bar printed on the can.

Rules that later steps must not re-derive:
- **One tool is active at a time, so the shared right trigger no longer clashes.**
  The planter GameObject is off while the can is out. This closes the entry that
  used to sit in `### Known issues, parked`. Keep it that way, or a grabbable can
  will plant a seed on the press that pours.
- ~~The tool switch action is defined inline on the component.~~ The button is
  gone. The tool menu replaced it. `SelectTool(int)` is the only switch path.
- **The pond refill is a `Physics.CheckSphere` against layer 4, not a trigger
  callback.** A hand held can has no Rigidbody, so `OnTriggerStay` never fires.
- Watering targets the nearest plant with `HasWaterMeter`. A dead plant is
  already excluded, because it moves to layer 2. Do not add a filter.
- **The can prefab shipped with a Rigidbody and a convex `MeshCollider`.** Both
  are removed. A non-kinematic Rigidbody fights a parented transform, and layer 0
  sits inside `occluderMask`, so the can would stop the player's own rays.
- **The imported mesh carries a baked rotation.** `WateringCan.tiltReference`
  exists for that. Point it at a transform whose up axis leaves the top of the
  can, or the tilt reads the wrong angle.
- `Sogeti.Watering` is the second asmdef. Pure rules go there so EditMode tests
  reach them, and `Sogeti.Planting` stays about planting.

Tests: `Assets/Tests/EditMode/PourFlowTests.cs` and `WaterTankTests.cs`,
25 new cases. 160 total.

**Wired into the rig and verified in Play Mode.** `RightHandToolSwitch` sits on
`PlayerRig > Camera Offset > Right Controller` with the `SeedPlanter` and the
watering can in its `tools` array. Tool swap, tilt pour, plant drinking, the
dry-out, and the pond refill all pass, with `requireTriggerToPour` ticked and
unticked.

### Water meter step, built
A bar and a droplet float above every living plant. Code in
`Assets/Scripts/Planting/` and `Assets/Scripts/UI/WorldUI/`.

Design answers from the user:
- **Range gated.** A meter shows inside 15 m and hides past 16 m. Two distances,
  because one threshold flickers while the player stands on the edge.
- **A dead plant stays visible and stops blocking.** Tick
  `Plant.blocksPlacementWhenDead` to get the old behaviour and compare the two.
- **A bar plus a droplet.** The droplet pulses on the critical band only.

New types:
- `WaterUrgency`, an enum with `Healthy`, `Low` and `Critical`.
- `WaterUrgencyBands`, static and pure. It maps `Water01` to a band.
- `PlantWaterMeter`, the scene component. It reads `Plant.Water01` and
  `Plant.HasWaterMeter`. It owns neither.

New assets:
- `Assets/Prefabs/Plants/Plant.prefab` gains a `WaterMeter` child on the root,
  a sibling of `Visual`. Every stage swap destroys the contents of `Visual`.
- `Assets/Materials/UI/`, five URP Unlit materials. All have `_Cull` off.
- `Assets/Textures/UI/Droplet.png`, project placeholder art, not third party.

Rules that later steps must not re-derive:
- **The player camera culls layer 8.** A meter there renders nowhere. Layer 2 is
  the one layer the camera draws that no gameplay mask reads. See
  `## Layer conventions`.
- **A spacing rule alone cannot free a dead plant's spot.** The plant capsule is
  layer 7, and layer 7 sits inside `occluderMask`. The planting ray is refused
  before `PlantingRules.CheckSpacing` runs. `Plant` moves the whole hierarchy to
  layer 2 on death instead. `NeighbourPlant` and `PlantingRules` stay unchanged.
- The same move stops the husk blocking teleport. The two masks are the same
  layers, so no layer separates the two cases.
- The toggle applies at death. Restart Play Mode after you change it.
- `WorldUILookAtPlayer` runs the billboard. `rotationOffset` is `0, 180, 0`, so
  the quad front faces the player and the bar does not drain the wrong way.
- The fill swaps `sharedMaterial` per band. A `MaterialPropertyBlock` would drop
  the renderer out of the SRP batcher, one extra draw call per plant.
- `PlantWaterMeter` reads the `CameraPosition` atom, not `Camera.main`. It skips
  the first frame, while the atom still holds the origin.
- Stage 3 has no meter, so the meter only ever sits above grass or a shrub. A
  fixed height of 1.2 m clears both.

Tests: `Assets/Tests/EditMode/WaterUrgencyBandsTests.cs`, 16 new cases. 135 total.

**Verified in Play Mode.** The meter appears, drains through green/amber/red
with the droplet pulse, hides on death, respects the 15/16 m range, faces the
player, and the `blocksPlacementWhenDead` toggle behaves as designed.

### Planting step, done
Runtime code in `Assets/Scripts/Planting/` and `Assets/Scripts/Interaction/`.

Design answers from the user:
- **Split hands.** The left hand teleports. The right hand plants. There is no
  mode to switch, so the player never loses a ray mid aim.
- **A blocked spot states its reason.** A red ghost plus a short world space
  line, for example "Pine is too close". The target player has little game
  experience, so a colour alone does not teach the spacing rule.
- **The right trigger plants.** `XRI Right Interaction/Activate`. In the XRI
  defaults `Select` is the grip, not the trigger.

New types:
- `PlacementStatus`, `PlacementResult`, `NeighbourPlant`. The verdict on one
  spot, plus the numbers behind it.
- `PlantingRules`. Pure functions: surface allow list, slope filter, search
  radius, spacing. This is where a new planting rule goes.
- `PlacementReason`. One short player-facing line per status.
- `PlantPlacementQuery`. The raycast and the overlap test. Buffers allocate once.
- `Plant`. One planted plant. Owns the collider, the layer, the growth state and
  the child visual.
- `SeedPlanter`, `PlantingPreview`. The right hand ray and the ghost.

New prefabs:
- `Assets/Prefabs/Plants/Plant.prefab`. The plant root. Capsule collider
  r 0.35, h 1.6, layer `UniversalObstacle`, plus an empty `Visual` child.
  One prefab serves every seed type.
- `Assets/Prefabs/Player/SeedPlanter.prefab`. The planter, the preview and the
  reason label, already wired to the four seed assets and the plant prefab.

Rules that later steps must not re-derive:
- **The overlap radius is wider than `SeedDefinition.LargestSpacing`.** That
  earlier note was wrong. Spacing is mutual, so a neighbour type that demands
  more than this seed does still has to be found. `SeedPlanter` passes its seed
  list to `PlantingRules.NeighbourSearchRadius`, which takes the largest demand
  in that list. A 3 m sphere would never see a pine that demands 5 m.
- Spacing measures ground distance. Height is ignored. A sphere query of the
  same radius still finds every plant that can reject the spot.
- A refusal reports the worst offender, not the first hit, so the label names
  the neighbour that actually decides the spot.
- `PlacementStatus.Valid` is deliberately not zero. A default `PlacementResult`
  must never read as a legal spot.
- Planting also refuses ground steeper than 30 degrees, the teleport tolerance.
  Ground you cannot stand on is ground you cannot plant on.
- A collider on a blocker layer with no `Plant` component is scenery. It refuses
  the spot inside `obstacleClearance`, measured against its bounding box.
- The plant root takes a random heading once, at plant time. Re-rolling it on a
  stage swap would spin the tree.

Tests: `Assets/Tests/EditMode/`, 3 new files, 46 new cases. 130 in total.

### Planting step, scene wiring
Four facts about the rig. A rebuilt rig loses all four.

1. `SeedPlanter.prefab` sits under `PlayerRig > Camera Offset > Right
   Controller`, Transform at zero.
2. The `Teleport Interactor` child of that `Right Controller` is disabled.
3. **Disabling that child is not enough.** On the same `Right Controller`,
   `Controller Input Action Manager` has three cleared fields: `Teleport
   Interactor`, `Teleport Mode`, `Teleport Mode Cancel`. That script calls
   `m_TeleportInteractor.gameObject.SetActive(true)` the moment the teleport
   action fires, so the Editor checkbox alone is undone at runtime. All three
   fields are null guarded, so clearing them leaves grab and UI mediation intact.
4. `Ray Origin` is empty on the planter, so it falls back to its own transform.
   Do not wire `Right Controller Teleport Stabilized Origin`. That stabilizer
   aims against the teleport interactor, which this hand no longer runs.

### Seed data model step, done
Runtime code in `Assets/Scripts/Planting/`, assembly `Sogeti.Planting`.

- `GrowthStage`, a `[Serializable]` class. One stage owns its drain speed, the
  award for **leaving** it, and that award's grace, ramp, and floor. Every field
  reads against the same clock, the time spent inside that one stage.
- `SeedDefinition`, a ScriptableObject. `Create > Trees for All > Seed
  Definition`. A new seed type needs an asset, never code.
- `PlantGrowth`, a pure C# water meter and stage machine. It takes delta time as
  an argument, so it is fully testable on PC with no headset.
- `GrowthPoints`, static and stateless. Full points inside the grace window,
  then linear to a floor, then flat.
- Four seed assets in `Assets/ScriptableObjects/Seeds/`: Oak, Pine, Blossom,
  Broadleaf. Stage 1 and 2 art repeats across types on purpose. Polygon Trees
  ships 2 grass and 3 shrub prefabs, and a sprout gives away no species.
- `menuIcon` is empty on all four. The tool menu step fills it.

Rules that later steps must not re-derive:
- The meter opens at 50% on every stage except the last. Empty kills, full
  advances, the last stage has no meter.
- `Tick` advances at most one stage and kills at most once per call. A frame
  hitch must not gift a stage.
- Death visuals resolve stage override first, then
  `defaultDeathVisualPrefab`. Polygon Trees has no dead grass and no dead shrub,
  so stages 1 and 2 share `dryBranches` and only stage 3 gets a dry tree.
- `waterFlow01` on `Tick` is a 0 to 1 throttle, not a rate. The plant owns its
  fill rate, so a watering can cannot change how fast every seed type grows.
- Tuning lives in the assets: 30 s then 45 s to empty, 10 points for planting,
  then 20 and 40. The meter opens half full, so the real neglect deadline is
  15 s and 22.5 s.

Tests: `Assets/Tests/EditMode/`, assembly `Sogeti.Planting.Tests`, 84 cases.

`Assets/Scripts/Planting/` holds the only asmdef in `Assets/`. EditMode tests
force it, for two Unity rules:
- An asmdef cannot reference `Assembly-CSharp`. Unity forbids referencing the
  predefined assemblies, so a test asmdef cannot see code that stays there.
- `Assembly-CSharp-Editor` references neither `nunit.framework` nor
  `UnityEngine.TestRunner`, so tests cannot live there either.

The asmdef needs no references, and `autoReferenced` keeps `Assembly-CSharp`
compiling as before. Give new gameplay code its own asmdef only when it also
needs tests. A wide asmdef over all of `Assets/Scripts/` would have to list
Unity Atoms, and then every future package by hand.

### Teleport step, done
- `Assets/Prefabs/Player/PlayerRig.prefab` is a prefab variant of the Starter
  Assets rig. It owns the teleport raycast masks and the 1.7 m camera offset.
- `Assets/Prefabs/Environment/` holds a rock obstacle prefab on
  `UniversalObstacle`.
- Pond colliders live in `World.prefab`, not as scene overrides.
- `TeleportationArea` uses `Filter Selection By Hit Normal` at 30 degrees.
- The blocked teleport reticle is assigned on both Teleport Interactors.
- `DevelopmentScene` is first in the build list.
- `TunnelingVignette` is a child of `Main Camera`.
- The stray scene level `Input` object is deleted.
- `Project Settings > XR Interaction Toolkit`: untick **Automatically
   Instantiate Simulator Prefab**. The setting spawns the new XR Interaction
   Simulator, but the scene holds the classic XR Device Simulator.
- `Project Settings > XR Plug-in Management > PC, Mac & Linux Standalone`:
   untick **XR Simulation**. Two loaders are active. With no headset, OpenXR
   fails and XR Simulation takes over. It resets `CameraYOffset` to 0 and the
   camera drops to the floor. Every Play Mode test is wrong until you fix this.
- Wire the `TeleportationArea` **Teleportation Provider** field to
   `PlayerRig > Locomotion > Teleportation`. This is a scene override. A prefab
   cannot reference a scene object.
- The pond blocker top sits about 2 cm above the terrain. Setting `World > Pond`
   Box Collider Center Y to `-0.05` would add margin. Skipped, the gap works.

### PC test setup
No headset. Test with the classic **XR Device Simulator** in the scene.

Keys, read from `XR Device Simulator Controls.inputactions` and
`XR Device Controller Controls.inputactions`:

| Key | Effect |
| --- | --- |
| `W` `A` `S` `D` `Q` `E` | Move the rig |
| Hold right mouse | Look with the head |
| Hold `Left Shift` / `Space` | Manipulate left / right controller |
| `T` / `Y` / `U` | Toggle manipulate left / right / body |
| `1` | Toggle `W` `A` `S` `D` between rig movement and Primary 2D Axis |
| `I` `K` `J` `L` | Primary 2D Axis of the resting hand |
| Left mouse | Trigger. `G` grip. `M` menu. |
| `B` / `N` | Primary / secondary button of the manipulated hand |
| `Tab` | Cycle devices. `Esc` stop. `V` reset. |

Hands are split. The left hand teleports. The right hand plants.

To teleport: press `T`, then `1`. Aim with the mouse. Hold `W` to raise the arc.
Release `W` to teleport. Press `1` again to restore rig movement.

To plant: press `Y`. Aim with the mouse. Click left mouse, the trigger.
Planting is bound to `XRI Right Interaction/Activate`, not `Select`. `Select` is
the grip in the XRI defaults.

XRI binds teleport to the Primary 2D Axis, north sector. The simulated
controller and the Quest thumbstick resolve the same binding.

### Known issues, parked
- ~~The right trigger is shared.~~ Closed by the watering can step.
  `RightHandToolSwitch` keeps one tool active, so only one reader of `Activate`
  is ever alive. A grabbable object would re-open this.
- XRI interaction layers 1 to 3 duplicate the physics layer names. Nothing
  reads them. Clearing them removes a source of confusion.
- The 8 disabled terrain tiles sit at `y=0`, the active tile at `y=-1`. They do
  not form a seamless world. Re-align before you enable them. Ignore.
