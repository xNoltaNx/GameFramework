using UnityEngine;
using GameFramework.Core.StateMachine;

namespace GameFramework.Locomotion.States
{
    public abstract class LocomotionState : IState
    {
        protected FirstPersonLocomotionController controller;
        protected CharacterController characterController;
        protected Transform transform;

        public LocomotionState(FirstPersonLocomotionController controller)
        {
            this.controller = controller;
            this.characterController = controller.CharacterController;
            this.transform = controller.transform;
        }

        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void Exit() { }
        public virtual bool CanTransitionTo(IState newState) => true;

        public abstract void HandleMovement(Vector2 movementInput, bool sprintHeld, bool crouchHeld);
        public abstract void HandleJump(bool jumpPressed, bool jumpHeld);
    }
}