using UnityEngine;

namespace GameFramework.Locomotion.States
{
    public abstract class GroundedState : LocomotionState
    {
        public GroundedState(FirstPersonLocomotionController controller) : base(controller) { }

        public override void Update()
        {
            if (!controller.IsGrounded)
            {
                controller.ChangeToFallingState();
            }
        }

        public override void HandleJump(bool jumpPressed, bool jumpHeld)
        {
            if (jumpPressed && controller.CanJump())
            {
                controller.ChangeToJumpingState();
            }
        }

        protected bool CanInitiateSlide()
        {
            // Check if player is moving at or above the required sprint speed threshold
            Vector3 horizontalVelocity = new Vector3(controller.Velocity.x, 0f, controller.Velocity.z);
            float currentSpeed = horizontalVelocity.magnitude;
            float requiredSpeed = controller.SprintSpeed * controller.SlideSpeedThreshold;
            
            return currentSpeed >= requiredSpeed;
        }
    }
}