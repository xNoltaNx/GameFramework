using UnityEngine;

namespace GameFramework.Locomotion.States
{
    public class StandingState : GroundedState
    {
        private bool wasMovingLastFrame = false;
        private bool wasSprintingLastFrame = false;

        public StandingState(FirstPersonLocomotionController controller) : base(controller) { }

        public override void Enter()
        {
            controller.SetCrouching(false);
            controller.ResetSlideAvailability();
            
            // Reset tracking variables when entering state
            wasMovingLastFrame = false;
            wasSprintingLastFrame = false;
        }

        public override void HandleMovement(Vector2 movementInput, bool sprintHeld, bool crouchHeld)
        {
            // Notify camera about movement input
            NotifyCameraMovementInput(movementInput);
            
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
            bool isCurrentlyMoving = movementInput.magnitude > 0.1f;
            bool isCurrentlySprinting = sprintHeld && isCurrentlyMoving;
            
            // Update controller state
            controller.IsSprinting = isCurrentlySprinting;
            
            // Check for state changes that require camera notification
            bool movementStateChanged = wasMovingLastFrame != isCurrentlyMoving;
            bool sprintStateChanged = wasSprintingLastFrame != isCurrentlySprinting;
            
            if (movementStateChanged || sprintStateChanged)
            {
                NotifyCameraStateChange();
                
                // Simple debug logging - you can enable/disable this manually
                #if UNITY_EDITOR
                Debug.Log($"[StandingState] Movement change - Was Moving: {wasMovingLastFrame} -> {isCurrentlyMoving}, Was Sprinting: {wasSprintingLastFrame} -> {isCurrentlySprinting}");
                #endif
            }
            
            // Update tracking variables
            wasMovingLastFrame = isCurrentlyMoving;
            wasSprintingLastFrame = isCurrentlySprinting;
            
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