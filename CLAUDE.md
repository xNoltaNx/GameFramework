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