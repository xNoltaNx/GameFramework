using UnityEngine;

namespace GameFramework.Locomotion.States
{
    public class FallingState : AirborneState
    {
        public FallingState(FirstPersonLocomotionController controller) : base(controller) { }

        public override void Update()
        {
            base.Update();
        }
    }
}