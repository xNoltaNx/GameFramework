using UnityEngine;
using GameFramework.Core.StateMachine;
using GameFramework.Core.Interfaces;

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

        public virtual void Enter() 
        {
            NotifyCameraStateChange();
        }
        
        public virtual void Update() { }
        public virtual void Exit() { }
        public virtual bool CanTransitionTo(IState newState) => true;

        public abstract void HandleMovement(Vector2 movementInput, bool sprintHeld, bool crouchHeld);
        public abstract void HandleJump(bool jumpPressed, bool jumpHeld);

        protected virtual void NotifyCameraStateChange()
        {
            var cameraController = GetCameraController();
            if (cameraController != null)
            {
                string stateName = GetStateName();
                bool isMoving = controller.IsMoving;
                bool isSprinting = controller.IsSprinting;
                float movementSpeed = GetCurrentMovementSpeed();
                
                cameraController.NotifyLocomotionStateChanged(stateName, isMoving, isSprinting, movementSpeed);
            }
        }

        protected virtual void NotifyCameraMovementInput(Vector2 movementInput)
        {
            var cameraController = GetCameraController();
            cameraController?.NotifyMovementInput(movementInput);
        }

        protected virtual void NotifyCameraLanding(float landingVelocity)
        {
            var cameraController = GetCameraController();
            cameraController?.NotifyLanding(landingVelocity);
        }

        private static ICameraController cachedCameraController;
        
        private ICameraController GetCameraController()
        {
            // Cache the camera controller to avoid repeated lookups
            if (cachedCameraController == null)
            {
                // First try to get from the controller itself
                cachedCameraController = controller.GetComponent<ICameraController>();
                
                // If not found, try children but be more specific
                if (cachedCameraController == null)
                {
                    cachedCameraController = controller.GetComponentInChildren<ICameraController>();
                }
            }
            
            return cachedCameraController;
        }

        protected virtual string GetStateName()
        {
            return this.GetType().Name.Replace("State", "");
        }

        protected virtual float GetCurrentMovementSpeed()
        {
            Vector3 horizontalVelocity = new Vector3(controller.Velocity.x, 0f, controller.Velocity.z);
            return horizontalVelocity.magnitude;
        }
    }
}