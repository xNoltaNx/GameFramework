# Trigger and Event System - Quick Start Guide

This guide will get you up and running with the GameFramework trigger and event system in just a few minutes.

## Overview

The trigger and event system provides a powerful, designer-friendly way to create interactive gameplay without writing code. It consists of:

- **Event Channels**: ScriptableObject-based events for decoupled communication
- **Triggers**: Components that detect conditions and fire events
- **Actions**: Components that respond to trigger events
- **Conditions**: Components that add logic to trigger evaluation

## Quick Setup

### 1. Create Event Channels

Right-click in your Project window and navigate to:
`Create > GameFramework > Events`

Create these example events:
- **Game Event**: "Player Entered Area"
- **Int Event**: "Score Changed"
- **Vector3 Event**: "Player Position"

### 2. Add a Collision Trigger

1. Create a GameObject with a Collider (set as Trigger)
2. Add Component: `GameFramework > Events > Collision Trigger`
3. Configure:
   - Trigger Event: On Enter
   - Trigger Layers: Set to Player layer
   - Required Tag: "Player"

### 3. Add Actions

Add these action components to the same GameObject:
- `GameFramework > Events > Actions > Raise Game Event Action`
- `GameFramework > Events > Actions > Move Action`

Configure the Raise Game Event Action:
- Game Event: Assign your "Player Entered Area" event

### 4. Add Event Listeners

1. Create another GameObject
2. Add Component: `GameFramework > Events > Game Event Listener`
3. Configure:
   - Game Event: Assign your "Player Entered Area" event
   - On Event Raised: Add actions (e.g., activate a door, play sound)

## Common Patterns

### Door System
```
Trigger (Collision) → Action (Raise Event) → Listener (Move Door)
```

### Collectible System
```
Trigger (Collision) → Actions (Destroy Self + Raise Score Event) → Listener (Update UI)
```

### Area Detection
```
Trigger (Proximity) → Action (Raise Position Event) → Listener (Update Minimap)
```

### Timed Events
```
Trigger (Timer) → Action (Raise Event) → Listener (Spawn Enemy)
```

## Best Practices

1. **Use Event Channels** for communication between different systems
2. **Group related triggers** under parent GameObjects for organization
3. **Name your events clearly** (e.g., "PlayerDied" not "Event1")
4. **Use conditions** to add complex logic without scripting
5. **Enable debug mode** during development for better visibility

## Performance Tips

- Add a **Trigger Manager** to your scene for automatic optimization
- Use **spatial partitioning** for large numbers of proximity triggers
- Set appropriate **update intervals** for proximity triggers
- Use **conditions** instead of complex trigger logic

## Debugging

- Enable Debug Mode on triggers to see evaluation logs
- Use the context menu "Test Trigger" to manually fire triggers
- Check Event Channel statistics in the inspector during play mode
- Use the Performance Monitor component to track system performance

## Next Steps

- Explore the example scenes in `Assets/Scenes/EventExamples/`
- Read the complete documentation in `Assets/Scripts/Events/Documentation/`
- Check out advanced features like custom conditions and actions
- Learn about Visual Scripting integration

## Support

For more help:
- Check the API documentation
- Look at example scripts in `Assets/Scripts/Events/Examples/`
- Review the component tooltips and help boxes
- Use the built-in context menu options for testing