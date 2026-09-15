# AGENTS.md

## Agent workload
Switch to Opus 5 with 1M context when a task needs it. Switch back after.

## Project
"Trees for All" is a VR tree planting game for Sogeti. It reflects Sogeti's
partnership with the Trees for All foundation. The player plants and grows trees
inside a limited play space.

| Item | Value |
| --- | --- |
| Engine | Unity 6000.3.24f1, URP |
| XR stack | XR Interaction Toolkit 3.3.2, OpenXR, Meta XR SDK |
| Target | Meta Quest 3, standalone |
| Work scene | `Assets/Scenes/DevelopmentScene.unity` |
| Player rig | `Assets/Prefabs/Player/PlayerRig.prefab` |
| World | `Assets/Prefabs/Terrain/World.prefab` |

The project is complete. Every assignment rule is built and verified in Play
Mode. `## Architecture` is the reference for future changes.

- `PlayerRig.prefab` is a variant of the XRI Starter Assets rig. Edit the variant.
- `World.prefab` holds the terrain tiles, the `TeleportationArea` and the pond.
- `Assets/Samples/` holds the imported XRI sample. Treat it as read only.

## Repo layout
The git root sits one level above the Unity project.

```
Sogeti/                 <- git root: .gitignore, README.md, assignment PDF
└─ Trees for All/       <- Unity project: this file, Assets/, Packages/
```

## Assignment rules
Source: `Opdrachtomschrijving XR developer Sogeti.pdf` at the git root. These are
fixed. Ask the user before you relax one.

1. The game runs in VR.
2. The game runs standalone on a Quest 3.
3. The player plants 2 or more seed types.
4. The plantable area or the time is limited. The game has no infinite space.
5. A seed spot depends on nearby seeds, plants and obstacles. The game shows the
   limits.
6. Each seed has 3 growth stages. The player acts to advance a stage. A missed or
   late action kills the plant.
7. The goal is the highest score.
8. Optional: one small bonus feature. It adds little weight to the grade.

Grading notes:
- A grid system is allowed. It is not required.
- Placeholder art is allowed. A good look is a plus.
- The player has little game experience. Favor clear onboarding and simple
  controls over complex mechanics.
- Use comfort friendly locomotion: teleport and snap turn. Ask before you add
  smooth camera rotation or movement. Novice players get motion sick.
- List every external asset in the root `README.md`. Skip Unity template assets
  and package assets.

## No VR hardware
The developer has no headset. All tests run on PC.

- Test in Play Mode with the XR Device Simulator or the Meta XR Simulator.
- Build on standard OpenXR and Input System actions. The PC simulator and the
  Quest 3 build then behave the same.
- Avoid Quest only APIs. Hide a needed one behind an interface.
- Never call a feature done from a successful build. Verify it in Play Mode.
- A Quest 3 build is unverified past "it builds and installs".
- Name what only hardware can confirm, such as haptics feel or hand tracking.
- State the test cost of a design choice that touches hardware.

## Conventions
Code:
- Put new gameplay scripts under `Assets/Scripts/`.
- Match the namespace to the folder. `Assets/Scripts/Interaction` is
  `Sogeti.Interaction`.
- Write small methods with one responsibility and explicit dependencies.
- Keep members private. Use `readonly` where it fits. Expose a property, not a
  public field.
- Prefer events and ScriptableObjects over direct references.
- Define each seed type as a ScriptableObject asset. A new seed type needs no code
  change.
- Keep allocations and heavy work out of `Update()`. The Quest 3 is mobile class
  hardware. Profile before you add cost to a hot path.

Comments:
- Explain why. Skip comments that repeat the code.
- Write one line per comment. Describe the code as it is now.
- State each design decision in a short comment next to the code.
- Never mention agents or this file in a commit message.

Never:
- Do not add third party assets or packages. Ask first.
- Do not write giant MonoBehaviours or god classes.
- Do not use mutable statics or singletons. Static pure functions are fine. The
  rules layer is built from them.
- Do not commit `Library/`, `Temp/`, `Logs/`, `UserSettings/` or other
  `.gitignore` paths.
- Do not commit secrets or API keys. Use `.env.example` placeholders.

## Architecture

### Assemblies
The rules assemblies stay package free, so the EditMode tests in
`Assets/Tests/EditMode/` run with no headset.

