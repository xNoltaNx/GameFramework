using UnityEngine;

namespace GameFramework.Locomotion.States
{
    public class SlidingState : GroundedState
    {
        private float slideTimer;
        private Vector3 slideDirection;

        public SlidingState(FirstPersonLocomotionController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter(); // Call base to notify camera state change
            
            controller.SetCrouching(true);
        }

        public void StartSlide(Vector2 movementInput)
        {
            slideTimer = controller.SlideDuration;
            slideDirection = controller.GetMovementDirection(movementInput).normalized;
            
            Vector3 slideVelocity = slideDirection * controller.SlideSpeed;
            controller.SetVelocity(new Vector3(slideVelocity.x, controller.Velocity.y, slideVelocity.z));
        }

        public override void Update()
        {
            // Don't call base.Update() to avoid GroundedState's immediate airborne transition
            
            slideTimer -= Time.deltaTime;
            HandleSlideMovement();
            
            Vector3 horizontalVelocity = new Vector3(controller.Velocity.x, 0f, controller.Velocity.z);
            
            // Exit conditions - check these AFTER movement is applied
            if (slideTimer <= 0f)
            {
                controller.ChangeToCrouchingState();
                return;
            }
            
            if (horizontalVelocity.magnitude < controller.MinSlideSpeed)
            {
                controller.ChangeToCrouchingState();
                return;
            }
            
            // Only check grounded after a brief delay to allow slide to establish
            if (slideTimer < controller.SlideDuration - 0.1f && !controller.IsGrounded)
            {
                controller.ChangeToFallingState();
                return;
            }
        }

        public override void HandleMovement(Vector2 movementInput, bool sprintHeld, bool crouchHeld)
        {
            // Notify camera about movement input
            NotifyCameraMovementInput(movementInput);
            
            if (!crouchHeld && controller.CanStandUp())
            {
                controller.ChangeToStandingState();
            }
        }

        public override void HandleJump(bool jumpPressed, bool jumpHeld)
        {
            if (jumpPressed && controller.CanJump())
            {
                controller.PerformSlideJump(slideDirection);
                controller.ChangeToJumpingState();
            }
        }

        private void HandleSlideMovement()
        {
            Vector3 currentHorizontalVelocity = new Vector3(controller.Velocity.x, 0f, controller.Velocity.z);
            Vector3 deceleratedVelocity = Vector3.Lerp(currentHorizontalVelocity, Vector3.zero, controller.SlideDeceleration * Time.deltaTime);
            
            if (deceleratedVelocity.magnitude < controller.MinSlideSpeed && slideTimer > 0.1f)
            {
                deceleratedVelocity = deceleratedVelocity.normalized * controller.MinSlideSpeed;
            }
            
            controller.SetVelocity(new Vector3(deceleratedVelocity.x, controller.Velocity.y, deceleratedVelocity.z));
            controller.MoveCharacter();
        }
    }
}