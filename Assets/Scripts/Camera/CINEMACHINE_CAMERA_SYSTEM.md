# Cinemachine Camera System Documentation

## Overview

The enhanced Cinemachine Camera System provides a sophisticated, designer-friendly camera experience for first-person games. It replaces the legacy camera effects system with a more powerful, flexible, and performant solution built on Unity's Cinemachine framework.

## Key Features

### ✨ **Enhanced Camera Effects**
- **State-based virtual cameras** for different movement states (standing, walking, sprinting, crouching, sliding, airborne)
- **Dynamic head bob simulation** using Cinemachine noise profiles
- **Movement-responsive camera roll** for natural strafe feedback
- **Advanced camera shake system** with multiple impulse sources
- **Smooth FOV transitions** between movement states

### 🎛️ **Designer-Friendly Controls**
- **MovementStateCameraProfile** ScriptableObjects for easy configuration
- **Preset system** with Subtle, Standard, Dynamic, and Cinematic configurations
- **Real-time parameter adjustment** with immediate visual feedback
- **Accessibility options** for motion sensitivity
- **Performance optimization** settings

### 🔧 **Technical Advantages**
- **Built on Cinemachine** for industry-standard camera control
- **Custom extensions** for game-specific effects
- **Performance optimized** with distance-based LOD
- **Modular architecture** for easy customization
- **Comprehensive debugging** tools and visualizations

## Quick Setup

### Automatic Setup (Recommended)
1. Open the setup wizard: `GameFramework > Camera > Setup Cinemachine Camera System`
2. Assign your player GameObject and main camera
3. Choose a camera profile preset
4. Click "Setup Complete Camera System"
5. Done! The system is ready to use.

### Manual Setup
1. Add `FirstPersonCameraController` to your player GameObject
2. Add `CinemachineBrain` to your main camera
3. Create a `MovementStateCameraProfile` asset
4. Configure virtual cameras for each movement state
5. Assign the profile to the camera controller

## Core Components

### 1. FirstPersonCameraController
The main interface between your locomotion system and the camera. Handles player input and coordinates all camera effects.

**Key Methods:**
```csharp
// Movement state notifications
NotifyLocomotionStateChanged(stateName, isMoving, isSprinting, speed);
NotifyLanding(landingVelocity);
NotifyMovementInput(movementInput);

// Camera control
SetCameraProfile(profile);
TriggerCameraShake(presetName);
SetShakeEnabled(enabled);
```

### 2. CinemachineCameraManager
Manages virtual cameras and handles state transitions. Automatically switches between cameras based on movement state.

**Features:**
- Automatic virtual camera creation
- Smooth state transitions
- Performance optimization
- Event system for camera changes

### 3. MovementStateCameraProfile
ScriptableObject that defines all camera behavior for different movement states.

**Configuration Options:**
- Field of view per state
- Head bob intensity and frequency
- Camera roll settings
- Noise profiles
- Transition speeds

### 4. CameraShakeManager
Handles all camera shake effects using Cinemachine Impulse Sources.

**Shake Types:**
- Landing impacts (velocity-based)
- Environmental effects
- Custom shake events
- Preset-based shakes

### 5. Custom Cinemachine Extensions
- **CinemachineMovementRoll**: Provides strafe-based camera roll
- **CinemachineEnhancedNoise**: Advanced head bob with movement scaling

## Configuration Guide

### Creating Camera Profiles

1. **Create Profile Asset:**
   ```
   Right-click in Project > Create > GameFramework > Camera > Movement Camera Profile
   ```

2. **Configure States:**
   Each movement state has its own configuration:
   - **Field of View**: Camera FOV for this state
   - **Head Bob**: Enable and configure procedural head movement
   - **Camera Roll**: Enable roll effects during strafing
   - **Noise Settings**: Cinemachine noise profile configuration

3. **Apply Presets:**
   Use the preset system for quick configuration:
   - **Subtle**: Minimal effects for motion-sensitive players
   - **Standard**: Balanced effects for most players
   - **Dynamic**: Enhanced effects for immersion
   - **Cinematic**: Dramatic effects for trailers/cutscenes

### Virtual Camera Setup

Virtual cameras are automatically created for each movement state:
- `vcam_Standing`: Minimal effects, stable camera
- `vcam_Walking`: Moderate head bob
- `vcam_Sprinting`: Increased effects, higher FOV
- `vcam_Crouching`: Reduced effects, lower FOV
- `vcam_Sliding`: Dynamic roll and FOV effects
- `vcam_Airborne`: Slight FOV increase, landing preparation

### Camera Shake Configuration

The shake system supports multiple preset types:

```csharp
// Landing shakes (auto-triggered)
"Landing_Light"   // Small drops
"Landing_Medium"  // Standard falls
"Landing_Heavy"   // High-impact landings

// Environmental shakes
"Explosion_Small" // Nearby explosions
"Explosion_Large" // Major explosions
"Environmental"   // Ambient effects

// Gameplay shakes
"Footstep"        // Subtle movement feedback
```

