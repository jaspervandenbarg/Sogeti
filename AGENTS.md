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

The project is a fresh Unity template. No gameplay code exists yet. The XR
Interaction Toolkit Starter Assets sample is imported for reference under
`Assets/Samples/`.

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
- State each design decision as a short code comment near the relevant
  code. Keep the assignment's own intent (show design reasoning) but
  follow this repo's comment style: one line, state the why, not the what.
- List any external asset source in the root `README.md` when the asset
  is not part of the default Unity template or an installed package.

## Repo conventions
- Put new gameplay scripts under `Trees for All/Assets/Scripts/`.
- Do not commit `Library/`, `Temp/`, `Logs/`, `UserSettings/`, or other
  paths already listed in `.gitignore`.
- Do not commit secrets or personal API keys. Use `.env.example`-style
  placeholders if a script needs a key.

## Coding style
- Use /Assets/Scripts as main script folder
- Use namspaces e.g. /Assets/Scripts/Interaction -> CompanyName.Interaction
- Use descriptive naming
- Small methods
- Single responsibility
- Explicit dependencies
- Readonly where appropriate
- Private by default
- Prefer set/get to get access to private variables over making the variable public.
- Prefer Events/Scriptable objects instead of direct references when possible.
- Prefer reusability and modularity.
- Write performance optimized code.

## Workflow
- Ask before installing packages

## Comment / Documentation style
- Write comments that explain "why". A few high level comments explaining the purpose of classes or methods is very helpful. Comments explaining tricky code are also helpful.
- Avoid comments that are redundant with the code. Do not comment before each line of code explaining what it does unless there is something that is not obvious going on.

## Things to avoid
- Don't add third-party assets without asking.
- Giant MonoBehaviours.
- God classes.
- Direct references.
- Static global state.