### Trees for All

Please refer to the "Opdrachtomschrijving XR developer Sogeti.pdf" for the assignment.

## Notes and Disclaimers
1. This project was build by `Jasper van den Barg` with the help of `AI agents`
2. There was no access to VR hardware or a Quest 3 during the development so all testing was done in the Unity editor.
3. Decisions were made based on the point 2.
4. I did not receive any response on point 2 until `15-09-2026`, the day before the technical interview. By this time the assignment was already largely finished.
5. On editor startup ignore `UnityAtoms` errors, just clear them and it will be fine.

## Design Decisions

# Gameplay Decisions
1. UI was designed based on testing limitations so menus and time/score is wrist bound because HUD positioning could not be tested properly.
2. Controls are also based on testing limitations. `Controllers` were picked over `Hands` so there are no grabables because this was difficult for testing.
   Planting seeds is done through a Ray to help with testing.
   Watering can is locked to controller to help with testing.
   Movement is done through teleportation.

# Technical Decisions
1. 

# What I would have done differently
1. 

## How to play

# Testing on Quest 3
Build the project and install the apk on the Quest 3

# Testing the game on PC with a mouse and keyboard. No headset is needed.

**Start**
1. A panel spawns in front of you. Aim at it with `Y`, the right hand ray
   planting and the tool menu also use, and click a button with the left
   mouse button.
2. `Controls` shows the control list. `Back` returns to the start screen.
3. `Play` starts the round and hides the panel. Teleport, the tool menu,
   planting and watering all unlock at the same moment.

**Move**
1. Hold the right mouse button to look around.
2. Press `T` to aim the teleport arc.
3. Hold `W` to raise the arc. Release `W` to teleport.
4. Press `1` to toggle between controller input and walking.
5. Press `R` to toggle between translation and rotation.

**Pick a tool or a seed**
1. Press `T`, then `N`, to open the menu on the left wrist. This hides the HUD.
2. Press `Y` to aim the right hand ray. Click an entry to pick it.
3. The menu marks one entry at a time, because the hand holds one tool at a time.
4. Press `N` again to close the menu. The HUD shows again.

**Plant a seed**
1. Press `Y` to aim the seed ray.
2. Aim at green ground. Click the left mouse button to plant.
3. Red ground blocks planting. Read the label for the reason.

**Water a plant**
1. Pick the watering can from the menu.
3. Tilt past 30 degrees to pour.
4. Dip the spout in the pond to refill the can.

**Goal**
- The round lasts 3 minutes. A HUD on the left wrist shows the time and the
  score once you press `Play`. It shares its spot with the tool menu, so only
  one of the two shows at a time.
- Grow each plant through 3 stages.
- Water a plant before its meter empties. An empty meter kills the plant.
- Earn points for each stage a plant reaches. Score as many points as possible.
- When the clock hits zero, a panel shows your score and a `Restart` button.
  Restart reloads the level, so the score never carries over between rounds.

## Packages used:
- Terrain Toolkit: tilled grass and rock textures
- Unity Atoms: https://github.com/unity-atoms/unity-atoms.git
- Environment Assets
- - https://assetstore.unity.com/packages/3d/vegetation/trees/polygon-trees-224068
- Garden realistic tools: the watering can model
- - https://assetstore.unity.com/packages/3d/props/tools/garden-realistic-tools-68960

## Project art
Twinkle sounds for tree growth
- https://freesound.org/people/Chanhun/sounds/161322/

Watering sound
- https://freesound.org/people/wobesound/sounds/488401/
- https://freesound.org/people/wyronroberth/sounds/516257/ 

Made for this project, not third party.

- `Assets/Textures/UI/Droplet.png`: the water meter droplet.
- `Assets/Textures/UI/SeedIcons/`: one menu icon per seed. Baked from the last
  growth stage of each seed by `Trees for All/Bake Seed Icons`, so the art comes
  from Polygon Trees above.
- `Assets/Textures/UI/ToolIcons/ToolIconWateringCan.png`: the menu icon for the
  watering can.