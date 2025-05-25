# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity GameFramework (Unity 6000.1.4f1) designed for modular, component-based game development. The framework emphasizes Clean Code principles and enables rapid prototyping through swappable systems for indie teams.

## Architecture

### Component-Based Design
The framework uses interface-driven architecture with swappable components:

- **FirstPersonCharacterController**: Central coordinator that orchestrates all subsystems
- **IInputHandler**: Contract for input handling (currently PlayerInputHandler)  
- **ICameraController**: Contract for camera control (currently FirstPersonCameraController)
- **ILocomotionController**: Contract for movement systems (currently FirstPersonLocomotionController)
- **IInventoryController**: Contract for item collection management (currently InventoryController)
- **IEquipmentController**: Contract for item attachment and equipment (currently EquipmentController)
- **IInteractionController**: Contract for world interaction detection (currently InteractionController)

### Dependency Flow
```
FirstPersonCharacterController (Coordinator)
├── IInputHandler → handles Unity Input System integration
├── ICameraController → manages first-person camera controls  
├── ILocomotionController → handles movement, jumping, crouching
├── IInventoryController → manages item collection and storage
├── IEquipmentController → handles item attachment to bones/transforms
└── IInteractionController → detects and handles world interactions
```

### Key Patterns
- **Interface Segregation**: Each interface focuses on single responsibility
- **Dependency Injection**: Components can be swapped via setter methods
- **Auto-Configuration**: Missing components are automatically created and configured
- **Composition over Inheritance**: Systems are composed rather than inherited

## Input System

The project uses Unity's Input System with comprehensive action maps in `Assets/InputSystem_Actions.inputactions`:

**Player Actions**: Move, Look, Attack, Interact (Hold), Crouch, Jump, Previous, Next, Sprint
**UI Actions**: Navigate, Submit, Cancel, Point, Click, RightClick, MiddleClick, ScrollWheel, TrackedDevice controls

**Supported Control Schemes**: Keyboard&Mouse, Gamepad, Touch, Joystick, XR

## Render Pipeline

Universal Render Pipeline (URP 17.1.0) with separate configurations:
- Mobile: `Assets/Settings/Mobile_RPAsset.asset`
- PC: `Assets/Settings/PC_RPAsset.asset`

## Key Dependencies

```json
"com.unity.inputsystem": "1.14.0"
"com.unity.render-pipelines.universal": "17.1.0"  
"com.unity.test-framework": "1.5.1"
"com.unity.ai.navigation": "2.0.7"
```

## Development Commands

Since this is a Unity project, standard Unity Editor workflows apply:
- Open project in Unity Editor
- Build via Build Settings (File → Build Settings)
- Run tests via Test Runner (Window → General → Test Runner)
- Use Unity's built-in profiler for performance analysis

## Item System

### ScriptableObject-Based Items
- **ItemDefinition**: Base class for all items with name, description, icon, weight, value
- **EquippableItemDefinition**: Extends ItemDefinition for equippable items with slots and attachment data

### World Interaction
- **WorldItem**: Component for items in the world, implements IInteractable
- **InteractionController**: Detects nearby interactables via raycast and sphere detection
- Uses "Interact" input action (E key by default)

### Inventory & Equipment
- **InventoryController**: Manages item stacks with configurable capacity
- **EquipmentController**: Handles attachment to transforms/bones with configurable slots
- **Equipment Slots**: MainHand, OffHand, TwoHanded, Head, Chest, Legs, Feet, Back, Ring, Necklace

### Attachment System
Equipment items can be attached to specific transforms on the character:
- Configurable attachment points per slot
- Local position/rotation/scale offsets
- Auto-finds common bone names (hand_r, head, spine, etc.)

## Namespace Organization

```csharp
GameFramework.Core.Interfaces    // Interface definitions
GameFramework.Input             // Input handling systems
GameFramework.Camera            // Camera control systems  
GameFramework.Locomotion        // Movement systems
GameFramework.Character         // Character coordination
GameFramework.Items             // Item system (inventory, equipment, definitions)
GameFramework.Interaction       // World interaction system
```

## Framework Philosophy

**Core Principles**:
- Single Responsibility Principle for all components
- Prefer composition over inheritance
- Design for testability and modularity
- Clear interfaces between systems
- Auto-configuration to reduce setup complexity

**Modular Systems**: Core systems (input, camera, locomotion, game modes) are designed as interchangeable components that can be easily swapped to change game behavior.

## Adding New Systems

When implementing new modular systems:
1. Define interface in `GameFramework.Core.Interfaces`
2. Implement concrete class following existing patterns
3. Add auto-configuration logic to character controller
4. Ensure proper dependency injection support
5. Follow Clean Code principles throughout

## Current State

The framework currently implements a working first-person character controller with modular input, camera, and locomotion systems. The tutorial system in `Assets/TutorialInfo/` should be removed when development begins.