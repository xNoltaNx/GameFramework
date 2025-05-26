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
    }
}