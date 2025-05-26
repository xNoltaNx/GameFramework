using UnityEngine;

namespace GameFramework.Locomotion.States
{
    public abstract class AirborneState : LocomotionState
    {
        public AirborneState(FirstPersonLocomotionController controller) : base(controller) { }

        public override void Update()
        {
            controller.ApplyGravity();
            
            if (controller.IsGrounded && controller.Velocity.y <= 0f)
            {
                controller.ChangeToGroundedState();
            }
        }

        public override void HandleMovement(Vector2 movementInput, bool sprintHeld, bool crouchHeld)
        {
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