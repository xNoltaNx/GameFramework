using UnityEngine;

namespace GameFramework.Locomotion.States
{
    public class JumpingState : AirborneState
    {
        public JumpingState(FirstPersonLocomotionController controller) : base(controller) { }

        public override void Update()
        {
            base.Update();
            
            if (controller.Velocity.y <= 0f)
            {
                controller.ChangeToFallingState();
            }
        }
    }
}