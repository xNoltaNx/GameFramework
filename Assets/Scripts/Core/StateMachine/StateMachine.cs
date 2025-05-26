using System.Collections.Generic;

namespace GameFramework.Core.StateMachine
{
    public class StateMachine<T> where T : IState
    {
        private T currentState;
        private T previousState;
        private Dictionary<System.Type, T> states = new Dictionary<System.Type, T>();

        public T CurrentState => currentState;
        public T PreviousState => previousState;

        public void RegisterState(T state)
        {
            var stateType = state.GetType();
            if (!states.ContainsKey(stateType))
            {
                states[stateType] = state;
            }
        }

        public void ChangeState<TState>() where TState : T
        {
            var stateType = typeof(TState);
            if (states.TryGetValue(stateType, out T newState))
            {
                ChangeState(newState);
            }
        }

        public void ChangeState(T newState)
        {
            if (currentState != null && !currentState.CanTransitionTo(newState))
                return;

            previousState = currentState;
            currentState?.Exit();
            currentState = newState;
            currentState?.Enter();
        }

        public void Update()
        {
            currentState?.Update();
        }

        public bool IsInState<TState>() where TState : T
        {
            return currentState != null && currentState.GetType() == typeof(TState);
        }

        public TStateType GetState<TStateType>() where TStateType : T
        {
            var stateType = typeof(TStateType);
            if (states.TryGetValue(stateType, out T state))
            {
                return (TStateType)state;
            }
            return default(TStateType);
        }
    }
}