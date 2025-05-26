using UnityEngine;

namespace GameFramework.Core.Interfaces
{
    public interface ILocomotionController
    {
        bool IsGrounded { get; }
        bool IsMoving { get; }
        bool IsSprinting { get; }
        bool IsCrouching { get; }
        bool IsSliding { get; }
        Vector3 Velocity { get; }
        
        void Initialize(CharacterController characterController, Transform cameraTransform);
        void HandleMovement(Vector2 movementInput, bool sprintHeld, bool crouchHeld);
        void HandleJump(bool jumpPressed, bool jumpHeld);
        void SetMovementSpeed(float walkSpeed, float sprintSpeed);
        void SetJumpHeight(float jumpHeight);
    }
}