using UnityEngine;
using Unity.Cinemachine;
using GameFramework.Core;

namespace GameFramework.Camera
{
    /// <summary>
    /// Designer-friendly configuration for movement-based camera behavior using Cinemachine.
    /// Provides intuitive controls for creating dynamic camera experiences across different movement states.
    /// </summary>
    [CreateAssetMenu(fileName = "MovementCameraProfile", menuName = "GameFramework/Camera/Movement Camera Profile")]
    public class MovementStateCameraProfile : ScriptableObject
    {
        [Header("Profile Information")]
        [SerializeField] private string profileName = "Default Movement Camera";
        [TextArea(2, 4)]
        [SerializeField] private string description = "Camera behavior configuration for different movement states";
        
        [Header("Global Settings")]
        [Range(0f, 2f)]
        [Tooltip("Master intensity multiplier for all camera effects. Use for accessibility options.")]
        [SerializeField] private float globalIntensity = 1f;
        
        [Range(0.1f, 3f)]
        [Tooltip("Speed of transitions between different camera states")]
        [SerializeField] private float transitionSpeed = 1.5f;
        
        [Header("Movement State Configurations")]
        [SerializeField] private MovementCameraState standingState = new MovementCameraState("Standing", 75f);
        [SerializeField] private MovementCameraState walkingState = new MovementCameraState("Walking", 75f, true);
        [SerializeField] private MovementCameraState sprintingState = new MovementCameraState("Sprinting", 80f, true, true);
        [SerializeField] private MovementCameraState crouchingState = new MovementCameraState("Crouching", 70f, true, false, true);
        [SerializeField] private MovementCameraState slidingState = new MovementCameraState("Sliding", 85f, false, true, true);
        [SerializeField] private MovementCameraState airborneState = new MovementCameraState("Airborne", 78f);
        
        [Header("Camera Shake Settings")]
        [SerializeField] private CameraShakeProfile shakeProfile = new CameraShakeProfile();
        
        [Header("Advanced Settings")]
        [Tooltip("Enable debug logging for camera state changes")]
        [SerializeField] private bool enableDebugLogging = false;
        
        [Tooltip("Performance optimization - reduces update frequency for distant effects")]
        [SerializeField] private bool enablePerformanceMode = false;

        // Public Properties
        public string ProfileName => profileName;
        public string Description => description;
        public float GlobalIntensity => globalIntensity;
        public float TransitionSpeed => transitionSpeed;
        public bool EnableDebugLogging => enableDebugLogging;
        public bool EnablePerformanceMode => enablePerformanceMode;
        
        // State Properties
        public MovementCameraState StandingState => standingState;
        public MovementCameraState WalkingState => walkingState;
        public MovementCameraState SprintingState => sprintingState;
        public MovementCameraState CrouchingState => crouchingState;
        public MovementCameraState SlidingState => slidingState;
        public MovementCameraState AirborneState => airborneState;
        
        public CameraShakeProfile ShakeProfile => shakeProfile;

        /// <summary>
        /// Get the camera state configuration for a specific movement state
        /// </summary>
        public MovementCameraState GetStateConfiguration(MovementStateType stateType)
        {
            return stateType switch
            {
                MovementStateType.Standing => standingState,
                MovementStateType.Walking => walkingState,
                MovementStateType.Sprinting => sprintingState,
                MovementStateType.Crouching => crouchingState,
                MovementStateType.Sliding => slidingState,
                MovementStateType.Airborne => airborneState,
                _ => standingState
            };
        }

        /// <summary>
        /// Apply a preset configuration to this profile
        /// </summary>
        public void ApplyPreset(CameraProfilePreset preset)
        {
            switch (preset)
            {
                case CameraProfilePreset.Subtle:
                    ApplySubtlePreset();
                    break;
                case CameraProfilePreset.Standard:
                    ApplyStandardPreset();
                    break;
                case CameraProfilePreset.Dynamic:
                    ApplyDynamicPreset();
                    break;
                case CameraProfilePreset.Cinematic:
                    ApplyCinematicPreset();
                    break;
            }
        }

        private void ApplySubtlePreset()
        {
            globalIntensity = 0.5f;
            transitionSpeed = 2f;
            
            // Reduce all noise amplitudes
            walkingState.noiseSettings.amplitudeGain = 0.3f;
            sprintingState.noiseSettings.amplitudeGain = 0.4f;
            crouchingState.noiseSettings.amplitudeGain = 0.2f;
            
            // Minimal FOV changes
            sprintingState.fieldOfView = 77f;
            slidingState.fieldOfView = 80f;
        }

        private void ApplyStandardPreset()
        {
            globalIntensity = 1f;
            transitionSpeed = 1.5f;
            
            // Standard noise settings
            walkingState.noiseSettings.amplitudeGain = 0.6f;
            sprintingState.noiseSettings.amplitudeGain = 0.8f;
            crouchingState.noiseSettings.amplitudeGain = 0.4f;
        }

        private void ApplyDynamicPreset()
        {
            globalIntensity = 1.3f;
            transitionSpeed = 1.2f;
            
            // Enhanced effects
            walkingState.noiseSettings.amplitudeGain = 0.8f;
            sprintingState.noiseSettings.amplitudeGain = 1.2f;
            slidingState.noiseSettings.amplitudeGain = 0.7f;
            
            // More dramatic FOV changes
            sprintingState.fieldOfView = 82f;
            slidingState.fieldOfView = 88f;
        }

