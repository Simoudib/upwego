# Enhanced 3D Player Movement System

This system provides a complete first-person character controller with mouse look camera control for Unity games using Mirror networking.

## Features

- Smooth first-person camera control with mouse
- WASD movement relative to camera direction
- Sprinting with Left Shift
- Crouching with Left Control or C key
- Jumping with Space
- Cursor lock/unlock with Escape key
- Networked movement with Mirror
- Ground checking to prevent mid-air jumps
- Adjustable sensitivity and movement parameters

## Setup Instructions

1. Add the `PlayerMovementHandler` script to your player character GameObject
2. Make sure your player has a `CharacterController` component
3. Create an empty GameObject as a child of your player and position it at ground level
4. Assign this GameObject to the `Ground Check` field in the inspector
5. Set the `Ground Mask` to include the layers that should be considered ground
6. Optionally create a camera holder GameObject as a child of your player at eye level
7. Assign this holder to the `Camera Holder` field (if not assigned, one will be created automatically)
8. Make sure you have a camera with the "MainCamera" tag in your scene

## Controls

- **W/A/S/D**: Move forward/left/backward/right
- **Mouse**: Look around
- **Space**: Jump
- **Left Shift**: Sprint
- **Left Ctrl or C**: Crouch
- **Escape**: Toggle cursor lock

## Customization

You can adjust the following parameters in the Inspector:

### Movement Settings
- **Move Speed**: Base movement speed
- **Sprint Multiplier**: Speed multiplier when sprinting
- **Crouch Multiplier**: Speed multiplier when crouching
- **Jump Height**: Maximum height of jumps
- **Gravity**: Gravity strength (negative value)

### Camera Settings
- **Mouse Sensitivity**: How quickly the camera rotates with mouse movement
- **Invert Mouse Y**: Whether to invert the vertical mouse axis
- **Vertical Look Min Max**: Minimum and maximum vertical look angles

## Notes

- This script is designed to work with Mirror networking
- The script automatically handles local vs. network player behavior
- Camera and movement only function for the local player