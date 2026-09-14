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

Teleport and planting work in Play Mode. Plants grow and die, but nothing on
screen shows the growth yet. See `## Progress` at the end of this file for the
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
| 3 `Terrain` | The only surface to stand on and plant on. Carries the `TeleportationArea`. | no, it is the target | no |
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

## Things to avoid
- Don't add third-party assets without asking.
- Giant MonoBehaviours.
- God classes.
- Static global state.
- Quest-only APIs with no PC simulation path, unless isolated behind an interface (see testing constraint above).

## High level architecture
Keep this current. It saves a rediscovery pass at the start of the next session.

### Assemblies
- `Sogeti.Planting` (`Assets/Scripts/Planting/`) is the only asmdef in `Assets/`.
  It holds data and rules. It references no package. The EditMode tests force it.
- All other gameplay code compiles into `Assembly-CSharp`. That assembly
  auto-references `Sogeti.Planting`, so glue code needs no new asmdef.
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
- ScriptableObjects hold data. Plain C# events carry state changes.
- Unity Atoms keeps one job, the camera position feeding the world space UI.
  A shared score or timer Atom is static global state.
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
- The user gets points for each tree stage reached per tree
- - the longer it takes to reach a stage the fewer points the user receives.
- - later stages provide more points e.g. seed -> +10 points, sprout -> +20 points, fully grown tree +40
- Game has a timer after which the game ends
- - Timer time to be determined
- - Show user score
- - Show Restart button

## Next Step Claude progress (overwrite when finished)
**Show growth in the scene: the water meter UI above each plant.**

Planting is built and verified. `Plant` already swaps the stage visual and the
death visual, so a neglected seed does turn into dry branches on screen. The
player just cannot see it coming. Do not re-plan the growth rules, they live in
`PlantGrowth` and are tested.

### Goal
The player reads, at a glance, which plant needs water and how urgent it is.

### Decisions already made, do not re-open
- The meter belongs to the plant root, not to the stage prefab.
- `Plant.HasWaterMeter` is the only switch. A dead or fully grown plant hides it.
- The meter faces the player on the yaw axis only. Tilting world space UI
  towards the headset reads as unstable in VR. Copy `PlantingPreview`.
- `Plant` exposes `Water01`. The meter reads it. It never owns the number.

### Build
1. A `PlantWaterMeter` component under `Assets/Scripts/UI/WorldUI/`.
2. A meter prefab, parented to the `Plant` prefab root, above the visual anchor.
3. Colour the fill by urgency, so a dying plant reads from across the field.

### Ask the user first
1. Does every plant show a meter always, or only inside a look or aim range?
2. Does a dead plant stay in the world, or fade out after a few seconds?

### Done when
The player plants a seed and watches the meter drain, so the death at 15 s is
readable in advance instead of a surprise. Verify in Play Mode with the
simulator.

### Not in this step
The watering can, the tool menu, the score display, the timer, the intro UI.

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
  plants now. All 7 checks under `### Teleport step, what "verified" means`
  passed.
- **Seed and tree data model: done.** See `### Seed data model step, done`.
- **Planting: done, verified in Play Mode.** See `### Planting step, done`.
- Growth stages: `Plant` swaps the stage visual and the death visual, verified
  in Play Mode. The water meter UI is not started. See `## Next Step`.
- Tool and seed menu: not started.
- Watering can: not started.
- Scoring: not started.
- Timer and end screen: not started.
- Intro UI: not started.

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

### Planting step, what "verified" means
The checks that closed the step. Re-run them if planting regresses.

1. **Ghost follows the aim.** Press `Y`. A ghost plant tracks the hit point and
   stands upright.
2. **Legal ground accepts.** Aim at open ground. The ghost turns green. The
   right trigger plants a tree that stays put.
3. **Every refusal reads.** Aim at the pond, a rock, and a steep bank. The ghost
   turns red and the label names the reason.
4. **Spacing holds.** Aim within 3 m of a planted oak. The label reads "Oak is
   too close". Aim past 4 m and it turns green again.
5. **Neglect kills.** Plant a seed and wait 15 s. The visual swaps to dry
   branches. There is no watering can yet, so every plant dies.
6. **The hands stay split.** Press `T` then `1`. The left hand teleports. The
   right hand still plants.

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

### Teleport step, what "verified" means
The checks that close the step. Re-run them if teleport regresses.

1. **Camera height.** The horizon sits near 1.7 m. A floor level view means the
   XR Simulation loader is active again on Standalone.
2. **One simulator.** The Hierarchy shows exactly one simulator object.
3. **Teleport works.** Aim at open ground about 5 m ahead. Expect a blue arc and
   a ring reticle with a direction arrow. Release. The rig moves and stays upright.
4. **Obstacles refuse.** Aim at a rock, then at the pond. Expect a red line and
   no reticle. Releasing does not move you.
5. **No teleport through an obstacle.** Put a rock between you and open ground.
   Aim past it. The ray stops at the rock.
6. **Slope filter.** Aim at the steepest bank. Expect red above about 30 degrees,
   blue below. Raise the tolerance to 35 if gentle slopes read as invalid.
7. **Debugger.** `Window > Analysis > XR Interaction Debugger`, Interactors tab.
   Terrain lists the `World` TeleportationArea as a valid target. A rock lists
   nothing.

Demo worth recording: untick `PlayerObstacle` from the right Teleport Interactor
raycast mask, then aim at the pond. The arc passes through and you land in the
water. This shows why obstacles stay inside the mask. Re-tick it after.

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
- The right trigger is shared. The planter and the Near-Far Interactor both read
  `Activate`, but the interactor only uses it on an object it already holds.
  Nothing is grabbable yet, so they do not clash. The watering can step
  revisits this.
- XRI interaction layers 1 to 3 duplicate the physics layer names. Nothing
  reads them. Clearing them removes a source of confusion.
- The 8 disabled terrain tiles sit at `y=0`, the active tile at `y=-1`. They do
  not form a seamless world. Re-align before you enable them. Ignore.