## Integration with Locomotion System

The camera system integrates seamlessly with your existing locomotion controller:

```csharp
public class YourLocomotionController : MonoBehaviour
{
    private ICameraController cameraController;
    
    private void Start()
    {
        cameraController = GetComponent<ICameraController>();
    }
    
    private void Update()
    {
        // Notify camera of state changes
        cameraController.NotifyLocomotionStateChanged(
            currentState.ToString(), 
            isMoving, 
            isSprinting, 
            currentSpeed
        );
        
        // Update movement input for camera effects
        cameraController.NotifyMovementInput(movementInput);
    }
    
    private void OnLanding(float velocity)
    {
        // Trigger landing shake
        cameraController.NotifyLanding(velocity);
    }
}
```

## Performance Optimization

The system includes several performance features:

### Distance-Based LOD
- Reduces update frequency when far from camera
- Configurable distance thresholds
- Automatic performance mode switching

### Smart Shake Management
- Limited simultaneous shake effects
- Shake throttling to prevent spam
- Automatic cleanup of completed effects

### Cinemachine Optimizations
- Efficient virtual camera switching
- Minimal garbage allocation
- Optimized noise calculations

## Accessibility Features

### Motion Sensitivity Options
```csharp
// Global intensity scaling (0-2)
cameraController.SetGlobalShakeIntensity(0.5f); // 50% intensity

// Enable/disable specific effects
profile.StandingState.enableHeadBob = false;
profile.WalkingState.enableCameraRoll = false;

// Preset for sensitive players
profile.ApplyPreset(CameraProfilePreset.Subtle);
```

### User Settings Integration
The system supports user preference integration:
- Shake intensity scaling
- Head bob enable/disable
- Camera roll sensitivity
- FOV override options

## Debugging and Visualization

### Debug Features
- Real-time camera state visualization
- Movement input display
- Shake effect logging
- Performance metrics

### Gizmos
- Camera rig orientation visualization
- Target relationship display
- Movement input vectors
- Shake intensity indicators

### Console Commands
```csharp
// Test specific shake effects
cameraController.TriggerCameraShake("Landing_Heavy");

// Override camera state for testing
cameraController.CameraManager.SetCameraState(MovementStateType.Sprinting);

// Log current effect state
cameraController.CameraManager.LogCurrentEffectState();
```

## Best Practices

### 1. Profile Management
- Create different profiles for different game modes
- Use version control for profile assets
- Test profiles with various movement speeds
- Consider player feedback when tuning

### 2. Performance Considerations
- Enable performance mode for open-world games
- Use appropriate update frequencies
- Monitor shake effect counts
- Optimize noise profile complexity

### 3. Player Experience
- Start with subtle effects and allow players to increase
- Provide accessibility options
- Test with motion-sensitive players
- Consider cultural differences in motion tolerance

### 4. Integration
- Notify camera system of all relevant state changes
- Use consistent movement speed units
- Handle edge cases (teleportation, state transitions)
- Test with network lag simulation

## Troubleshooting

### Common Issues

**Camera not responding to movement:**
- Check if CinemachineBrain is attached to main camera
- Verify virtual cameras are created and configured
- Ensure movement notifications are being sent

**Choppy camera transitions:**
- Increase transition speed in profile
- Check for conflicting camera controllers
- Verify Cinemachine Brain blend settings

**Performance issues:**
- Enable performance mode
- Reduce noise profile complexity
- Check shake effect limits
- Monitor update frequencies

**No camera shake:**
- Verify CinemachineImpulseListener on camera
- Check shake intensity settings
- Ensure impulse sources are configured
- Test with manual shake triggers

### Debug Commands
```csharp
// In Unity Console or custom debug UI
cameraController.TriggerCustomShake(Vector3.up, 0.5f);
cameraController.SetShakeEnabled(false);
cameraController.CameraManager.ConfigureCameras();
```

## Migration from Legacy System

If you're upgrading from the old camera effects system:

1. **Backup your project** before starting migration
2. Use the **Setup Wizard** to create the new system
3. **Copy settings** from old profiles to new MovementStateCameraProfile
4. **Test thoroughly** with your existing gameplay
5. **Remove legacy components** after validation
6. **Update any direct references** to old camera effects

The new system maintains the same public interface as the legacy system, so most code should work without changes.

## Support and Resources

- **Unity Cinemachine Documentation**: https://docs.unity3d.com/Packages/com.unity.cinemachine@2.8/manual/index.html
- **GameFramework Camera Examples**: Check the Examples folder in the Camera content
- **Community Forum**: For questions and feature requests
- **Bug Reports**: Use the GitHub issue tracker

---

*This documentation covers the enhanced Cinemachine Camera System v1.0. For the latest updates and features, check the version history and changelog.*