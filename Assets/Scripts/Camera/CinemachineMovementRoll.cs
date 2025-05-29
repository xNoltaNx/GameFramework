using UnityEngine;
using Unity.Cinemachine;
using GameFramework.Core;

namespace GameFramework.Camera
{
    /// <summary>
    /// Custom Cinemachine extension that adds movement-responsive camera roll effects.
    /// Provides smooth, configurable camera roll based on player input and movement state.
    /// </summary>
    [System.Serializable]
    [AddComponentMenu("Cinemachine/Extensions/Movement Roll")]
    public class CinemachineMovementRoll : CinemachineExtension
    {
        [Header("Roll Settings")]
        [Range(0f, 30f)]
        [Tooltip("Maximum roll angle in degrees")]
        public float maxRollAngle = 5f;
        
        [Range(0.1f, 10f)]
        [Tooltip("Speed of roll transitions")]
        public float rollSpeed = 3f;
        
        [Tooltip("Invert the roll direction")]
        public bool invertRoll = false;
        
        [Header("Input Scaling")]
        [Range(0f, 2f)]
        [Tooltip("Multiplier for input sensitivity")]
        public float inputSensitivity = 1f;
        
        [Range(0f, 1f)]
        [Tooltip("Minimum movement speed to start rolling")]
        public float speedThreshold = 0.1f;
        
        [Range(0f, 10f)]
        [Tooltip("Movement speed at which roll reaches maximum intensity")]
        public float maxSpeedForRoll = 8f;
        
        [Header("Easing")]
        public EasingSettings rollEasing = new EasingSettings(EasingType.EaseOutQuad);
        
        [Header("Advanced")]
        [Tooltip("Enable debug visualization")]
        public bool showDebugInfo = false;
        
        [Range(0f, 1f)]
        [Tooltip("Smoothing factor for input (0 = no smoothing, 1 = maximum smoothing)")]
        public float inputSmoothing = 0.1f;

        // Runtime state
        private float currentRoll = 0f;
        private float targetRoll = 0f;
        private float smoothedInput = 0f;
        private Vector2 lastMovementInput = Vector2.zero;
        private float lastMovementSpeed = 0f;

        // Static access for movement data
        private static Vector2 s_MovementInput = Vector2.zero;
        private static float s_MovementSpeed = 0f;
        
        /// <summary>
        /// Update movement data from external sources (e.g., CinemachineCameraManager)
        /// </summary>
        public static void UpdateMovementData(Vector2 movementInput, float movementSpeed)
        {
            s_MovementInput = movementInput;
            s_MovementSpeed = movementSpeed;
        }

        protected override void PostPipelineStageCallback(
            CinemachineVirtualCameraBase vcam,
            CinemachineCore.Stage stage,
            ref CameraState state,
            float deltaTime)
        {
            if (stage != CinemachineCore.Stage.Finalize)
                return;
                
            UpdateRollEffect(deltaTime);
            ApplyRollToCamera(ref state);
        }

        private void UpdateRollEffect(float deltaTime)
        {
            // Get current movement data
            Vector2 currentInput = s_MovementInput;
            float currentSpeed = s_MovementSpeed;
            
            // Smooth input for more natural feel
            if (inputSmoothing > 0f)
            {
                smoothedInput = Mathf.Lerp(smoothedInput, currentInput.x, (1f - inputSmoothing) * 10f * deltaTime);
            }
            else
            {
                smoothedInput = currentInput.x;
            }
            
            // Calculate speed influence
            float speedInfluence = 1f;
            if (currentSpeed > speedThreshold)
            {
                speedInfluence = Mathf.Clamp01(currentSpeed / maxSpeedForRoll);
            }
            else
            {
                speedInfluence = 0f;
            }
            
            // Calculate target roll
            float rawRoll = smoothedInput * maxRollAngle * inputSensitivity * speedInfluence;
            targetRoll = invertRoll ? -rawRoll : rawRoll;
            
            // Apply easing to roll transition
            currentRoll = rollEasing.Lerp(currentRoll, targetRoll, rollSpeed * deltaTime);
            
            // Cache values for debug
            lastMovementInput = currentInput;
            lastMovementSpeed = currentSpeed;
            
            if (showDebugInfo && Application.isPlaying)
            {
                // Debug visualization - use the current transform
                Debug.DrawRay(transform.position, Vector3.up * currentRoll * 0.1f, Color.blue, 0.1f);
            }
        }

        private void ApplyRollToCamera(ref CameraState state)
        {
            if (Mathf.Abs(currentRoll) < 0.01f) return;
            
            // Apply roll to the camera's rotation
            Quaternion rollRotation = Quaternion.Euler(0f, 0f, currentRoll);
            state.RawOrientation = state.RawOrientation * rollRotation;
        }

        /// <summary>
        /// Get the current roll angle for debugging or UI display
        /// </summary>
        public float GetCurrentRoll()
        {
            return currentRoll;
        }

        /// <summary>
        /// Get the target roll angle for debugging
        /// </summary>
        public float GetTargetRoll()
        {
            return targetRoll;
        }

        /// <summary>
        /// Manually set the roll intensity (useful for cinematic sequences)
        /// </summary>
        public void SetRollOverride(float rollAngle, float transitionSpeed = -1f)
        {
            targetRoll = rollAngle;
            if (transitionSpeed > 0f)
            {
                // Use custom transition speed
                float customSpeed = transitionSpeed;
                currentRoll = rollEasing.Lerp(currentRoll, targetRoll, customSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// Reset roll to neutral position
        /// </summary>
        public void ResetRoll()
        {
            targetRoll = 0f;
        }

#if UNITY_EDITOR
        [Header("Editor Debug")]
        [Range(-30f, 30f)]
        [SerializeField] private float debugRollOverride = 0f;
        
        private void OnValidate()
        {
            // Clamp values to reasonable ranges
            maxRollAngle = Mathf.Clamp(maxRollAngle, 0f, 30f);
            rollSpeed = Mathf.Clamp(rollSpeed, 0.1f, 10f);
            inputSensitivity = Mathf.Clamp(inputSensitivity, 0f, 2f);
            
            if (Application.isPlaying && debugRollOverride != 0f)
            {
                SetRollOverride(debugRollOverride);
                debugRollOverride = 0f;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!showDebugInfo) return;
            
            // Extensions don't have direct transform access, gizmos would be drawn from the virtual camera
            // This would need to be implemented differently for Cinemachine extensions
        }
#endif
    }
}