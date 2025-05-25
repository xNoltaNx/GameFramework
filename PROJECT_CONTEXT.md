# Unity GameFramework - Modular Component-Based Game Development Framework

## Project Overview
This is a modular Unity game development framework designed for small indie teams to rapidly prototype games, create game jam entries, and build production-ready titles. The framework follows Clean Code principles and implements a component-based architecture for maximum flexibility and reusability.

## Architecture Philosophy
- **Component Pattern**: Systems are built using swappable components that can be easily mixed and matched
- **Clean Code Principles**: Following Robert C. Martin's guidelines for maintainable, readable code
- **Modular Design**: Core systems (input, camera, locomotion, game modes) are decoupled and interchangeable
- **Rapid Prototyping Focus**: Designed for quick iteration and experimentation

## Current Project Structure
```
Assets/
├── InputSystem_Actions.inputactions    # Comprehensive input mapping (Player + UI)
├── Scenes/                            # Unity scenes
├── Settings/                          # Render pipeline and volume settings
└── TutorialInfo/                      # Bootstrap tutorial system (removable)
    ├── Scripts/
    │   ├── Readme.cs                  # ScriptableObject-based documentation
    │   └── Editor/
    │       └── ReadmeEditor.cs        # Custom editor for project docs
    └── Icons/
```

## Existing Systems Analysis
1. **Input System**: Already configured with comprehensive action maps for Player and UI controls supporting multiple input devices (Keyboard/Mouse, Gamepad, Touch, XR, Joystick)

2. **Render Pipeline**: Set up with Universal Render Pipeline (URP) with separate configurations for Mobile and PC platforms

3. **Documentation System**: Custom ScriptableObject-based readme system with editor integration

## Framework Goals
- **Swappable Input Systems**: Easily switch between different input handling approaches
- **Modular Camera Systems**: Third-person, first-person, top-down, cinematic cameras as interchangeable components
- **Flexible Locomotion**: Character movement systems that can be swapped (platformer, FPS, racing, etc.)
- **Configurable Game Modes**: Different rule sets and win conditions as components
- **Clean Architecture**: Single Responsibility Principle, clear interfaces, minimal dependencies

## Development Guidelines
- Follow Clean Code principles throughout
- Prefer composition over inheritance
- Use interfaces for system contracts
- Implement dependency injection where appropriate
- Write self-documenting code with clear naming
- Maintain high cohesion, low coupling
- Design for testability and modularity

## Base Prompt for Future Development

When working on this project, use this context:

**"This is a Unity GameFramework project focused on modular, component-based game development for indie teams. The framework emphasizes Clean Code principles and enables rapid prototyping through swappable systems. Core systems (input, camera, locomotion, game modes) should be designed as interchangeable components. Follow single responsibility principle, prefer composition over inheritance, and maintain clear interfaces between systems. The project already includes comprehensive input mapping and URP setup. Focus on creating reusable, testable components that can be easily swapped to change game behavior."**

## Next Steps
- Remove tutorial system when development begins
- Implement first-person character controller with modular components
- Create swappable input, camera, and locomotion systems
- Build foundation for game mode system