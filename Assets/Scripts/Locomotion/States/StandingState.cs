using UnityEngine;
using GameFramework.Core.Interfaces;

namespace GameFramework.Locomotion.States
{
    public class StandingState : GroundedState
    {
        private bool wasMovingLastFrame = false;
        private bool wasSprintingLastFrame = false;

        public StandingState(FirstPersonLocomotionController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter(); // Call base to notify camera state change

            controller.ResetSlideAvailability();
            
            // Initialize tracking variables to force change detection on first frame
            wasMovingLastFrame = false; // Force change detection
            wasSprintingLastFrame = false; // Force change detection
            
            // Debug logging is now controlled by camera profile debug settings
        }

        public override void HandleMovement(Vector2 movementInput, bool sprintHeld, bool crouchHeld)
        {
            // Notify camera about movement input
            NotifyCameraMovementInput(movementInput);
            
            if (crouchHeld)
            {
                if (sprintHeld && movementInput.magnitude > controller.Config.MovementInputDeadzone && controller.CanSlide && CanInitiateSlide())
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
            bool hasMovementInput = movementInput.magnitude > controller.Config.MovementInputDeadzone;
            bool isCurrentlyMoving = hasMovementInput || controller.IsMoving; // Use input OR velocity
            bool isCurrentlySprinting = sprintHeld && hasMovementInput; // Sprint requires input
            
            // Update controller state
            controller.IsSprinting = isCurrentlySprinting;
            
            // Check for state changes that require camera notification
            bool movementStateChanged = wasMovingLastFrame != isCurrentlyMoving;
            bool sprintStateChanged = wasSprintingLastFrame != isCurrentlySprinting;
            
            if (movementStateChanged || sprintStateChanged)
            {
                // Override the base method to use our own movement detection
                NotifyCameraStateChangeWithCustomMovement(isCurrentlyMoving, isCurrentlySprinting);
                
                // Debug logging - only when needed for debugging issues
                // Debug.Log($"[StandingState] Movement change - Was Moving: {wasMovingLastFrame} -> {isCurrentlyMoving}, Was Sprinting: {wasSprintingLastFrame} -> {isCurrentlySprinting}, HasInput: {hasMovementInput}, Controller.IsMoving: {controller.IsMoving}");
            }
            
            // Update tracking variables
            wasMovingLastFrame = isCurrentlyMoving;
            wasSprintingLastFrame = isCurrentlySprinting;
            
            Vector3 inputDirection = controller.GetMovementDirection(movementInput);
            Vector3 targetVelocity = inputDirection * speed;
            
            controller.ApplyMovement(targetVelocity);
        }

        private void NotifyCameraStateChangeWithCustomMovement(bool isMoving, bool isSprinting)
        {
            var cameraController = GetCameraController();
            if (cameraController != null)
            {
                string stateName = GetStateName();
                
                // Use intended movement speed based on state, not current velocity
                // This prevents high sprint velocities from affecting walking head bob
                float movementSpeed = 0f;
                if (isMoving)
                {
                    movementSpeed = isSprinting ? controller.SprintSpeed : controller.WalkSpeed;
                }
                
                cameraController.NotifyLocomotionStateChanged(stateName, isMoving, isSprinting, movementSpeed);
            }
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