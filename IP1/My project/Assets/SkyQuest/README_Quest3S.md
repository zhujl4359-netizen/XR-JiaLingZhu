# Sky XR: Two Independent Play Modes

The project provides `SkyDesktop.unity` for keyboard play in Unity and `SkyQuest3S.unity` for standalone Meta Quest 3/3S play. The two scenes share flight, ancestors, social actions, items, communication, and the extended sky sanctuary. Android builds use the Quest scene.

For every key and Touch controller action, see [SkyQuest_Controls_Quest3S_and_Keyboard.md](SkyQuest_Controls_Quest3S_and_Keyboard.md).

## Sky Sanctuary

Follow the golden path west from the spawn point to reach the sanctuary. The expanded area includes a vaulted hall, west cloister, four-light obstacle courtyard, northern circular plaza and sky gate, eastern theater, and character displays. Floors, walls, and columns have collision. The courtyard's four stone walls have alternating left and right gaps. Walk around or fly over them; lighting all four blue lanterns raises the exit gate.

The COTL traveler, 2021 pilgrim, Shibiyama dancer, prayer character, skeleton, and original C1 player character are displayed on the building grounds with their feet aligned to floors or pedestals. The original C1 controls are disabled to avoid conflicts. The antler mask is on a west cloister display.

Walkable floor area grew from approximately 1,259 to 5,094 square meters. The architecture uses existing project models and lightweight procedural geometry. Detailed stone, metal, wood, cloth, foliage, bark, and cloud materials are under `Assets/SkyQuest/Environment`.

## Comfort and validation

Ground and flight speeds have gentle acceleration. Turning is one 20-degree snap per thumbstick flick; tracked head motion has no artificial shake or roll. Movement speed is adjustable from 0.5x to 2.0x. Ground movement uses body collision and automatic stepping over obstacles up to about 0.38 m. Optional teleport and seated height calibration are available.

Unity version: 6000.3.20f1. The Quest build output is `Build/SkyQuest3S.apk`; the keyboard standalone output is `Build/SkyDesktopDemo.app`. Editor and build checks cannot replace headset testing of frame rate, comfort, and voice recognition.
