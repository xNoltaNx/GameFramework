using UnityEngine;
using Unity.Cinemachine;

namespace GameFramework.Camera
{
    /// <summary>
    /// Enhanced Cinemachine noise extension that provides movement-responsive procedural camera effects.
    /// Offers advanced controls for creating realistic head bob and environmental camera movement.
    /// </summary>
    [System.Serializable]
    [AddComponentMenu("Cinemachine/Extensions/Enhanced Noise")]
    public class CinemachineEnhancedNoise : CinemachineExtension
    {
        [Header("Noise Profile")]
        [Tooltip("Base noise profile for procedural movement")]
        public NoiseSettings noiseProfile;
        
        [Header("Amplitude Settings")]
        [Range(0f, 3f)]
        [Tooltip("Base amplitude for noise effects")]
        public float baseAmplitude = 1f;
        
        [Range(0f, 2f)]
        [Tooltip("Minimum amplitude when stationary")]
        public float minimumAmplitude = 0.1f;
        
        [Range(1f, 5f)]
        [Tooltip("Maximum amplitude multiplier at full speed")]
        public float maximumAmplitude = 2f;
        
        [Header("Frequency Settings")]
        [Range(0f, 5f)]
        [Tooltip("Base frequency for noise effects")]
        public float baseFrequency = 1f;
        
        [Range(0.5f, 3f)]
        [Tooltip("Frequency multiplier based on movement speed")]
        public float speedFrequencyMultiplier = 1.5f;
        
        [Header("Movement Response")]
        [Tooltip("Enable amplitude scaling based on movement speed")]
        public bool scaleWithMovement = true;
        
        [Range(0f, 1f)]
        [Tooltip("Minimum movement speed to begin scaling")]
        public float movementThreshold = 0.1f;
        
        [Range(1f, 15f)]
        [Tooltip("Movement speed that produces maximum effect")]
        public float maxMovementSpeed = 8f;
        
        [Header("State-Based Modulation")]
        [Tooltip("Enable different noise patterns for different movement states")]
        public bool enableStateModulation = true;
        
        [SerializeField] private StateNoiseModifier standingModifier = new StateNoiseModifier(0.2f, 0.8f);
        [SerializeField] private StateNoiseModifier walkingModifier = new StateNoiseModifier(1f, 1f);
        [SerializeField] private StateNoiseModifier sprintingModifier = new StateNoiseModifier(1.5f, 1.3f);
        [SerializeField] private StateNoiseModifier crouchingModifier = new StateNoiseModifier(0.5f, 0.7f);
        [SerializeField] private StateNoiseModifier slidingModifier = new StateNoiseModifier(0.8f, 1.2f);
        [SerializeField] private StateNoiseModifier airborneModifier = new StateNoiseModifier(0.3f, 0.9f);
        
        [Header("Smoothing")]
        [Range(0f, 1f)]
        [Tooltip("Smoothing factor for amplitude transitions")]
        public float amplitudeSmoothing = 0.1f;
        
        [Range(0f, 1f)]
        [Tooltip("Smoothing factor for frequency transitions")]
        public float frequencySmoothing = 0.05f;
        
        [Header("Advanced")]
        [Tooltip("Enable debug information")]
        public bool showDebugInfo = false;
        
        [Tooltip("Use custom seed for reproducible noise")]
        public bool useCustomSeed = false;
        
        [SerializeField] private int customSeed = 12345;

        // Runtime state
        private float currentAmplitude = 1f;
        private float targetAmplitude = 1f;
        private float currentFrequency = 1f;
        private float targetFrequency = 1f;
        private MovementStateType currentState = MovementStateType.Standing;
        
        // Noise generation
        private float noiseOffsetX = 0f;
        private float noiseOffsetY = 0f;
        private float noiseOffsetZ = 0f;
        
        // Static access for movement data
        private static Vector2 s_MovementInput = Vector2.zero;
        private static float s_MovementSpeed = 0f;
        private static MovementStateType s_MovementState = MovementStateType.Standing;
        
        /// <summary>
        /// Update movement data from external sources
        /// </summary>
        public static void UpdateMovementData(Vector2 movementInput, float movementSpeed, MovementStateType state)
        {
            s_MovementInput = movementInput;
            s_MovementSpeed = movementSpeed;
            s_MovementState = state;
        }

        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage,
            ref CameraState state,
            float deltaTime)
        {
            if (stage != CinemachineCore.Stage.Body)
                return;
                
            if (noiseProfile == null)
                return;
                
            UpdateNoiseParameters(deltaTime);
            ApplyNoiseToCamera(ref state, deltaTime);
        }

