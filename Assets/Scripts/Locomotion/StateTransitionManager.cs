using UnityEngine;
using GameFramework.Core.StateMachine;
using GameFramework.Core.Interfaces;
using GameFramework.Locomotion.States;

namespace GameFramework.Locomotion
{
    public class StateTransitionManager
    {
        private readonly StateMachine<LocomotionState> stateMachine;
        private readonly LocomotionStateFactory stateFactory;
        private readonly FirstPersonLocomotionController controller;
        private readonly LocomotionConfiguration config;
        
        public StateTransitionManager(
            StateMachine<LocomotionState> stateMachine, 
            LocomotionStateFactory stateFactory,
            FirstPersonLocomotionController controller,
            LocomotionConfiguration config)
        {
            this.stateMachine = stateMachine ?? throw new System.ArgumentNullException(nameof(stateMachine));
            this.stateFactory = stateFactory ?? throw new System.ArgumentNullException(nameof(stateFactory));
            this.controller = controller ?? throw new System.ArgumentNullException(nameof(controller));
            this.config = config ?? throw new System.ArgumentNullException(nameof(config));
        }
        
        public bool TryTransitionToState<T>() where T : LocomotionState
        {
            try
            {
                var targetState = stateFactory.GetState<T>();
                if (targetState != null && stateMachine.CurrentState?.CanTransitionTo(targetState) == true)
                {
                    stateMachine.ChangeState<T>();
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to transition to state {typeof(T).Name}: {ex.Message}");
            }
            
            return false;
        }
        
        public bool TryTransitionToSliding(Vector2 movementInput)
        {
            try
            {
                if (TryTransitionToState<SlidingState>())
                {
                    var slidingState = stateFactory.GetState<SlidingState>() as SlidingState;
                    slidingState?.StartSlide(movementInput);
                    // Note: ResetSlideAvailability is called by the controller, not here
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to transition to sliding state: {ex.Message}");
            }
            
            return false;
        }
        
        public bool TryTransitionToMantle(Vector3 mantleTarget)
        {
            try
            {
                if (TryTransitionToState<MantleState>())
                {
                    var mantleState = stateFactory.GetState<MantleState>() as MantleState;
                    mantleState?.StartMantle(mantleTarget);
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to transition to mantle state: {ex.Message}");
            }
            
            return false;
        }
        
        public void TransitionToGroundedState()
        {
            // Check input state to determine appropriate grounded state
            if (controller.IsCrouching)
            {
                TryTransitionToState<CrouchingState>();
            }
            else
            {
                TryTransitionToState<StandingState>();
            }
        }
        
        public bool IsInState<T>() where T : LocomotionState
        {
            return stateMachine.IsInState<T>();
        }
        
        public T GetCurrentState<T>() where T : LocomotionState
        {
            var currentState = stateMachine.CurrentState;
            return currentState as T;
        }
    }
}