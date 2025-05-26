namespace GameFramework.Core.StateMachine
{
    public interface IState
    {
        void Enter();
        void Update();
        void Exit();
        bool CanTransitionTo(IState newState);
    }
}