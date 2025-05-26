using UnityEngine;

namespace GameFramework.Locomotion.States
{
    public class StandingState : GroundedState
    {
        public StandingState(FirstPersonLocomotionController controller) : base(controller) { }

        public override void Enter()
        {
            controller.SetCrouching(false);
            controller.ResetSlideAvailability();
        }

        public override void HandleMovement(Vector2 movementInput, bool sprintHeld, bool crouchHeld)
        {
            if (crouchHeld)
            {
                if (sprintHeld && movementInput.magnitude > 0.1f && controller.CanSlide)
                {
                    controller.ChangeToSlidingState(movementInput);
                }
                else
                {
                    controller.ChangeToCrouchingState();
                }
                return;
            }

            float speed = sprintHeld ? controller.SprintSpeed : controller.WalkSpeed;
            controller.IsSprinting = sprintHeld && movementInput.magnitude > 0.1f;
            
            Vector3 inputDirection = controller.GetMovementDirection(movementInput);
            Vector3 targetVelocity = inputDirection * speed;
            
            controller.ApplyMovement(targetVelocity);
        }

        public override void HandleJump(bool jumpPressed, bool jumpHeld)
        {
            if (jumpPressed && controller.CanJump())
            {
                controller.PerformNormalJump();
                controller.ChangeToJumpingState();
            }
        }
    }
}