using UnityEngine;

namespace GameFramework.Locomotion.States
{
    public abstract class AirborneState : LocomotionState
    {
        public AirborneState(FirstPersonLocomotionController controller) : base(controller) { }

        public override void Update()
        {
            controller.ApplyGravity();
            
            // Transition to grounded when touching ground and falling/stationary
            if (controller.IsGrounded && controller.Velocity.y <= 0f)
            {
                // Notify camera about landing impact
                NotifyCameraLanding(controller.Velocity.y);
                
                controller.ChangeToGroundedState();
            }
        }

        public override void HandleMovement(Vector2 movementInput, bool sprintHeld, bool crouchHeld)
        {
            // Handle crouch input while airborne - set for landing state
            controller.SetCrouching(crouchHeld);
            
            // Notify camera about movement input changes
            NotifyCameraMovementInput(movementInput);
            
            if (controller.CanMantle(movementInput, out Vector3 mantleTarget))
            {
                controller.ChangeToMantleState(mantleTarget);
                return;
            }
            
            controller.HandleAirMovement(movementInput);
        }

        public override void HandleJump(bool jumpPressed, bool jumpHeld)
        {
        }
    }
}