| Assembly | Path | Holds |
| --- | --- | --- |
| `Sogeti.Planting` | `Assets/Scripts/Planting/` | Seed data and spot rules |
| `Sogeti.Watering` | `Assets/Scripts/Watering/` | `PourFlow`, `WaterTank` |
| `Sogeti.Game` | `Assets/Scripts/Game/` | Clock, score, phase machine |
| `Sogeti.Atoms` | `Assets/Scripts/Atoms/` | Hand written Atoms types |
| `Sogeti.Atoms.Editor` | `Assets/Scripts/Atoms/Editor/` | One inspector per custom Variable |

- `Sogeti.Game` references `Sogeti.Planting` alone. `ScoreTally` keys its rows by
  `SeedDefinition`.
- `Sogeti.Atoms` references Unity Atoms, `Sogeti.Planting` and `Sogeti.Game`. The
  reference runs one way, so the other three stay package free.
- Add an editor to `Sogeti.Atoms.Editor` for every new Variable type. Unity Atoms
  ships an editor per generated type. A hand written type without one gets the
  default inspector, which edits the wrong field.
- All other gameplay code compiles into `Assembly-CSharp`. It auto references
  `Sogeti.Planting`, `Sogeti.Game` and `Sogeti.Atoms`, so glue code needs no
  asmdef.
- Code that needs a package belongs in `Assembly-CSharp`. This covers Input
  System, TextMesh Pro and XRI.
- An asmdef cannot reference `Assembly-CSharp`. `Assembly-CSharp-Editor` carries
  neither `nunit.framework` nor `UnityEngine.TestRunner`. Give new code its own
  asmdef only when it also needs EditMode tests.

### Four layers of a feature
1. **Data.** `SeedDefinition`, `GrowthStage`. ScriptableObjects, no behaviour.
2. **Rules.** `PlantingRules`, `GrowthPoints`, `PlantGrowth`. Pure C#. No scene,
   no physics, no `Time.deltaTime`. This layer carries the tests.
3. **Queries.** `PlantPlacementQuery`. Runs the raycast and the overlap test, then
   calls layer 2. Plain C#, but it needs a live scene.
4. **Scene.** `Plant`, `SeedPlanter`, `PlantingPreview`. MonoBehaviours only.

Put a new rule in layer 2. A test reaches it there without a headset.

### Layers and masks
Unity physics layers decide what a gameplay ray hits. XRI interaction layers gate
interactor and interactable matching only. Keep the two apart.

| Layer | Meaning | Blocks teleport | Blocks planting |
| --- | --- | --- | --- |
| 2 `Ignore Raycast` | Non-interactive world UI: the water meter and the can level bar. A dead plant moves here. | no | no |
| 3 `Terrain` | The only surface to stand on and plant on. Carries the `TeleportationArea`. | no, it is the target | no |
| 4 `Water` | Visual water and the can refill trigger. Colliders here are triggers. | no, the pond body blocks instead | yes |
| 5 `UI` | Interactive uGUI: the tool menu panel and the seed readout. | only with a collider | only with a collider |
| 6 `PlayerObstacle` | Blocks the player only: pond body, boundary walls. | yes | no |
| 7 `UniversalObstacle` | Blocks the player and placement: rocks, scenery, props. | yes | yes |

- Teleport works by occlusion, not exclusion. Obstacle layers stay inside the
  teleport raycast mask. A closer collider with no interactable blocks the terrain
  behind it. Drop a layer from the mask and the ray passes through it.
- Both Teleport Interactors use this raycast mask: `Default`, `Terrain`, `UI`,
  `PlayerObstacle`, `UniversalObstacle`.
- An obstacle collider is never a trigger. The teleport ray ignores triggers.
- An obstacle collider top sits at least 0.2 m above the terrain.
- An obstacle carries no XRI interactable component.
- Planting is an allow list on `Terrain`. Refuse every other hit. One rule covers
  water, rocks and the pond body.
- The planting `occluderMask` is 233. The teleport mask is 2147483881, which is
  233 plus the XR Simulation bit. They cover the same layers, so no layer blocks
  the player and passes the planting ray.
- Pass `QueryTriggerInteraction.Ignore` in every gameplay raycast.
  `Physics.queriesHitTriggers` is on and the pond refill trigger overlaps the
  ground.

World UI layer choice:
- The camera culls layer 8 `Overlay UI`. The scene overrides the culling mask to
  4294967039. Only an `OVROverlayCanvas` draws there. World UI the camera must
  draw belongs on layer 2 or layer 5.
