# Claude Code Guidelines for GameFramework

## Unity Development Notes

- Do NOT create .meta files manually - Unity will generate them automatically
- Do NOT attempt to test Unity-specific changes via command line
- Focus on code implementation and let Unity handle asset management
- When refactoring, prioritize code structure and maintainability over immediate testing

## Cinemachine 3+ API Usage

This project uses **Cinemachine 3.0+** which has significant API changes from 2.x versions:

- **ALWAYS use web search** to verify current API signatures when encountering Cinemachine compilation errors
- Namespace changed from `Cinemachine` to `Unity.Cinemachine`
- Field names removed "m_" prefixes (e.g., `m_DefaultBlend` → `DefaultBlend`)
- Enum access changed (e.g., `CinemachineBlendDefinition.Style.EaseInOut` → `CinemachineBlendDefinition.Styles.EaseInOut`)
- Method signatures may have changed (e.g., `PostPipelineStageCallback` parameters)
- Component attachment methods changed (e.g., `vcam.AddComponent` → `vcam.gameObject.AddComponent`)

### Key Resources:
- Search: "Unity Cinemachine 3.0 [specific API]" for current documentation
- Use Unity documentation: https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/

## Assembly Architecture & Dependency Management

This project uses Assembly Definition files (.asmdef) for proper code organization. **Maintaining clean dependencies is critical.**

### Assembly Dependency Rules:
1. **Core** - Contains all interfaces, no dependencies on other game assemblies
2. **UI, Items, Camera, Locomotion, Input, Interaction** - Depend on Core + specific needs
3. **Character** - High-level assembly that can depend on most others
4. **NEVER create circular dependencies** between assemblies

### Circular Dependency Prevention:
- Use **interfaces in Core** instead of concrete classes when crossing assembly boundaries
- Example: Use `IInventoryController` instead of `InventoryController` when referencing from other assemblies
- Example: Use `ICharacterController` instead of `FirstPersonCharacterController`
- When adding assembly references, check for cycles: A→B→A is forbidden

### Interface-Based Architecture:
- All major systems should implement interfaces defined in Core
- Use `FindObjectOfType<MonoBehaviour>()` + interface casting when Unity's generic constraints prevent direct interface usage
- Prefer dependency injection through interfaces over concrete class dependencies

### Warning Signs of Architecture Issues:
- Compilation errors about missing assembly references
- "Cyclic dependency detected" errors
- Having to add many assembly references to make code compile
- Direct references to concrete classes across assembly boundaries

## Camera System Setup Guidelines

### Camera Positioning:
- **Use actual camera GameObject position** - Position your camera where you want it, setup wizard uses that position
- **Virtual cameras positioned at main camera location** - All virtual cameras created at main camera's exact position and rotation
- Camera rig created at actual camera position (not calculated offsets)
- FirstPersonCameraController respects camera's placed position in scene
- Setup wizard creates follow targets at camera's actual position and sets main camera reference properly

### Virtual Camera Hierarchy:
- `vCameras` GameObject created as **child of PlayerController** for organization
- Virtual cameras parented under `vCameras` for clean hierarchy
- Setup wizard automatically creates `vCameras` as child of player and configures proper parenting
- CinemachineCameraManager has `virtualCameraParent` field for controlling virtual camera hierarchy

### Camera Target Configuration:
- **Follow Target**: Set to camera rig or camera position (where virtual cameras should be positioned)
- **Look At Target**: Set to null for first-person cameras (not typically needed)
- Setup wizard automatically configures follow target to camera's actual position
- Virtual cameras will follow the camera rig, maintaining the positioned camera height

### Camera Profile Management:
- Setup wizard automatically searches for existing MovementStateCameraProfile assets
- If existing profiles found, wizard defaults to using first found profile
- Option to create new profile or use existing one
- Profile is automatically assigned to CinemachineCameraManager during setup

## CLGF Component Identification System

This project uses a **Claude Code Game Framework (CLGF)** component identification system for easy visual recognition in the Unity Inspector.

### CLGF Naming Convention:
- All custom framework components should display `CLGF: [COMPONENT_NAME]` labels
- Use descriptive, uppercase component names (e.g., "GAME EVENT LISTENER", "AUDIO ACTION")
- Include emoji icons for quick visual identification

