using UnityEngine;

namespace GameFramework.Core.Interfaces
{
    public interface IInputHandler
    {
        Vector2 MovementInput { get; }
        Vector2 LookInput { get; }
        bool JumpPressed { get; }
        bool JumpHeld { get; }
        bool SprintHeld { get; }
        bool CrouchHeld { get; }
        bool AttackPressed { get; }
        bool InteractPressed { get; }
        
        void EnableInput();
        void DisableInput();
    }
}