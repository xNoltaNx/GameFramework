using UnityEngine;

namespace GameFramework.Locomotion.States
{
    public class CrouchingState : GroundedState
    {
        private bool justEntered = false;
        
        public CrouchingState(FirstPersonLocomotionController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter(); // Call base to notify camera state change
            
            controller.SetCrouching(true);
            justEntered = true;
        }
        
        public override void Exit()
        {
            controller.SetCrouching(false);
            justEntered = false;
        }

        public override void HandleMovement(Vector2 movementInput, bool sprintHeld, bool crouchHeld)
        {
            // Notify camera about movement input changes
            NotifyCameraMovementInput(movementInput);
            
            if (!crouchHeld && controller.CanStandUp())
            {
                controller.ChangeToStandingState();
                return;
            }

            if (crouchHeld && sprintHeld && movementInput.magnitude > controller.Config.MovementInputDeadzone && controller.CanSlide && CanInitiateSlide())
            {
                controller.ChangeToSlidingState(movementInput);
                return;
            }

            Vector3 inputDirection = controller.GetMovementDirection(movementInput);
            Vector3 targetVelocity = inputDirection * controller.CrouchSpeed;
            
            if (justEntered)
            {
                Vector3 currentHorizontalVelocity = new Vector3(controller.Velocity.x, 0f, controller.Velocity.z);
                float currentSpeed = currentHorizontalVelocity.magnitude;
                
                if (currentSpeed > controller.CrouchSpeed)
                {
                    if (movementInput.magnitude < controller.Config.MovementInputDeadzone)
                    {
                        // No input - preserve momentum direction
                        targetVelocity = currentHorizontalVelocity.normalized * Mathf.Max(controller.CrouchSpeed, currentSpeed * 0.8f);
                    }
                    else
                    {
                        // Has input - gradually transition to crouch speed
                        float transitionSpeed = Mathf.Lerp(currentSpeed, controller.CrouchSpeed, 0.3f);
                        targetVelocity = inputDirection * transitionSpeed;
                    }
                }
                justEntered = false;
            }
            
            controller.ApplyMovement(targetVelocity);
        }

        public override void HandleJump(bool jumpPressed, bool jumpHeld)
        {
        }

    }
}