using GameFramework.Core.StateMachine;

namespace GameFramework.Core.Interfaces
{
    public interface ILocomotionStateFactory
    {
        T CreateState<T>() where T : IState;
        void InitializeStates(ILocomotionController controller);
        T GetState<T>() where T : IState;
    }
}