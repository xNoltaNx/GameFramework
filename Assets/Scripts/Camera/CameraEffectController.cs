// DEPRECATED: Legacy camera effects system - replaced with Cinemachine integration
// This file contains the previous custom camera effects implementation for reference
// 
// Previous Features:
// - Head bob system with vertical/horizontal amplitude and frequency control
// - Camera roll effects based on strafe input  
// - Dynamic FOV changes for different movement states
// - Camera shake system for landing impacts
// - State-based effect configuration (standing, walking, sprinting, crouching, sliding, airborne)
// - Preset system with disabled/minimal/standard/cinematic/extreme configurations
//
// TODO: Replace with Cinemachine virtual cameras and noise profiles:
// - Use Cinemachine Impulse Sources for camera shake
// - Implement FOV changes with Cinemachine Brain transitions
// - Create custom Cinemachine components for head bob simulation
// - Use Cinemachine Noise for procedural camera movement
// - Set up virtual camera priorities for different movement states

using UnityEngine;

namespace GameFramework.Camera
{
    // LEGACY: This component is deprecated and should be removed from scenes
    // Replace with Cinemachine virtual cameras for enhanced camera control
    [System.Obsolete("CameraEffectController is deprecated. Use Cinemachine virtual cameras instead.")]
    public class CameraEffectController : MonoBehaviour
    {
        // TODO: Migration Guide for Cinemachine Integration:
        // 1. Install Cinemachine package via Package Manager
        // 2. Create Cinemachine Brain on main camera
        // 3. Set up virtual cameras for different movement states:
        //    - vcam_Standing: Low priority, minimal noise
        //    - vcam_Walking: Medium noise for head bob simulation  
        //    - vcam_Sprinting: Higher FOV, increased noise, screen shake
        //    - vcam_Crouching: Lower FOV, subtle roll effects
        //    - vcam_Sliding: High FOV, dramatic roll, noise effects
        //    - vcam_Airborne: Slight FOV increase, landing shake impulse
        // 4. Use Cinemachine Impulse Sources for camera shake
        // 5. Configure Noise profiles for procedural head bob
        // 6. Set up state transitions via priority switching
        
        [Header("DEPRECATED - Remove from Scene")]
        [SerializeField] private bool legacyMode = false;
        
        private void Start()
        {
            Debug.LogWarning("[CameraEffectController] This component is deprecated. " +
                           "Please remove it and implement Cinemachine virtual cameras instead.");
        }

        // State-specific effect setters for backward compatibility
        [System.Obsolete("Use Cinemachine virtual camera priorities instead")]
        public void SetStandingEffects() => Debug.LogWarning("Legacy method - use Cinemachine");
        
        [System.Obsolete("Use Cinemachine virtual camera priorities instead")]
        public void SetWalkingEffects() => Debug.LogWarning("Legacy method - use Cinemachine");
        
        [System.Obsolete("Use Cinemachine virtual camera priorities instead")]
        public void SetSprintingEffects() => Debug.LogWarning("Legacy method - use Cinemachine");
        
        [System.Obsolete("Use Cinemachine virtual camera priorities instead")]
        public void SetCrouchingEffects() => Debug.LogWarning("Legacy method - use Cinemachine");
        
        [System.Obsolete("Use Cinemachine virtual camera priorities instead")]
        public void SetSlidingEffects() => Debug.LogWarning("Legacy method - use Cinemachine");
        
        [System.Obsolete("Use Cinemachine virtual camera priorities instead")]
        public void SetAirborneEffects() => Debug.LogWarning("Legacy method - use Cinemachine");
        
        [System.Obsolete("Use Cinemachine Impulse Sources instead")]
        public void TriggerLandingShake() => Debug.LogWarning("Legacy method - use Cinemachine Impulse");
        
        [System.Obsolete("Update movement data through Cinemachine state machine instead")]
        public void UpdateMovementData(float movementSpeed, Vector2 movementInput, bool moving, bool sprinting)
        {
            // Legacy compatibility - no-op
        }
    }
    
    // Legacy settings classes for reference - to be removed after migration
    [System.Obsolete("Replace with Cinemachine Noise Settings")]
    [System.Serializable]
    public class HeadBobSettings
    {
        // Previous implementation: vertical/horizontal amplitude and frequency
        // Cinemachine equivalent: Use Basic Multi Channel Perlin noise with custom frequency
    }
    
    [System.Obsolete("Replace with Cinemachine Roll settings or custom rotation component")]
    [System.Serializable] 
    public class CameraRollSettings
    {
        // Previous implementation: strafe-based camera roll
        // Cinemachine equivalent: Custom component or composer extension
    }
    
    [System.Obsolete("Replace with Cinemachine FOV transitions")]
    [System.Serializable]
    public class FOVEffectSettings
    {
        // Previous implementation: dynamic FOV based on movement state
        // Cinemachine equivalent: Use different virtual cameras with varying FOV
    }
    
    [System.Obsolete("Replace with Cinemachine Impulse Sources")]
    [System.Serializable]
    public class CameraShakeSettings
    {
        // Previous implementation: manual shake calculations
        // Cinemachine equivalent: CinemachineImpulseSource with custom profiles
    }
    
    [System.Obsolete("Replace with Cinemachine virtual camera configurations")]
    [System.Serializable]
    public class StateCameraEffects
    {
        // Previous implementation: per-state effect configurations  
        // Cinemachine equivalent: Individual virtual cameras per movement state
    }
}