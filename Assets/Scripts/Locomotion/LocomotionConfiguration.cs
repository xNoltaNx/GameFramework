using UnityEngine;

namespace GameFramework.Locomotion
{
    [System.Serializable]
    public class LocomotionConfiguration
    {
        [Header("State Transition Settings")]
        [SerializeField] private float groundCheckDelay = 0.1f;
        [SerializeField] private float slideExitDelay = 0.1f;
        [SerializeField] private float mantleGroundCheckDelay = 0.05f;
        
        [Header("Slide Settings")]
        [SerializeField] private float slideSpeedThreshold = 0.85f;
        [SerializeField] private float minSlideSpeed = 3f;
        [SerializeField] private float slideDeceleration = 8f;
        
        [Header("Mantle Settings")]
        [SerializeField] private float mantleMinHeight = 0.5f;
        [SerializeField] private float mantleArcHeightMultiplier = 1.5f;
        [SerializeField] private float mantleArcMinHeight = 0.5f;
        [SerializeField] private float mantleSafeClearanceDistance = 0.6f;
        
        [Header("Air Control Settings")]
        [SerializeField] private float directionChangeThreshold = 0.8f;
        [SerializeField] private float airControlMinInput = 0.1f;
        [SerializeField] private float airDragThreshold = 0.1f;
        
        [Header("General Settings")]
        [SerializeField] private float movementInputDeadzone = 0.1f;
        [SerializeField] private float velocityThreshold = 0.1f;
        
        // Public properties for access
        public float GroundCheckDelay => groundCheckDelay;
        public float SlideExitDelay => slideExitDelay;
        public float MantleGroundCheckDelay => mantleGroundCheckDelay;
        public float SlideSpeedThreshold => slideSpeedThreshold;
        public float MinSlideSpeed => minSlideSpeed;
        public float SlideDeceleration => slideDeceleration;
        public float MantleMinHeight => mantleMinHeight;
        public float MantleArcHeightMultiplier => mantleArcHeightMultiplier;
        public float MantleArcMinHeight => mantleArcMinHeight;
        public float MantleSafeClearanceDistance => mantleSafeClearanceDistance;
        public float DirectionChangeThreshold => directionChangeThreshold;
        public float AirControlMinInput => airControlMinInput;
        public float AirDragThreshold => airDragThreshold;
        public float MovementInputDeadzone => movementInputDeadzone;
        public float VelocityThreshold => velocityThreshold;
        
        // Factory method for default configuration
        public static LocomotionConfiguration CreateDefault()
        {
            return new LocomotionConfiguration();
        }
    }
}