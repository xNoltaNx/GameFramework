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
    }
}