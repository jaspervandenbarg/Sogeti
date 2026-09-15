# AGENTS.md

## Workload
- Allowed to auto switch to Opus 5 with 1m context when deemed necessary.
- Switch back to default after.

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

The project is complete. Every assignment requirement (see `## Assignment
rules`) is built and verified in Play Mode. `## Architecture` below is the
living reference for future changes.

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

## Architecture
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
  edits the wrong field. Add a new editor whenever a new Variable type is written.
- All other gameplay code compiles into `Assembly-CSharp`. That assembly
  auto-references `Sogeti.Planting`, `Sogeti.Game` and `Sogeti.Atoms`, so glue
  code needs no new asmdef.
- Code that needs a package (Input System, TextMesh Pro, XRI) belongs in
  `Assembly-CSharp`. Adding those references to `Sogeti.Planting` would pull
  packages into the test assembly for no gain.
- An asmdef cannot reference `Assembly-CSharp` (Unity forbids referencing
  predefined assemblies), and `Assembly-CSharp-Editor` carries neither
  `nunit.framework` nor `UnityEngine.TestRunner`. Give new gameplay code its
  own asmdef only when it also needs EditMode tests, or a wide asmdef over
  `Assets/Scripts/` would have to list Unity Atoms and every future package.

### The four layers of a gameplay feature
1. **Data.** `SeedDefinition`, `GrowthStage`. ScriptableObject assets, no behaviour.
2. **Rules.** `PlantingRules`, `GrowthPoints`, `PlantGrowth`. Pure C#. No scene,
   no physics, no `Time.deltaTime`. This layer carries the tests.
3. **Queries.** `PlantPlacementQuery`. Runs the raycast and the overlap test,
   then calls layer 2. Plain C#, but it needs a live scene.
4. **Scene.** `Plant`, `SeedPlanter`, `PlantingPreview`. MonoBehaviours only.

Put a new rule in layer 2, where a test can reach it without a headset.

### Communication (Atoms)
ScriptableObjects hold data. Unity Atoms assets carry shared state between
systems that must not know each other. Plain C# events carry per-instance state.
Atom assets live under `Assets/ScriptableObjects/UnityAtoms/`, in `Round/`
(session state: `Phase`, `Score`, `SecondsRemaining`, `RoundEnded`,
`StartRoundRequested`, `RestartRequested`) and `Hand/` (hand state:
`SelectedSeed`, `ToolMenuOpen`, `WateringCanFill01`).

**Atoms are the broadcast channel, not the store.** `ScoreTally` still owns the
score. `GameCountdown` still owns the clock. `GameSession` still owns the phase
machine. Each writes its value out to an Atom, and consumers read the Atom only.
A missing Atom must cost the broadcast, never the system itself, so null-guard
every Atom field. `GameSession` lives on `Assets/Prefabs/Player/GameSession.prefab`,
nested inside `PlayerRig.prefab`, not as a bespoke scene GameObject, so it ships
with every Atom field already filled.

One exception: `SelectedSeed`. The Variable owns the seed because every
candidate owner gets deactivated. `RightHandToolSwitch` calls `SetActive` on the
tool objects, so `SeedPlanter` sleeps while the menu is open.

Rules a future change must not re-derive:
- Keep Atoms in `Assembly-CSharp` and `Sogeti.Atoms`. `Sogeti.Game`,
  `Sogeti.Planting` and `Sogeti.Watering` stay package-free, so the EditMode
  tests run with no headset.
- **A value shown as text belongs in an `IntVariable`.** Atoms drops a write
  that changes nothing, so whole seconds throttle the TMP rebuild for free. A
  `FloatVariable` of raw seconds fires every frame, 90 times a second on Quest 3.
- **Commands use `VoidEvent` with the no-argument `Register(Action)`.** The
  `Action<T>` overload replays the last value to each new subscriber, which
  would reload the scene every time something registers for a restart.
- **Give every Variable asset a Changed Event sub-asset**, with replay buffer 1.
  `SetValue` raises nothing while that field is null, so a write made before
  the first listener registers is lost. `RoundEnded` is an Event, not a
  Variable, so it needs no Changed sub-asset.
- **Force the opening value out in `Start`**, with `SetValue(value,
  forceEvent: true)`. A plain write of the Initial Value changes nothing, leaves
  the replay buffer empty, and a consumer enabled later reads a blank channel.
  `GameSession` does this for `Score`, `SecondsRemaining` and `Phase`.
  `AtomVariable.OnEnable` copies Initial Value over Value on entering Play Mode,
  so setting Value alone in the Inspector is wiped on the first frame; use
  Initial Value there instead.
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
  the HUD root, so it lives on `ToolMenu.prefab`'s root instead. `SetActive(false)`
  would unregister it, and the close write would find no listener to bring the
  HUD back.