- **A clickable canvas belongs on layer 5, never on layer 2.** The Near-Far
  Interactor hands its own mask to the UI raycast.
  `Left_NearFarInteractor.prefab` sets `m_RaycastMask.m_Bits: 2147483689`, which
  covers layers 0, 3, 5 and 31. A panel on layer 2 never receives a hit.
- Layer 5 sits inside both masks, but a Canvas carries no collider. Both masks
  gate colliders, so a panel there blocks no ray.

### Communication through Atoms
ScriptableObjects hold data. Unity Atoms assets carry shared state between systems
that must not know each other. Plain C# events carry per-instance state. Atom
assets live under `Assets/ScriptableObjects/UnityAtoms/`:

| Folder | Assets |
| --- | --- |
| `Round/` | `Phase`, `Score`, `SecondsRemaining`, `RoundEnded`, `StartRoundRequested`, `RestartRequested` |
| `Hand/` | `SelectedSeed`, `ToolMenuOpen`, `WateringCanFill01` |
| `Vector3/` | `CameraPosition` |

**Atoms are the broadcast channel, not the store.** `ScoreTally` owns the score.
`GameCountdown` owns the clock. `GameSession` owns the phase machine. Each writes
its value out to an Atom. Consumers read the Atom only. A missing Atom costs the
broadcast, never the system, so null guard every Atom field.

`SelectedSeed` is the one exception. The Variable owns the seed because every
candidate owner gets deactivated. `RightHandToolSwitch` calls `SetActive` on the
tool objects, so `SeedPlanter` sleeps while the menu is open.

Rules a future change must not re-derive:
- Keep Atoms code in `Assembly-CSharp` and `Sogeti.Atoms`. The rules assemblies
  stay package free for the tests.
- **A value shown as text belongs in an `IntVariable`.** Atoms drops a write that
  changes nothing, so whole seconds throttle the TMP rebuild for free. A
  `FloatVariable` of raw seconds fires 90 times a second on Quest 3.
- **Commands use `VoidEvent` with the no-argument `Register(Action)`.** The
  `Action<T>` overload replays the last value to each new subscriber, which would
  reload the scene on every restart registration.
- **Give every Variable asset a Changed Event sub-asset**, with replay buffer 1.
  `SetValue` raises nothing while that field is null, so a write made before the
  first listener registers is lost. `RoundEnded` is an Event, so it needs none.
- **Force the opening value out in `Start`** with `SetValue(value, forceEvent:
  true)`. A plain write of the Initial Value changes nothing and leaves the replay
  buffer empty, so a consumer enabled later reads a blank channel. `GameSession`
  does this for `Score`, `SecondsRemaining` and `Phase`. `AtomVariable.OnEnable`
  copies Initial Value over Value on entering Play Mode. Set Initial Value in the
  Inspector, never Value.
- **A Variable with no continuous producer needs the reverse fix. The consumer
  reads `.Value` directly on enable and also registers for `.Changed`.** Nothing
  writes a starting value for `SelectedSeed`, so the replay buffer stays empty
  until the player picks a seed. `SelectedSeedReadout`, `WateringCanLevelMeter`
  and `HudMenuLink` all read `.Value` once in `OnEnable` for this reason. Skip it
  and the seed readout keeps the shipped prefab text until the first pick.
- **A listener must not live on the GameObject it toggles.** `HudMenuLink` hides
  the HUD root, so it lives on the `ToolMenu.prefab` root. `SetActive(false)`
  would unregister it, and the close write would find no listener.

Per-plant state stays on plain C# events. A shared asset cannot carry
per-instance state.
- `SeedPlanter` raises `PlantPlaced` and `PlantRefused`. `Plant` raises `Planted`,
  `StageAdvanced` and `Died`. Each event carries its point award.
- `ScoreCollector` subscribes to those events and never polls a plant.
- `ScoreCollector.PlantPlaced` is the only planting hook. `Plant.Planted` carries
  the same award and fires first, inside `Initialize`, so both hooks count every
  seed twice.

### Growth and scoring
- `GrowthStage` owns its drain speed, the award for leaving it, and that award's
  grace, ramp and floor. All read against the time spent inside that one stage.
- The meter opens at 50% on every stage except the last. Empty kills. Full
  advances. The last stage has no meter.
- `PlantGrowth.Tick` advances at most one stage and kills at most once per call. A
  frame hitch cannot gift a stage.
- `waterFlow01` on `Tick` is a 0 to 1 throttle, not a rate. The plant owns its
  fill rate, so a watering can cannot change how fast a seed type grows.
