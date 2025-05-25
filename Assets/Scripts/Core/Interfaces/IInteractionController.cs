using UnityEngine;

namespace GameFramework.Core.Interfaces
{
    public interface IInteractionController
    {
        IInteractable CurrentInteractable { get; }
        bool HasInteractable { get; }
        
        void HandleInteraction();
        void SetInteractionRange(float range);
        void SetLayerMask(LayerMask mask);
    }
}