Per-plant state stays on plain C# events. A shared asset cannot carry
per-instance state.
- `SeedPlanter` raises `PlantPlaced` and `PlantRefused`. `Plant` raises
  `Planted`, `StageAdvanced` and `Died`, and each event carries its point award.
  `ScoreCollector` subscribes to those and must never poll every plant.
  `ScoreCollector.PlantPlaced` is the only planting hook — `Plant.Planted`
  carries the same award and fires first, inside `Initialize`, so reading both
  counts every seed twice.

### Growth and scoring
- `GrowthStage` owns its drain speed, the award for leaving it, and that
  award's grace, ramp, and floor, all read against the time spent inside that
  one stage. The meter opens at 50% on every stage except the last; empty
  kills, full advances, the last stage has no meter.
- `PlantGrowth.Tick` advances at most one stage and kills at most once per
  call, so a frame hitch cannot gift a stage. `waterFlow01` on `Tick` is a 0 to
  1 throttle, not a rate — the plant owns its fill rate, so a watering can
  cannot change how fast every seed type grows.
- Death visuals resolve a per-stage override first, then
  `defaultDeathVisualPrefab`.
- A dead plant moves its whole hierarchy to layer 2. That single move frees its
  spacing (the plant capsule was layer 7, inside `occluderMask`, so
  `PlantingRules.CheckSpacing` never even ran) and stops it blocking teleport,
  since both masks cover the same layers.
- The water meter and the can-level bar sit on layer 2 (the one layer the
  camera draws that no gameplay mask reads) or layer 5 if clickable; see
  `## Layer conventions`. `WorldUILookAtPlayer.rotationOffset` stays
  `0, 180, 0` so a world-space Canvas faces the player instead of mirroring.
- A meter's fill swaps `sharedMaterial` per band rather than using a
  `MaterialPropertyBlock`, which would drop the renderer out of the SRP
  batcher. `PlantWaterMeter` reads the `CameraPosition` Atom, not `Camera.main`,
  and skips the first frame while the Atom still holds the origin.
- `PlantingRules.NeighbourSearchRadius` takes the largest spacing demand across
  the seed list passed in, not just this seed's own — spacing is mutual, so a
  narrower search would miss a neighbour type that demands more room. A
  refusal reports the worst offender, not the first hit found.
- Planting refuses ground steeper than 30 degrees (the teleport tolerance) and
  any collider on a blocker layer with no `Plant` component, treated as
  scenery, refused inside `obstacleClearance` of its bounding box.
- The plant root's heading is randomised once, at plant time, not on stage swaps.

### Tool switching and watering
- `RightHandToolSwitch` keeps exactly one right-hand tool active at a time
  (`SelectTool(int)` is the only switch path), so the shared right trigger
  never clashes between planting and pouring.
- The pond refill is a `Physics.CheckSphere` against layer 4 (`Water`), not a
  trigger callback — a hand-held can has no Rigidbody, so `OnTriggerStay`
  never fires.
- The watering can prefab carries no Rigidbody and no convex `MeshCollider`: a
  non-kinematic Rigidbody fights a parented transform, and layer 0 sits inside
  `occluderMask`, so either would block the player's own rays.
- `WateringCan.tiltReference` compensates for the imported mesh's baked
  rotation; point it at a transform whose up axis leaves the top of the can.
- The tilt angle alone is direction-blind, so a can tilted up reads the same as
  one tilted down. `PourFlow.SpoutTiltDegrees` also asks whether `spout` sits
  below `tiltReference` in world height, and only that pair pours.

### Tool and seed menu
- `SeedIconBaker` (`Assets/Editor/`) passes `allowScriptableRenderPipeline:
  true` to `PreviewRenderUtility.Render()`. The default is false, and URP
  SubShaders don't match the built-in pipeline, so dropping that argument
  draws the magenta error shader over correct geometry. It also frames icons
  against mesh renderers only — an idle `ParticleSystemRenderer` (the watering
  can) reports bounds far larger than its emitter.
- `ToolMenuRow.entries` maps array position to tool index; slot 0 stays empty
  so the watering can sits at slot 1. A seed pick already switches the hand to
  the planter, so the Tools row holds the can alone.
- The left interactor's `m_EnableUIInteraction` must stay off in
  `PlayerRig.prefab`, or its ray hits its own panel.
- The scene `EventSystem` needs `XR UI Input Module`, not `Input System UI
  Input Module`, with empty action fields, so the mouse fallback stays on.
  Without it `m_EnableUIInteraction` does nothing and the panel reads as dead
  with no error.

### Round, HUD and panels
- `GamePanelView` (and `ToolMenuController`) sit on their always-active root,
  not on the child they hide — their `Awake` hides that child, so a component
  living on the hidden child would re-hide itself the moment it activates.
- Each screen is a full-stretch group with its action button anchored to the
  bottom edge, so Play, Back, and Play-again land in the same spot regardless
  of panel height.