- Death visuals resolve a per-stage override first, then
  `defaultDeathVisualPrefab`.
- A dead plant moves its whole hierarchy to layer 2. That move frees its spacing
  and stops it blocking teleport. The plant capsule was layer 7, inside
  `occluderMask`, so `PlantingRules.CheckSpacing` never ran on it.
- `PlantingRules.NeighbourSearchRadius` takes the largest spacing demand across
  the seed list passed in, not this seed's own. Spacing is mutual, so a narrower
  search misses a neighbour type that demands more room. A refusal reports the
  worst offender, not the first hit.
- Planting refuses ground steeper than 30 degrees, the teleport tolerance. It also
  refuses any collider on a blocker layer with no `Plant` component, within
  `obstacleClearance` of its bounding box.
- The plant root heading is randomised once, at plant time, not on stage swaps.
- A meter fill swaps `sharedMaterial` per band. A `MaterialPropertyBlock` would
  drop the renderer out of the SRP batcher.
- `PlantWaterMeter` reads the `CameraPosition` Atom, not `Camera.main`. It skips
  the first frame while the Atom still holds the origin.
- `WorldUILookAtPlayer.rotationOffset` stays `0, 180, 0`. A world space Canvas
  then faces the player instead of mirroring.

### Tools and watering
- `RightHandToolSwitch` keeps exactly one right hand tool active. `SelectTool(int)`
  is the only switch path, so the shared right trigger never clashes between
  planting and pouring.
- The pond refill is a `Physics.CheckSphere` against layer 4, not a trigger
  callback. A hand held can has no Rigidbody, so `OnTriggerStay` never fires.
- The watering can prefab carries no Rigidbody and no convex `MeshCollider`. A
  non-kinematic Rigidbody fights a parented transform. Layer 0 sits inside
  `occluderMask`, so either one blocks the player own rays.
- `WateringCan.tiltReference` compensates for the mesh baked rotation. Point it at
  a transform whose up axis leaves the top of the can.
- The tilt angle alone is direction blind, so a can tilted up reads like one
  tilted down. `PourFlow.SpoutTiltDegrees` also asks whether `spout` sits below
  `tiltReference` in world height. Only that pair pours.

### Tool and seed menu
- `SeedIconBaker` (`Assets/Editor/`) passes `allowScriptableRenderPipeline: true`
  to `PreviewRenderUtility.Render()`. The default is false, and URP SubShaders do
  not match the built-in pipeline, so the magenta error shader covers correct
  geometry. It frames icons against mesh renderers only. An idle
  `ParticleSystemRenderer` reports bounds far larger than its emitter.
- `ToolMenuRow.entries` maps array position to tool index. Slot 0 stays empty so
  the watering can sits at slot 1. A seed pick already switches the hand to the
  planter, so the Tools row holds the can alone.
- `m_EnableUIInteraction` on the left interactor stays off in `PlayerRig.prefab`.
  Otherwise its ray hits its own panel.
- The scene `EventSystem` needs `XR UI Input Module`, not `Input System UI Input
  Module`, with empty action fields, so the mouse fallback stays on. Without it
  `m_EnableUIInteraction` does nothing and the panel reads as dead with no error.

### Round, HUD and panels
- `GamePanelView` and `ToolMenuController` sit on their always-active root, not on
  the child they hide. Their `Awake` hides that child, so a component on the
  hidden child would re-hide itself the moment it activates.
- Each screen is a full-stretch group with its action button anchored to the
  bottom edge. Play, Back and Play again then land in the same spot at any panel
  height.
- `GameSession` raises the clock on whole seconds only. `StartRound` resets the
  tally before it raises `PhaseChanged`, or the HUD flashes the old score.
- A world panel places itself one frame after a phase request. `PlantWaterMeter`
  skips a frame for the same reason. The head pose is not settled on the frame a
  scene loads.
- Plants keep growing after the round ends, on purpose, so the world looks alive
  behind the end panel. `ScoreCollector` drops every award unless the phase is
  `Playing`.
- **`GameHud` is a nested prefab instance under `PlayerRig > ... > Left
  Controller`**, in the same wrist slot as `ToolMenu`. `HudMenuLink` hides the HUD
  when the menu opens and shows it again on close, so exactly one is visible.
