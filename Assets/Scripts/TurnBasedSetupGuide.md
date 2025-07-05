# Turn-Based System Setup Guide

## Overview
Your grid-based movement system has been converted to a turn-based system where players can control multiple characters, each with limited movement points per turn.

## New Scripts Created

### 1. Character.cs
- Replaces `PlayerMovement.cs` with turn-based functionality
- Features:
  - Movement points per turn (default: 3)
  - Visual selection indicators
  - Can be used for both player and enemy characters
  - Event system for turn management

### 2. TurnManager.cs
- Manages the overall turn-based gameplay
- Features:
  - Character selection and turn switching
  - Movement point tracking
  - Phase management (Player vs Enemy turns)
  - Input handling

### 3. TurnBasedUI.cs
- Provides visual feedback for the turn system
- Features:
  - Shows current turn and selected character
  - Displays movement points
  - Character status indicators with color coding
  - Control instructions

## Setup Instructions

### Step 1: Replace PlayerMovement with Character
1. Remove the `PlayerMovement` component from your player objects
2. Add the `Character` component instead
3. Configure the Character settings:
   - `Character Name`: Give each character a unique name
   - `Is Player Controlled`: Check for player characters, uncheck for enemies
   - `Max Movement Points`: How many moves per turn (default: 3)
   - `Start X/Y`: Starting grid position
   - `Selection Indicator`: Optional GameObject to show when selected

### Step 2: Set Up TurnManager
1. Create an empty GameObject in your scene called "TurnManager"
2. Add the `TurnManager` component
3. Configure settings:
   - `Player Characters`: Drag your player Character objects here
   - `Enemy Characters`: Leave empty for now (enemies not implemented yet)
   - `Auto End Turn When No Movements`: Automatically end turn when character has no moves left
   - `Turn Transition Delay`: Time between turn changes

### Step 3: Set Up UI (Optional but Recommended)
1. Create a Canvas in your scene if you don't have one
2. Create UI Text elements for:
   - Turn info (shows current phase and character)
   - Movement points (shows remaining moves)
   - Instructions (shows controls)
3. Add the `TurnBasedUI` component to a GameObject
4. Link the UI Text elements to the script
5. For character status displays, create a parent UI element and assign it to `Character List Parent`

### Step 4: Create Selection Indicators
1. For each character, create a simple GameObject (like a colored circle or arrow)
2. Position it as a child of the character
3. Assign it to the character's `Selection Indicator` field
4. The system will automatically show/hide these indicators

## Controls

- **WASD**: Move the selected character
- **Tab**: Switch between player characters
- **Space**: End the current character's turn
- **Enter**: End the current phase (switch between player and enemy turns)

## Visual Feedback

The UI system provides color-coded feedback:
- **Green**: Currently selected character
- **White**: Available character with moves remaining
- **Red**: Character with no moves remaining

## System Features

### Movement Points
- Each character starts their turn with full movement points
- Moving in any direction costs 1 movement point
- When a character runs out of movement points, their turn can end automatically
- Characters regain all movement points at the start of their next turn

### Turn Flow
1. **Player Phase**: Players control their characters one at a time
2. **Character Selection**: Use Tab to switch between characters
3. **Movement**: Use WASD to move the selected character
4. **Turn End**: Either use all movement points or manually end turn
5. **Enemy Phase**: Currently auto-skips back to player phase (enemies not implemented)

### Future Enemy Support
The system is designed to easily add enemy characters:
1. Create Character objects with `Is Player Controlled` unchecked
2. Add them to the TurnManager's `Enemy Characters` list
3. Implement AI logic in the `StartEnemyTurn()` method of TurnManager

## Customization

### Movement Points
- Adjust `Max Movement Points` in the Character component
- Different characters can have different movement allowances

### Visual Indicators
- Customize selection indicators for each character
- Modify UI colors in the TurnBasedUI component
- Add character portraits or other visual elements

### Turn Timing
- Adjust `Turn Transition Delay` for faster/slower turn changes
- Modify the `autoEndTurnWhenNoMovements` setting

## Troubleshooting

### Characters Not Moving
- Ensure the character has movement points remaining
- Check that it's the player's turn
- Verify the character is selected (should show selection indicator)

### UI Not Updating
- Make sure UI Text components are assigned in TurnBasedUI
- Check that TurnManager is found by TurnBasedUI (same scene)

### Character Selection Issues
- Verify characters are added to TurnManager's Player Characters list
- Ensure `Is Player Controlled` is checked on player characters
- Check that characters have selection indicators assigned

The system is now ready to use! You can easily expand it by adding more characters, implementing enemy AI, or adding special abilities that cost different amounts of movement points. 