        private void ApplyCinematicPreset()
        {
            globalIntensity = 1.5f;
            transitionSpeed = 1f;
            
            // Cinematic effects
            sprintingState.noiseSettings.amplitudeGain = 1.5f;
            slidingState.noiseSettings.amplitudeGain = 1f;
            
            // Dramatic FOV changes
            sprintingState.fieldOfView = 85f;
            slidingState.fieldOfView = 95f;
            crouchingState.fieldOfView = 65f;
        }

        private void OnValidate()
        {
            // Ensure valid values
            globalIntensity = Mathf.Clamp(globalIntensity, 0f, 2f);
            transitionSpeed = Mathf.Clamp(transitionSpeed, 0.1f, 3f);
        }
    }

    /// <summary>
    /// Configuration for a specific movement state's camera behavior
    /// </summary>
    [System.Serializable]
    public class MovementCameraState
    {
        [Header("State Info")]
        public string stateName;
        
        [Header("Camera Settings")]
        [Range(30f, 120f)]
        [Tooltip("Field of view for this movement state")]
        public float fieldOfView = 75f;
        
        [Tooltip("Enable procedural head bob for this state")]
        public bool enableHeadBob = false;
        
        [Tooltip("Enable FOV transition effects")]
        public bool enableFOVEffects = false;
        
        [Tooltip("Enable camera roll effects")]
        public bool enableCameraRoll = false;
        
        [Header("Head Bob (Cinemachine Noise)")]
        public GameFrameworkNoiseSettings noiseSettings = new GameFrameworkNoiseSettings();
        
        [Header("Camera Roll Settings")]
        [Range(0f, 15f)]
        [Tooltip("Maximum camera roll angle in degrees")]
        public float maxRollAngle = 2f;
        
        [Range(1f, 10f)]
        [Tooltip("Speed of camera roll transitions")]
        public float rollSpeed = 3f;
        
        [Tooltip("Invert the camera roll direction")]
        public bool invertRoll = false;
        
        [Header("Easing Settings")]
        public EasingSettings transitionEasing = new EasingSettings(EasingType.EaseInOutQuad);

        public MovementCameraState()
        {
            // Default constructor for serialization
            stateName = "Default";
            fieldOfView = 75f;
            noiseSettings = new GameFrameworkNoiseSettings();
        }

        public MovementCameraState(string name, float fov, bool headBob = false, bool fovEffects = false, bool roll = false)
        {
            stateName = name;
            fieldOfView = fov;
            enableHeadBob = headBob;
            enableFOVEffects = fovEffects;
            enableCameraRoll = roll;
            
            // Initialize default noise settings for head bob
            noiseSettings = new GameFrameworkNoiseSettings
            {
                amplitudeGain = headBob ? 0.6f : 0f,
                frequencyGain = 1f
            };
        }
    }

    /// <summary>
    /// Camera shake configuration using Cinemachine Impulse Sources
    /// </summary>
    [System.Serializable]
    public class CameraShakeProfile
    {
        [Header("Landing Shake")]
        [Tooltip("Enable camera shake when landing")]
        public bool enableLandingShake = true;
        
        [Range(0f, 2f)]
        [Tooltip("Base intensity of landing shake")]
        public float landingIntensity = 1f;
        
        [Range(0f, 20f)]
        [Tooltip("Minimum velocity to trigger landing shake")]
        public float velocityThreshold = 5f;
        
        [Header("Custom Shake Events")]
        [Tooltip("Enable manual shake triggers")]
        public bool enableCustomShake = true;
        
        [Range(0f, 5f)]
        [Tooltip("Default intensity for custom shake events")]
        public float defaultShakeIntensity = 1f;
        
        [Header("Shake Dampening")]
        [Range(0.1f, 5f)]
        [Tooltip("Time for shake to fully decay")]
        public float decayTime = 1f;
        
        [Range(0f, 1f)]
        [Tooltip("Directional influence of shake (0 = omnidirectional, 1 = highly directional)")]
        public float directionalInfluence = 0.1f;
    }

    /// <summary>
    /// Enhanced Cinemachine noise settings with game-specific parameters
    /// </summary>
    [System.Serializable]
    public class GameFrameworkNoiseSettings
    {
        [Header("Noise Profile")]
        [Tooltip("Cinemachine noise profile asset")]
        public NoiseSettings noiseProfile;
        
        [Range(0f, 2f)]
        [Tooltip("Overall amplitude of the noise")]
        public float amplitudeGain = 0.6f;
        
        [Range(0f, 5f)]
        [Tooltip("Frequency multiplier for the noise")]
        public float frequencyGain = 1f;
        
        [Header("Movement-Based Scaling")]
        [Tooltip("Scale noise based on movement speed")]
        public bool scaleWithMovement = true;
        
        [Range(0f, 1f)]
        [Tooltip("Minimum noise intensity when not moving")]
        public float minimumIntensity = 0.1f;
        
        [Range(1f, 3f)]
        [Tooltip("Maximum intensity multiplier at full speed")]
        public float maximumIntensity = 1.5f;
    }

    /// <summary>
    /// Movement state types for camera configuration
    /// </summary>
    public enum MovementStateType
    {
        Standing,
        Walking,
        Sprinting,
        Crouching,
        Sliding,
        Airborne
    }

    /// <summary>
    /// Preset configurations for quick setup
    /// </summary>
    public enum CameraProfilePreset
    {
        Subtle,     // Minimal camera effects for sensitive players
        Standard,   // Balanced effects for most players
        Dynamic,    // Enhanced effects for immersion
        Cinematic   // Dramatic effects for trailers/cinematics
    }
}