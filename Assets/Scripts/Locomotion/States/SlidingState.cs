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
            base.Update();
            
            slideTimer -= Time.deltaTime;
            
            Vector3 horizontalVelocity = new Vector3(controller.Velocity.x, 0f, controller.Velocity.z);
            
            if (slideTimer <= 0f || !controller.IsGrounded || horizontalVelocity.magnitude < controller.MinSlideSpeed)
            {
                controller.ChangeToCrouchingState();
                return;
            }
            
            HandleSlideMovement();
        }

        public override void HandleMovement(Vector2 movementInput, bool sprintHeld, bool crouchHeld)
        {
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