- `GameSession` raises the clock on whole seconds only; Atoms drops a write
  that changes nothing, which throttles the TMP rebuild for free.
  `StartRound` resets the tally before raising `PhaseChanged`, or the HUD
  flashes the old score for a frame.
- A world panel places itself one frame after a phase request, and
  `PlantWaterMeter` skips a frame for the same reason: the head pose is not
  settled on the frame a scene loads.
- Plants keep growing after the round ends, on purpose, so the world looks
  alive behind the end panel. `ScoreCollector` drops every award unless the
  phase is `Playing`.
- **`GameHud` is a nested prefab instance under `PlayerRig.prefab > ... > Left
  Controller`**, the same parent and wrist slot as `ToolMenu` — opening the
  tool menu hides the HUD via `HudMenuLink`, closing it shows the HUD again,
  so exactly one is ever visible.
- **`ControllerInputActionManager.enabled = false` is a clean teleport gate.**
  Its `OnDisable` only tears down interactor events, so `OnStartTeleport` can
  never fire; its `OnEnable` forces the Teleport Interactor GameObject
  inactive, so re-enabling self-heals. `PlayerActionGate` still deactivates
  the left Teleport Interactor directly too, or a player holding the teleport
  stick as the clock expires leaves a live arc. On the resume path, activate
  GameObjects before enabling Behaviours — the reverse order revives a live
  interactor with nothing driving it.
- `ToolMenuController.Close()` un-stows the hand, so the gate closes the menu
  before stowing, or the player gets a live planter behind the end panel.

### Rig facts a rebuild would lose
- `GameSession`, `ScoreCollector`, `PlayerActionGate`, `GameRestarter` and
  `GamePanelController` all live on one GameObject, inside
  `Assets/Prefabs/Player/GameSession.prefab`.
- `GamePanel.prefab` starts active; `GameHud.prefab` starts inactive.
  `GamePanelView.Awake()` is what lets `GamePanel` show itself later — an
  inactive root never runs that `Awake` and can never show itself again.
  `GameHud` has no such self-managed visibility, so it alone starts inactive,
  driven directly by `PlayerActionGate.objectsToDeactivate`.
- `PlayerActionGate.behavioursToSuspend`: `Left Controller > Controller Input
  Action Manager`, `Locomotion > Move > Dynamic Move Provider`,
  `Locomotion > Turn > Snap Turn Provider`.
- `PlayerActionGate.objectsToDeactivate`: `Left Controller > Teleport
  Interactor`, and the `GameHud` root.
- `PlayerActionGate.toolSwitch` is `RightHandToolSwitch` on `Right
  Controller`; `.toolMenu` is `ToolMenuController` on `Left Controller >
  ToolMenu`.
- `GamePanelController.playerHead` points at `PlayerRig > Camera Offset >
  Main Camera` — a scene override, since a prefab cannot reference a scene
  object. It is the only field on `GamePanelController`/`GameHud` still wired
  this way; `session`/`gate`/`restarter` fields were replaced by Atom fields.
- `ScoreCollector.planter` points at the `SeedPlanter` on `Right Controller`.
- `GameSession.roundSeconds` is `180`. `autoStartOnLoad` stays unticked, so a
  restart returns to the start panel.
- `SeedPlanter.prefab` sits under `PlayerRig > Camera Offset > Right
  Controller`. That controller's `Teleport Interactor` is disabled, and its
  `Controller Input Action Manager` has `Teleport Interactor`, `Teleport
  Mode`, and `Teleport Mode Cancel` cleared — otherwise that manager
  reactivates the teleport interactor at runtime the moment the teleport
  action fires. `SeedPlanter`'s `Ray Origin` is left empty (falls back to its
  own transform); don't wire the Teleport Stabilized Origin, which aims
  against an interactor this hand no longer runs.
- `RightHandToolSwitch` sits on the same `Right Controller`, with
  `SeedPlanter` and the watering can in its `tools` array.
- `TeleportationArea`'s `Teleportation Provider` field is a scene override
  pointing at `PlayerRig > Locomotion > Teleportation`, for the same
  prefab-can't-reference-a-scene-object reason.
- `Project Settings > XR Interaction Toolkit`: **Automatically Instantiate
  Simulator Prefab** is unticked — that setting spawns the newer XR
  Interaction Simulator, but the scene holds the classic XR Device Simulator.
- `Project Settings > XR Plug-in Management > PC, Mac & Linux Standalone`:
  **XR Simulation** is unticked. With both loaders active and no headset,
  OpenXR fails and XR Simulation takes over, resetting the camera's Y offset
  to 0 and dropping the camera to the floor.

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
- XRI interaction layers 1 to 3 duplicate the physics layer names. Nothing
  reads them. Cosmetic cleanup only.
- The 8 disabled terrain tiles sit at `y=0`, the active tile at `y=-1`. They do
  not form a seamless world if re-enabled. Ignore unless someone re-enables them.
