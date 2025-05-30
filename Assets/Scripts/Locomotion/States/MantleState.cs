using UnityEngine;
using GameFramework.Core;

namespace GameFramework.Locomotion.States
{
    public class MantleState : LocomotionState
    {
        private Vector3 startPosition;
        private Vector3 targetPosition;
        private Vector3 peakPosition;
        private float mantleTimer;
        private bool mantleComplete = false;

        public MantleState(FirstPersonLocomotionController controller) : base(controller) { }

        public override void Enter()
        {
            base.Enter(); // Call base to notify camera state change
            
            mantleComplete = false;
            controller.SetVelocity(Vector3.zero);
            mantleTimer = 0f;
        }
        
        public override void Exit()
        {
            // Ensure velocity is reset when exiting mantle
            controller.SetVelocity(Vector3.zero);
        }

        public void StartMantle(Vector3 target)
        {
            startPosition = controller.transform.position;
            targetPosition = target;
            
            // Create arc peak position - midpoint elevated above both start and target
            Vector3 midpoint = Vector3.Lerp(startPosition, targetPosition, 0.5f);
            float arcHeight = Mathf.Max(1.5f, targetPosition.y - startPosition.y + 0.5f);
            peakPosition = new Vector3(midpoint.x, Mathf.Max(startPosition.y, targetPosition.y) + arcHeight, midpoint.z);
            
            mantleTimer = 0f;
            mantleComplete = false;
        }

        public override void Update()
        {
            if (mantleComplete)
            {
                // Check if we're actually grounded after mantling, otherwise fall
                if (controller.IsGrounded)
                {
                    controller.ChangeToStandingState();
                }
                else
                {
                    controller.ChangeToFallingState();
                }
                return;
            }

            mantleTimer += Time.deltaTime;
            float progress = mantleTimer / controller.MantleDuration;

            if (progress >= 1f)
            {
                // Use final precise movement to target
                Vector3 finalMovement = targetPosition - controller.transform.position;
                controller.CharacterController.Move(finalMovement);
                controller.SetVelocity(Vector3.zero);
                
                mantleComplete = true;
                return;
            }

            // Apply easing to the progress
            float easedProgress = controller.MantleEasing.Evaluate(progress);
            
            // Calculate arc position using eased progress
            Vector3 currentPosition = CalculateArcPosition(easedProgress);
            Vector3 previousPosition = controller.transform.position;
            Vector3 movement = currentPosition - previousPosition;
            
            // Use CharacterController.Move for collision-safe movement
            controller.CharacterController.Move(movement);
            
            // Update velocity for other systems
            Vector3 mantleVelocity = movement / Time.deltaTime;
            controller.SetVelocity(mantleVelocity);
        }

        public override void HandleMovement(Vector2 movementInput, bool sprintHeld, bool crouchHeld)
        {
            // Ignore all input during mantling - mantle must complete atomically
        }

        public override void HandleJump(bool jumpPressed, bool jumpHeld)
        {
            // Ignore jump input during mantling - mantle must complete atomically
        }

        
        private Vector3 CalculateArcPosition(float t)
        {
            // Quadratic Bezier curve: (1-t)²P₀ + 2(1-t)tP₁ + t²P₂
            float oneMinusT = 1f - t;
            float tSquared = t * t;
            float oneMinusTSquared = oneMinusT * oneMinusT;
            
            return oneMinusTSquared * startPosition + 
                   2f * oneMinusT * t * peakPosition + 
                   tSquared * targetPosition;
        }
    }
}