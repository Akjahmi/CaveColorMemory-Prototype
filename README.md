# Cave Color Memory

A two-player memory game for the CAVE at HTW Berlin (Projektstudium, Informatik in Kultur und Gesundheit, 3rd semester).

The CAVE floor is divided into 16 coloured tiles (4 × 4) in four colours. Each player names a colour for the other player. The colours are visible for 6 seconds and then disappear. The players must remember where their colour was and step onto the right tiles.

**Status:** prototype. Everything runs on a normal PC with the keyboard. The Kinect parts (walking, selecting by standing still, the new-round gesture) are written but still need testing and tuning in the real CAVE.

## How to play

1. **Choose colours.** Player 1 says (or types) a colour for Player 2. Then Player 2 chooses a colour for Player 1. The colours are Red, Blue, Green and Yellow.
2. **Memorise.** All 16 tiles show their colour for 6 seconds.
3. **Play.** The tiles turn grey. Each player has 4 attempts. A player selects a tile by standing on it for about one second. The tile then shows its real colour.
4. **Result.** When both players have used their 4 attempts, the score appears on the front wall. Raising one hand above the head starts a new round.

Each player gets one point for every tile of their colour. There are 4 tiles of each colour.

## Controls

| Action | In the CAVE | On the PC |
|---|---|---|
| Choose a colour | Say "Red", "Blue", "Green" or "Yellow" | Type the colour name, then Enter |
| Move | Walk on the floor | Player 1: W A S D · Player 2: arrow keys |
| Select a tile | Stand on it for 1.2 seconds | Player 1: Space · Player 2: Enter |
| New round | Raise one hand above your head for 1 second | R |

There is also a mouse test: left click selects a tile for Player 1, right click for Player 2.

## Setup

- **Unity 2022.3.59f1** (Built-in Render Pipeline)
- **CAVE package of HTW Berlin** (`de.htw-berlin.cave`), already listed in `Packages/manifest.json`. The sample "Example CAVE Setup" is imported under `Assets/Samples`.
- **Kinect for Windows SDK 2.0** on the CAVE computer
- **Voice input:** Windows with a microphone, and microphone access allowed for desktop apps (Windows Settings → Privacy → Microphone). The speech recognition uses the Windows speech language, so English colour names work best on an English Windows. Without it, typing still works.

**Start:** open the `My project` folder in Unity, open `Assets/Scenes/ColorMemoryCaveTest.unity` and press Play.

**Scenes:**

| Scene | Use |
|---|---|
| `ColorMemoryCaveTest` | Main scene with the CAVE prefab. Use this one, also on the PC. |
| `ColorMemoryScene` | Old PC-only test scene without the CAVE. |

**Displays:** on a single screen, Display 1 shows the normal top-down PC view with a text panel. With several screens (the real CAVE), the PC camera and the panel switch off and the text appears on the front wall. In the editor, the Game view can show Display 2 (front wall) and Displays 6 and 7 (floor, left and right eye). The floor looks too close from the desk; that is normal, because the CAVE draws it from the player's head position.

## Scripts

All scripts are in `Assets/Scripts/Main`.

| Script | Job |
|---|---|
| `GameManager.cs` | **Main script.** The rules: phases, colour choice, attempts, score. Adds the helper scripts automatically. |
| `GameState.cs` | The four phases: ChoosingColors, Memorize, Playing, GameOver. |
| `Board.cs` | The floor: creates the tiles, fits them to the CAVE floor, shuffles, shows, hides and reveals colours, turns a position into a tile. |
| `ColorPalette.cs` | The four colours and their names. |
| `PlayerTracker.cs` | Kinect: hip position of up to two players, selecting by standing still, the new-round gesture. |
| `KeyboardInput.cs` | Keyboard: typing colours, moving, selecting, R for a new round. |
| `VoiceInput.cs` | Microphone: recognises the four colour names (Windows speech recognition, offline). |
| `CaveClickSimulator.cs` | Mouse test for selecting tiles. |
| `GameHud.cs` | Text panel on the PC screen. |
| `WallDisplay.cs` | Text on the CAVE front wall. |
| `RoundSaver.cs` | Saves every round as a JSON file. |
| `RoundResultData.cs` | The structure of the saved JSON file. |

**Saved rounds** are written to `Application.persistentDataPath/RoundResults` (on Windows usually `C:\Users\<name>\AppData\LocalLow\DefaultCompany\My project\RoundResults`). Each file holds the board colours, each player's target colour, selected tiles and score.

## Ideas taken from other projects

- **Hip tracking and border tolerance** from the CaveShow26 floor demo (`Bodenspiel`): the hip (SpineBase) is more stable than the feet, and a player must step a few centimetres past a tile border before the tile changes.
- **Arm-raise gesture and Windows speech recognition** from the CaveShow26 `CaveRunner`.
- **Game states and small separate scripts** from TetrisCave2026.

## Values to tune in the CAVE

All of these can be changed in the Inspector without editing code.

| Value | Where | Default | What it does |
|---|---|---|---|
| Dwell Time | Player Tracker | 1.2 s | How long a player stands on a tile to select it |
| Hysteresis | Board | 0.08 m | How far past a tile border before the tile changes |
| Hand Above Head | Player Tracker | 0.1 m | How high a hand must be for the new-round gesture |
| Raise Hold Time | Player Tracker | 1 s | How long the hand must stay up |
| Restart Delay | Player Tracker | 2 s | Pause after game over before the gesture counts |
| Preview Duration | Game Manager | 6 s | How long the colours are visible |
| Height | Wall Display | 1.6 m | Height of the text on the front wall |

## Known limits

- **Two players, one Kinect:** one player can hide the other from the sensor. Then that player's marker stops until they are seen again.
- **Player order:** the first person the Kinect sees becomes Player 1. If both are lost and found again, the numbers can swap.
- **Voice:** whoever speaks during a turn chooses the colour; the game cannot tell the two voices apart.
- **Both players may choose the same colour.**
