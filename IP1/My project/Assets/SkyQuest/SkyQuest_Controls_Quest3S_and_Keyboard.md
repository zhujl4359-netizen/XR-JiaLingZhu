# SKY XR Controls and Interaction Guide

This guide covers `SkyQuest3S.unity` for Meta Quest 3/3S and `SkyDesktop.unity` for keyboard play. The game uses only the headset, Touch controllers, or keyboard.

## Quest 3S: Movement, view, and flight

| Action | Control |
|---|---|
| Look around | Turn your head naturally. Head tracking has no added camera shake. |
| Walk | Move the left thumbstick. |
| Adjust movement speed | Point the right controller at the slider in the upper-right of your view and hold the right index trigger. Drag between 0.5x and 2.0x. |
| Snap turn | Flick the right thumbstick left or right for one 20-degree turn. Return it to center before the next turn. |
| Switch first-person / third-person view | Press right B. |
| Take off | Hold both index triggers and swing both hands downward once. |
| Glide | Extend both arms. Controller direction steers flight; different hand heights steer turns. |
| Rise / descend | Pitch your hands while gliding. Bring your arms inward to descend slowly. |
| Wind currents | Glide into a blue current for lift and controller feedback. |
| Collect energy | Approach a floating golden orb. |
| Optional teleport | Press right A+B together to toggle teleport mode. Aim at the ground and press the right trigger. |
| Seated calibration | Click both thumbsticks together to reset height for your current seated position. |

Walking, flight, and turning use gentle acceleration. Ground movement steps over obstacles up to about 0.38 m high and follows stairs downward. The body collider prevents walking through walls and columns.

## Quest 3S: Physical interactions

- **Pick up a lantern:** Move your right hand near a blue lantern and hold the right grip.
- **Light a lantern:** While holding it, press the right index trigger. The light turns on and the controller vibrates.
- **Light a candle:** Move your left hand near a small candle and press the left index trigger.
- **Push the stone gate:** Put both hands near the gate and hold both grips. The gate rises with the push. It also opens after all four lanterns are lit.

Wind, water, candles, and ancestors have positioned audio sources. The hall uses mild reverb. Flight adds sparse cloud particles, while banners and flames respond to the environment.

## Quest 3S: Ancestors, item tree, and mask

1. Every gallery character except your playable avatar has a carved memorial behind it: the Shibiyama dancer, Sky prayer guardian, COTL traveller, Candle pilgrim, Skeleton pilgrim, and original C1 character. Approach a memorial to summon its character automatically (about 6 m); move about 8 m away to dismiss it.
2. Approach the summoned character. Point the right controller at the star icon above its head and press the right index trigger to open that character's Magic tree. Left X also opens or closes the nearest tree.
3. Point at a Magic node with the right controller and hold the right index trigger for about 0.55 seconds. A distinct colored, material-finished reward cube appears beside that ancestor. Each character has five different Magic entries and tracks its exchanges separately.
4. Near the starting area, approach the carved memorial stone. The original Ancestor appears automatically within about 6 m and disappears after you move about 8 m away.
5. Approach the summoned Ancestor. A star icon appears above its head. Point the right controller at the icon and press the right index trigger to open the original cape, hair, mask, emote, and instrument tree on the right side of your view. Left X is an alternate open/close control while nearby.
6. In the original item tree, point at an item with the right hand and hold the right index trigger for about 0.55 seconds to obtain it.
7. The original item moves from the tree to the right hand and shows `OBTAINED`.
8. Press right A to equip the held original item. A mask appears on the avatar's face and shows `EQUIPPED`.
9. The tree closes if you move too far from its ancestor.

## Quest 3S: Social actions and stories

1. **Share heart fire:** Stand near an ancestor, hold both hands at your chest, and hold both grips for about 0.6 seconds.
2. **Offer a white candle:** Point your right hand at the ancestor's chest and hold the right index trigger for about 0.7 seconds.
3. **Greet an ancestor:** Wave your right hand left and right four times. Story stars then unlock.
4. **Read a story:** Point at a story star and press the right trigger.

Each step has a status message, short confirmation sound, and controller feedback. Later story stars appear only after earlier steps are complete.

## Quest 3S: Tea table and communication

| Action | Control |
|---|---|
| Preview tea table | Press left Y. |
| Move preview | Aim the right controller at walkable ground. |
| Place tea table | Press the right index trigger while aiming at level ground. |
| Cancel preview | Press left Y again. |
| Open spatial keyboard | Click the left thumbstick. |
| Type | Point the right controller at a QWERTY key and press the right index trigger. |
| Delete / space / send | Press `DEL`, `SPACE`, or `SEND`. |
| Voice input | Press `MIC`. Recognized text enters a draft before you send it. |
| Close keyboard | Click the left thumbstick again. |

The spatial keyboard remains available if voice recognition is unavailable. Sent messages are stored for the current play session.

## Keyboard-only play: `SkyDesktop.unity`

Open `Assets/SkyQuest/SkyDesktop.unity` in Unity and press Play.

| Action | Key |
|---|---|
| Move | `WASD`; movement follows the current mouse-facing direction. |
| Adjust movement speed | Drag the slider at the upper-right of the game window from 0.5x to 2.0x. The setting is saved. |
| Look and turn | Move the mouse to turn smoothly; left / right arrow also snap-turn 20 degrees per press. |
| Look up / down | Up / down arrow. |
| Take off / rise / descend | `Space` to take off or rise; `Left Ctrl` to descend. |
| First-person view | `1` |
| Third-person view | `3` |
| Summon a gallery ancestor | Approach any of the six carved gallery memorials; its character appears within about 6 m and disappears beyond about 8 m. |
| Open a Magic exchange tree | Approach the summoned character and click its star icon. `F` also opens or closes the nearest tree. The tree appears on the right. |
| Summon the original Ancestor | Approach the carved memorial stone near the starting area. The Ancestor appears automatically and disappears when you move away. |
| Open the original item tree | Approach the summoned Ancestor and click the star icon above its head. The exchange tree appears on the right. `F` is an alternate open/close key. |
| Choose a Magic or original item | `1` to `5` while its tree is open. |
| Exchange Magic / obtain original item | `Enter` |
| Equip held item | `E` |
| Share heart fire / offer candle / greet | `H` / `C` / `G` |
| Read stories | `7` / `8` / `9` |
| Preview / place tea table | `P` / `R` |
| Communication panel | `K` to open; type; `Enter` to send; `Esc` to close; `Backspace` to delete. |
| Optional teleport | `T` toggles teleport mode; look at the ground with the arrow keys and press `V`. |
| Seated calibration | `F1` |

Keyboard and Quest scenes share the same ancestor, wind, energy, obstacle, tea table, and communication systems.

Mouse controls the desktop character's facing direction. The camera keeps a fixed third-person distance from the character, and movement stops immediately when WASD is released.

## Scene files

- Quest: `Assets/SkyQuest/SkyQuest3S.unity`
- Keyboard: `Assets/SkyQuest/SkyDesktop.unity`
- Main interaction scripts: `SkyQuestLocomotion.cs`, `SkyQuestAncestor.cs`, `SkyQuestSocial.cs`, `SkyQuestProps.cs`, `SkyQuestCommunication.cs`

The design report's sky exploration, flight, ancestor interactions, view switching, comfort options, communication, and tea table placement are mapped to these systems.
