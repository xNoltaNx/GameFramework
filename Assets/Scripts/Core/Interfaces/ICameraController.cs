using UnityEngine;

namespace GameFramework.Core.Interfaces
{
    public interface ICameraController
    {
        UnityEngine.Camera Camera { get; }
        Transform CameraTransform { get; }
        
        void Initialize(Transform target);
        void HandleLookInput(Vector2 lookInput);
        void SetSensitivity(float sensitivity);
        void SetFieldOfView(float fov);
        void SetTarget(Transform target);
        
        // TODO: Update for Cinemachine virtual camera management
        // These methods will be used to switch between Cinemachine virtual cameras
        void NotifyLocomotionStateChanged(string stateName, bool isMoving, bool isSprinting, float movementSpeed);
        void NotifyLanding(float landingVelocity);
        void NotifyMovementInput(Vector2 movementInput);
    }
}