### Editor Color Themes:
1. **Event Components (Blue)**: 🎧 Listeners, channels, and event-related components
2. **Event Actions (Orange)**: 🚀 Event-raising actions and event triggers  
3. **Object Control Actions (Green)**: 🔊 Actions that control other GameObjects (audio, animation, transform, physics, particles, lights)
4. **Character Components (Purple)**: 🚶 Player controllers, character systems, locomotion
5. **Camera Components (Teal)**: 📷 Camera controllers, Cinemachine managers, view systems
6. **UI Components (Pink)**: 🎒 Inventory UI, hotbar, menus, drag systems
7. **System Components (Red)**: 📦 Core controllers, input handlers, managers

### Implementation Pattern:
All custom components should have corresponding custom editors that inherit from `CLGFBaseEditor`:

```csharp
[CustomEditor(typeof(YourComponent))]
public class YourComponentEditor : CLGFBaseEditor
{
    protected override CLGFTheme Theme => CLGFTheme.ObjectControl; // or Event, Action, etc.
    protected override string ComponentIcon => "🎯";
    protected override string ComponentName => "YOUR COMPONENT";
    protected override int ComponentIconSize => 12; // Optional: customize icon size (default: 12)
}
```

### Color Theme Specifications:
- **Event Theme**: Light blue background, dark blue label - Events and listeners
- **Action Theme**: Light orange background, dark orange label - Event actions and triggers
- **ObjectControl Theme**: Light green background, dark green label - Object manipulation actions
- **Character Theme**: Light purple background, dark purple label - Character and locomotion
- **Camera Theme**: Light teal background, dark teal label - Camera and view systems  
- **UI Theme**: Light pink background, dark pink label - User interface components
- **System Theme**: Light red background, dark red label - Core systems and managers

### Benefits:
- **Instant Recognition**: Colored backgrounds and CLGF labels make framework components immediately identifiable
- **Consistent Branding**: All framework components follow the same visual pattern
- **Easy Maintenance**: Adding new colored components requires minimal code
- **Professional Appearance**: Clean, consistent styling that integrates well with Unity's Inspector

### Usage Examples:

#### Simple Theme-Based Editor (Recommended):
```csharp
[CustomEditor(typeof(GameEventListener))]
public class GameEventListenerEditor : CLGFBaseEditor
{
    protected override CLGFTheme Theme => CLGFTheme.Event;
    protected override string ComponentIcon => "🎧";
    protected override string ComponentName => "GAME EVENT LISTENER";
}
```

#### Custom Colors (Advanced):
```csharp
[CustomEditor(typeof(SpecialComponent))]
public class SpecialComponentEditor : CLGFBaseEditor
{
    protected override CLGFTheme Theme => CLGFTheme.Custom;
    protected override Color CustomBackgroundColor => new Color(1f, 0.5f, 0.5f, 0.2f); // Light red
    protected override Color CustomLabelBackgroundColor => new Color(0.8f, 0.2f, 0.2f, 0.8f); // Dark red
    protected override string ComponentIcon => "🔥";
    protected override string ComponentName => "SPECIAL COMPONENT";
}
```

### Component Categories & Recommended Icons:
- **Event Components**: 🎧 📡 📢 📻 (Listeners, channels, broadcasters)
- **Event Actions**: 🚀 ⚡ 🎯 📤 (Event triggers and raisers)
- **Object Control**: 🔊 🎬 📐 ⚽ 💡 ✨ (Audio, animation, transform, physics, lights, particles)
- **Character Components**: 🚶 🏃 🧗 ✈️ (Player controllers, locomotion, character systems)
- **Camera Components**: 📷 🎬 📳 👁️ (Camera controllers, managers, shake systems)
- **UI Components**: 🎒 🔥 👆 🖥️ (Inventory, hotbar, drag systems, menus)
- **System Components**: 📦 ⚔️ 🎮 🤝 (Controllers, managers, input handlers)

### Icon Size Guidelines:
- **Default Size**: 12px - Standard for most components
- **Important Components**: 14-16px - Key framework components (listeners, core actions)
- **Subtle Components**: 8-10px - Background/utility components
- **Prominent Components**: 18-20px - Main system components (player controller, game manager)

#### Icon Size Examples:
```csharp
protected override int ComponentIconSize => 16; // Game Event Listener (important)
protected override int ComponentIconSize => 14; // Audio Action (medium importance)  
protected override int ComponentIconSize => 10; // Particle Action (subtle effect)
```

### Migration from Old Editors:
When updating existing custom editors, replace the old base classes:
- Replace `BaseTriggerActionEditor` → `CLGFBaseEditor` with `Theme = CLGFTheme.Action`
- Replace `BaseGameEventEditor` → `CLGFBaseEditor` with `Theme = CLGFTheme.Event`
- Remove manual color definitions and use the theme system instead