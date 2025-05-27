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
        
        // Camera effects support
        void NotifyLocomotionStateChanged(string stateName, bool isMoving, bool isSprinting, float movementSpeed);
        void NotifyLanding(float landingVelocity);
        void NotifyMovementInput(Vector2 movementInput);
    }
}