        private void UpdateNoiseParameters(float deltaTime)
        {
            // Update current state
            currentState = s_MovementState;
            
            // Calculate target amplitude based on movement
            if (scaleWithMovement)
            {
                float movementRatio = Mathf.Clamp01((s_MovementSpeed - movementThreshold) / (maxMovementSpeed - movementThreshold));
                float speedAmplitude = Mathf.Lerp(minimumAmplitude, maximumAmplitude, movementRatio);
                targetAmplitude = baseAmplitude * speedAmplitude;
            }
            else
            {
                targetAmplitude = baseAmplitude;
            }
            
            // Apply state-based modulation
            if (enableStateModulation)
            {
                var stateModifier = GetStateModifier(currentState);
                targetAmplitude *= stateModifier.amplitudeMultiplier;
                targetFrequency = baseFrequency * stateModifier.frequencyMultiplier;
                
                // Add speed-based frequency scaling
                if (scaleWithMovement && s_MovementSpeed > movementThreshold)
                {
                    float speedRatio = Mathf.Clamp01(s_MovementSpeed / maxMovementSpeed);
                    targetFrequency *= Mathf.Lerp(1f, speedFrequencyMultiplier, speedRatio);
                }
            }
            else
            {
                targetFrequency = baseFrequency;
            }
            
            // Smooth transitions
            if (amplitudeSmoothing > 0f)
            {
                currentAmplitude = Mathf.Lerp(currentAmplitude, targetAmplitude, (1f - amplitudeSmoothing) * 10f * deltaTime);
            }
            else
            {
                currentAmplitude = targetAmplitude;
            }
            
            if (frequencySmoothing > 0f)
            {
                currentFrequency = Mathf.Lerp(currentFrequency, targetFrequency, (1f - frequencySmoothing) * 10f * deltaTime);
            }
            else
            {
                currentFrequency = targetFrequency;
            }
        }

        private void ApplyNoiseToCamera(ref CameraState state, float deltaTime)
        {
            if (currentAmplitude <= 0.001f) return;
            
            // Update noise offsets
            noiseOffsetX += currentFrequency * deltaTime;
            noiseOffsetY += currentFrequency * deltaTime * 1.1f; // Slightly different frequency for Y
            noiseOffsetZ += currentFrequency * deltaTime * 0.9f; // Different frequency for Z
            
            // Generate noise using the profile
            Vector3 noise = Vector3.zero;
            
            if (noiseProfile != null)
            {
                // Sample noise using the new API - GetValueAt now returns Vector3 and takes time + timeOffsets
                Vector3 timeOffsets = new Vector3(noiseOffsetX, noiseOffsetY, noiseOffsetZ);
                if (noiseProfile.PositionNoise.Length > 0)
                {
                    noise = noiseProfile.PositionNoise[0].GetValueAt(Time.time, timeOffsets) * currentAmplitude;
                }
                noise.z *= 0.5f; // Reduced Z movement
            }
            else
            {
                // Fallback to simple Perlin noise
                noise.x = (Mathf.PerlinNoise(noiseOffsetX, 0f) - 0.5f) * 2f * currentAmplitude;
                noise.y = (Mathf.PerlinNoise(noiseOffsetY, 100f) - 0.5f) * 2f * currentAmplitude;
                noise.z = (Mathf.PerlinNoise(noiseOffsetZ, 200f) - 0.5f) * 2f * currentAmplitude * 0.5f;
            }
            
            // Apply noise to camera position
            state.PositionCorrection += noise;
            
            if (showDebugInfo)
            {
                Debug.DrawRay(state.GetFinalPosition(), noise * 10f, Color.cyan, 0.1f);
            }
        }

        private StateNoiseModifier GetStateModifier(MovementStateType state)
        {
            return state switch
            {
                MovementStateType.Standing => standingModifier,
                MovementStateType.Walking => walkingModifier,
                MovementStateType.Sprinting => sprintingModifier,
                MovementStateType.Crouching => crouchingModifier,
                MovementStateType.Sliding => slidingModifier,
                MovementStateType.Airborne => airborneModifier,
                _ => walkingModifier
            };
        }

        /// <summary>
        /// Override noise amplitude manually (useful for special effects)
        /// </summary>
        public void SetAmplitudeOverride(float amplitude, float duration = 0f)
        {
            if (duration > 0f)
            {
                // TODO: Implement timed override
                Debug.LogWarning("[CinemachineEnhancedNoise] Timed overrides not yet implemented");
            }
            
            targetAmplitude = amplitude;
            currentAmplitude = amplitude;
        }

        /// <summary>
        /// Get current noise amplitude for debugging
        /// </summary>
        public float GetCurrentAmplitude() => currentAmplitude;
        
        /// <summary>
        /// Get current noise frequency for debugging
        /// </summary>
        public float GetCurrentFrequency() => currentFrequency;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            // Initialize noise offsets
            if (useCustomSeed)
            {
                Random.InitState(customSeed);
            }
            
            noiseOffsetX = Random.Range(0f, 1000f);
            noiseOffsetY = Random.Range(0f, 1000f);
            noiseOffsetZ = Random.Range(0f, 1000f);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Clamp values to reasonable ranges
            baseAmplitude = Mathf.Max(0f, baseAmplitude);
            minimumAmplitude = Mathf.Clamp(minimumAmplitude, 0f, maximumAmplitude);
            baseFrequency = Mathf.Max(0f, baseFrequency);
            maxMovementSpeed = Mathf.Max(1f, maxMovementSpeed);
        }

        private void OnDrawGizmosSelected()
        {
            if (!showDebugInfo || !Application.isPlaying) return;
            
            // Extensions don't have direct transform access, gizmos would be drawn from the virtual camera
            // This would need to be implemented differently for Cinemachine extensions
        }
#endif
    }

    /// <summary>
    /// Modifier settings for different movement states
    /// </summary>
    [System.Serializable]
    public class StateNoiseModifier
    {
        [Range(0f, 3f)]
        [Tooltip("Amplitude multiplier for this state")]
        public float amplitudeMultiplier = 1f;
        
        [Range(0f, 3f)]
        [Tooltip("Frequency multiplier for this state")]
        public float frequencyMultiplier = 1f;

        public StateNoiseModifier(float amplitude, float frequency)
        {
            amplitudeMultiplier = amplitude;
            frequencyMultiplier = frequency;
        }
    }
}