- **`ControllerInputActionManager.enabled = false` is a clean teleport gate.** Its
  `OnDisable` only tears down interactor events, so `OnStartTeleport` can never
  fire. Its `OnEnable` forces the Teleport Interactor GameObject inactive, so
  re-enabling self-heals. `PlayerActionGate` still deactivates the left Teleport
  Interactor directly, or a player who holds the teleport stick as the clock
  expires leaves a live arc. On resume, activate GameObjects before you enable
  Behaviours. The reverse order revives an interactor with nothing driving it.
- `ToolMenuController.Close()` un-stows the hand. The gate closes the menu before
  it stows, or the player gets a live planter behind the end panel.

### Rig wiring
- `GameSession`, `ScoreCollector`, `PlayerActionGate`, `GameRestarter` and
  `GamePanelController` all live on one GameObject in
  `Assets/Prefabs/Player/GameSession.prefab`, nested inside `PlayerRig.prefab`. It
  therefore ships with every Atom field filled.
- `GamePanel.prefab` starts active. `GameHud.prefab` starts inactive.
  `GamePanelView.Awake()` is what lets `GamePanel` show itself later, and an
  inactive root never runs that `Awake`. `GameHud` has no self-managed visibility,
  so `PlayerActionGate.objectsToDeactivate` drives it.
- `PlayerActionGate.behavioursToSuspend`: `Left Controller > Controller Input
  Action Manager`, `Locomotion > Move > Dynamic Move Provider`, `Locomotion > Turn
  > Snap Turn Provider`.
- `PlayerActionGate.objectsToDeactivate`: `Left Controller > Teleport Interactor`,
  and the `GameHud` root.
- `PlayerActionGate.toolSwitch` is `RightHandToolSwitch` on `Right Controller`.
  `.toolMenu` is `ToolMenuController` on `Left Controller > ToolMenu`.
- `GamePanelController.playerHead` points at `PlayerRig > Camera Offset > Main
  Camera`. It is a scene override, because a prefab cannot reference a scene
  object. It is the last such field. Atom fields replaced `session`, `gate` and
  `restarter`.
- `TeleportationArea` has its `Teleportation Provider` as a scene override on
  `PlayerRig > Locomotion > Teleportation`, for the same reason.
- `ScoreCollector.planter` points at the `SeedPlanter` on `Right Controller`.
- `GameSession.roundSeconds` is `180`. `autoStartOnLoad` stays unticked, so a
  restart returns to the start panel.
- `SeedPlanter.prefab` sits under `PlayerRig > Camera Offset > Right Controller`.
  That controller `Teleport Interactor` is disabled, and its `Controller Input
  Action Manager` has `Teleport Interactor`, `Teleport Mode` and `Teleport Mode
  Cancel` cleared. Otherwise the manager reactivates the teleport interactor the
  moment the teleport action fires.
- `SeedPlanter.Ray Origin` stays empty and falls back to its own transform. Do not
  wire the Teleport Stabilized Origin. It aims against an interactor this hand no
  longer runs.
- `RightHandToolSwitch` sits on the same `Right Controller`, with `SeedPlanter`
  and the watering can in its `tools` array.
- `Project Settings > XR Interaction Toolkit`: **Automatically Instantiate
  Simulator Prefab** is unticked. That setting spawns the newer XR Interaction
  Simulator, but the scene holds the classic XR Device Simulator.
- `Project Settings > XR Plug-in Management > PC, Mac & Linux Standalone`: **XR
  Simulation** is unticked. With both loaders active and no headset, OpenXR fails,
  XR Simulation takes over, and the camera drops to the floor.

## PC test setup
Test with the classic **XR Device Simulator** in the scene. Keys come from
`XR Device Simulator Controls.inputactions` and
`XR Device Controller Controls.inputactions`.

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

The hands are split. The left hand teleports. The right hand plants.

To teleport: press `T`, then `1`. Aim with the mouse. Hold `W` to raise the arc.
Release `W` to teleport. Press `1` again to restore rig movement.

To plant: press `Y`. Aim with the mouse. Click left mouse for the trigger.
Planting binds to `XRI Right Interaction/Activate`, not `Select`. `Select` is the
grip in the XRI defaults.

XRI binds teleport to the Primary 2D Axis, north sector. The simulated controller
and the Quest thumbstick resolve the same binding.

## Parked issues
- XRI interaction layers 1 to 3 duplicate the physics layer names. Nothing reads
  them. Cosmetic cleanup only.
- The 8 disabled terrain tiles sit at `y=0`. The active tile sits at `y=-1`. They
  do not form a seamless world if re-enabled.
