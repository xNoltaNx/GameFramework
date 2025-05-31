using System;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Core.Interfaces;
using GameFramework.Core.StateMachine;
using GameFramework.Locomotion.States;

namespace GameFramework.Locomotion
{
    public class LocomotionStateFactory
    {
        private readonly Dictionary<Type, IState> stateInstances = new Dictionary<Type, IState>();
        private ILocomotionController controller;

        public void InitializeStates(ILocomotionController controller)
        {
            this.controller = controller ?? throw new ArgumentNullException(nameof(controller));
            
            if (!(controller is FirstPersonLocomotionController locomotionController))
            {
                throw new ArgumentException("Controller must be FirstPersonLocomotionController", nameof(controller));
            }

            // Clear any existing states
            stateInstances.Clear();

            // Create and register all locomotion states
            stateInstances[typeof(StandingState)] = new StandingState(locomotionController);
            stateInstances[typeof(CrouchingState)] = new CrouchingState(locomotionController);
            stateInstances[typeof(SlidingState)] = new SlidingState(locomotionController);
            stateInstances[typeof(JumpingState)] = new JumpingState(locomotionController);
            stateInstances[typeof(FallingState)] = new FallingState(locomotionController);
            stateInstances[typeof(MantleState)] = new MantleState(locomotionController);
        }

        public T CreateState<T>() where T : IState
        {
            if (controller == null)
            {
                throw new InvalidOperationException("Factory must be initialized before creating states. Call InitializeStates() first.");
            }

            Type stateType = typeof(T);
            if (stateInstances.TryGetValue(stateType, out IState state))
            {
                return (T)state;
            }

            throw new ArgumentException($"State type {stateType.Name} is not supported by this factory.");
        }

        public T GetState<T>() where T : IState
        {
            return CreateState<T>();
        }
    }
}