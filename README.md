# Unity VR C# TableTennis CodeBase
 Table tennis project in C#

This is a small fun project made with the Massive Loop SDK for a Massive Loop game world!

Feel free to check it out at : https://home.massiveloop.com/
and our SDK is available for download at : https://massiveloop.com/download/sdk/

With documentation available at : https://docs.massiveloop.com/

We recently have allowed the usage of C# scripting which gives a lot of freedom to our creators.
This is one of many fun projects that I've started in order to test out the limits of our new C# compiler.
Also I think that creating a fun small Table Tennis game that could be played in VR & Multiplayer sounds pretty fun!

Initial AI video down below : 
=================
[![Video](https://img.youtube.com/vi/0XjjUWD9HGE/0.jpg)](https://youtu.be/0XjjUWD9HGE)

# 🎵 Jukebox Script for Unity

This script powers a fully interactive jukebox in Unity, complete with dynamic song playback, an audio visualizer, and user-friendly controls. Designed for projects that require immersive audio experiences, the Jukebox script integrates seamlessly into Unity's audio system and includes customizable features for enhanced functionality.

## Key Features
- **Audio Playback**: Supports a playlist of audio clips with play, pause, skip forward, and skip backward functionality.
- **Autoplay**: Automatically plays the next song in the playlist when the current one finishes, with the ability to toggle autoplay on or off.
- **Visualizer Integration**: Generates dynamic visual effects synchronized with the audio spectrum data for an engaging visual experience.
- **Volume Control**: Provides adjustable volume settings with smooth transitions.
- **Progress Bar**: Displays the current playback time of the song via a slider that updates in real time.
- **Customizable Buttons**: Includes clickable buttons for all playback controls, with visual feedback to indicate states like pause/play and autoplay on/off.
- **Song Title Display**: Displays the name of the current song using Unity's TextMeshPro.

## How It Works
1. Attach the script to a GameObject in your Unity scene.
2. Configure the `Songs` array with your desired audio clips.
3. Hook up the clickable buttons and visualizer GameObjects in the Unity Inspector.
4. Enjoy an immersive jukebox experience with autoplay, real-time visualizer updates, and easy-to-use controls.

This script is ideal for interactive audio experiences in games, VR/AR environments, or any Unity-based application where music playback and visualization are required.

---
# Dynamic Size and Throw Script for Unity (VR)

This script adds dynamic interactivity to VR objects using the **MLGrab** system, enabling realistic resizing and throwing mechanics. Perfect for VR projects requiring intuitive and immersive object manipulation.

## Features

- **Primary Grab (One-Handed Interaction):**
  - Allows picking up objects and holding them in hand.
  - Objects can be thrown with realistic impulse physics when released.
  - Throw force is customizable and clamped to ensure consistent behavior.

- **Secondary Grab (Two-Handed Interaction):**
  - Enables resizing of objects based on the distance between hands.
  - Size changes are dynamically applied in real-time while holding the object.
  - Adjustable minimum and maximum scale limits for precise control.

- **Customizable Parameters:**
  - `minSize` and `maxSize`: Relative scale limits for resizing.
  - `absoluteMinSize` and `absoluteMaxSize`: Absolute size limits to prevent extreme scaling.
  - `throwForceMultiplier`: Strength of the throwing force.
  - `maxThrowForce`: Caps the maximum force applied during a throw.

- **Realistic Physics Integration:**
  - Dynamically calculates and applies force in the hand's forward direction on release.
  - Ensures smooth and predictable interactions using Unity's Rigidbody system.

- **Event-Driven System:**
  - Custom event handling for triggering additional behaviors (e.g., network synchronization) when throwing objects.

## How It Works

1. **Primary Grab:**
   - When the object is grabbed, it follows the motion of the player’s hand.
   - On release, a force is applied in the hand's forward direction, simulating a realistic throw.

2. **Secondary Grab:**
   - When both hands grab the object, the script tracks the distance between them.
   - The object's scale changes dynamically based on the hand distance, with customizable limits to prevent excessive resizing.

3. **Physics Integration:**
   - A Rigidbody component ensures the object interacts naturally with the environment after release or resizing.

## Usage

- Attach the script to an object with an **MLGrab** component.
- Assign the `objectToChange` field to the target GameObject.
- Ensure the object has a Rigidbody component for physics interactions.
- Configure size and throw parameters in the Inspector for your specific use case.

## Example Code Behavior

- When grabbing and releasing, the console logs the applied throw force for debugging:
  ```plaintext
  Object thrown with impulse force: [force_vector]


# DodgeBall Game Manager in Unity

## Overview

The `DodgeBallGameManager` script manages a team-based dodgeball game in Unity. It controls player interactions, team assignments, game state management, teleportation, and scoring. This script uses **MassiveLoop SDK** for multiplayer support and **TextMeshPro** for UI updates.

---

## Key Features

- **Team Selection**: Players are assigned to either the Blue or Red team.
- **Game Start & End Management**: Handles game activation, countdown timers, and results.
- **Teleportation**: Teleports players to the arena or back to spawn points.
- **Scoring System**: Keeps track of team scores and updates the UI.
- **Ball Reset**: Resets the game balls to their original position.

---

## Dependencies

- **MassiveLoop SDK**: For multiplayer features (`MLPlayer`, `MLClickable`, and network events).
- **TextMeshPro**: For displaying team details, scores, and timers.
- **UnityEngine.UI**: Required for basic UI interaction.

---

## Script Breakdown

### Variables

| **Type**                   | **Name**                          | **Description** |
|----------------------------|-----------------------------------|-----------------|
| `GameObject`               | BlueTeleportObjectLocation        | Blue team's teleport location. |
| `GameObject`               | RedTeleportObjectLocation         | Red team's teleport location. |
| `GameObject`               | ResetBallPosition                 | Position to reset game balls. |
| `GameObject`               | GameBall / GameBalls              | The game ball(s) in the arena. |
| `ML.SDK.MLClickable`       | taggersClickable / runnersClickable | Clickables for selecting teams. |
| `ML.SDK.MLClickable`       | StartGameClickable                | Clickable to start the game. |
| `TextMeshPro`              | timeText                          | Displays the remaining time. |
| `TextMeshPro`              | GameStatusText                    | Displays game status text. |
| `TextMeshPro`              | BlueTeamString / RedTeamString    | Displays team members. |
| `TextMeshPro`              | BlueTeamScore / RedTeamScore      | Displays team scores. |
| `Stopwatch`                | stopwatch                         | Tracks elapsed game time. |
| `List<string>`             | blueTeam / redTeam                | Holds player names for each team. |
| `int`                      | blueTeamNumberScore / redTeamNumberScore | Tracks each team's score. |
| `bool`                     | isGameActive                      | Game state flag. |
| `Collider`                 | ArenaCollider                     | Arena collider for teleport checks. |
| `GameObject`               | BlueCelebration / RedCelebration  | Celebration effects for winning teams. |

---

### Key Methods

#### 1. **OnStartGameEvent**
```csharp
public void OnStartGameEvent(object[] args)
