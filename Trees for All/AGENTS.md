# AGENTS.md

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

The project is a fresh Unity template. No gameplay code exists yet. The XR
Interaction Toolkit Starter Assets sample is imported for reference under
`Assets/Samples/`.

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
- List any external asset source in the root `README.md` when the asset
  is not part of the default Unity template or an installed package.

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

## Steps
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
